﻿using System;
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
        public object[] Args { get; }

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
        public int GenericTypeArgsCount { get; }
        public Type GenericType { get; }

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

}