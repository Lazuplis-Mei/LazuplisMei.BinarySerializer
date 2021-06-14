using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LazuplisMei.BinarySerializer.Core
{

    /// <summary>
    /// main converter for object
    /// </summary>
    public static class ObjectConverter
    {

        /// <summary>
        /// create an object
        /// </summary>
        public static object CreateInstance(Type type)
        {
            object result;
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            var constructor = type.GetConstructor(new Type[0]);
            if (constructor == null)
            {
                var constructors = type.GetConstructors(Serializer.BindingFlags |
                    BindingFlags.NonPublic).Where(
                    c => c.GetCustomAttribute<BinaryConstructorAttribute>() != null);
                if (constructors.Count() == 0)
                    throw new ConstructorNotFoundException(type.Name);

                constructor = constructors.First();
                var attribute = constructor.GetCustomAttribute<BinaryConstructorAttribute>();
                var paramInfos = constructor.GetParameters();
                List<object> args = new List<object>();

                for (int i = 0; i < paramInfos.Length; i++)
                {
                    if (i < attribute.Args.Length)
                    {
                        args.Add(attribute.Args[i]);
                    }
                    else
                    {
                        var paramtype = paramInfos[i].ParameterType;
                        if (paramtype.IsValueType)
                        {
                            args.Add(Activator.CreateInstance(paramtype));
                        }
                        else
                        {
                            args.Add(null);
                        }
                    }
                }

                result = constructor.Invoke(args.ToArray());
            }
            else
            {
                result = constructor.Invoke(new object[0]);
            }

            return result;
        }


        /// <summary>
        /// main serialization method
        /// </summary>
        public static void WriteBytes<T>(T obj, Stream stream)
        {
            WriteBytes(typeof(T), obj, stream);
        }

        /// <summary>
        /// main serialization method
        /// </summary>
        public static void WriteBytes(Type type, object obj, Stream stream)
        {
            var fieldInfos = type.GetFields(Serializer.BindingFlags).OrderBy(Utilities.GetIndex).ThenBy(f => f.Name);
            var propertyInfos = type.GetProperties(Serializer.BindingFlags).OrderBy(Utilities.GetIndex).ThenBy(f => f.Name);
            foreach (var field in fieldInfos)
            {
                if (field.GetCustomAttribute<BinaryIgnoreAttribute>() == null)
                {
                    Serializer.Serialize(field.FieldType, field.GetValue(obj), stream);
                }
            }
            foreach (var property in propertyInfos)
            {
                if (property.GetCustomAttribute<BinaryIgnoreAttribute>() == null)
                {
                    if (property.GetMethod == null)
                    {
                        throw new PropertyCannotReadException(property.Name);
                    }
                    if (property.GetMethod.GetParameters().Length == 0)
                    {
                        Serializer.Serialize(property.PropertyType, property.GetValue(obj), stream);
                    }
                }
            }
        }


        /// <summary>
        /// main deserialization method
        /// </summary>
        public static T ReadBytes<T>(Stream stream)
        {
            return (T)ReadBytes(typeof(T), stream);
        }

        /// <summary>
        /// main deserialization method
        /// </summary>
        public static object ReadBytes(Type type, Stream stream)
        {
            var result = CreateInstance(type);
            var fieldInfos = type.GetFields(Serializer.BindingFlags).OrderBy(Utilities.GetIndex).ThenBy(f => f.Name);
            var propertyInfos = type.GetProperties(Serializer.BindingFlags).OrderBy(Utilities.GetIndex).ThenBy(f => f.Name);

            foreach (var field in fieldInfos)
            {
                if (field.GetCustomAttribute<BinaryIgnoreAttribute>() == null)
                {
                    field.SetValue(result, Serializer.Deserialize(field.FieldType, stream));
                }
            }

            foreach (var property in propertyInfos)
            {
                if (property.GetCustomAttribute<BinaryIgnoreAttribute>() == null)
                {
                    if (property.SetMethod != null)
                    {
                        if (property.SetMethod.GetParameters().Length == 1)
                        {
                            var value = Serializer.Deserialize(property.PropertyType, stream);
                            property.SetValue(result, value);
                        }
                    }
                }
            }
            return result;
        }

    }
}
