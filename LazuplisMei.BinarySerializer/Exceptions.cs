using System;
using System.Collections.Generic;
using System.Text;

namespace LazuplisMei.BinarySerializer
{

    public class PropertyCannotReadException : Exception
    {
        public PropertyCannotReadException()
        {

        }

        public PropertyCannotReadException(string propertyName)
            : base($"Property cannot read\npropertyName : {propertyName}")
        {
            
        }
    }

    public class ConstructorNotFoundException : Exception
    {
        public ConstructorNotFoundException()
        {

        }

        public ConstructorNotFoundException(string typeName)
            : base($"constructor not found\ntypeName : {typeName}")
        {

        }
    }


    /// <summary>
    /// Cannot find the specified type , please refer to <see cref="Serializer.AddAssembly(System.Reflection.Assembly)"/>
    /// </summary>
    public class TypeNotFoundException : Exception
    {
        public TypeNotFoundException()
        {

        }

        public TypeNotFoundException(string typeName)
            : base($"type not found\ntypeName : {typeName}")
        {

        }
    }
}
