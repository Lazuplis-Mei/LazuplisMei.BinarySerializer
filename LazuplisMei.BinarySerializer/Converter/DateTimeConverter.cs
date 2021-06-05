using LazuplisMei.BinarySerializer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LazuplisMei.BinarySerializer.Converter
{
    class DateTimeConverter : BinaryConverter<DateTime>
    {
        private static DateTimeConverter _instance;
        public static DateTimeConverter Instance => _instance ??= new DateTimeConverter();
        public override DateTime ReadBytes(Stream stream)
        {
            return DateTime.FromBinary(stream.ReadInt64());
        }
        public override void WriteBytes(Stream stream, DateTime obj)
        {
            stream.WriteInt64(obj.ToBinary());
        }
    }

    class DateTimeOffsetConverter : BinaryConverter<DateTimeOffset>
    {
        private static DateTimeOffsetConverter _instance;
        public static DateTimeOffsetConverter Instance => _instance ??= new DateTimeOffsetConverter();
        public override DateTimeOffset ReadBytes(Stream stream)
        {
            return DateTimeOffset.FromFileTime(stream.ReadInt64());
        }
        public override void WriteBytes(Stream stream,DateTimeOffset obj)
        {
            stream.WriteInt64(obj.ToFileTime());
        }
    }

    class TimeSpanConverter : BinaryConverter<TimeSpan>
    {
        private static TimeSpanConverter _instance;
        public static TimeSpanConverter Instance => _instance ??= new TimeSpanConverter();
        public override TimeSpan ReadBytes(Stream stream)
        {
            return TimeSpan.FromTicks(stream.ReadInt64());
        }
        public override void WriteBytes(Stream stream,TimeSpan obj)
        {
            stream.WriteInt64(obj.Ticks);
        }
    }

}
