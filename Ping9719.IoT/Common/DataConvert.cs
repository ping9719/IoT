using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ping9719.IoT.Common
{
    /// <summary>
    /// 数据转换
    /// </summary>
    public static class DataConvert
    {
        /// <summary>
        /// 字节数组转16进制字符
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public static string ByteArrayToString(this byte[] byteArray)
        {
            return string.Join(" ", byteArray.Select(t => t.ToString("X2")));
        }
        /// <summary>
        /// 开头是否相等
        /// </summary>
        public static bool StartsWith(this byte[] byteArray, byte[] byteArray2)
        {
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
        /// 结尾是否相等
        /// </summary>
        public static bool EndsWith(this byte[] byteArray, byte[] byteArray2)
        {
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
        /// 16进制字符串转字节数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="strict">严格模式（严格按两个字母间隔一个空格）</param>
        /// <returns></returns>
        public static byte[] StringToByteArray(this string str, bool strict = true)
        {
            if (string.IsNullOrWhiteSpace(str) || str.Trim().Replace(" ", "").Length % 2 != 0)
                throw new ArgumentException("请传入有效的参数");

            if (strict)
            {
                return str.Split(' ').Where(t => t?.Length == 2).Select(t => Convert.ToByte(t, 16)).ToArray();
            }
            else
            {
                str = str.Trim().Replace(" ", "");
                var list = new List<byte>();
                for (int i = 0; i < str.Length; i++)
                {
                    var string16 = str[i].ToString() + str[++i].ToString();
                    list.Add(Convert.ToByte(string16, 16));
                }
                return list.ToArray();
            }
        }

        /// <summary>
        /// Asciis字符串数组字符串装字节数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="strict"></param>
        /// <returns></returns>
        public static byte[] AsciiStringToByteArray(this string str, bool strict = true)
        {
            if (string.IsNullOrWhiteSpace(str) || str.Trim().Replace(" ", "").Length % 2 != 0)
                throw new ArgumentException("请传入有效的参数");

            if (strict)
            {
                List<string> stringList = new List<string>();
                foreach (var item in str.Split(' '))
                {
                    stringList.Add(((char)(Convert.ToByte(item, 16))).ToString());
                }
                return StringToByteArray(string.Join("", stringList), false);
            }
            else
            {
                str = str.Trim().Replace(" ", "");
                var stringList = new List<string>();
                for (int i = 0; i < str.Length; i++)
                {
                    var stringAscii = str[i].ToString() + str[++i].ToString();
                    stringList.Add(((char)Convert.ToByte(stringAscii, 16)).ToString());
                }
                return StringToByteArray(string.Join("", stringList), false);
            }
        }

        /// <summary>
        /// Asciis数组字符串装字节数组
        /// 如：30 31 =》 00 01
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] AsciiArrayToByteArray(this byte[] str)
        {
            if (!str?.Any() ?? true)
                throw new ArgumentException("请传入有效的参数");

            List<string> stringList = new List<string>();
            foreach (var item in str)
            {
                stringList.Add(((char)item).ToString());
            }
            return StringToByteArray(string.Join("", stringList), false);
        }

        /// <summary>
        /// 字节数组转换成Ascii字节数组
        /// 如：00 01 => 30 31
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] ByteArrayToAsciiArray(this byte[] str)
        {
            return Encoding.ASCII.GetBytes(string.Join("", str.Select(t => t.ToString("X2"))));
        }

        /// <summary>
        /// Int转二进制
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minLength">补0长度</param>
        /// <returns></returns>
        public static string IntToBinaryArray(this int value, int minLength = 0)
        {
            //Convert.ToString(12,2); // 将12转为2进制字符串，结果 “1100”
            return Convert.ToString(value, 2).PadLeft(minLength, '0');
        }

        /// <summary>
        /// Byte转二进制bool数组
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minLength">补长度</param>
        /// <returns></returns>
        public static bool[] ByteToBinaryBoolArray(this byte value, int minLength = 8, bool isReverse = true)
        {
            if (isReverse)
                return Convert.ToString(value, 2).PadLeft(minLength, '0').Select(o => o == '1').Reverse().ToArray();
            return Convert.ToString(value, 2).PadLeft(minLength, '0').Select(o => o == '1').ToArray();
        }

        /// <summary>
        /// 二进制转Int
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int BinaryArrayToInt(this string value)
        {
            //Convert.ToInt("1100",2); // 将2进制字符串转为整数，结果 12
            return Convert.ToInt32(value, 2);
        }

        /// <summary>
        /// 取整数的某一位
        /// </summary>
        /// <param name="value">要取某一位的整数</param>
        /// <param name="index">要取的位置索引，自右至左为0-7</param>
        /// <returns>返回某一位的值</returns>
        public static bool GetBitValue(int value, int index)
        {
            return (value >> index & 1) == 1;
        }

        /// <summary>
        /// 将整数的某位置设为0或1
        /// </summary>
        /// <param name="value">整数</param>
        /// <param name="index">整数的某位</param>
        /// <param name="newValue">是否置1，TURE表示置1，FALSE表示置0</param>
        /// <returns>返回修改过的值</returns>
        public static byte SetBitValue(byte value, int index, bool newValue)
        {
            return newValue ? (byte)(value | (0x1 << index)) : (byte)(value & ~(0x1 << index));
        }

        /// <summary>
        /// 将整数的某位置设为0或1
        /// </summary>
        /// <param name="value">整数</param>
        /// <param name="index">整数的某位</param>
        /// <param name="newValue">是否置1，TURE表示置1，FALSE表示置0</param>
        /// <returns>返回修改过的值</returns>
        public static int SetBitValue(int value, int index,  bool newValue)
        {
            return newValue ? value | (0x1 << index) : value & ~(0x1 << index);
        }
    }
}
