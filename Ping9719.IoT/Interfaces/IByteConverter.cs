using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT
{
    /// <summary>
    /// byte转换器接口
    /// </summary>
    public interface IByteConverter
    {
        /// <summary>
        /// 当前类型占用的字节长度
        /// </summary>
        int ByteLength { get; }
        /// <summary>
        /// 将字节数组转换为对象
        /// </summary>
        /// <param name="bytes">byte数组</param>
        /// <param name="format">来源的格式</param>
        /// <returns>对象</returns>
        object ToObject(IEnumerable<byte> bytes, EndianFormat format);
        /// <summary>
        /// 将对象转换为字节数组
        /// </summary>
        /// <param name="data">对象</param>
        /// <param name="format">目标的格式</param>
        /// <returns>byte数组</returns>
        byte[] ToBytes(object data, EndianFormat format);
    }

    #region 基础的
    public class ByteByteConverter : IByteConverter
    {
        public int ByteLength => 1;
        public object ToObject(IEnumerable<byte> bytes, EndianFormat format) => bytes.First();
        public byte[] ToBytes(object data, EndianFormat format) => new[] { (byte)data };
    }

    public class SByteByteConverter : IByteConverter
    {
        public int ByteLength => 1;
        public object ToObject(IEnumerable<byte> bytes, EndianFormat format) => (sbyte)bytes.First();
        public byte[] ToBytes(object data, EndianFormat format) => new[] { (byte)(sbyte)data };
    }
    public class Int16ByteConverter : IByteConverter
    {
        public int ByteLength => 2;
        public object ToObject(IEnumerable<byte> bytes, EndianFormat format) => BitConverter.ToInt16(DataConvert.EndianToNet(bytes, format, 0, ByteLength), 0);
        public byte[] ToBytes(object data, EndianFormat format) => DataConvert.EndianToNet(BitConverter.GetBytes((short)data), format);
    }

    public class UInt16ByteConverter : IByteConverter
    {
        public int ByteLength => 2;
        public object ToObject(IEnumerable<byte> bytes, EndianFormat format) => BitConverter.ToUInt16(DataConvert.EndianToNet(bytes, format, 0, ByteLength), 0);
        public byte[] ToBytes(object data, EndianFormat format) => DataConvert.EndianToNet(BitConverter.GetBytes((ushort)data), format);
    }
    public class Int32ByteConverter : IByteConverter
    {
        public int ByteLength => 4;
        public object ToObject(IEnumerable<byte> bytes, EndianFormat format) => BitConverter.ToInt32(DataConvert.EndianToNet(bytes, format, 0, ByteLength), 0);
        public byte[] ToBytes(object data, EndianFormat format) => DataConvert.EndianToNet(BitConverter.GetBytes((int)data), format);
    }

    public class UInt32ByteConverter : IByteConverter
    {
        public int ByteLength => 4;
        public object ToObject(IEnumerable<byte> bytes, EndianFormat format) => BitConverter.ToUInt32(DataConvert.EndianToNet(bytes, format, 0, ByteLength), 0);
        public byte[] ToBytes(object data, EndianFormat format) => DataConvert.EndianToNet(BitConverter.GetBytes((uint)data), format);
    }

    public class SingleByteConverter : IByteConverter
    {
        public int ByteLength => 4;
        public object ToObject(IEnumerable<byte> bytes, EndianFormat format) => BitConverter.ToSingle(DataConvert.EndianToNet(bytes, format, 0, ByteLength), 0);
        public byte[] ToBytes(object data, EndianFormat format) => DataConvert.EndianToNet(BitConverter.GetBytes((float)data), format);
    }
    public class Int64ByteConverter : IByteConverter
    {
        public int ByteLength => 8;
        public object ToObject(IEnumerable<byte> bytes, EndianFormat format) => BitConverter.ToInt64(DataConvert.EndianToNet(bytes, format, 0, ByteLength), 0);
        public byte[] ToBytes(object data, EndianFormat format) => DataConvert.EndianToNet(BitConverter.GetBytes((long)data), format);
    }

    public class UInt64ByteConverter : IByteConverter
    {
        public int ByteLength => 8;
        public object ToObject(IEnumerable<byte> bytes, EndianFormat format) => BitConverter.ToUInt64(DataConvert.EndianToNet(bytes, format, 0, ByteLength), 0);
        public byte[] ToBytes(object data, EndianFormat format) => DataConvert.EndianToNet(BitConverter.GetBytes((ulong)data), format);
    }

    public class DoubleByteConverter : IByteConverter
    {
        public int ByteLength => 8;
        public object ToObject(IEnumerable<byte> bytes, EndianFormat format) => BitConverter.ToDouble(DataConvert.EndianToNet(bytes, format, 0, ByteLength), 0);
        public byte[] ToBytes(object data, EndianFormat format) => DataConvert.EndianToNet(BitConverter.GetBytes((double)data), format);
    }
    #endregion

    #region 特殊的
    /// <summary>
    /// 单个 bool 转换器
    /// </summary>
    public class BoolByteConverter : IByteConverter
    {
        public int ByteLength => 1;
        public object ToObject(IEnumerable<byte> bytes, EndianFormat format) => BitConverter.ToBoolean(bytes.ToArray(), 0);
        public byte[] ToBytes(object data, EndianFormat format) => BitConverter.GetBytes((bool)data);
    }

    /// <summary>
    /// 位布尔数组转换器（将 1 个 byte 转换为 8 个 bool 的数组，按位处理）
    /// </summary>
    public class BoolBitByteConverter : IByteConverter
    {
        public int ByteLength => 1;
        public object ToObject(IEnumerable<byte> bytes, EndianFormat format = EndianFormat.DCBA) => Convert.ToString(bytes.First(), 2).PadLeft(8, '0').Select(o => o == '1').Reverse();
        public byte[] ToBytes(object data, EndianFormat format) => new byte[] { (byte)((IEnumerable<bool>)data).Select((b, i) => b ? 1 << i : 0).Aggregate(0, (a, b) => a | b) };
    }

    /// <summary>
    /// 字符串转换器
    /// </summary>
    public class StringByteConverter : IByteConverter
    {
        /// <summary>
        /// 字符串编码，如果为 null 则使用十六进制字符串（大写，无分隔符）
        /// </summary>
        public Encoding Encoding { get; set; }
        /// <summary>
        /// 字节长度，如果为 -1 则转换全部
        /// </summary>
        public int ByteLength { get; set; }

        public StringByteConverter(Encoding encoding, int byteLength = -1)
        {
            Encoding = encoding;
            ByteLength = byteLength;
        }

        public object ToObject(IEnumerable<byte> bytes, EndianFormat format)
        {
            byte[] data = ByteLength < 0 ? bytes.ToArray() : bytes.Take(ByteLength).ToArray();

            if (Encoding == null)
                return BitConverter.ToString(data).Replace("-", "");
            else
                return Encoding.GetString(data).TrimEnd('\0');
        }

        public byte[] ToBytes(object data, EndianFormat format)
        {
            string str = (string)data;
            byte[] buffer = new byte[ByteLength];

            if (Encoding == null)
            {
                // 十六进制字符串解析（自动处理奇数长度）
                if (str.Length % 2 != 0) str = "0" + str;
                for (int i = 0; i < Math.Min(str.Length / 2, ByteLength); i++)
                {
                    buffer[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
                }
            }
            else
            {
                // 编码转换（自动截断或补零）
                byte[] encoded = Encoding.GetBytes(str);
                Array.Copy(encoded, buffer, Math.Min(encoded.Length, ByteLength));
            }

            return buffer;
        }
    }
    #endregion

}
