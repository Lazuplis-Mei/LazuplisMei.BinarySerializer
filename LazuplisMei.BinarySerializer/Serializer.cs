using LazuplisMei.BinarySerializer.Converter;
using LazuplisMei.BinarySerializer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LazuplisMei.BinarySerializer
{
    /// <summary>
    /// main serializer
    /// </summary>
    public static class Serializer
    {

        #region Fields

        private static readonly List<Type> _Converters;

        /// <summary>
        /// indicates whether only <see langword="public"/> member will be serialized
        /// </summary>
        public static bool OnlyPublicMember;

        /// <summary>
        /// indicates whether to automatically add the corresponding assembly of type<br/>
        /// while using <see cref="Serialize(Type, object, Stream)"/>
        /// </summary>
        public static bool AutoAppendAssembly;

        /// <summary>
        /// indicates the encoding used by <see cref="StringConverter"/>
        /// </summary>
        public static Encoding DefaultEncoding;

        /// <summary>
        /// the module corresponding to the loaded assembly used by<see cref="TypeConverter"/>
        /// </summary>
        public static readonly List<Module> Modules = new List<Module>();

        /// <summary>
        /// current BindingFlags of reflection
        /// </summary>
        public static BindingFlags BindingFlags => BindingFlags.Instance | BindingFlags.Public |
            (OnlyPublicMember ? BindingFlags.Default : BindingFlags.NonPublic);

        #endregion

        #region ConfigMethods

        /// <summary>
        /// add a custom BinaryConverter
        /// </summary>
        public static void AddConverter<T>() where T : IBinaryConverter
        {
            if (!_Converters.Contains(typeof(T)))
            {
                _Converters.Insert(0, typeof(T));
            }
        }

        /// <summary>
        /// remove a BinaryConverter
        /// </summary>
        public static void RemoveConverter<T>() where T : IBinaryConverter
        {
            _Converters.Remove(typeof(T));
        }

        /// <summary>
        /// add an assembly used by <see cref="TypeConverter"/>
        /// </summary>
        public static void AddAssembly(Assembly assembly)
        {
            Module module = assembly.Modules.First();
            if (!Modules.Contains(module))
            {
                Modules.Insert(0, module);
            }
        }

        /// <summary>
        /// remove assembly
        /// </summary>
        /// <param name="assembly">程序集</param>
        public static void RemoveAssembly(Assembly assembly)
        {
            Modules.Remove(assembly.Modules.First());
        }

        static Serializer()
        {
            _Converters = new List<Type>();
            OnlyPublicMember = true;
            DefaultEncoding = Encoding.Default;

            AddConverter<ElementTypeConverter>();
            AddConverter<NullableConverter>();

            AddConverter<ListConverter>();
            AddConverter<IEnumerableConverter>();
            AddConverter<ArrayConverter>();
            AddConverter<HashSetConverter>();
            AddConverter<StackConverter>();
            AddConverter<QueueConverter>();

            AddConverter<DictionaryConverter>();
            AddConverter<KeyValuePairConverter>();

            AddConverter<StringConverter>();

            
            AddConverter<DateTimeConverter>();
            AddConverter<DateTimeOffsetConverter>();
            AddConverter<TimeSpanConverter>();
            AddConverter<EnocdingConverter>();

            AddConverter<TypeConverter>();

            AddConverter<BigIntegerConverter>();
            AddConverter<ComplexConverter>();

            AddConverter<VectorConverter>();

            AddAssembly(typeof(int).Assembly);
        }

        #endregion

        #region Serialization

        private static byte[] Compress(MemoryStream stream)
        {
            var memory = new MemoryStream();
            GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true);
            gzip.Write(stream.ToArray(), 0, (int)stream.Length);
            gzip.Close();
            return memory.ToArray();
        }

        /// <summary>
        /// serialize object
        /// </summary>
        public static byte[] Serialize(object obj, bool compress = false)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            MemoryStream stream = new MemoryStream();
            Serialize(obj, stream);
            return compress ? Compress(stream) : stream.ToArray();
        }

        /// <summary>
        /// serialize <see langword="T"/> object
        /// </summary>
        public static byte[] Serialize<T>(T obj, bool compress = false)
        {
            MemoryStream stream = new MemoryStream();
            Serialize(obj, stream);
            return compress ? Compress(stream) : stream.ToArray();
        }

        /// <summary>
        /// serialize object to stream
        /// </summary>
        public static void Serialize(object obj, Stream stream)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            Serialize(obj.GetType(), obj, stream);
        }

        /// <summary>
        /// serialize <see langword="T"/> object to stream
        /// </summary>
        public static void Serialize<T>(T obj, Stream stream)
        {
            Serialize(typeof(T), obj, stream);
        }

        
        private static bool SerializeInternal(Type type, object obj, Stream stream)
        {
            if (AutoAppendAssembly) AddAssembly(type.Assembly);

            if (type.GetInterfaces().Contains(typeof(IBinarySerializable)))
            {
                var serializable = (IBinarySerializable)obj;
                serializable.Serialize(stream);
                return true;
            }

            foreach (var _converter in _Converters)
            {
                var converter = _converter.GetInstance<IBinaryConverter>();

                if (converter.CanConvert(type))
                {
                    converter.WriteBytes(stream, obj);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// serialize object to stream with specified type expected
        /// </summary>
        /// <param name="type">some base type of <paramref name="obj"/></param>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
        public static void Serialize(Type type, object obj, Stream stream)
        {
            if (!SerializeInternal(type, obj, stream))
            {
                if (obj == null)
                {
                    stream.WriteInt32(-1);
                }
                else
                {
                    //occupy a position
                    int startPos = (int)stream.Position;
                    stream.WriteInt32(0);

                    if (type.BaseType != null)
                        ObjectConverter.WriteBytes(type, obj, stream);
                    else
                    {
                        if(!SerializeInternal(obj.GetType(), obj, stream))
                        {
                            throw new ArgumentException("cannot serialize with object type", nameof(type));
                        }
                    }

                    //write object size
                    int currentPos = (int)stream.Position;
                    stream.Position = startPos;
                    stream.WriteInt32(currentPos - startPos - 4);
                    stream.Position = currentPos;
                }
            }
        }

        /// <summary>
        /// serialize not only values but also Type and properties/fields Name
        /// </summary>
        public static void SerializeWithTypeInfo(object obj, Stream stream)
        {
            Type type = obj.GetType();
            var propList = new List<FieldPropertyInfo>();
            propList.AddRange(type.GetFields(BindingFlags).Select(f => new FieldPropertyInfo(f)));
            propList.AddRange(type.GetProperties(BindingFlags).Select(p => new FieldPropertyInfo(p)));
            var props = propList.OrderBy(p => p.Index).ThenBy(p => p.Name);

            //occupy a position
            int startPos = (int)stream.Position;
            stream.WriteInt32(0);
            int count = 0;

            foreach (var prop in props)
            {
                if (prop.TryGetValue(obj, out object value))
                {
                    Serialize(prop.Name, stream);
                    Serialize(prop.Type, stream);
                    Serialize(prop.Type, value, stream);
                    count++;
                }
            }

            //write properties count
            int currentPos = (int)stream.Position;
            stream.Position = startPos;
            stream.WriteInt32(count);
            stream.Position = currentPos;
        }

        #endregion

        #region Deserialization

        private static MemoryStream Decompress(MemoryStream stream)
        {
            GZipStream zipStream = new GZipStream(stream, CompressionMode.Decompress);
            var memory = new MemoryStream();
            zipStream.CopyTo(memory);
            zipStream.Close();
            memory.Position = 0;
            return memory;
        }

        /// <summary>
        /// deserialize object
        /// </summary>
        public static T Deserialize<T>(byte[] bytes, bool decompress = false)
        {
            MemoryStream stream = new MemoryStream(bytes);
            return Deserialize<T>(decompress ? Decompress(stream) : stream);
        }

        /// <summary>
        /// deserialize object with specified type expected
        /// </summary>
        public static object Deserialize(Type type, byte[] bytes, bool decompress = false)
        {
            MemoryStream stream = new MemoryStream(bytes);
            return Deserialize(type, decompress ? Decompress(stream) : stream);
        }

        /// <summary>
        /// deserialize object from stream
        /// </summary>
        public static T Deserialize<T>(Stream stream)
        {
            return (T)Deserialize(typeof(T), stream);
        }

        /// <summary>
        /// deserialize object from stream with specified type expected
        /// </summary>
        public static object Deserialize(Type type, Stream stream)
        {
            if (type.GetInterfaces().Contains(typeof(IBinarySerializable)))
            {
                var serializable = (IBinarySerializable)ObjectConverter.CreateInstance(type);
                serializable.Deserialize(stream);
                return serializable;
            }

            foreach (var _converter in _Converters)
            {
                var converter = _converter.GetInstance<IBinaryConverter>();
                if (converter.CanConvert(type))
                {
                    converter.ReadBytes(stream, out object result);
                    return result;
                }
            }

            int count = stream.ReadInt32();
            if (count != -1)
            {
                if (count == 0)
                    return new object();
                if (type.BaseType != null)
                    return ObjectConverter.ReadBytes(type, stream);
                return stream.ReadBytes(count);
            }
            return null;
        }

        /// <summary>
        /// deserialize by not only values but also Type and properties/fields Name
        /// </summary>
        public static dynamic DeserializeWithTypeInfo(Stream stream)
        {
            IDictionary<string, object> result = new System.Dynamic.ExpandoObject();
            int count = stream.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var name = Deserialize<string>(stream);
                var type = Deserialize<Type>(stream);
                result.Add(name, Deserialize(type, stream));
            }
            return result;
        }

        #endregion

    }
}
