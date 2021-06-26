using LazuplisMei.BinarySerializer.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LazuplisMei.BinarySerializer
{

    /// <summary>
    /// attribute for fields and attributes, indicating that they will not serialized
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BinaryIgnoreAttribute : Attribute
    {

    }

    /// <summary>
    /// specify a constructor as the entry point for deserialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class BinaryConstructorAttribute : Attribute
    {
        /// <summary>
        /// arguments for invoke constructor
        /// </summary>
        public object[] Args { get; }

        /// <summary>
        /// binary constructor
        /// </summary>
        public BinaryConstructorAttribute(params object[] args)
        {
            Args = args;
        }
    }

    /// <summary>
    /// specify the base class and type parameters of the generic type converter
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class GenericConverterAttribute : Attribute
    {
        /// <summary>
        /// generic typeArgs count
        /// </summary>
        public int GenericTypeArgsCount { get; }
        /// <summary>
        /// generic type
        /// </summary>
        public Type GenericType { get; }

        /// <summary>
        /// generic converter
        /// </summary>
        public GenericConverterAttribute(Type type)
        {
            if (type?.IsGenericType == true)
            {
                GenericType = type;
                GenericTypeArgsCount = ((TypeInfo)type).GenericTypeParameters.Length;
            }
            else
            {
                throw new ArgumentException("must be a generic type");
            }
        }

        /// <summary>
        /// check whether the type is the specified generic type with typeargs
        /// </summary>
        public bool CheckType(Type type, Type[] typeargs)
        {
            if (typeargs.Length == GenericTypeArgsCount)
            {
                try
                {
                    return GenericType.MakeGenericType(typeargs) == type;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }


    /// <summary>
    /// index for field or property while  serializing
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BinaryIndexAttribute : Attribute
    {
        /// <summary>
        /// default minvalue for the index
        /// </summary>
        public static int DefaultMinValue { get; set; } = int.MinValue;
        /// <summary>
        /// Index
        /// </summary>
        public int Index { get;}
        /// <summary>
        /// index for field or property while  serializing
        /// </summary>
        public BinaryIndexAttribute(int index)
        {
            Index = index;
        }
    }

    /// <summary>
    /// specify a BinaryConverter for field or property
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BinaryConverterAttribute : Attribute
    {
        /// <summary>
        /// BinaryConverter
        /// </summary>
        public IBinaryConverter BinaryConverter { get; }
        /// <summary>
        /// BinaryConverter
        /// </summary>
        public BinaryConverterAttribute(Type type)
        {
            if (type?.GetInterface(nameof(IBinaryConverter)) == null)
                throw new ArgumentException("type must implemente IBinaryConverter");
            BinaryConverter = type.GetInstance<IBinaryConverter>();
        }
    }
}
