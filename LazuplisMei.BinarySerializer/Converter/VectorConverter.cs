using LazuplisMei.BinarySerializer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace LazuplisMei.BinarySerializer.Converter
{
    [GenericConverter(typeof(Vector<>))]
    class VectorConverter : GenericConverter
    {
        public override object GenericReadBytes(Stream stream)
        {
            var list = ListConverter.ListReadBytes(TypeArgs[0], stream);
            var list_TToArray = typeof(List<>).MakeGenericType(TypeArgs).GetMethod("ToArray");
            return Activator.CreateInstance(CurrentType, list_TToArray.Invoke(list));
        }

        public override void GenericWriteBytes(Stream stream, object obj)
        {
            var count = (int)CurrentType.GetProperty("Count").GetValue(null);
            var arr = Array.CreateInstance(TypeArgs[0], count);
            CurrentType.GetMethod("CopyTo", new[] { arr.GetType() }).Invoke(obj, arr);
            ListConverter.ListWriteBytes(TypeArgs[0], stream, arr);
        }
    }

}
