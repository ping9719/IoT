using Ping9719.IoT.Enums;
using System;
using System.Collections.Generic;

namespace Ping9719.IoT.Interfaces
{
    /// <summary>
    /// 不推荐使用
    /// </summary>
    public interface IIoTClient
    {
        /// <summary>
        /// 版本
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 是否是连接的
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 警告日志委托
        /// 为了可用性，会对异常网络进行重试。此类日志通过委托接口给出去。
        /// </summary>
        Action<string, Exception> WarningLog { get; set; }

        /// <summary>
        /// 打开连接（如果已经是连接状态会先关闭再打开）
        /// </summary>
        /// <returns></returns>
        IoTResult Open();

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        IoTResult Close();

        /// <summary>
        /// 发送报文，并获取响应报文
        /// </summary>
        /// <param name="command">发送命令</param>
        /// <returns></returns>
        IoTResult<byte[]> SendPackageSingle(byte[] command);

        #region Read 

        /// <summary>
        /// 分批读取
        /// </summary>
        /// <param name="addresses">地址集合</param>
        /// <param name="batchNumber">批量读取数量</param>
        /// <returns></returns>
        IoTResult<Dictionary<string, object>> BatchRead(Dictionary<string, DataTypeEnum> addresses, int batchNumber);

        /// <summary>
        /// 读取Byte
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        IoTResult<byte> ReadByte(string address);

        /// <summary>
        /// 读取Boolean
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        IoTResult<bool> ReadBoolean(string address);

        /// <summary>
        /// 读取UInt16
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        IoTResult<ushort> ReadUInt16(string address);

        /// <summary>
        /// 读取Int16
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        IoTResult<short> ReadInt16(string address);

        /// <summary>
        /// 读取UInt32
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        IoTResult<uint> ReadUInt32(string address);

        /// <summary>
        /// 读取Int32
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        IoTResult<int> ReadInt32(string address);

        /// <summary>
        /// 读取UInt64
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        IoTResult<ulong> ReadUInt64(string address);

        /// <summary>
        /// 读取Int64
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        IoTResult<long> ReadInt64(string address);

        /// <summary>
        /// 读取Float
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        IoTResult<float> ReadFloat(string address);

        /// <summary>
        /// 读取Double
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        IoTResult<double> ReadDouble(string address);

        /// <summary>
        /// 读取String
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        IoTResult<string> ReadString(string address);

        #endregion

        #region Write

        /// <summary>
        /// 分批写入 
        /// </summary>
        /// <param name="addresses">地址集合</param>
        /// <param name="batchNumber">批量读取数量</param>
        /// <returns></returns>
        IoTResult BatchWrite(Dictionary<string, object> addresses, int batchNumber);

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        IoTResult Write(string address, byte value);

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        IoTResult Write(string address, bool value);

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        IoTResult Write(string address, sbyte value);

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        IoTResult Write(string address, ushort value);

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        IoTResult Write(string address, short value);

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        IoTResult Write(string address, uint value);

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        IoTResult Write(string address, int value);

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        IoTResult Write(string address, ulong value);

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        IoTResult Write(string address, long value);

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        IoTResult Write(string address, float value);

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        IoTResult Write(string address, double value);

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        IoTResult Write(string address, string value);

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <param name="type">数据类型</param>
        /// <returns></returns>
        IoTResult Write(string address, object value, DataTypeEnum type);
        #endregion
    }
}
