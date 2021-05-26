using LazuplisMei.BinarySerializer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LazuplisMei.BinarySerializer.Converter
{
    /// <summary>
    /// element keyword type
    /// </summary>
    public enum ElementType
    {
        NotElementType = -1,
        Boolean,
        Char,
        Int16,
        Int32,
        Int64,
        UInt16,
        UInt32,
        UInt64,
        Single,
        Double,
        Byte,
        SByte,
        Decimal
    }

    public class ElementTypeConverter : IBinaryConverter
    {
        private static ElementTypeConverter _instance;
        public static ElementTypeConverter Instance => _instance ??= new ElementTypeConverter();

        private static readonly Type[] _elementTypes = {
             typeof(bool),
             typeof(char),
             typeof(short),
             typeof(int),
             typeof(long),
             typeof(ushort),
             typeof(uint),
             typeof(ulong),
             typeof(float),
             typeof(double),
             typeof(byte),
             typeof(sbyte),
             typeof(decimal)
        };
        private static readonly int[] _elementTypesSize = {
             sizeof(bool),
             sizeof(char),
             sizeof(short),
             sizeof(int),
             sizeof(long),
             sizeof(ushort),
             sizeof(uint),
             sizeof(ulong),
             sizeof(float),
             sizeof(double),
             sizeof(byte),
             sizeof(sbyte),
             sizeof(decimal)
        };

        public ElementType Type { get; private set; }
        public int SizeOfType { get; private set; }

        public bool CanConvert(Type type)
        {
            Type = (ElementType)Array.IndexOf(_elementTypes, type);
            if (Type != ElementType.NotElementType)
            {
                SizeOfType = _elementTypesSize[(int)Type];
                return true;
            }
            return false;
        }
        public void ReadBytes(Stream stream, out object obj)
        {
            byte[] buffer = new byte[_elementTypesSize.Max()];
            stream.Read(buffer, 0, SizeOfType);
            obj = Type switch
            {
                ElementType.Boolean => BitConverter.ToBoolean(buffer, 0),
                ElementType.Char => BitConverter.ToChar(buffer, 0),
                ElementType.Int16 => BitConverter.ToInt16(buffer, 0),
                ElementType.Int32 => BitConverter.ToInt32(buffer, 0),
                ElementType.Int64 => BitConverter.ToInt64(buffer, 0),
                ElementType.UInt16 => BitConverter.ToUInt16(buffer, 0),
                ElementType.UInt32 => BitConverter.ToUInt32(buffer, 0),
                ElementType.UInt64 => BitConverter.ToUInt64(buffer, 0),
                ElementType.Single => BitConverter.ToSingle(buffer, 0),
                ElementType.Double => BitConverter.ToDouble(buffer, 0),
                ElementType.Byte => buffer[0],
                ElementType.SByte => (sbyte)buffer[0],
                ElementType.Decimal => new decimal(buffer.BytesToInts()),
                _ => throw new ArgumentException(),
            };
        }
        public void WriteBytes(Stream stream, object obj)
        {
            byte[] buffer = Type switch
            {
                ElementType.Boolean => BitConverter.GetBytes((bool)obj),
                ElementType.Char => BitConverter.GetBytes((char)obj),
                ElementType.Int16 => BitConverter.GetBytes((short)obj),
                ElementType.Int32 => BitConverter.GetBytes((int)obj),
                ElementType.Int64 => BitConverter.GetBytes((long)obj),
                ElementType.UInt16 => BitConverter.GetBytes((ushort)obj),
                ElementType.UInt32 => BitConverter.GetBytes((uint)obj),
                ElementType.UInt64 => BitConverter.GetBytes((ulong)obj),
                ElementType.Single => BitConverter.GetBytes((float)obj),
                ElementType.Double => BitConverter.GetBytes((double)obj),
                ElementType.Byte => new byte[] { (byte)obj },
                ElementType.SByte => new byte[] { (byte)(sbyte)obj },
                ElementType.Decimal => decimal.GetBits((decimal)obj).IntsToBytes(),
                _ => throw new ArgumentException(),
            };
            stream.Write(buffer, 0, SizeOfType);
        }

    }

}
