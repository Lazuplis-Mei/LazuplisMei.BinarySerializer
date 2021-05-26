using LazuplisMei.BinarySerializer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LazuplisMei.BinarySerializer.Converter
{

    [GenericConverter(typeof(Nullable<>))]
    public class NullableConverter : GenericConverter
    {

        public override object GenericReadBytes(Stream stream)
        {
            return Serializer.Deserialize(TypeArgs[0], stream);
        }

        public override void GenericWriteBytes(Stream stream, object obj)
        {
            Serializer.Serialize(obj, stream);
        }
    }
}
