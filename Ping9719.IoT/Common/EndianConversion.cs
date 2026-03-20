using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Ping9719.IoT.Common
{
    /// <summary>
    /// 大小端转换
    /// </summary>
    public static class EndianConversion
    {
        /// <summary>
        /// 转为Net的字节顺序，DCBA（一般用于读）
        /// </summary>
        /// <param name="value"></param>
        /// <param name="format">进来的顺序</param>
        /// <returns></returns>
        public static byte[] EndianIotToNet(this IEnumerable<byte> value, EndianFormat format)
        {
            if (value == null || value.Count() == 0)
                return new byte[0];

            var bytes = value.ToArray();
            if (format == EndianFormat.DCBA)
                return bytes;

            byte[] buffer = value.ToArray();
            if (bytes.Length == 2)
            {
                buffer = new byte[2];
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
            }
            else if (bytes.Length == 4)
            {
                buffer = new byte[4];
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
            }
            else if (bytes.Length == 8)
            {
                buffer = new byte[8];
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
            }
            return buffer;
        }
        /// <summary>
        /// 将Net的字节顺序，DCBA，转为目标的（一般用于写）
        /// </summary>
        /// <param name="value"></param>
        /// <param name="format">目标的顺序</param>
        /// <returns></returns>
        public static byte[] EndianNetToIot(this IEnumerable<byte> value, EndianFormat format)
        {
            if (value == null || value.Count() == 0)
                return new byte[0];

            var bytes = value.ToArray();
            if (format == EndianFormat.DCBA)
                return bytes;

            byte[] buffer = value.ToArray();
            if (bytes.Length == 2)
            {
                buffer = new byte[2];
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
            }
            else if (bytes.Length == 4)
            {
                buffer = new byte[4];
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
            }
            else if (bytes.Length == 8)
            {
                buffer = new byte[8];
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
            }
            return buffer;
        }

        /// <summary>
        /// 批量字节格式转换为批量的指定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="format"></param>
        /// <param name="bool1To8">bool转换是否采用1对8的方式</param>
        /// <param name="bool1To8Reverse">采用了1对8的方式后是否进行反转</param>
        /// <param name="endianIotToNet">转换字节是否调用的<see cref="EndianIotToNet"/>方法,fasle为 <see cref="EndianNetToIot"/></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static T[] ByteToObj<T>(this byte[] value, EndianFormat format = EndianFormat.ABCD, bool bool1To8 = false, bool bool1To8Reverse = true, bool endianIotToNet = true)
        {
            var sl = WordHelp.OccupyBitNum<T>();
            if (value.Length % sl != 0)
                throw new NotImplementedException($"转换失败，类型{typeof(T).Name}不为{sl}的倍数");

            var tType = typeof(T);
            var endianAction = endianIotToNet ? new Func<IEnumerable<byte>, EndianFormat, byte[]>(EndianIotToNet) : new Func<IEnumerable<byte>, EndianFormat, byte[]>(EndianNetToIot);
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
                return value.Chunk(sl).Select(o => endianAction.Invoke(o, format)).Select(o => (T)(object)BitConverter.ToSingle(o, 0)).ToArray();
            else if (tType == typeof(double))
                return value.Chunk(sl).Select(o => endianAction.Invoke(o, format)).Select(o => (T)(object)BitConverter.ToDouble(o, 0)).ToArray();
            else if (tType == typeof(short))
                return value.Chunk(sl).Select(o => endianAction.Invoke(o, format)).Select(o => (T)(object)BitConverter.ToInt16(o, 0)).ToArray();
            else if (tType == typeof(int))
                return value.Chunk(sl).Select(o => endianAction.Invoke(o, format)).Select(o => (T)(object)BitConverter.ToInt32(o, 0)).ToArray();
            else if (tType == typeof(long))
                return value.Chunk(sl).Select(o => endianAction.Invoke(o, format)).Select(o => (T)(object)BitConverter.ToInt64(o, 0)).ToArray();
            else if (tType == typeof(ushort))
                return value.Chunk(sl).Select(o => endianAction.Invoke(o, format)).Select(o => (T)(object)BitConverter.ToUInt16(o, 0)).ToArray();
            else if (tType == typeof(uint))
                return value.Chunk(sl).Select(o => endianAction.Invoke(o, format)).Select(o => (T)(object)BitConverter.ToUInt32(o, 0)).ToArray();
            else if (tType == typeof(ulong))
                return value.Chunk(sl).Select(o => endianAction.Invoke(o, format)).Select(o => (T)(object)BitConverter.ToUInt64(o, 0)).ToArray();
            else
                throw new NotImplementedException("暂不支持的类型");
        }
        /// <summary>
        /// 批量的指定类型转换为批量字节格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="format"></param>
        /// <param name="bool1To8">bool转换是否采用1对8的方式</param>
        /// <param name="bool1To8Reverse">采用了1对8的方式后是否进行反转</param>
        /// <param name="endianIotToNet">转换字节是否调用的<see cref="EndianIotToNet"/>方法,fasle为 <see cref="EndianNetToIot"/></param>
        /// <returns></returns>
        public static byte[] ObjToByte<T>(this IEnumerable<T> value, EndianFormat format = EndianFormat.ABCD, bool endianIotToNet = false)
        {
            var sl = WordHelp.OccupyBitNum<T>();
            var endianAction = endianIotToNet ? new Func<IEnumerable<byte>, EndianFormat, byte[]>(EndianIotToNet) : new Func<IEnumerable<byte>, EndianFormat, byte[]>(EndianNetToIot);

            var tType = typeof(T);
            if (tType == typeof(bool))
                return value.SelectMany(o => endianAction.Invoke(BitConverter.GetBytes((bool)(object)o), format)).ToArray();
            else if (tType == typeof(byte))
                return value.Select(o => (byte)(object)o).ToArray();
            else if (tType == typeof(float))
                return value.SelectMany(o => endianAction.Invoke(BitConverter.GetBytes((float)(object)o), format)).ToArray();
            else if (tType == typeof(double))
                return value.SelectMany(o => endianAction.Invoke(BitConverter.GetBytes((double)(object)o), format)).ToArray();
            else if (tType == typeof(short))
                return value.SelectMany(o => endianAction.Invoke(BitConverter.GetBytes((short)(object)o), format)).ToArray();
            else if (tType == typeof(int))
                return value.SelectMany(o => endianAction.Invoke(BitConverter.GetBytes((int)(object)o), format)).ToArray();
            else if (tType == typeof(long))
                return value.SelectMany(o => endianAction.Invoke(BitConverter.GetBytes((long)(object)o), format)).ToArray();
            else if (tType == typeof(ushort))
                return value.SelectMany(o => endianAction.Invoke(BitConverter.GetBytes((ushort)(object)o), format)).ToArray();
            else if (tType == typeof(uint))
                return value.SelectMany(o => endianAction.Invoke(BitConverter.GetBytes((uint)(object)o), format)).ToArray();
            else if (tType == typeof(ulong))
                return value.SelectMany(o => endianAction.Invoke(BitConverter.GetBytes((ulong)(object)o), format)).ToArray();
            else
            {
                throw new NotImplementedException("暂不支持的类型");
            }
        }
    }
}
