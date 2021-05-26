using LazuplisMei.BinarySerializer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LazuplisMei.BinarySerializer.Converter
{
    
    [GenericConverter(typeof(List<>))]
    class ListConverter : GenericConverter
    {

        /// <summary>
        /// read a list of data from stream
        /// </summary>
        public static object ListReadBytes(Type internalType, Stream stream)
        {
            var listType = typeof(List<>).MakeGenericType(internalType);
            object list = Activator.CreateInstance(listType);
            var list_Add = listType.GetMethod("Add");

            int count = stream.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                list_Add.Invoke(list, Serializer.Deserialize(internalType, stream));
            }
            return list;
        }

        public override object GenericReadBytes(Stream stream)
        {
            return ListReadBytes(TypeArgs[0], stream);
        }

        /// <summary>
        /// write a list of data to stream
        /// </summary>
        public static void ListWriteBytes(Type internalType, Stream stream, object obj)
        {
            var list = ((System.Collections.IEnumerable)obj).Cast<object>();
            stream.WriteInt32(list.Count());
            foreach (var item in list)
            {
                Serializer.Serialize(internalType, item, stream);
            }
        }

        public override void GenericWriteBytes(Stream stream, object obj)
        {
            ListWriteBytes(TypeArgs[0], stream, obj);
        }
    }

    /// <summary>
    /// any IEnumerable<T> will be treated as List<T>
    /// </summary>
    [GenericConverter(typeof(IEnumerable<>))]
    class IEnumerableConverter : GenericConverter
    {
        public override object GenericReadBytes(Stream stream)
        {
            return ListConverter.ListReadBytes(TypeArgs[0], stream);
        }

        public override void GenericWriteBytes(Stream stream, object obj)
        {
            ListConverter.ListWriteBytes(TypeArgs[0], stream, obj);
        }
    }

    class ArrayConverter : IBinaryConverter
    {
        public Type InternalType { get; private set; }
        public int Rank { get; private set; }

        public bool CanConvert(Type type)
        {
            if (type.IsArray)
            {
                InternalType = type.GetElementType();
                Rank = type.GetArrayRank();
                return true;
            }
            return false;
        }
        public void ReadBytes(Stream stream, out object obj)
        {
            int[] lens = new int[Rank];
            for (int i = 0; i < Rank; i++)
            {
                lens[i] = stream.ReadInt32();
            }

            var list = (System.Collections.IEnumerable)ListConverter.ListReadBytes(InternalType, stream);
            var arr = Array.CreateInstance(InternalType, lens);
            int[] indices = new int[arr.Rank];

            foreach (var item in list)
            {
                arr.SetValue(item, indices);
                indices[arr.Rank - 1]++;
                for (int i = 1; i < arr.Rank; i++)
                {
                    if (indices[arr.Rank - i] < lens[arr.Rank - i])
                        break;
                    indices[arr.Rank - i] = 0;
                    indices[arr.Rank - i - 1]++;
                }
            }

            obj = arr;
        }
        public void WriteBytes(Stream stream, object obj)
        {
            for (int i = 0; i < Rank; i++)
            {
                stream.WriteInt32(((Array)obj).GetLength(i));
            }
            ListConverter.ListWriteBytes(InternalType, stream, obj);
        }
    }

    
}
