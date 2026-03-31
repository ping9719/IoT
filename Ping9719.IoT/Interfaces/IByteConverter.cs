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

    #endregion

}
