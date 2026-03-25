using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Common
{
    /// <summary>
    /// 数据帮助类  
    /// </summary>
    public static class DataHelp
    {
        /// <summary>
        /// 获取类型占用的字节数量(Byte)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>0为无法判断</returns>
        public static ushort GetByteCount<T>()
        {
            var tType = typeof(T);
            if (tType == typeof(bool) || tType == typeof(byte))
                return 1;
            else if (tType == typeof(short) || tType == typeof(ushort))
                return 2;
            else if (tType == typeof(int) || tType == typeof(uint) || tType == typeof(float))
                return 4;
            else if (tType == typeof(double) || tType == typeof(long) || tType == typeof(ulong))
                return 8;
            else
                return 0;
        }
        /// <summary>
        /// 获取类型占用的字数量(Word = 2Byte)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>0为无法判断</returns>
        public static ushort GetWordCount<T>()
        {
            var tType = typeof(T);
            if (tType == typeof(bool) || tType == typeof(byte) || tType == typeof(short) || tType == typeof(ushort))
                return 1;
            else if (tType == typeof(int) || tType == typeof(uint) || tType == typeof(float))
                return 2;
            else if (tType == typeof(double) || tType == typeof(long) || tType == typeof(ulong))
                return 4;
            else
                return 0;
        }

        /// <summary>
        /// 数组是否相等
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="byteArray2"></param>
        /// <returns></returns>
        public static bool ArrayEquals(this byte[] byteArray, byte[] byteArray2)
        {
            if (ReferenceEquals(byteArray, byteArray2))
                return true;
            if (byteArray == null || byteArray2 == null)
                return false;
            if (byteArray.Length != byteArray2.Length)
                return false;

            for (int i = 0; i < byteArray.Length; i++)
            {
                if (byteArray[i] != byteArray2[i])
                    return false;
            }

            return true;
        }
        /// <summary>
        /// 两个数组开头是否相等
        /// </summary>
        public static bool StartsWith(this byte[] byteArray, byte[] byteArray2)
        {
            if (byteArray == byteArray2)
                return true;
            if (byteArray == null || byteArray2 == null)
                return false;
            if (byteArray.Length < byteArray2.Length)
                return false;

            for (int i = 0; i < byteArray2.Length; i++)
            {
                if (byteArray2[i] != byteArray[i])
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 两个数组结尾是否相等
        /// </summary>
        public static bool EndsWith(this byte[] byteArray, byte[] byteArray2)
        {
            if (byteArray == byteArray2)
                return true;
            if (byteArray == null || byteArray2 == null)
                return false;
            if (byteArray.Length < byteArray2.Length)
                return false;
            for (int i = 1; i <= byteArray2.Length; i++)
            {
                if (byteArray2[byteArray2.Length - i] != byteArray[byteArray.Length - i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 取整数的某一位
        /// </summary>
        /// <param name="value">要取某一位的整数</param>
        /// <param name="index">要取的位置索引，自右至左为0-7</param>
        /// <returns>返回某一位的值</returns>
        public static bool GetBit(int value, int index) => (value >> index & 1) == 1;
        /// <summary>
        /// 将整数的某位置设为0或1
        /// </summary>
        /// <param name="value">整数</param>
        /// <param name="index">整数的某位</param>
        /// <param name="newValue">是否置1，TURE表示置1，FALSE表示置0</param>
        /// <returns>返回修改过的值</returns>
        public static int SetBit(int value, int index, bool newValue) => newValue ? value | (0x1 << index) : value & ~(0x1 << index);
    }
}
