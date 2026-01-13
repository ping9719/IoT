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
        /// 字节格式转换,应该淘汰使用 <see cref="EndianIotToNet"/> 和 <see cref="EndianNetToIot"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="format"></param>
        /// <param name="reverse">是否反转</param>
        /// <returns></returns>
        public static byte[] ByteFormatting(this byte[] value, EndianFormat format = EndianFormat.ABCD, bool reverse = true)
        {
            if (!reverse)
            {
                switch (format)
                {
                    case EndianFormat.ABCD:
                        format = EndianFormat.DCBA;
                        break;
                    case EndianFormat.BADC:
                        format = EndianFormat.CDAB;
                        break;
                    case EndianFormat.CDAB:
                        format = EndianFormat.BADC;
                        break;
                    case EndianFormat.DCBA:
                        format = EndianFormat.ABCD;
                        break;
                }
            }

            byte[] buffer = value;
            if (value.Length == 2)
            {
                buffer = new byte[2];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = value[0];
                        buffer[1] = value[1];
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = value[1];
                        buffer[1] = value[0];
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = value[0];
                        buffer[1] = value[1];
                        break;
                    case EndianFormat.DCBA://这里写反了？
                        buffer[0] = value[0];
                        buffer[1] = value[1];
                        break;
                }
            }
            else if (value.Length == 4)
            {
                buffer = new byte[4];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = value[0];
                        buffer[1] = value[1];
                        buffer[2] = value[2];
                        buffer[3] = value[3];
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = value[1];
                        buffer[1] = value[0];
                        buffer[2] = value[3];
                        buffer[3] = value[2];
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = value[2];
                        buffer[1] = value[3];
                        buffer[2] = value[0];
                        buffer[3] = value[1];
                        break;
                    case EndianFormat.DCBA:
                        buffer[0] = value[3];
                        buffer[1] = value[2];
                        buffer[2] = value[1];
                        buffer[3] = value[0];
                        break;
                }
            }
            else if (value.Length == 8)
            {
                buffer = new byte[8];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = value[0];
                        buffer[1] = value[1];
                        buffer[2] = value[2];
                        buffer[3] = value[3];
                        buffer[4] = value[4];
                        buffer[5] = value[5];
                        buffer[6] = value[6];
                        buffer[7] = value[7];
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = value[1];
                        buffer[1] = value[0];
                        buffer[2] = value[3];
                        buffer[3] = value[2];
                        buffer[4] = value[5];
                        buffer[5] = value[4];
                        buffer[6] = value[7];
                        buffer[7] = value[6];
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = value[6];
                        buffer[1] = value[7];
                        buffer[2] = value[4];
                        buffer[3] = value[5];
                        buffer[4] = value[2];
                        buffer[5] = value[3];
                        buffer[6] = value[0];
                        buffer[7] = value[1];
                        break;
                    case EndianFormat.DCBA:
                        buffer[0] = value[7];
                        buffer[1] = value[6];
                        buffer[2] = value[5];
                        buffer[3] = value[4];
                        buffer[4] = value[3];
                        buffer[5] = value[2];
                        buffer[6] = value[1];
                        buffer[7] = value[0];
                        break;
                }
            }
            return buffer;
        }

        /// <summary>
        /// 字节格式转换,应该淘汰使用 <see cref="EndianIotToNet"/> 和 <see cref="EndianNetToIot"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static byte[] ToByteFormat(this byte[] value, EndianFormat format = EndianFormat.ABCD)
        {
            if (value == null || value.Length == 0)
                return new byte[0];

            byte[] buffer = value;
            if (value.Length == 1)
            {
                buffer = new byte[1];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = value.First();
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = value.Last();
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = value.First();
                        break;
                    case EndianFormat.DCBA:
                        buffer[0] = value.Last();
                        break;
                }
            }
            else if (value.Length == 2)
            {
                buffer = new byte[2];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = value[0];
                        buffer[1] = value[1];
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = value[1];
                        buffer[1] = value[0];
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = value[0];
                        buffer[1] = value[1];
                        break;
                    case EndianFormat.DCBA:
                        buffer[0] = value[1];
                        buffer[1] = value[0];
                        break;
                }
            }
            else if (value.Length == 4)
            {
                buffer = new byte[4];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = value[0];
                        buffer[1] = value[1];
                        buffer[2] = value[2];
                        buffer[3] = value[3];
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = value[1];
                        buffer[1] = value[0];
                        buffer[2] = value[3];
                        buffer[3] = value[2];
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = value[2];
                        buffer[1] = value[3];
                        buffer[2] = value[0];
                        buffer[3] = value[1];
                        break;
                    case EndianFormat.DCBA:
                        buffer[0] = value[3];
                        buffer[1] = value[2];
                        buffer[2] = value[1];
                        buffer[3] = value[0];
                        break;
                }
            }
            else if (value.Length == 8)
            {
                buffer = new byte[8];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = value[0];
                        buffer[1] = value[1];
                        buffer[2] = value[2];
                        buffer[3] = value[3];
                        buffer[4] = value[4];
                        buffer[5] = value[5];
                        buffer[6] = value[6];
                        buffer[7] = value[7];
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = value[1];
                        buffer[1] = value[0];
                        buffer[2] = value[3];
                        buffer[3] = value[2];
                        buffer[4] = value[5];
                        buffer[5] = value[4];
                        buffer[6] = value[7];
                        buffer[7] = value[6];
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = value[6];
                        buffer[1] = value[7];
                        buffer[2] = value[4];
                        buffer[3] = value[5];
                        buffer[4] = value[2];
                        buffer[5] = value[3];
                        buffer[6] = value[0];
                        buffer[7] = value[1];
                        break;
                    case EndianFormat.DCBA:
                        buffer[0] = value[7];
                        buffer[1] = value[6];
                        buffer[2] = value[5];
                        buffer[3] = value[4];
                        buffer[4] = value[3];
                        buffer[5] = value[2];
                        buffer[6] = value[1];
                        buffer[7] = value[0];
                        break;
                }
            }
            return buffer;
        }

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
            if (format == EndianFormat.DCBA)
                return value.ToArray();

            var value1 = value.ToArray();
            byte[] buffer = value.ToArray();
            if (value.Count() == 1)
            {
                buffer = new byte[1];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = value1.Last();
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = value1.First();
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = value1.Last();
                        break;
                }
            }
            else if (value.Count() == 2)
            {
                buffer = new byte[2];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = value1[1];
                        buffer[1] = value1[0];
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = value1[0];
                        buffer[1] = value1[1];
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = value1[1];
                        buffer[1] = value1[0];
                        break;
                }
            }
            else if (value.Count() == 4)
            {
                buffer = new byte[4];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = value1[3];
                        buffer[1] = value1[2];
                        buffer[2] = value1[1];
                        buffer[3] = value1[0];
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = value1[2];
                        buffer[1] = value1[3];
                        buffer[2] = value1[0];
                        buffer[3] = value1[1];
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = value1[1];
                        buffer[1] = value1[0];
                        buffer[2] = value1[3];
                        buffer[3] = value1[2];
                        break;
                }
            }
            else if (value.Count() == 8)
            {
                buffer = new byte[8];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = value1[7];
                        buffer[1] = value1[6];
                        buffer[2] = value1[5];
                        buffer[3] = value1[4];
                        buffer[4] = value1[3];
                        buffer[5] = value1[2];
                        buffer[6] = value1[1];
                        buffer[7] = value1[0];
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = value1[6];
                        buffer[1] = value1[7];
                        buffer[2] = value1[4];
                        buffer[3] = value1[5];
                        buffer[4] = value1[2];
                        buffer[5] = value1[3];
                        buffer[6] = value1[0];
                        buffer[7] = value1[1];
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = value1[1];
                        buffer[1] = value1[0];
                        buffer[2] = value1[3];
                        buffer[3] = value1[2];
                        buffer[4] = value1[5];
                        buffer[5] = value1[4];
                        buffer[6] = value1[7];
                        buffer[7] = value1[6];
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
            if (format == EndianFormat.DCBA)
                return value.ToArray();

            byte[] value1 = value.ToArray();
            byte[] buffer = value.ToArray();
            if (value.Count() == 1)
            {
                buffer = new byte[1];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = value1.Last();
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = value1.Last();
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = value1.Last();
                        break;
                }
            }
            else if (value.Count() == 2)
            {
                buffer = new byte[2];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = value1[1];
                        buffer[1] = value1[0];
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = value1[0];
                        buffer[1] = value1[1];
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = value1[1];
                        buffer[1] = value1[0];
                        break;
                }
            }
            else if (value1.Count() == 4)
            {
                buffer = new byte[4];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = value1[3];
                        buffer[1] = value1[2];
                        buffer[2] = value1[1];
                        buffer[3] = value1[0];
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = value1[2];
                        buffer[1] = value1[3];
                        buffer[2] = value1[0];
                        buffer[3] = value1[1];
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = value1[1];
                        buffer[1] = value1[0];
                        buffer[2] = value1[3];
                        buffer[3] = value1[2];
                        break;
                }
            }
            else if (value1.Count() == 8)
            {
                buffer = new byte[8];
                switch (format)
                {
                    case EndianFormat.ABCD:
                        buffer[0] = value1[7];
                        buffer[1] = value1[6];
                        buffer[2] = value1[5];
                        buffer[3] = value1[4];
                        buffer[4] = value1[3];
                        buffer[5] = value1[2];
                        buffer[6] = value1[1];
                        buffer[7] = value1[0];
                        break;
                    case EndianFormat.BADC:
                        buffer[0] = value1[6];
                        buffer[1] = value1[7];
                        buffer[2] = value1[4];
                        buffer[3] = value1[5];
                        buffer[4] = value1[2];
                        buffer[5] = value1[3];
                        buffer[6] = value1[0];
                        buffer[7] = value1[1];
                        break;
                    case EndianFormat.CDAB:
                        buffer[0] = value1[1];
                        buffer[1] = value1[0];
                        buffer[2] = value1[3];
                        buffer[3] = value1[2];
                        buffer[4] = value1[5];
                        buffer[5] = value1[4];
                        buffer[6] = value1[7];
                        buffer[7] = value1[6];
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
        /// <param name="boolConv8"></param>
        /// <param name="boolConv8Reverse"></param>
        /// <param name="endianAction">转换字节 调用的方法 默认为 <see cref="ToByteFormat"/></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static T[] ByteToObj<T>(this byte[] value, EndianFormat format = EndianFormat.ABCD, bool boolConv8 = false, bool boolConv8Reverse = true, Func<byte[], byte[]> endianAction = null)
        {
            var sl = WordHelp.OccupyBitNum<T>();
            if (value.Length % sl != 0)
                throw new NotImplementedException($"转换失败，类型{typeof(T).Name}不为{sl}的倍数");

            var tType = typeof(T);
            List<T> buffer = new List<T>(value.Length / sl);
            if (tType == typeof(bool))
            {
                if (boolConv8)
                {
                    foreach (var item in value)
                    {
                        var qu = DataConvert.ByteToBinaryBoolArray(item, 8, boolConv8Reverse).Select(o => (T)(object)o);
                        buffer.AddRange(qu);
                    }
                }
                else
                {
                    for (var i = 0; i < value.Length; i = i + sl)
                    {
                        var qu = value.Skip(i).Take(sl).ToArray();
                        qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                        buffer.Add((T)(object)BitConverter.ToBoolean(qu, 0));
                    }
                }
            }
            else if (tType == typeof(byte))
            {
                foreach (var item in value)
                {
                    buffer.Add((T)(object)item);
                }
            }
            else if (tType == typeof(float))
            {
                for (var i = 0; i < value.Length; i = i + sl)
                {
                    var qu = value.Skip(i).Take(sl).ToArray();
                    qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                    buffer.Add((T)(object)BitConverter.ToSingle(qu, 0));
                }
            }
            else if (tType == typeof(double))
            {
                for (var i = 0; i < value.Length; i = i + sl)
                {
                    var qu = value.Skip(i).Take(sl).ToArray();
                    qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                    buffer.Add((T)(object)BitConverter.ToDouble(qu, 0));
                }
            }
            else if (tType == typeof(short))
            {
                for (var i = 0; i < value.Length; i = i + sl)
                {
                    var qu = value.Skip(i).Take(sl).ToArray();
                    qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                    buffer.Add((T)(object)BitConverter.ToInt16(qu, 0));
                }
            }
            else if (tType == typeof(int))
            {
                for (var i = 0; i < value.Length; i = i + sl)
                {
                    var qu = value.Skip(i).Take(sl).ToArray();
                    qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                    buffer.Add((T)(object)BitConverter.ToInt32(qu, 0));
                }
            }
            else if (tType == typeof(long))
            {
                for (var i = 0; i < value.Length; i = i + sl)
                {
                    var qu = value.Skip(i).Take(sl).ToArray();
                    qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                    buffer.Add((T)(object)BitConverter.ToInt64(qu, 0));
                }
            }
            else if (tType == typeof(ushort))
            {
                for (var i = 0; i < value.Length; i = i + sl)
                {
                    var qu = value.Skip(i).Take(sl).ToArray();
                    qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                    buffer.Add((T)(object)BitConverter.ToUInt16(qu, 0));
                }
            }
            else if (tType == typeof(uint))
            {
                for (var i = 0; i < value.Length; i = i + sl)
                {
                    var qu = value.Skip(i).Take(sl).ToArray();
                    qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                    buffer.Add((T)(object)BitConverter.ToUInt32(qu, 0));
                }
            }
            else if (tType == typeof(ulong))
            {
                for (var i = 0; i < value.Length; i = i + sl)
                {
                    var qu = value.Skip(i).Take(sl).ToArray();
                    qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                    buffer.Add((T)(object)BitConverter.ToUInt64(qu, 0));
                }
            }
            else
            {
                throw new NotImplementedException("暂不支持的类型");
            }
            return buffer.ToArray();
        }

        /// <summary>
        /// 批量的指定类型转换为批量字节格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="format"></param>
        /// <param name="endianAction">转换字节 调用的方法 默认为 <see cref="ToByteFormat"/></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static byte[] ObjToByte<T>(this IEnumerable<T> value, EndianFormat format = EndianFormat.ABCD, Func<byte[], byte[]> endianAction = null)
        {
            var sl = WordHelp.OccupyBitNum<T>();
            var tType = typeof(T);
            List<byte> buffer = new List<byte>(value.Count() * sl);
            if (tType == typeof(bool))
            {
                foreach (var item in value)
                {
                    var iv = (bool)(object)item;
                    buffer.AddRange(BitConverter.GetBytes(iv));
                }
            }
            else if (tType == typeof(byte))
            {
                foreach (var item in value)
                {
                    buffer.Add((byte)(object)item);
                }
            }
            else if (tType == typeof(float))
            {
                foreach (var item in value)
                {
                    var iv = (float)(object)item;

                    var qu = BitConverter.GetBytes(iv);
                    qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                    buffer.AddRange(qu);
                }
            }
            else if (tType == typeof(double))
            {
                foreach (var item in value)
                {
                    var iv = (double)(object)item;
                    var qu = BitConverter.GetBytes(iv);
                    qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                    buffer.AddRange(qu);
                }
            }
            else if (tType == typeof(short))
            {
                foreach (var item in value)
                {
                    var iv = (short)(object)item;
                    var qu = BitConverter.GetBytes(iv);
                    qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                    buffer.AddRange(qu);
                }
            }
            else if (tType == typeof(int))
            {
                foreach (var item in value)
                {
                    var iv = (int)(object)item;
                    var qu = BitConverter.GetBytes(iv);
                    qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                    buffer.AddRange(qu);
                }
            }
            else if (tType == typeof(long))
            {
                foreach (var item in value)
                {
                    var iv = (long)(object)item;
                    var qu = BitConverter.GetBytes(iv);
                    qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                    buffer.AddRange(qu);
                }
            }
            else if (tType == typeof(ushort))
            {
                foreach (var item in value)
                {
                    var iv = (ushort)(object)item;
                    var qu = BitConverter.GetBytes(iv);
                    qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                    buffer.AddRange(qu);
                }
            }
            else if (tType == typeof(uint))
            {
                foreach (var item in value)
                {
                    var iv = (uint)(object)item;
                    var qu = BitConverter.GetBytes(iv);
                    qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                    buffer.AddRange(qu);
                }
            }
            else if (tType == typeof(ulong))
            {
                foreach (var item in value)
                {
                    var iv = (ulong)(object)item;
                    var qu = BitConverter.GetBytes(iv);
                    qu = endianAction == null ? ToByteFormat(qu, format) : endianAction.Invoke(qu);
                    buffer.AddRange(qu);
                }
            }
            else
            {
                throw new NotImplementedException("暂不支持的类型");
            }
            return buffer.ToArray();
        }
    }
}
