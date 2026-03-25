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
        public static string BytesToHexString(this byte[] byteArray, string sepa = " ")
        {
            if (byteArray == null)
                return null;

            return string.Join(sepa, byteArray.Select(b => b.ToString("X2")));
        }
        /// <summary>
        /// 16进制字符串转字节数组
        /// </summary>
        /// <param name="str16"></param>
        /// <param name="sepa">分隔符</param>
        /// <returns></returns>
        public static byte[] HexStringToBytes(this string str16, string sepa = " ")
        {
            if (str16 == null)
                return null;

            var cleanHex = str16.Trim().Replace(sepa, "");
            if (string.IsNullOrEmpty(cleanHex) || cleanHex.Length % 2 != 0)
                throw new ArgumentException($"16进制字符串格式无效：{str16}", nameof(str16));

            var bytes = new byte[cleanHex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(cleanHex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        /// <summary>
        /// ASCII字节数组转字节数组
        /// 如：30 31 => 00 01
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] AsciiBytesToBytes(this byte[] asciiBytes)
        {
            if (asciiBytes == null)
                return null;
            if (asciiBytes.Length == 0)
                return new byte[0];

            string hexString = Encoding.ASCII.GetString(asciiBytes);
            return HexStringToBytes(hexString);
        }
        /// <summary>
        /// 字节数组转换成Ascii字节数组
        /// 如：00 01 => 30 31
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] BytesToAsciiBytes(this byte[] bytes)
        {
            if (bytes == null)
                return null;

            return Encoding.ASCII.GetBytes(bytes.BytesToHexString(""));
        }

        /// <summary>
        /// Byte转二进制bool数组
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minLength">补长度</param>
        /// <returns></returns>
        public static bool[] ByteToBin(this byte value, int minLength = 8, bool isReverse = true)
        {
            if (isReverse)
                return Convert.ToString(value, 2).PadLeft(minLength, '0').Select(o => o == '1').Reverse().ToArray();
            return Convert.ToString(value, 2).PadLeft(minLength, '0').Select(o => o == '1').ToArray();
        }
        /// <summary>
        /// Byte转二进制bool数组
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minLength">补长度</param>
        /// <returns></returns>
        public static bool[] ByteToBin(this byte[] value, bool isReverse = true)
        {
            if (isReverse)
                return value.Select(o => Convert.ToString(o, 2).PadLeft(8, '0').Select(o => o == '1').Reverse()).SelectMany(o => o).ToArray();
            return value.Select(o => Convert.ToString(o, 2).PadLeft(8, '0').Select(o => o == '1')).SelectMany(o => o).ToArray();
        }

    }
}
