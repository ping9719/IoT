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
        /// <param name="asciiBytes"></param>
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
        /// <param name="bytes"></param>
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
        /// <returns></returns>
        public static bool[] ByteToBin(this byte[] value, bool isReverse = true)
        {
            if (isReverse)
                return value.Select(o => Convert.ToString(o, 2).PadLeft(8, '0').Select(o => o == '1').Reverse()).SelectMany(o => o).ToArray();
            return value.Select(o => Convert.ToString(o, 2).PadLeft(8, '0').Select(o => o == '1')).SelectMany(o => o).ToArray();
        }

        /// <summary>
        /// 目标顺序和Net顺序转换。
        /// 读：将目标转为Net的字节顺序；写：将Net转为目标的字节顺序。Net顺序为DCBA。
        /// </summary>
        /// <param name="value">长度只能为2，4，8的数组，其他返回本身</param>
        /// <param name="format">读：进来的顺序，写：目标的顺序</param>
        /// <param name="offset">偏移量，开始位置</param>
        /// <param name="count">数量，取的数量，-1为全部</param>
        /// <returns></returns>
        public static byte[] EndianToNet(this IEnumerable<byte> value, EndianFormat format, int offset = 0, int count = -1)
        {
            if (value == null || value.Count() == 0)
                return new byte[0];

            var bytes = value.Skip(offset).Take(count < 0 ? int.MaxValue : count).ToArray();
            if (format == EndianFormat.DCBA)
                return bytes;

            if (bytes.Length == 2)
            {
                var buffer = new byte[2];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = bytes[1];
                        buffer[1] = bytes[0];
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = bytes[0];
                        buffer[1] = bytes[1];
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = bytes[1];
                        buffer[1] = bytes[0];
                        break;
                }
                return buffer;
            }
            else if (bytes.Length == 4)
            {
                var buffer = new byte[4];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = bytes[3];
                        buffer[1] = bytes[2];
                        buffer[2] = bytes[1];
                        buffer[3] = bytes[0];
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = bytes[2];
                        buffer[1] = bytes[3];
                        buffer[2] = bytes[0];
                        buffer[3] = bytes[1];
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = bytes[1];
                        buffer[1] = bytes[0];
                        buffer[2] = bytes[3];
                        buffer[3] = bytes[2];
                        break;
                }
                return buffer;
            }
            else if (bytes.Length == 8)
            {
                var buffer = new byte[8];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = bytes[7];
                        buffer[1] = bytes[6];
                        buffer[2] = bytes[5];
                        buffer[3] = bytes[4];
                        buffer[4] = bytes[3];
                        buffer[5] = bytes[2];
                        buffer[6] = bytes[1];
                        buffer[7] = bytes[0];
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = bytes[6];
                        buffer[1] = bytes[7];
                        buffer[2] = bytes[4];
                        buffer[3] = bytes[5];
                        buffer[4] = bytes[2];
                        buffer[5] = bytes[3];
                        buffer[6] = bytes[0];
                        buffer[7] = bytes[1];
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = bytes[1];
                        buffer[1] = bytes[0];
                        buffer[2] = bytes[3];
                        buffer[3] = bytes[2];
                        buffer[4] = bytes[5];
                        buffer[5] = bytes[4];
                        buffer[6] = bytes[7];
                        buffer[7] = bytes[6];
                        break;
                }
                return buffer;
            }

            return bytes;
        }
        /// <summary>
        /// 批量字节格式转换为批量的指定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="format">进来的顺序 或 目标的顺序</param>
        /// <param name="bool1To8">bool转换是否采用1对8的方式</param>
        /// <param name="bool1To8Reverse">采用了1对8的方式后是否进行反转</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static T[] EndianToObj<T>(this byte[] value, EndianFormat format = EndianFormat.ABCD, bool bool1To8 = false, bool bool1To8Reverse = true)
        {
            var sl = DataHelp.GetByteCount<T>();
            if (value.Length % sl != 0)
                throw new NotImplementedException($"转换失败，类型{typeof(T).Name}不为{sl}的倍数");

            var tType = typeof(T);
            if (tType == typeof(bool))
            {
                if (bool1To8)
                    return value.Select(o => DataConvert.ByteToBin(o, 8, bool1To8Reverse).Select(o2 => (T)(object)o2)).SelectMany(o => o).ToArray();
                else
                    return value.Select(o => (T)(object)(o != 0)).ToArray();
            }
            else if (tType == typeof(byte))
                return value.Select(o => (T)(object)o).ToArray();
            else if (tType == typeof(float))
                return value.Chunk(sl).Select(o => EndianToNet(o, format)).Select(o => (T)(object)BitConverter.ToSingle(o, 0)).ToArray();
            else if (tType == typeof(double))
                return value.Chunk(sl).Select(o => EndianToNet(o, format)).Select(o => (T)(object)BitConverter.ToDouble(o, 0)).ToArray();
            else if (tType == typeof(short))
                return value.Chunk(sl).Select(o => EndianToNet(o, format)).Select(o => (T)(object)BitConverter.ToInt16(o, 0)).ToArray();
            else if (tType == typeof(int))
                return value.Chunk(sl).Select(o => EndianToNet(o, format)).Select(o => (T)(object)BitConverter.ToInt32(o, 0)).ToArray();
            else if (tType == typeof(long))
                return value.Chunk(sl).Select(o => EndianToNet(o, format)).Select(o => (T)(object)BitConverter.ToInt64(o, 0)).ToArray();
            else if (tType == typeof(ushort))
                return value.Chunk(sl).Select(o => EndianToNet(o, format)).Select(o => (T)(object)BitConverter.ToUInt16(o, 0)).ToArray();
            else if (tType == typeof(uint))
                return value.Chunk(sl).Select(o => EndianToNet(o, format)).Select(o => (T)(object)BitConverter.ToUInt32(o, 0)).ToArray();
            else if (tType == typeof(ulong))
                return value.Chunk(sl).Select(o => EndianToNet(o, format)).Select(o => (T)(object)BitConverter.ToUInt64(o, 0)).ToArray();
            else
                throw new NotImplementedException("暂不支持的类型");
        }
        /// <summary>
        /// 批量的指定类型转换为批量字节格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="format"><see href="endianIotToNet"/> 为 true：进来的顺序，false 目标的顺序</param>
        /// <returns></returns>
        public static byte[] EndianToByte<T>(this IEnumerable<T> value, EndianFormat format = EndianFormat.ABCD)
        {
            var tType = typeof(T);
            if (tType == typeof(bool))
                return value.SelectMany(o => EndianToNet(BitConverter.GetBytes((bool)(object)o), format)).ToArray();
            else if (tType == typeof(byte))
                return value.Select(o => (byte)(object)o).ToArray();
            else if (tType == typeof(float))
                return value.SelectMany(o => EndianToNet(BitConverter.GetBytes((float)(object)o), format)).ToArray();
            else if (tType == typeof(double))
                return value.SelectMany(o => EndianToNet(BitConverter.GetBytes((double)(object)o), format)).ToArray();
            else if (tType == typeof(short))
                return value.SelectMany(o => EndianToNet(BitConverter.GetBytes((short)(object)o), format)).ToArray();
            else if (tType == typeof(int))
                return value.SelectMany(o => EndianToNet(BitConverter.GetBytes((int)(object)o), format)).ToArray();
            else if (tType == typeof(long))
                return value.SelectMany(o => EndianToNet(BitConverter.GetBytes((long)(object)o), format)).ToArray();
            else if (tType == typeof(ushort))
                return value.SelectMany(o => EndianToNet(BitConverter.GetBytes((ushort)(object)o), format)).ToArray();
            else if (tType == typeof(uint))
                return value.SelectMany(o => EndianToNet(BitConverter.GetBytes((uint)(object)o), format)).ToArray();
            else if (tType == typeof(ulong))
                return value.SelectMany(o => EndianToNet(BitConverter.GetBytes((ulong)(object)o), format)).ToArray();
            else
            {
                throw new NotImplementedException("暂不支持的类型");
            }
        }

    }
}
