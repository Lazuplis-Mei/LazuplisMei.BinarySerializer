using System;
using System.Collections.Generic;
using System.Text;

namespace LazuplisMei.BinarySerializer
{
    /// <summary>
    /// property cannot read
    /// </summary>
    public class PropertyCannotReadException : Exception
    {
        /// <summary>
        /// property cannot read
        /// </summary>
        public PropertyCannotReadException()
        {

        }
        /// <summary>
        /// property cannot read
        /// </summary>
        public PropertyCannotReadException(string propertyName)
            : base($"Property cannot read\npropertyName : {propertyName}")
        {
            
        }
    }

    /// <summary>
    /// constructor not found
    /// </summary>
    public class ConstructorNotFoundException : Exception
    {
        /// <summary>
        /// constructor not found
        /// </summary>
        public ConstructorNotFoundException()
        {

        }
        /// <summary>
        /// constructor not found
        /// </summary>
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
        /// <summary>
        /// type not found
        /// </summary>
        public TypeNotFoundException()
        {

        }
        /// <summary>
        /// type not found
        /// </summary>
        public TypeNotFoundException(string typeName)
            : base($"type not found\ntypeName : {typeName}")
        {

        }
    }
}
