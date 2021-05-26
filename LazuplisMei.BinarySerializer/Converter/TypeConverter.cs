using LazuplisMei.BinarySerializer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LazuplisMei.BinarySerializer.Converter
{

    class GuidConverter : BinaryConverter<Guid>
    {
        private static GuidConverter _instance;
        public static GuidConverter Instance => _instance ??= new GuidConverter();

        public override Guid ReadBytes(Stream stream)
        {
            return new Guid(stream.ReadBytes(16));
        }

        public override void WriteBytes(Stream stream, Guid obj)
        {
            stream.WriteBytes(obj.ToByteArray());
        }
    }


    /// <summary>
    /// mainly for reflection
    /// </summary>
    class TypeConverter : BinaryConverter<Type>
    {
        private static TypeConverter _instance;
        public static TypeConverter Instance => _instance ??= new TypeConverter();

        private enum ResolveFlag : byte
        {
            NormalType,
            ArrayType,
            GenericType
        }
        public override Type ReadBytes(Stream stream)
        {
            ResolveFlag flag = (ResolveFlag)stream.ReadByte();
            int token = stream.ReadInt32();
            Guid guid = GuidConverter.Instance.ReadBytes(stream);

            foreach (var module in Serializer.Modules)
            {
                try
                {
                    Type type = module.ResolveType(token);
                    if (type.GUID == guid)
                    {
                        switch (flag)
                        {
                            case ResolveFlag.NormalType:
                                return type;
                            case ResolveFlag.ArrayType:
                                return type.MakeArrayType();
                            case ResolveFlag.GenericType:
                            {
                                int count = stream.ReadByte();
                                if (count == 0) return type;

                                Type[] types = new Type[count];
                                for (int i = 0; i < count; i++)
                                {
                                    types[i] = Serializer.Deserialize<Type>(stream);
                                }
                                return type.MakeGenericType(types);
                            }
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }
            throw new TypeNotFoundException();
        }

        public override void WriteBytes(Stream stream, Type obj)
        {
            if (obj.IsArray)
            {
                obj = obj.GetElementType();

                stream.WriteByte((byte)ResolveFlag.ArrayType);
                stream.WriteInt32(obj.MetadataToken);
                GuidConverter.Instance.WriteBytes(stream, obj.GUID);
            }
            else if (obj.IsGenericType)
            {
                stream.WriteByte((byte)ResolveFlag.GenericType);
                stream.WriteInt32(obj.MetadataToken);
                GuidConverter.Instance.WriteBytes(stream, obj.GUID);

                Type[] types = obj.GenericTypeArguments;
                stream.WriteByte((byte)types.Length);
                foreach (var item in types)
                {
                    Serializer.Serialize(item, stream);
                }
            }
            else
            {
                stream.WriteByte((byte)ResolveFlag.NormalType);
                stream.WriteInt32(obj.MetadataToken);
                GuidConverter.Instance.WriteBytes(stream, obj.GUID);
            }
        }
    }
}
