using LazuplisMei.BinarySerializer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace LazuplisMei.BinarySerializer.Converter
{
    class BigIntegerConverter : BinaryConverter<BigInteger>
    {
        private static BigIntegerConverter _instance;
        public static BigIntegerConverter Instance => _instance ??= new BigIntegerConverter();

        public override BigInteger ReadBytes(Stream stream)
        {
            var count = stream.ReadInt32();
            var buffer = stream.ReadBytes(count);
            return new BigInteger(buffer);
        }

        public override void WriteBytes(Stream stream, BigInteger obj)
        {
            var buffer = obj.ToByteArray();
            stream.WriteInt32(buffer.Length);
            stream.WriteBytes(buffer);
        }
    }

    class ComplexConverter : BinaryConverter<Complex>
    {
        private static ComplexConverter _instance;
        public static ComplexConverter Instance => _instance ??= new ComplexConverter();

        public override Complex ReadBytes(Stream stream)
        {
            var real = stream.ReadDouble();
            var imaginary = stream.ReadDouble();
            return new Complex(real, imaginary);
        }

        public override void WriteBytes(Stream stream, Complex obj)
        {
            stream.WriteDouble(obj.Real);
            stream.WriteDouble(obj.Imaginary);
        }
    }

}
