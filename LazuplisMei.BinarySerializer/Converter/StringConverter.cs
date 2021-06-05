using LazuplisMei.BinarySerializer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LazuplisMei.BinarySerializer.Converter
{
    class StringConverter : BinaryConverter<string>
    {
        private static StringConverter _instance;
        public static StringConverter Instance => _instance ??= new StringConverter();
        public override string ReadBytes(Stream stream)
        {
            byte[] bytes = stream.ReadBytes(stream.ReadInt32());
            return Serializer.DefaultEncoding.GetString(bytes);
        }

        public override void WriteBytes(Stream stream, string obj)
        {
            byte[] bytes = Serializer.DefaultEncoding.GetBytes(obj);
            stream.WriteInt32(bytes.Length);
            stream.WriteBytes(bytes);
        }
    }

    class EnocdingConverter : BinaryConverter<Encoding>
    {
        private static EnocdingConverter _instance;
        public static EnocdingConverter Instance => _instance ??= new EnocdingConverter();
        public override Encoding ReadBytes(Stream stream)
        {
            return Encoding.GetEncoding(stream.ReadInt32());
        }
        public override void WriteBytes(Stream stream, Encoding obj)
        {
            stream.WriteInt32(obj.CodePage);
        }
    }
}
