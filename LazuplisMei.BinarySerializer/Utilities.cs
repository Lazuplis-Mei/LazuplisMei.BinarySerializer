using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace LazuplisMei.BinarySerializer
{

    /// <summary>
    /// ExtensionMethods
    /// </summary>
    public static class Utilities
    {
        #region ReflectionExtension

        /// <summary>
        /// Indicates whether the type is <see langword="TBase"/> or inherited from <see langword="TBase"/>
        /// </summary>
        public static bool IsOrBaseFrom<TBase>(this Type self)
        {
            return self.IsOrBaseFrom(typeof(TBase));
        }

        /// <summary>
        /// Indicates whether the type is <see langword="TBase"/> or inherited from <see langword="TBase"/>
        /// </summary>
        public static bool IsOrBaseFrom(this Type self, Type tBase)
        {
            while (self.BaseType != null)
            {
                if (self == tBase) return true;
                self = self.BaseType;
            }
            return tBase.BaseType == null;
        }

        /// <summary>
        /// get <see langword="public/private"/> member value
        /// </summary>
        /// <exception cref="PropertyCannotReadException"/>
        public static T GetMemberValue<T>(this object self, string memberName)
        {
            var flag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type type = self.GetType();
            var field = type.GetField(memberName, flag);
            if (field == null)
            {
                var property = type.GetProperty(memberName, flag);
                if (property.GetMethod == null)
                    throw new PropertyCannotReadException();
                return (T)property.GetValue(self);
            }
            return (T)field.GetValue(self);
        }

        /// <summary>
        /// get <see langword="(readonly) public/private"/> member value
        /// </summary>
        public static void SetMemberValue<T>(this object self, string memberName, T value)
        {
            var flag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type type = self.GetType();
            var field = type.GetField(memberName, flag);
            if (field == null)
            {
                var property = type.GetProperty(memberName, flag);
                if (property.SetMethod != null)
                {
                    property.SetValue(self, value);
                }
            }
            else
            {
                field.SetValue(self, value);
            }
        }

        /// <summary>
        /// invoke method
        /// </summary>
        public static object Invoke(this MethodInfo self, object obj, params object[] args)
        {
            return self.Invoke(obj, args);
        }

        /// <summary>
        /// use (static)self.Instance if exists
        /// </summary>
        public static T GetInstance<T>(this Type self)
        {
            var flag = BindingFlags.Static | BindingFlags.Public;
            var instance = self.GetProperty("Instance", flag);
            return (T)instance?.GetValue(null) ?? (T)Activator.CreateInstance(self);
        }

        #endregion

        #region BytesIntsConvert

        /// <summary>
        /// convert <see langword="byte[]"/> to <see langword="int[]"/>
        /// </summary>
        public static int[] BytesToInts(this byte[] self)
        {
            int re = 4 - self.Length % 4;
            byte[] buffer = self;
            if (re != 4)
            {
                buffer = new byte[self.Length + re];
                self.CopyTo(buffer, 0);
            }

            var result = new int[buffer.Length / 4];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = BitConverter.ToInt32(buffer, i * 4);
            }

            return result;
        }

        /// <summary>
        /// convert <see langword="int[]"/> to <see langword="byte[]"/>
        /// </summary>
        public static byte[] IntsToBytes(this int[] self)
        {
            var result = new byte[self.Length * 4];

            for (int i = 0; i < self.Length; i++)
            {
                byte[] buffer = BitConverter.GetBytes(self[i]);
                buffer.CopyTo(result, i * 4);
            }

            return result;
        }
        
        #endregion

        #region StreamExtension

        /// <summary>
        /// read <see cref="byte"/>[] from stream
        /// </summary>
        public static byte[] ReadBytes(this Stream self, int count)
        {
            byte[] result = new byte[count];
            self.Read(result, 0, count);
            return result;
        }

        /// <summary>
        /// write <see cref="byte"/>[] to stream
        /// </summary>
        public static void WriteBytes(this Stream self, byte[] bytes)
        {
            self.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// read a <see cref="int"/> from stream
        /// </summary>
        public static int ReadInt32(this Stream self)
        {
            return self.ReadByte() | (self.ReadByte() << 8) | (self.ReadByte() << 16) | (self.ReadByte() << 24);
        }

        /// <summary>
        /// write a <see cref="int"/> to stream
        /// </summary>
        public static void WriteInt32(this Stream stream, int num)
        {
            stream.Write(BitConverter.GetBytes(num), 0, 4);
        }

        /// <summary>
        /// read a <see cref="long"/> from stream
        /// </summary>
        public static long ReadInt64(this Stream self)
        {
            return (uint)self.ReadInt32() | ((long)self.ReadInt32() << 32);
        }

        /// <summary>
        /// write a <see cref="long"/> to stream
        /// </summary>
        public static void WriteInt64(this Stream stream, long num)
        {
            stream.Write(BitConverter.GetBytes(num), 0, 8);
        }
        
        #endregion

    }

}
