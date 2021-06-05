using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LazuplisMei.BinarySerializer.Core
{

    /// <summary>
    /// provide base interface for BinaryConverters
    /// </summary>
    public interface IBinaryConverter
    {
        //Optional : if converter have no fields and properties
        //recommend subclasses to implement singleton mode
        //private static XXXConverter _instance;
        //public static XXXConverter Instance => _instance ??= new XXXConverter();

        /// <summary>
        /// indicates whether the specified type can be converted
        /// </summary>
        bool CanConvert(Type type);

        /// <summary>
        /// basic interface serialization method 
        /// </summary>
        void WriteBytes(Stream stream, object obj);

        /// <summary>
        /// basic interface deserialization method 
        /// </summary>
        void ReadBytes(Stream stream, out object obj);

    }

    /// <summary>
    /// provide base <see langword="abstract"/> class for BinaryConverters
    /// </summary>
    public abstract class BinaryConverter<T> : IBinaryConverter
    {
        //Optional : if converter have no fields and properties
        //recommend subclasses to implement singleton mode
        //private static XXXConverter _instance;
        //public static XXXConverter Instance => _instance ??= new XXXConverter();

        /// <summary>
        /// indicates whether the specified type <see langword="T"/> can be converted
        /// </summary>
        public bool CanConvert(Type type)
        {
            return type.IsOrBaseFrom<T>();
        }

        /// <summary>
        /// implemented serialization method 
        /// </summary>
        public void WriteBytes(Stream stream, object obj)
        {
            if (obj == null)
            {
                stream.WriteByte(0);
            }
            else
            {
                stream.WriteByte(1);
                WriteBytes(stream, (T)obj);
            }
        }

        /// <summary>
        /// implemented deserialization method 
        /// </summary>
        public void ReadBytes(Stream stream, out object obj)
        {
            obj = null;
            if (stream.ReadByte() != 0)
            {
                obj = ReadBytes(stream);
            }
        }

        /// <summary>
        /// basic serialization method 
        /// </summary>
        public abstract void WriteBytes(Stream stream, T obj);

        /// <summary>
        /// basic deserialization method 
        /// </summary>
        public abstract T ReadBytes(Stream stream);
    }

    /// <summary>
    /// for custom implementation of serialization 
    /// </summary>
    public interface IBinarySerializable
    {
        /// <summary>
        /// custom implementation of serialization 
        /// </summary>
        void Serialize(Stream stream);
        /// <summary>
        /// custom implementation of deserialization 
        /// </summary>
        void Deserialize(Stream stream);
    }
}
