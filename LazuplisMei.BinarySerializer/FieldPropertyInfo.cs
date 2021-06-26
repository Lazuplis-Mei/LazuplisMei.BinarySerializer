using LazuplisMei.BinarySerializer.Core;
using System;
using System.IO;
using System.Reflection;

namespace LazuplisMei.BinarySerializer
{
    class FieldPropertyInfo
    {
        private readonly FieldInfo _FieldInfo;
        private readonly PropertyInfo _PropertyInfo;

        public FieldPropertyInfo(FieldInfo field)
        {
            _FieldInfo = field;
        }

        public FieldPropertyInfo(PropertyInfo property)
        {
            _PropertyInfo = property;
        }

        private bool IsProperty => _PropertyInfo != null;

        private MemberInfo CurrentMember => IsProperty ? (MemberInfo)_PropertyInfo : _FieldInfo;

        private IBinaryConverter BinaryConverter => CurrentMember.GetCustomAttribute<BinaryConverterAttribute>()?.BinaryConverter;

        public string Name => CurrentMember.Name;

        public Type Type => IsProperty ? _PropertyInfo.PropertyType : _FieldInfo.FieldType;

        public bool Ignored => CurrentMember.GetCustomAttribute<BinaryIgnoreAttribute>() != null;

        public int Index => CurrentMember.GetCustomAttribute<BinaryIndexAttribute>()?.Index ?? BinaryIndexAttribute.DefaultMinValue;


        public bool TrySerialize(Stream stream, object value)
        {
            if (BinaryConverter != null)
            {
                if (BinaryConverter.CanConvert(Type))
                {
                    BinaryConverter.WriteBytes(stream, value);
                    return true;
                }
            }
            return false;
        }

        public bool TryDeserialize(Stream stream, out object result)
        {
            result = null;
            if (BinaryConverter != null)
            {
                if (BinaryConverter.CanConvert(Type))
                {
                    BinaryConverter.ReadBytes(stream, out result);
                    return true;
                }
            }
            return false;
        }

        public bool TryGetValue(object obj, out object value)
        {
            value = null;
            if (!Ignored)
            {
                if (IsProperty)
                {
                    if (_PropertyInfo.GetMethod == null)
                    {
                        throw new PropertyCannotReadException(_PropertyInfo.Name);
                    }
                    if (_PropertyInfo.GetMethod.GetParameters().Length == 0)
                    {
                        value = _PropertyInfo.GetValue(obj);
                        return true;
                    }
                }
                else
                {
                    value = _FieldInfo.GetValue(obj);
                    return true;
                }
            }
            return false;
        }

        public void SetValue(object obj, object value)
        {
            if (IsProperty)
            {
                if (_PropertyInfo.SetMethod != null)
                {
                    if (_PropertyInfo.SetMethod.GetParameters().Length == 1)
                    {
                        _PropertyInfo.SetValue(obj, value);
                    }
                }
            }
            else
            {
                _FieldInfo.SetValue(obj, value);
            }
        }
    }


}
