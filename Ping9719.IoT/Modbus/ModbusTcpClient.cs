﻿using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Ping9719.IoT.Communication;
using Ping9719.IoT;

namespace Ping9719.IoT.Modbus
{
    /// <summary>
    /// ModbusTcp协议客户端
    /// </summary>
    public class ModbusTcpClient : IIoT
    {
        private EndianFormat format;
        private bool plcAddresses;
        private byte stationNumber = 1;


        ///// <summary>
        ///// 字符串编码格式。默认ASCII
        ///// </summary>
        //public Encoding Encoding { get; set; } = Encoding.ASCII;

        ///// <summary>
        ///// 是否是连接的
        ///// </summary>
        //public override bool IsConnected => socket?.Connected ?? false;

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="ipAndPoint"></param>
        ///// <param name="timeout">超时时间（毫秒）</param>
        ///// <param name="format">大小端设置</param>
        ///// <param name="plcAddresses">PLC地址</param>
        ///// <param name="plcAddresses">PLC地址</param>
        //public ModbusTcpClient(IPEndPoint ipAndPoint, int timeout = 1500, EndianFormat format = EndianFormat.ABCD, byte stationNumber = 1, bool plcAddresses = false)
        //{
        //    this.timeout = timeout;
        //    ipEndPoint = ipAndPoint;
        //    this.format = format;
        //    this.plcAddresses = plcAddresses;
        //    this.stationNumber = stationNumber;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="ip"></param>
        ///// <param name="port"></param>
        ///// <param name="timeout">超时时间（毫秒）</param>
        ///// <param name="format">大小端设置</param>
        ///// <param name="plcAddresses">PLC地址</param>
        //public ModbusTcpClient(string ip, int port, int timeout = 1500, EndianFormat format = EndianFormat.ABCD, byte stationNumber = 1, bool plcAddresses = false)
        //{
        //    this.timeout = timeout;
        //    SetIpEndPoint(ip, port);
        //    this.format = format;
        //    this.plcAddresses = plcAddresses;
        //    this.stationNumber = stationNumber;
        //}

        public ClientBase Client { get; private set; }//通讯管道

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <param name="format">大小端设置</param>
        /// <param name="plcAddresses">PLC地址</param>
        public ModbusTcpClient(ClientBase client, int timeout = 1500, EndianFormat format = EndianFormat.ABCD, byte stationNumber = 1, bool plcAddresses = false)
        {
            Client = client;
            Client.TimeOut = timeout;
            Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            Client.ConnectionMode = ConnectionMode.AutoOpen;

            this.format = format;
            this.plcAddresses = plcAddresses;
            this.stationNumber = stationNumber;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <param name="format">大小端设置</param>
        /// <param name="plcAddresses">PLC地址</param>
        public ModbusTcpClient(string ip, int port = 1500, int timeout = 1500, EndianFormat format = EndianFormat.ABCD, byte stationNumber = 1, bool plcAddresses = false) : this(new TcpClient(ip, port), timeout, format, stationNumber, plcAddresses) { }//默认使用TcpClient

        ///// <summary>
        ///// 发送报文，并获取响应报文（建议使用SendPackageReliable，如果异常会自动重试一次）
        ///// </summary>
        ///// <param name="command"></param>
        ///// <returns></returns>
        //public override IoTResult<byte[]> SendPackageSingle(byte[] command)
        //{
        //    //从发送命令到读取响应为最小单元，避免多线程执行串数据（可线程安全执行）
        //    lock (this)
        //    {
        //        IoTResult<byte[]> result = new IoTResult<byte[]>();
        //        try
        //        {
        //            socket.Send(command);
        //            var socketReadResul = SocketRead(8);
        //            if (!socketReadResul.IsSucceed)
        //                return socketReadResul;
        //            var headPackage = socketReadResul.Value;
        //            int length = headPackage[4] * 256 + headPackage[5] - 2;
        //            socketReadResul = SocketRead(length);
        //            if (!socketReadResul.IsSucceed)
        //                return socketReadResul;
        //            var dataPackage = socketReadResul.Value;

        //            result.Value = headPackage.Concat(dataPackage).ToArray();
        //            return result.ToEnd();
        //        }
        //        catch (Exception ex)
        //        {
        //            result.AddError(ex);
        //            return result.ToEnd();
        //        }
        //    }
        //}

        #region Read 读取
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="readLength">读取长度</param>
        /// <param name="byteFormatting">大小端转换</param>
        /// <returns></returns>
        public IoTResult<byte[]> Read(string address, byte stationNumber = 1, byte functionCode = 3, ushort readLength = 1, bool byteFormatting = true)
        {
            var result = new IoTResult<byte[]>();

            //if (!socket?.Connected ?? true)
            //{
            //    var conentResult = Connect();
            //    if (!conentResult.IsSucceed)
            //    {
            //        conentResult.AddError($"读取 地址:{address} 站号:{stationNumber} 功能码:{functionCode} 失败。{conentResult.ErrorText}");
            //        return result;
            //    }
            //}
            try
            {
                var chenkHead = GetCheckHead(functionCode);
                //1 获取命令（组装报文）
                byte[] command = GetReadCommand(address, stationNumber, functionCode, readLength, chenkHead);
                result.Responses.Add(command);
                //获取响应报文
                var sendResult = Client.SendReceive(command);
                if (!sendResult.IsSucceed)
                {
                    sendResult.AddError($"读取 地址:{address} 站号:{stationNumber} 功能码:{functionCode} 失败。{sendResult.ErrorText}");
                    return result.ToEnd();
                }
                var dataPackage = sendResult.Value;
                byte[] resultBuffer = new byte[dataPackage.Length - 9];
                Array.Copy(dataPackage, 9, resultBuffer, 0, resultBuffer.Length);
                result.Responses.Add(dataPackage);
                //4 获取响应报文数据（字节数组形式）             
                if (byteFormatting)
                    result.Value = resultBuffer.Reverse().ToArray().ByteFormatting(format);
                else
                    result.Value = resultBuffer.Reverse().ToArray();

                if (chenkHead[0] != dataPackage[0] || chenkHead[1] != dataPackage[1])
                {
                    result.AddError($"读取 地址:{address} 站号:{stationNumber} 功能码:{functionCode} 失败。响应结果校验失败");
                    //SafeClose();
                }
                else if (ModbusHelper.VerifyFunctionCode(functionCode, dataPackage[7]))
                {

                    result.AddError(ModbusHelper.ErrMsg(dataPackage[8]));
                }
            }
            //catch (SocketException ex)
            //{

            //    if (ex.SocketErrorCode == SocketError.TimedOut)
            //    {
            //        result.AddError($"读取 地址:{address} 站号:{stationNumber} 功能码:{functionCode} 失败。连接超时");
            //        SafeClose();
            //    }
            //    else
            //    {
            //        result.AddError($"读取 地址:{address} 站号:{stationNumber} 功能码:{functionCode} 失败。{ex.Message}");
            //    }
            //}
            finally
            {
                //if (isAutoOpen) Dispose();
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 读取Int16类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<short> ReadInt16(string address, byte stationNumber = 1, byte functionCode = 3)
        {
            var readResut = Read(address, stationNumber, functionCode);
            var result = new IoTResult<short>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToInt16(readResut.Value, 0);
            return result.ToEnd();
        }

        /// <summary>
        /// 按位的方式读取
        /// </summary>
        /// <param name="address">寄存器地址:如1.00 ... 1.14、1.15</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="left">按位取值从左边开始取</param>
        /// <returns></returns>
        public IoTResult<short> ReadInt16Bit(string address, byte stationNumber = 1, byte functionCode = 3, bool left = true)
        {
            string[] adds = address.Split('.');
            var readResut = Read(adds[0].Trim(), stationNumber, functionCode);
            var result = new IoTResult<short>(readResut);
            if (result.IsSucceed)
            {
                result.Value = BitConverter.ToInt16(readResut.Value, 0);
                if (adds.Length >= 2)
                {
                    var index = int.Parse(adds[1].Trim());
                    var binaryArray = DataConvert.IntToBinaryArray(result.Value, 16);
                    if (left)
                    {
                        var length = binaryArray.Length - 16;
                        result.Value = short.Parse(binaryArray[length + index].ToString());
                    }
                    else
                        result.Value = short.Parse(binaryArray[binaryArray.Length - 1 - index].ToString());
                }
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 读取Int16类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param
        public IoTResult<short> ReadInt16(int address, byte stationNumber = 1, byte functionCode = 3)
        {
            return ReadInt16(address.ToString(), stationNumber, functionCode);
        }

        /// <summary>
        /// 读取UInt16类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<ushort> ReadUInt16(string address, byte stationNumber = 1, byte functionCode = 3)
        {
            var readResut = Read(address, stationNumber, functionCode);
            var result = new IoTResult<ushort>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToUInt16(readResut.Value, 0);
            return result.ToEnd();
        }

        /// <summary>
        /// 按位的方式读取
        /// </summary>
        /// <param name="address">寄存器地址:如1.00 ... 1.14、1.15</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="left">按位取值从左边开始取</param>
        /// <returns></returns>
        public IoTResult<ushort> ReadUInt16Bit(string address, byte stationNumber = 1, byte functionCode = 3, bool left = true)
        {
            string[] adds = address.Split('.');
            var readResut = Read(adds[0].Trim(), stationNumber, functionCode);
            var result = new IoTResult<ushort>(readResut);
            if (result.IsSucceed)
            {
                result.Value = BitConverter.ToUInt16(readResut.Value, 0);
                if (adds.Length >= 2)
                {
                    var index = int.Parse(adds[1].Trim());
                    var binaryArray = DataConvert.IntToBinaryArray(result.Value, 16);
                    if (left)
                    {
                        var length = binaryArray.Length - 16;
                        result.Value = ushort.Parse(binaryArray[length + index].ToString());
                    }
                    else
                        result.Value = ushort.Parse(binaryArray[binaryArray.Length - 1 - index].ToString());
                }
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 读取UInt16类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<ushort> ReadUInt16(int address, byte stationNumber = 1, byte functionCode = 3)
        {
            return ReadUInt16(address.ToString(), stationNumber, functionCode);
        }

        /// <summary>
        /// 读取Int32类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<int> ReadInt32(string address, byte stationNumber = 1, byte functionCode = 3)
        {
            var readResut = Read(address, stationNumber, functionCode, readLength: 2);
            var result = new IoTResult<int>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToInt32(readResut.Value, 0);
            return result.ToEnd();
        }

        /// <summary>
        /// 读取Int32类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<int> ReadInt32(int address, byte stationNumber = 1, byte functionCode = 3)
        {
            return ReadInt32(address.ToString(), stationNumber, functionCode);
        }


        /// <summary>
        /// 读取UInt32类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<uint> ReadUInt32(string address, byte stationNumber = 1, byte functionCode = 3)
        {
            var readResut = Read(address, stationNumber, functionCode, readLength: 2);
            var result = new IoTResult<uint>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToUInt32(readResut.Value, 0);
            return result.ToEnd();
        }

        /// <summary>
        /// 读取UInt32类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<uint> ReadUInt32(int address, byte stationNumber = 1, byte functionCode = 3)
        {
            return ReadUInt32(address.ToString(), stationNumber, functionCode);
        }

        /// <summary>
        /// 读取Int64类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<long> ReadInt64(string address, byte stationNumber = 1, byte functionCode = 3)
        {
            var readResut = Read(address, stationNumber, functionCode, readLength: 4);
            var result = new IoTResult<long>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToInt64(readResut.Value, 0);
            return result.ToEnd();
        }

        /// <summary>
        /// 读取Int64类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<long> ReadInt64(int address, byte stationNumber = 1, byte functionCode = 3)
        {
            return ReadInt64(address.ToString(), stationNumber, functionCode);
        }

        /// <summary>
        /// 读取UInt64类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<ulong> ReadUInt64(string address, byte stationNumber = 1, byte functionCode = 3)
        {
            var readResut = Read(address, stationNumber, functionCode, readLength: 4);
            var result = new IoTResult<ulong>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToUInt64(readResut.Value, 0);
            return result.ToEnd();
        }

        /// <summary>
        /// 读取UInt64类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<ulong> ReadUInt64(int address, byte stationNumber = 1, byte functionCode = 3)
        {
            return ReadUInt64(address.ToString(), stationNumber, functionCode);
        }

        /// <summary>
        /// 读取Float类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<float> ReadFloat(string address, byte stationNumber = 1, byte functionCode = 3)
        {
            var readResut = Read(address, stationNumber, functionCode, readLength: 2);
            var result = new IoTResult<float>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToSingle(readResut.Value, 0);
            return result.ToEnd();
        }

        /// <summary>
        /// 读取Float类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<float> ReadFloat(int address, byte stationNumber = 1, byte functionCode = 3)
        {
            return ReadFloat(address.ToString(), stationNumber, functionCode);
        }

        /// <summary>
        /// 读取Double类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<double> ReadDouble(string address, byte stationNumber = 1, byte functionCode = 3)
        {
            var readResut = Read(address, stationNumber, functionCode, readLength: 4);
            var result = new IoTResult<double>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToDouble(readResut.Value, 0);
            return result.ToEnd();
        }

        /// <summary>
        /// 读取Double类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<double> ReadDouble(int address, byte stationNumber = 1, byte functionCode = 3)
        {
            return ReadDouble(address.ToString(), stationNumber, functionCode);
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="encoding">编码</param>
        /// <param name="readLength">读取长度</param>
        /// <returns></returns>
        public IoTResult<string> ReadString(string address, byte stationNumber = 1, byte functionCode = 3, Encoding encoding = null, ushort readLength = 10)
        {
            if (encoding == null) encoding = Encoding.ASCII;

            readLength = (ushort)Math.Ceiling((float)readLength / 2);
            var readResut = Read(address, stationNumber, functionCode, readLength: readLength, byteFormatting: false);
            var result = new IoTResult<string>(readResut);
            if (result.IsSucceed)
                result.Value = encoding.GetString(readResut.Value.Reverse().ToArray())?.Replace("\0", "");
            return result.ToEnd();
        }

        /// <summary>
        /// 读取线圈类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<bool> ReadCoil(string address, byte stationNumber = 1, byte functionCode = 1)
        {
            var readResut = Read(address, stationNumber, functionCode);
            var result = new IoTResult<bool>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToBoolean(readResut.Value, 0);
            return result.ToEnd();
        }

        /// <summary>
        /// 读取线圈类型数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<bool> ReadCoil(int address, byte stationNumber = 1, byte functionCode = 1)
        {
            return ReadCoil(address.ToString(), stationNumber, functionCode);
        }

        /// <summary>
        /// 读取离散类型数据
        /// </summary>
        /// <param name="address">读取地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<bool> ReadDiscrete(string address, byte stationNumber = 1, byte functionCode = 2)
        {
            var readResut = Read(address, stationNumber, functionCode);
            var result = new IoTResult<bool>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToBoolean(readResut.Value, 0);
            return result.ToEnd();
        }

        /// <summary>
        /// 读取离散类型数据
        /// </summary>
        /// <param name="address">读取地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public IoTResult<bool> ReadDiscrete(int address, byte stationNumber = 1, byte functionCode = 2)
        {
            return ReadDiscrete(address.ToString(), stationNumber, functionCode);
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<short> ReadInt16(string beginAddress, string address, byte[] values)
        {
            if (!int.TryParse(address?.Trim(), out int addressInt) || !int.TryParse(beginAddress?.Trim(), out int beginAddressInt))
                throw new Exception($"只能是数字，参数address：{address}  beginAddress：{beginAddress}");
            try
            {
                var interval = addressInt - beginAddressInt;
                var byteArry = values.Skip(interval * 2).Take(2).Reverse().ToArray();
                return new IoTResult<short>
                {
                    Value = BitConverter.ToInt16(byteArry, 0)
                };
            }
            catch (Exception ex)
            {
                return new IoTResult<short>().AddError(ex);
            }
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<short> ReadInt16(int beginAddress, int address, byte[] values)
        {
            return ReadInt16(beginAddress.ToString(), address.ToString(), values);
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<ushort> ReadUInt16(string beginAddress, string address, byte[] values)
        {
            if (!int.TryParse(address?.Trim(), out int addressInt) || !int.TryParse(beginAddress?.Trim(), out int beginAddressInt))
                throw new Exception($"只能是数字，参数address：{address}  beginAddress：{beginAddress}");
            try
            {
                var interval = addressInt - beginAddressInt;
                var byteArry = values.Skip(interval * 2).Take(2).Reverse().ToArray();
                return new IoTResult<ushort>
                {
                    Value = BitConverter.ToUInt16(byteArry, 0)
                };
            }
            catch (Exception ex)
            {
                return new IoTResult<ushort>().AddError(ex);
            }
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<ushort> ReadUInt16(int beginAddress, int address, byte[] values)
        {
            return ReadUInt16(beginAddress.ToString(), address.ToString(), values);
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<int> ReadInt32(string beginAddress, string address, byte[] values)
        {
            if (!int.TryParse(address?.Trim(), out int addressInt) || !int.TryParse(beginAddress?.Trim(), out int beginAddressInt))
                throw new Exception($"只能是数字，参数address：{address}  beginAddress：{beginAddress}");
            try
            {
                var interval = (addressInt - beginAddressInt) / 2;
                var offset = (addressInt - beginAddressInt) % 2 * 2;//取余 乘以2（每个地址16位，占两个字节）
                var byteArry = values.Skip(interval * 2 * 2 + offset).Take(2 * 2).Reverse().ToArray().ByteFormatting(format);
                return new IoTResult<int>
                {
                    Value = BitConverter.ToInt32(byteArry, 0)
                };
            }
            catch (Exception ex)
            {
                return new IoTResult<int>().AddError(ex);
            }
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<int> ReadInt32(int beginAddress, int address, byte[] values)
        {
            return ReadInt32(beginAddress.ToString(), address.ToString(), values);
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<uint> ReadUInt32(string beginAddress, string address, byte[] values)
        {
            if (!int.TryParse(address?.Trim(), out int addressInt) || !int.TryParse(beginAddress?.Trim(), out int beginAddressInt))
                throw new Exception($"只能是数字，参数address：{address}  beginAddress：{beginAddress}");
            try
            {
                var interval = (addressInt - beginAddressInt) / 2;
                var offset = (addressInt - beginAddressInt) % 2 * 2;//取余 乘以2（每个地址16位，占两个字节）
                var byteArry = values.Skip(interval * 2 * 2 + offset).Take(2 * 2).Reverse().ToArray().ByteFormatting(format);
                return new IoTResult<uint>
                {
                    Value = BitConverter.ToUInt32(byteArry, 0)
                };
            }
            catch (Exception ex)
            {
                return new IoTResult<uint>().AddError(ex);
            }
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<uint> ReadUInt32(int beginAddress, int address, byte[] values)
        {
            return ReadUInt32(beginAddress.ToString(), address.ToString(), values);
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<long> ReadInt64(string beginAddress, string address, byte[] values)
        {
            if (!int.TryParse(address?.Trim(), out int addressInt) || !int.TryParse(beginAddress?.Trim(), out int beginAddressInt))
                throw new Exception($"只能是数字，参数address：{address}  beginAddress：{beginAddress}");
            try
            {
                var interval = (addressInt - beginAddressInt) / 4;
                var offset = (addressInt - beginAddressInt) % 4 * 2;//取余 乘以2（每个地址16位，占两个字节）
                var byteArry = values.Skip(interval * 2 * 4 + offset).Take(2 * 4).Reverse().ToArray().ByteFormatting(format);
                return new IoTResult<long>
                {
                    Value = BitConverter.ToInt64(byteArry, 0)
                };
            }
            catch (Exception ex)
            {
                return new IoTResult<long>().AddError(ex);
            }
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<long> ReadInt64(int beginAddress, int address, byte[] values)
        {
            return ReadInt64(beginAddress.ToString(), address.ToString(), values);
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<ulong> ReadUInt64(string beginAddress, string address, byte[] values)
        {
            if (!int.TryParse(address?.Trim(), out int addressInt) || !int.TryParse(beginAddress?.Trim(), out int beginAddressInt))
                throw new Exception($"只能是数字，参数address：{address}  beginAddress：{beginAddress}");
            try
            {
                var interval = (addressInt - beginAddressInt) / 4;
                var offset = (addressInt - beginAddressInt) % 4 * 2;//取余 乘以2（每个地址16位，占两个字节）
                var byteArry = values.Skip(interval * 2 * 4 + offset).Take(2 * 4).Reverse().ToArray().ByteFormatting(format);
                return new IoTResult<ulong>
                {
                    Value = BitConverter.ToUInt64(byteArry, 0)
                };
            }
            catch (Exception ex)
            {
                return new IoTResult<ulong>().AddError(ex);
            }
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<ulong> ReadUInt64(int beginAddress, int address, byte[] values)
        {
            return ReadUInt64(beginAddress.ToString(), address.ToString(), values);
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<float> ReadFloat(string beginAddress, string address, byte[] values)
        {
            if (!int.TryParse(address?.Trim(), out int addressInt) || !int.TryParse(beginAddress?.Trim(), out int beginAddressInt))
                throw new Exception($"只能是数字，参数address：{address}  beginAddress：{beginAddress}");
            try
            {
                var interval = (addressInt - beginAddressInt) / 2;
                var offset = (addressInt - beginAddressInt) % 2 * 2;//取余 乘以2（每个地址16位，占两个字节）
                var byteArry = values.Skip(interval * 2 * 2 + offset).Take(2 * 2).Reverse().ToArray().ByteFormatting(format);
                return new IoTResult<float>
                {
                    Value = BitConverter.ToSingle(byteArry, 0)
                };
            }
            catch (Exception ex)
            {
                return new IoTResult<float>().AddError(ex);
            }
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<float> ReadFloat(int beginAddress, int address, byte[] values)
        {
            return ReadFloat(beginAddress.ToString(), address.ToString(), values);
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<double> ReadDouble(string beginAddress, string address, byte[] values)
        {
            if (!int.TryParse(address?.Trim(), out int addressInt) || !int.TryParse(beginAddress?.Trim(), out int beginAddressInt))
                throw new Exception($"只能是数字，参数address：{address}  beginAddress：{beginAddress}");
            try
            {
                var interval = (addressInt - beginAddressInt) / 4;
                var offset = (addressInt - beginAddressInt) % 4 * 2;//取余 乘以2（每个地址16位，占两个字节）
                var byteArry = values.Skip(interval * 2 * 4 + offset).Take(2 * 4).Reverse().ToArray().ByteFormatting(format);
                return new IoTResult<double>
                {
                    Value = BitConverter.ToDouble(byteArry, 0)
                };
            }
            catch (Exception ex)
            {
                return new IoTResult<double>().AddError(ex);
            }
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<double> ReadDouble(int beginAddress, int address, byte[] values)
        {
            return ReadDouble(beginAddress.ToString(), address.ToString(), values);
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<bool> ReadCoil(string beginAddress, string address, byte[] values)
        {
            if (!int.TryParse(address?.Trim(), out int addressInt) || !int.TryParse(beginAddress?.Trim(), out int beginAddressInt))
                throw new Exception($"只能是数字，参数address：{address}  beginAddress：{beginAddress}");
            try
            {
                var interval = addressInt - beginAddressInt;
                var index = (interval + 1) % 8 == 0 ? (interval + 1) / 8 : (interval + 1) / 8 + 1;
                var binaryArray = Convert.ToInt32(values[index - 1]).IntToBinaryArray().ToArray().Reverse().ToArray();
                var isBit = false;
                if ((index - 1) * 8 + binaryArray.Length > interval)
                    isBit = binaryArray[interval - (index - 1) * 8].ToString() == 1.ToString();
                return new IoTResult<bool>()
                {
                    Value = isBit
                };
            }
            catch (Exception ex)
            {
                return new IoTResult<bool>().AddError(ex);
            }
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<bool> ReadCoil(int beginAddress, int address, byte[] values)
        {
            return ReadCoil(beginAddress.ToString(), address.ToString(), values);
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<bool> ReadDiscrete(string beginAddress, string address, byte[] values)
        {
            if (!int.TryParse(address?.Trim(), out int addressInt) || !int.TryParse(beginAddress?.Trim(), out int beginAddressInt))
                throw new Exception($"只能是数字，参数address：{address}  beginAddress：{beginAddress}");
            try
            {
                var interval = addressInt - beginAddressInt;
                var index = (interval + 1) % 8 == 0 ? (interval + 1) / 8 : (interval + 1) / 8 + 1;
                var binaryArray = Convert.ToInt32(values[index - 1]).IntToBinaryArray().ToArray().Reverse().ToArray();
                var isBit = false;
                if ((index - 1) * 8 + binaryArray.Length > interval)
                    isBit = binaryArray[interval - (index - 1) * 8].ToString() == 1.ToString();
                return new IoTResult<bool>()
                {
                    Value = isBit
                };
            }
            catch (Exception ex)
            {
                return new IoTResult<bool>().AddError(ex);
            }
        }

        /// <summary>
        /// 从批量读取的数据字节提取对应的地址数据
        /// </summary>
        /// <param name="beginAddress">批量读取的起始地址</param>
        /// <param name="address">读取地址</param>
        /// <param name="values">批量读取的值</param>
        /// <returns></returns>
        public IoTResult<bool> ReadDiscrete(int beginAddress, int address, byte[] values)
        {
            return ReadDiscrete(beginAddress.ToString(), address.ToString(), values);
        }

        /// <summary>
        /// 分批读取（批量读取，内部进行批量计算读取）
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        private IoTResult<List<ModbusOutput>> BatchRead(List<ModbusInput> addresses)
        {
            var result = new IoTResult<List<ModbusOutput>>();
            result.Value = new List<ModbusOutput>();
            var functionCodes = addresses.Select(t => t.FunctionCode).Distinct();
            foreach (var functionCode in functionCodes)
            {
                var stationNumbers = addresses.Where(t => t.FunctionCode == functionCode).Select(t => t.StationNumber).Distinct();
                foreach (var stationNumber in stationNumbers)
                {
                    var addressList = addresses.Where(t => t.FunctionCode == functionCode && t.StationNumber == stationNumber)
                        .DistinctBy(t => t.Address)
                        .ToDictionary(t => t.Address, t => t.DataType);
                    var tempResult = BatchRead(addressList, stationNumber, functionCode);
                    if (tempResult.IsSucceed)
                    {
                        foreach (var item in tempResult.Value)
                        {
                            result.Value.Add(new ModbusOutput()
                            {
                                Address = item.Key,
                                FunctionCode = functionCode,
                                StationNumber = stationNumber,
                                Value = item.Value
                            });
                        }
                    }
                    else
                    {
                        result.AddError(tempResult.Error);
                    }
                }
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 分批读取
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="retryCount">如果读取异常，重试次数</param>
        /// <returns></returns>
        public IoTResult<List<ModbusOutput>> BatchRead(List<ModbusInput> addresses, uint retryCount = 1)
        {
            var result = BatchRead(addresses);
            for (int i = 0; i < retryCount; i++)
            {
                if (!result.IsSucceed)
                {
                    //WarningLog?.Invoke( result.Error.FirstOrDefault());
                    result = BatchRead(addresses);
                }
                else
                    break;
            }
            return result;
        }

        private IoTResult<Dictionary<int, object>> BatchRead(Dictionary<string, DataTypeEnum> addressList, byte stationNumber, byte functionCode)
        {
            var result = new IoTResult<Dictionary<int, object>>();
            result.Value = new Dictionary<int, object>();

            var addresses = addressList.Select(t => new KeyValuePair<int, DataTypeEnum>(int.Parse(t.Key), t.Value)).ToList();

            var minAddress = addresses.Select(t => t.Key).Min();
            var maxAddress = addresses.Select(t => t.Key).Max();
            while (maxAddress >= minAddress)
            {
                int readLength = 121;//125 - 4 = 121

                var tempAddress = addresses.Where(t => t.Key >= minAddress && t.Key <= minAddress + readLength).ToList();
                //如果范围内没有数据。按正确逻辑不存在这种情况。
                if (!tempAddress.Any())
                {
                    minAddress = minAddress + readLength;
                    continue;
                }

                var tempMax = tempAddress.OrderByDescending(t => t.Key).FirstOrDefault();
                switch (tempMax.Value)
                {
                    case DataTypeEnum.Bool:
                    case DataTypeEnum.Byte:
                    case DataTypeEnum.Int16:
                    case DataTypeEnum.UInt16:
                        readLength = tempMax.Key + 1 - minAddress;
                        break;
                    case DataTypeEnum.Int32:
                    case DataTypeEnum.UInt32:
                    case DataTypeEnum.Float:
                        readLength = tempMax.Key + 2 - minAddress;
                        break;
                    case DataTypeEnum.Int64:
                    case DataTypeEnum.UInt64:
                    case DataTypeEnum.Double:
                        readLength = tempMax.Key + 4 - minAddress;
                        break;
                    default:
                        throw new Exception("Err BatchRead 未定义类型 -1");
                }

                var tempResult = Read(minAddress.ToString(), stationNumber, functionCode, Convert.ToUInt16(readLength), false);

                if (!tempResult.IsSucceed)
                {
                    // $"读取 地址:{minAddress} 站号:{stationNumber} 功能码:{functionCode} 失败。{tempResult.Err}";
                    result.AddError(tempResult.Error);
                    return result.ToEnd();
                }

                var rValue = tempResult.Value.Reverse().ToArray();
                foreach (var item in tempAddress)
                {
                    object tempVaue = null;

                    switch (item.Value)
                    {
                        case DataTypeEnum.Bool:
                            tempVaue = ReadCoil(minAddress, item.Key, rValue).Value;
                            break;
                        case DataTypeEnum.Byte:
                            throw new Exception("Err BatchRead 未定义类型 -2");
                        case DataTypeEnum.Int16:
                            tempVaue = ReadInt16(minAddress, item.Key, rValue).Value;
                            break;
                        case DataTypeEnum.UInt16:
                            tempVaue = ReadUInt16(minAddress, item.Key, rValue).Value;
                            break;
                        case DataTypeEnum.Int32:
                            tempVaue = ReadInt32(minAddress, item.Key, rValue).Value;
                            break;
                        case DataTypeEnum.UInt32:
                            tempVaue = ReadUInt32(minAddress, item.Key, rValue).Value;
                            break;
                        case DataTypeEnum.Int64:
                            tempVaue = ReadInt64(minAddress, item.Key, rValue).Value;
                            break;
                        case DataTypeEnum.UInt64:
                            tempVaue = ReadUInt64(minAddress, item.Key, rValue).Value;
                            break;
                        case DataTypeEnum.Float:
                            tempVaue = ReadFloat(minAddress, item.Key, rValue).Value;
                            break;
                        case DataTypeEnum.Double:
                            tempVaue = ReadDouble(minAddress, item.Key, rValue).Value;
                            break;
                        default:
                            throw new Exception("Err BatchRead 未定义类型 -3");
                    }

                    result.Value.Add(item.Key, tempVaue);
                }
                minAddress = minAddress + readLength;

                if (addresses.Any(t => t.Key >= minAddress))
                    minAddress = addresses.Where(t => t.Key >= minAddress).OrderBy(t => t.Key).FirstOrDefault().Key;
                else
                    return result.ToEnd();
            }
            return result.ToEnd();
        }

        #endregion

        #region Write 写入

        /// <summary>
        /// 线圈写入
        /// </summary>
        /// <param name="address">写入地址</param>
        /// <param name="value"></param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        public IoTResult Write(string address, bool value, byte stationNumber = 1, byte functionCode = 5)
        {
            var result = new IoTResult();
            //if (!socket?.Connected ?? true)
            //{
            //    var conentResult = Connect();
            //    if (!conentResult.IsSucceed)
            //        return result.AddError(conentResult.Error);
            //}
            try
            {
                var chenkHead = GetCheckHead(functionCode);
                var command = GetWriteCoilCommand(address, value, stationNumber, functionCode, chenkHead);
                result.Responses.Add(command);
                var sendResult = Client.SendReceive(command);
                if (!sendResult.IsSucceed)
                    return result.AddError(sendResult.Error).ToEnd();
                var dataPackage = sendResult.Value;
                result.Responses.Add(dataPackage);
                if (chenkHead[0] != dataPackage[0] || chenkHead[1] != dataPackage[1])
                {
                    result.AddError("响应结果校验失败");
                    //SafeClose();
                }
                else if (ModbusHelper.VerifyFunctionCode(functionCode, dataPackage[7]))
                {
                    result.AddError(ModbusHelper.ErrMsg(dataPackage[8]));
                }
            }
            //catch (SocketException ex)
            //{

            //    if (ex.SocketErrorCode == SocketError.TimedOut)
            //    {
            //        result.AddError("连接超时");
            //        SafeClose();
            //    }
            //    else
            //    {
            //        result.AddError(ex);
            //    }
            //}
            finally
            {
                //if (isAutoOpen) Dispose();
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">写入地址</param>
        /// <param name="values">写入字节数组</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="byteFormatting">大小端设置</param>
        /// <returns></returns>
        public IoTResult Write(string address, byte[] values, byte stationNumber = 1, byte functionCode = 16, bool byteFormatting = true)
        {
            var result = new IoTResult();
            //if (!socket?.Connected ?? true)
            //{
            //    var conentResult = Connect();
            //    if (!conentResult.IsSucceed)
            //        return result.AddError(conentResult.Error);
            //}
            try
            {
                if (byteFormatting)
                    values = values.ByteFormatting(format);
                var chenkHead = GetCheckHead(functionCode);
                var command = GetWriteCommand(address, values, stationNumber, functionCode, chenkHead);
                result.Responses.Add(command);
                var sendResult = Client.SendReceive(command);
                if (!sendResult.IsSucceed)
                    return result.AddError(sendResult.Error).ToEnd();
                var dataPackage = sendResult.Value;
                result.Responses.Add(dataPackage);
                if (chenkHead[0] != dataPackage[0] || chenkHead[1] != dataPackage[1])
                {
                    result.AddError("响应结果校验失败");
                    //SafeClose();
                }
                else if (ModbusHelper.VerifyFunctionCode(functionCode, dataPackage[7]))
                {

                    result.AddError(ModbusHelper.ErrMsg(dataPackage[8]));
                }
            }
            //catch (SocketException ex)
            //{

            //    if (ex.SocketErrorCode == SocketError.TimedOut)
            //    {
            //        result.AddError("连接超时");
            //        SafeClose();
            //    }
            //    else
            //    {
            //        result.AddError(ex);
            //    }
            //}
            finally
            {
                //if (isAutoOpen) Dispose();
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">写入的值</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        public IoTResult Write(string address, short value, byte stationNumber = 1, byte functionCode = 16)
        {
            var values = BitConverter.GetBytes(value).Reverse().ToArray();
            return Write(address, values, stationNumber, functionCode);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">写入的值</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        public IoTResult Write(string address, ushort value, byte stationNumber = 1, byte functionCode = 16)
        {
            var values = BitConverter.GetBytes(value).Reverse().ToArray();
            return Write(address, values, stationNumber, functionCode);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">写入的值</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        public IoTResult Write(string address, int value, byte stationNumber = 1, byte functionCode = 16)
        {
            var values = BitConverter.GetBytes(value).Reverse().ToArray();
            return Write(address, values, stationNumber, functionCode);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">写入的值</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        public IoTResult Write(string address, uint value, byte stationNumber = 1, byte functionCode = 16)
        {
            var values = BitConverter.GetBytes(value).Reverse().ToArray();
            return Write(address, values, stationNumber, functionCode);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">写入的值</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        public IoTResult Write(string address, long value, byte stationNumber = 1, byte functionCode = 16)
        {
            var values = BitConverter.GetBytes(value).Reverse().ToArray();
            return Write(address, values, stationNumber, functionCode);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">写入的值</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        public IoTResult Write(string address, ulong value, byte stationNumber = 1, byte functionCode = 16)
        {
            var values = BitConverter.GetBytes(value).Reverse().ToArray();
            return Write(address, values, stationNumber, functionCode);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">写入的值</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        public IoTResult Write(string address, float value, byte stationNumber = 1, byte functionCode = 16)
        {
            var values = BitConverter.GetBytes(value).Reverse().ToArray();
            return Write(address, values, stationNumber, functionCode);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">写入的值</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        public IoTResult Write(string address, double value, byte stationNumber = 1, byte functionCode = 16)
        {
            var values = BitConverter.GetBytes(value).Reverse().ToArray();
            return Write(address, values, stationNumber, functionCode);
        }

        /// <summary>
        /// 写字符串
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">字符串值</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public IoTResult Write(string address, string value, byte stationNumber = 1, byte functionCode = 16, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.ASCII;
            if (value.Length % 2 == 1)
                value = value + "\0";
            var values = encoding.GetBytes(value);
            return Write(address, values, stationNumber, functionCode, false);
        }
        #endregion

        #region 获取命令

        /// <summary>
        /// 获取随机校验头
        /// </summary>
        /// <returns></returns>
        private byte[] GetCheckHead(int seed)
        {
            var random = new Random(DateTime.Now.Millisecond + seed);
            return new byte[] { (byte)random.Next(255), (byte)random.Next(255) };
        }

        /// <summary>
        /// 获取读取命令
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="length">读取长度</param>
        /// <returns></returns>
        public byte[] GetReadCommand(string address, byte stationNumber, byte functionCode, ushort length, byte[] check = null)
        {
            var readAddress = ushort.Parse(address?.Trim());
            if (plcAddresses) readAddress = (ushort)(Convert.ToUInt16(address?.Trim().Substring(1)) - 1);

            byte[] buffer = new byte[12];
            buffer[0] = check?[0] ?? 0x19;
            buffer[1] = check?[1] ?? 0xB2;//Client发出的检验信息
            buffer[2] = 0x00;
            buffer[3] = 0x00;//表示tcp/ip 的协议的Modbus的协议
            buffer[4] = 0x00;
            buffer[5] = 0x06;//表示的是该字节以后的字节长度

            buffer[6] = stationNumber;  //站号
            buffer[7] = functionCode;   //功能码
            buffer[8] = BitConverter.GetBytes(readAddress)[1];
            buffer[9] = BitConverter.GetBytes(readAddress)[0];//寄存器地址
            buffer[10] = BitConverter.GetBytes(length)[1];
            buffer[11] = BitConverter.GetBytes(length)[0];//表示request 寄存器的长度(寄存器个数)
            return buffer;
        }

        /// <summary>
        /// 获取写入命令
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="values">批量读取的值</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public byte[] GetWriteCommand(string address, byte[] values, byte stationNumber, byte functionCode, byte[] check = null)
        {
            var writeAddress = ushort.Parse(address?.Trim());
            if (plcAddresses) writeAddress = (ushort)(Convert.ToUInt16(address?.Trim().Substring(1)) - 1);

            byte[] buffer = new byte[13 + values.Length];
            buffer[0] = check?[0] ?? 0x19;
            buffer[1] = check?[1] ?? 0xB2;//检验信息，用来验证response是否串数据了           
            buffer[4] = BitConverter.GetBytes(7 + values.Length)[1];
            buffer[5] = BitConverter.GetBytes(7 + values.Length)[0];//表示的是header handle后面还有多长的字节

            buffer[6] = stationNumber; //站号
            buffer[7] = functionCode;  //功能码
            buffer[8] = BitConverter.GetBytes(writeAddress)[1];
            buffer[9] = BitConverter.GetBytes(writeAddress)[0];//寄存器地址
            buffer[10] = (byte)(values.Length / 2 / 256);
            buffer[11] = (byte)(values.Length / 2 % 256);//写寄存器数量(除2是两个字节一个寄存器，寄存器16位。除以256是byte最大存储255。)              
            buffer[12] = (byte)values.Length;          //写字节的个数
            values.CopyTo(buffer, 13);                   //把目标值附加到数组后面
            return buffer;
        }

        /// <summary>
        /// 获取线圈写入命令
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value"></param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public byte[] GetWriteCoilCommand(string address, bool value, byte stationNumber, byte functionCode, byte[] check = null)
        {
            var writeAddress = ushort.Parse(address?.Trim());
            if (plcAddresses) writeAddress = (ushort)(Convert.ToUInt16(address?.Trim().Substring(1)) - 1);

            byte[] buffer = new byte[12];
            buffer[0] = check?[0] ?? 0x19;
            buffer[1] = check?[1] ?? 0xB2;//Client发出的检验信息     
            buffer[4] = 0x00;
            buffer[5] = 0x06;//表示的是该字节以后的字节长度

            buffer[6] = stationNumber;//站号
            buffer[7] = functionCode; //功能码
            buffer[8] = BitConverter.GetBytes(writeAddress)[1];
            buffer[9] = BitConverter.GetBytes(writeAddress)[0];//寄存器地址
            buffer[10] = (byte)(value ? 0xFF : 0x00);     //此处只可以是FF表示闭合00表示断开，其他数值非法
            buffer[11] = 0x00;
            return buffer;
        }

        #endregion

        #region IIoTBase
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="address">全写法"s=2;x=3;100"，对应站号，功能码，地址</param>
        public virtual IoTResult<T> Read<T>(string address)
        {
            var result = ModbusInput.AddressAnalysis<T>(address, true, stationNumber);
            if (!result.IsSucceed)
                return new IoTResult<T>(result);

            var tType = typeof(T);
            if (tType == typeof(bool))
            {
                var readResut = ReadCoil(result.Value.Address, result.Value.StationNumber, result.Value.FunctionCode);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(byte))
            {
                var readResut = Read(result.Value.Address, result.Value.StationNumber, result.Value.FunctionCode, 1, false);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value.FirstOrDefault());
            }
            else if (tType == typeof(float))
            {
                var readResut = ReadFloat(result.Value.Address, result.Value.StationNumber, result.Value.FunctionCode);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(double))
            {
                var readResut = ReadDouble(result.Value.Address, result.Value.StationNumber, result.Value.FunctionCode);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(short))
            {
                var readResut = ReadInt16(result.Value.Address, result.Value.StationNumber, result.Value.FunctionCode);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(int))
            {
                var readResut = ReadInt32(result.Value.Address, result.Value.StationNumber, result.Value.FunctionCode);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(long))
            {
                var readResut = ReadInt64(result.Value.Address, result.Value.StationNumber, result.Value.FunctionCode);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(ushort))
            {
                var readResut = ReadUInt16(result.Value.Address, result.Value.StationNumber, result.Value.FunctionCode);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(uint))
            {
                var readResut = ReadUInt32(result.Value.Address, result.Value.StationNumber, result.Value.FunctionCode);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(ulong))
            {
                var readResut = ReadUInt64(result.Value.Address, result.Value.StationNumber, result.Value.FunctionCode);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else
            {
                throw new NotImplementedException("暂不支持的类型");
            }
        }

        public virtual IoTResult<string> ReadString(string address, int length, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 读取多个
        /// </summary>
        /// <param name="address">全写法"s=2;x=3;100"，对应站号，功能码，地址</param>
        /// <param name="number">读取数量</param>
        public virtual IoTResult<IEnumerable<T>> Read<T>(string address, int number)
        {
            var result = ModbusInput.AddressAnalysis<T>(address, true, stationNumber);
            if (!result.IsSucceed)
                return new IoTResult<IEnumerable<T>>(result);

            var d = Convert.ToUInt16(result.Value.Address);
            var addNum = WordHelp.OccupyNum<T>();
            List<ModbusInput> modbusInputs = new List<ModbusInput>();
            for (int i = 0; i < number; i++)
            {
                modbusInputs.Add(new ModbusInput()
                {
                    Address = (d + addNum * i).ToString(),
                    FunctionCode = result.Value.FunctionCode,
                    StationNumber = result.Value.StationNumber,
                    DataType = result.Value.DataType,
                });
            }

            var readResut = BatchRead(modbusInputs);
            if (!readResut.IsSucceed)
                return new IoTResult<IEnumerable<T>>(readResut);
            else
                return new IoTResult<IEnumerable<T>>(readResut, readResut.Value.Select(o => (T)o.Value));
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">全写法"s=2;x=3;100"，对应站号，功能码，地址</param>
        public virtual IoTResult Write<T>(string address, T value)
        {
            var result = ModbusInput.AddressAnalysis<T>(address, false, stationNumber);
            if (!result.IsSucceed)
                return result;

            if (value is bool boolv)
            {
                return Write(result.Value.Address, boolv, result.Value.StationNumber, result.Value.FunctionCode);
            }
            else if (value is byte bytev)
            {
                return Write(result.Value.Address, bytev, result.Value.StationNumber, result.Value.FunctionCode);
            }
            else if (value is sbyte sbytev)
            {
                return Write(result.Value.Address, sbytev, result.Value.StationNumber, result.Value.FunctionCode);
            }
            else if (value is float floatv)
            {
                return Write(result.Value.Address, floatv, result.Value.StationNumber, result.Value.FunctionCode);
            }
            else if (value is double doublev)
            {
                return Write(result.Value.Address, doublev, result.Value.StationNumber, result.Value.FunctionCode);
            }
            else if (value is short Int16v)
            {
                return Write(result.Value.Address, Int16v, result.Value.StationNumber, result.Value.FunctionCode);
            }
            else if (value is int Int32v)
            {
                return Write(result.Value.Address, Int32v, result.Value.StationNumber, result.Value.FunctionCode);
            }
            else if (value is long Int64v)
            {
                return Write(result.Value.Address, Int64v, result.Value.StationNumber, result.Value.FunctionCode);
            }
            else if (value is ushort UInt16v)
            {
                return Write(result.Value.Address, UInt16v, result.Value.StationNumber, result.Value.FunctionCode);
            }
            else if (value is uint UInt32v)
            {
                return Write(result.Value.Address, UInt32v, result.Value.StationNumber, result.Value.FunctionCode);
            }
            else if (value is ulong UInt64v)
            {
                return Write(result.Value.Address, UInt64v, result.Value.StationNumber, result.Value.FunctionCode);
            }
            else
            {
                throw new NotImplementedException("暂不支持的类型");
            }
        }

        public virtual IoTResult WriteString(string address, string value, int length, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 写入，内部循环，失败了就跳出
        /// </summary>
        /// <param name="address">全写法"s=2;x=3;100"，对应站号，功能码，地址</param>
        public virtual IoTResult Write<T>(string address, params T[] value)
        {
            var result = ModbusInput.AddressAnalysis<T>(address, false, stationNumber);
            if (!result.IsSucceed)
                return result;

            var d = Convert.ToUInt16(result.Value.Address);
            var addNum = WordHelp.OccupyNum<T>();

            var tType = typeof(T);
            IoTResult readResut = new IoTResult();

            var isautoopen = false;
            //if (!IsConnected)
            //{
            //    Open();
            //    isautoopen = true;
            //}
            if (tType == typeof(bool))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    readResut = Write((d + addNum * i).ToString(), (bool)(object)value[i], result.Value.StationNumber, result.Value.FunctionCode);
                    if (!readResut.IsSucceed)
                        break;
                }
            }
            else if (tType == typeof(byte))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    readResut = Write((d + addNum * i).ToString(), (byte)(object)value[i], result.Value.StationNumber, result.Value.FunctionCode);
                    if (!readResut.IsSucceed)
                        break;
                }
            }
            else if (tType == typeof(float))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    readResut = Write((d + addNum * i).ToString(), (float)(object)value[i], result.Value.StationNumber, result.Value.FunctionCode);
                    if (!readResut.IsSucceed)
                        break;
                }
            }
            else if (tType == typeof(double))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    readResut = Write((d + addNum * i).ToString(), (double)(object)value[i], result.Value.StationNumber, result.Value.FunctionCode);
                    if (!readResut.IsSucceed)
                        break;
                }
            }
            else if (tType == typeof(short))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    readResut = Write((d + addNum * i).ToString(), (short)(object)value[i], result.Value.StationNumber, result.Value.FunctionCode);
                    if (!readResut.IsSucceed)
                        break;
                }
            }
            else if (tType == typeof(int))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    readResut = Write((d + addNum * i).ToString(), (int)(object)value[i], result.Value.StationNumber, result.Value.FunctionCode);
                    if (!readResut.IsSucceed)
                        break;
                }
            }
            else if (tType == typeof(long))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    readResut = Write((d + addNum * i).ToString(), (long)(object)value[i], result.Value.StationNumber, result.Value.FunctionCode);
                    if (!readResut.IsSucceed)
                        break;
                }
            }
            else if (tType == typeof(ushort))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    readResut = Write((d + addNum * i).ToString(), (ushort)(object)value[i], result.Value.StationNumber, result.Value.FunctionCode);
                    if (!readResut.IsSucceed)
                        break;
                }
            }
            else if (tType == typeof(uint))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    readResut = Write((d + addNum * i).ToString(), (uint)(object)value[i], result.Value.StationNumber, result.Value.FunctionCode);
                    if (!readResut.IsSucceed)
                        break;
                }
            }
            else if (tType == typeof(ulong))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    readResut = Write((d + addNum * i).ToString(), (ulong)(object)value[i], result.Value.StationNumber, result.Value.FunctionCode);
                    if (!readResut.IsSucceed)
                        break;
                }
            }
            else
            {
                //if (isautoopen)
                //    Close();

                throw new NotImplementedException("暂不支持的类型");
            }

            //if (isautoopen)
            //    Close();
            return readResut;
        }
        #endregion
    }
}
