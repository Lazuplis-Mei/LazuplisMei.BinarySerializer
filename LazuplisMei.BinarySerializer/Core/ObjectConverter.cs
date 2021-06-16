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
            var propList = new List<FieldPropertyInfo>();
            propList.AddRange(type.GetFields(Serializer.BindingFlags).Select(f => new FieldPropertyInfo(f)));
            propList.AddRange(type.GetProperties(Serializer.BindingFlags).Select(p => new FieldPropertyInfo(p)));
            var props = propList.OrderBy(p => p.Index).ThenBy(p => p.Name);

            foreach (var prop in props)
            {
                if (prop.TryGetValue(obj, out object value))
                {
                    Serializer.Serialize(prop.Type, value, stream);
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
            var propList = new List<FieldPropertyInfo>();
            propList.AddRange(type.GetFields(Serializer.BindingFlags).Select(f => new FieldPropertyInfo(f)));
            propList.AddRange(type.GetProperties(Serializer.BindingFlags).Select(p => new FieldPropertyInfo(p)));
            var props = propList.OrderBy(p => p.Index).ThenBy(p => p.Name);

            foreach (var prop in props)
            {
                if (!prop.Ignored)
                {
                    prop.SetValue(result, Serializer.Deserialize(prop.Type, stream));
                }
            }

            return result;
        }

    }
}
