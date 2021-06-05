using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace LazuplisMei.BinarySerializer.Core
{
    /// <summary>
    /// provide converter base classes for generic types<br/>
    /// to define a GenericTypeConverter, <see cref="GenericConverterAttribute"/> is required
    /// </summary>
    public abstract class GenericConverter : IBinaryConverter
    {
        //singleton mode is not allowed here and subclass

        /// <summary>
        /// property from <see cref="GenericConverterAttribute"/>
        /// </summary>
        public Type GenericType { get; private set; }
        /// <summary>
        /// type arguments
        /// </summary>
        public Type[] TypeArgs { get; private set; }
        /// <summary>
        /// target type to be convert
        /// </summary>
        public Type CurrentType { get; private set; }

        /// <summary>
        /// indicates whether the specified generic type can be converted
        /// </summary>
        public bool CanConvert(Type type)
        {
            var attribute = GetType().GetCustomAttribute<GenericConverterAttribute>();
            if (attribute != null)
            {
                GenericType = attribute.GenericType;
                TypeArgs = type.GenericTypeArguments;
                CurrentType = type;
                return attribute.CheckType(type, TypeArgs);
            }
            return false;
        }
        /// <summary>
        /// implemented generic serialization method 
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
                GenericWriteBytes(stream, obj);
            }
        }
        /// <summary>
        /// implemented generic deserialization method 
        /// </summary>
        public void ReadBytes(Stream stream, out object obj)
        {
            obj = null;
            if (stream.ReadByte() != 0)
            {
                obj = GenericReadBytes(stream);
            }
        }

        /// <summary>
        /// basic abstract generic serialization method
        /// </summary>
        public abstract void GenericWriteBytes(Stream stream, object obj);
        /// <summary>
        /// basic abstract generic deserialization method
        /// </summary>
        public abstract object GenericReadBytes(Stream stream);

    }
}
