using LazuplisMei.BinarySerializer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace LazuplisMei.BinarySerializer.Converter
{

    /// <summary>
    /// base implement for <see cref="DictionaryConverter"/>
    /// </summary>
    [GenericConverter(typeof(KeyValuePair<,>))]
    public class KeyValuePairConverter : GenericConverter
    {
        public override object GenericReadBytes(Stream stream)
        {
            object key = Serializer.Deserialize(TypeArgs[0], stream);
            object value = Serializer.Deserialize(TypeArgs[1], stream);
            return Activator.CreateInstance(CurrentType, key, value);
        }

        public override void GenericWriteBytes(Stream stream, object obj)
        {
            Serializer.Serialize(TypeArgs[0], CurrentType.GetProperty("Key").GetValue(obj), stream);
            Serializer.Serialize(TypeArgs[1], CurrentType.GetProperty("Value").GetValue(obj), stream);
        }

    }


    [GenericConverter(typeof(Dictionary<,>))]
    public class DictionaryConverter : GenericConverter
    {
        public override object GenericReadBytes(Stream stream)
        {
            var innerType = typeof(KeyValuePair<,>).MakeGenericType(TypeArgs);
            var converter = new IEnumerableConverter();
            converter.CanConvert(typeof(IEnumerable<>).MakeGenericType(innerType));
            var pairs = converter.GenericReadBytes(stream);
            return Activator.CreateInstance(CurrentType, pairs);
        }


        public override void GenericWriteBytes(Stream stream, object obj)
        {
            var innerType = typeof(KeyValuePair<,>).MakeGenericType(TypeArgs);
            var converter = new IEnumerableConverter();
            converter.CanConvert(typeof(IEnumerable<>).MakeGenericType(innerType));
            converter.GenericWriteBytes(stream, obj);
        }
    }
}
