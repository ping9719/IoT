using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Ping9719.IoT.Algorithm;
using Ping9719.IoT.Communication.TCP;
using Ping9719.IoT.Enums;
using Ping9719.IoT.Interfaces;
using Ping9719.IoT;
using Ping9719.IoT.Modbus.Models;

namespace Ping9719.IoT.Modbus
{
    /// <summary>
    /// Tcp的方式发送ModbusRtu协议报文 - 客户端
    /// </summary>
    public class ModbusRtuOverTcpClient : SocketBase, IIoTBase
    {
        private EndianFormat format;
        private bool plcAddresses;
        private byte stationNumber = 1;

        /// <summary>
        /// 是否是连接的
        /// </summary>
        public override bool IsConnected => socket?.Connected ?? false;


        /// <summary>
        /// 字符串编码格式。默认ASCII
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.ASCII;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <param name="format">大小端设置</param>
        /// <param name="plcAddresses">PLC地址</param>
        public ModbusRtuOverTcpClient(string ip, int port, int timeout = 1500, EndianFormat format = EndianFormat.ABCD, byte stationNumber = 1, bool plcAddresses = false)
        {
            this.timeout = timeout;
            SetIpEndPoint(ip, port);
            this.format = format;
            this.plcAddresses = plcAddresses;
            this.stationNumber = stationNumber;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ipEndPoint">ip地址和端口</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <param name="format">大小端设置</param>
        public ModbusRtuOverTcpClient(IPEndPoint ipEndPoint, int timeout = 1500, EndianFormat format = EndianFormat.ABCD, byte stationNumber = 1, bool plcAddresses = false)
        {
            this.timeout = timeout;
            this.ipEndPoint = ipEndPoint;
            this.format = format;
            this.plcAddresses = plcAddresses;
            this.stationNumber = stationNumber;
        }

        protected override IoTResult Connect()
        {
            var result = new IoTResult();
            SafeClose();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.ReceiveTimeout = timeout;
                socket.SendTimeout = timeout;

                //连接
                //socket.Connect(ipEndPoint);
                IAsyncResult connectResult = socket.BeginConnect(ipEndPoint, null, null);
                //阻塞当前线程           
                if (!connectResult.AsyncWaitHandle.WaitOne(timeout))
                    throw new TimeoutException("连接超时");
                socket.EndConnect(connectResult);
            }
            catch (Exception ex)
            {
                SafeClose();
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        #region 发送报文，并获取响应报文
        /// <summary>
        /// 发送报文，并获取响应报文
        /// </summary>
        /// <param name="command"></param>
        /// <param name="lenght"></param>
        /// <returns></returns>
        public IoTResult<byte[]> SendPackage(byte[] command, int lenght)
        {
            IoTResult<byte[]> _SendPackage()
            {
                lock (this)
                {
                    //从发送命令到读取响应为最小单元，避免多线程执行串数据（可线程安全执行）
                    IoTResult<byte[]> result = new IoTResult<byte[]>();
                    //发送命令
                    socket.Send(command);
                    //获取响应报文    
                    var socketReadResul = SocketRead(lenght);
                    if (!socketReadResul.IsSucceed)
                        return socketReadResul;
                    result.Value = socketReadResul.Value;
                    return result.ToEnd();
                }
            }

            try
            {
                var result = _SendPackage();
                if (!result.IsSucceed)
                {
                    WarningLog?.Invoke(result.Error.FirstOrDefault());
                    //如果出现异常，则进行一次重试         
                    var conentResult = Connect();
                    if (!conentResult.IsSucceed)
                        return new IoTResult<byte[]>(conentResult);

                    return _SendPackage();
                }
                else
                    return result;
            }
            catch (Exception ex)
            {
                WarningLog?.Invoke(ex);
                //如果出现异常，则进行一次重试
                //重新打开连接
                var conentResult = Connect();
                if (!conentResult.IsSucceed)
                    return new IoTResult<byte[]>(conentResult);

                return _SendPackage();
            }
        }

        public override IoTResult<byte[]> SendPackageSingle(byte[] command)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region  Read 读取
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="readLength">读取长度</param>
        /// <param name="byteFormatting"></param>
        /// <returns></returns>
        public IoTResult<byte[]> Read(string address, byte stationNumber = 1, byte functionCode = 3, ushort readLength = 1, bool byteFormatting = true)
        {
            if (isAutoOpen) Connect();

            var result = new IoTResult<byte[]>();
            try
            {
                //获取命令（组装报文）
                byte[] command = GetReadCommand(address, stationNumber, functionCode, readLength);
                var commandCRC16 = CRC.Crc16(command);
                result.Requests.Add(commandCRC16);

                //发送命令并获取响应报文
                int readLenght;
                if (functionCode == 1 || functionCode == 2)
                    readLenght = 5 + (int)Math.Ceiling((float)readLength / 8);
                else
                    readLenght = 5 + readLength * 2;
                var sendResult = SendPackage(commandCRC16, readLenght);
                if (!sendResult.IsSucceed)
                    return sendResult;
                var responsePackage = sendResult.Value;
                //var responsePackage = SendPackage(commandCRC16, readLenght);
                if (!responsePackage.Any())
                {
                    return result.AddError("响应结果为空").ToEnd();
                }
                else if (!CRC.CheckCrc16(responsePackage))
                {
                    result.AddError("响应结果CRC16验证失败");
                    //return result.ToEnd();
                }

                byte[] resultData = new byte[responsePackage.Length - 2 - 3];
                Array.Copy(responsePackage, 3, resultData, 0, resultData.Length);
                result.Responses.Add(responsePackage);
                //4 获取响应报文数据（字节数组形式）       
                if (byteFormatting)
                    result.Value = resultData.Reverse().ToArray().ByteFormatting(format);
                else
                    result.Value = resultData.Reverse().ToArray();
            }
            catch (Exception ex)
            {
                
                result.AddError(ex);
            }
            finally
            {
                if (isAutoOpen) Dispose();
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 读取Int16
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
        /// 读取UInt16
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
        /// 读取Int32
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
        /// 读取UInt32
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
        /// 读取Int64
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
        /// 读取UInt64
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
        /// 读取Float
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
        /// 读取Double
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
        /// 读取线圈
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
        /// 读取离散
        /// </summary>
        /// <param name="address"></param>
        /// <param name="stationNumber"></param>
        /// <param name="functionCode"></param>
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
                    result.AddError ($"读取 地址:{minAddress} 站号:{stationNumber} 功能码:{functionCode} 失败。{tempResult.ErrorText}");
                    return result.ToEnd();
                }

                var rValue = tempResult.Value.Reverse().ToArray();
                foreach (var item in tempAddress)
                {
                    object tempVaue = null;

                    switch (item.Value)
                    {
                        case DataTypeEnum.Bool:
                            tempVaue = ReadCoil(minAddress.ToString(), item.Key.ToString(), rValue).Value;
                            break;
                        case DataTypeEnum.Byte:
                            throw new Exception("Err BatchRead 未定义类型 -2");
                        case DataTypeEnum.Int16:
                            tempVaue = ReadInt16(minAddress.ToString(), item.Key.ToString(), rValue).Value;
                            break;
                        case DataTypeEnum.UInt16:
                            tempVaue = ReadUInt16(minAddress.ToString(), item.Key.ToString(), rValue).Value;
                            break;
                        case DataTypeEnum.Int32:
                            tempVaue = ReadInt32(minAddress.ToString(), item.Key.ToString(), rValue).Value;
                            break;
                        case DataTypeEnum.UInt32:
                            tempVaue = ReadUInt32(minAddress.ToString(), item.Key.ToString(), rValue).Value;
                            break;
                        case DataTypeEnum.Int64:
                            tempVaue = ReadInt64(minAddress.ToString(), item.Key.ToString(), rValue).Value;
                            break;
                        case DataTypeEnum.UInt64:
                            tempVaue = ReadUInt64(minAddress.ToString(), item.Key.ToString(), rValue).Value;
                            break;
                        case DataTypeEnum.Float:
                            tempVaue = ReadFloat(minAddress.ToString(), item.Key.ToString(), rValue).Value;
                            break;
                        case DataTypeEnum.Double:
                            tempVaue = ReadDouble(minAddress.ToString(), item.Key.ToString(), rValue).Value;
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
                    WarningLog?.Invoke(result.Error.FirstOrDefault());
                    result = BatchRead(addresses);
                }
                else
                    break;
            }
            return result;
        }
        #endregion

        #region Write 写入
        /// <summary>
        /// 线圈写入
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <param name="stationNumber"></param>
        /// <param name="functionCode"></param>
        public IoTResult Write(string address, bool value, byte stationNumber = 1, byte functionCode = 5)
        {
            if (isAutoOpen) Connect();
            var result = new IoTResult();
            try
            {
                var command = GetWriteCoilCommand(address, value, stationNumber, functionCode);
                var commandCRC16 = CRC.Crc16(command);
                result.Requests.Add(commandCRC16);
                //发送命令并获取响应报文
                //var responsePackage = SendPackage(commandCRC16, 8);
                var sendResult = SendPackage(commandCRC16, 8);
                if (!sendResult.IsSucceed)
                    return sendResult;
                var responsePackage = sendResult.Value;

                if (!responsePackage.Any())
                {
                    return result.AddError("响应结果为空").ToEnd();
                }
                else if (!CRC.CheckCrc16(responsePackage))
                {
                    result.AddError("响应结果CRC16验证失败");
                    //return result.ToEnd();
                }
                byte[] resultBuffer = new byte[responsePackage.Length - 2];
                Buffer.BlockCopy(responsePackage, 0, resultBuffer, 0, resultBuffer.Length);
                result.Responses.Add(responsePackage);
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            finally
            {
                if (isAutoOpen) Dispose();
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address"></param>
        /// <param name="values"></param>
        /// <param name="stationNumber"></param>
        /// <param name="functionCode"></param>
        /// <returns></returns>
        public IoTResult Write(string address, byte[] values, byte stationNumber = 1, byte functionCode = 16, bool byteFormatting = true)
        {
            if (isAutoOpen) Connect();

            var result = new IoTResult();
            try
            {
                values = values.ByteFormatting(format);
                var command = GetWriteCommand(address, values, stationNumber, functionCode);

                var commandCRC16 = CRC.Crc16(command);
                result.Requests.Add(commandCRC16) ;
                //var responsePackage = SendPackage(commandCRC16, 8);
                var sendResult = SendPackage(commandCRC16, 8);
                if (!sendResult.IsSucceed)
                    return sendResult;
                var responsePackage = sendResult.Value;

                if (!responsePackage.Any())
                {
                    return result.AddError("响应结果为空").ToEnd();
                }
                else if (!CRC.CheckCrc16(responsePackage))
                {
                    result.AddError("响应结果CRC16验证失败");
                    //return result.ToEnd();
                }
                byte[] resultBuffer = new byte[responsePackage.Length - 2];
                Array.Copy(responsePackage, 0, resultBuffer, 0, resultBuffer.Length);
                result.Responses.Add(responsePackage);
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            finally
            {
                if (isAutoOpen) Dispose();
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
        #endregion

        #region 获取命令

        /// <summary>
        /// 获取读取命令
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="length">读取长度</param>
        /// <returns></returns>
        public byte[] GetReadCommand(string address, byte stationNumber, byte functionCode, ushort length)
        {
            var readAddress = ushort.Parse(address?.Trim());
            if (plcAddresses) readAddress = (ushort)(Convert.ToUInt16(address?.Trim().Substring(1)) - 1);

            byte[] buffer = new byte[6];
            buffer[0] = stationNumber;  //站号
            buffer[1] = functionCode;   //功能码
            buffer[2] = BitConverter.GetBytes(readAddress)[1];
            buffer[3] = BitConverter.GetBytes(readAddress)[0];//寄存器地址
            buffer[4] = BitConverter.GetBytes(length)[1];
            buffer[5] = BitConverter.GetBytes(length)[0];//表示request 寄存器的长度(寄存器个数)
            return buffer;
        }

        /// <summary>
        /// 获取写入命令
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="values"></param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        public byte[] GetWriteCommand(string address, byte[] values, byte stationNumber, byte functionCode)
        {
            var writeAddress = ushort.Parse(address?.Trim());
            if (plcAddresses) writeAddress = (ushort)(Convert.ToUInt16(address?.Trim().Substring(1)) - 1);

            byte[] buffer = new byte[7 + values.Length];
            buffer[0] = stationNumber; //站号
            buffer[1] = functionCode;  //功能码
            buffer[2] = BitConverter.GetBytes(writeAddress)[1];
            buffer[3] = BitConverter.GetBytes(writeAddress)[0];//寄存器地址
            buffer[4] = (byte)(values.Length / 2 / 256);
            buffer[5] = (byte)(values.Length / 2 % 256);//写寄存器数量(除2是两个字节一个寄存器，寄存器16位。除以256是byte最大存储255。)              
            buffer[6] = (byte)values.Length;          //写字节的个数
            values.CopyTo(buffer, 7);                   //把目标值附加到数组后面
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
        public byte[] GetWriteCoilCommand(string address, bool value, byte stationNumber, byte functionCode)
        {
            var writeAddress = ushort.Parse(address?.Trim());
            if (plcAddresses) writeAddress = (ushort)(Convert.ToUInt16(address?.Trim().Substring(1)) - 1);

            byte[] buffer = new byte[6];
            buffer[0] = stationNumber;//站号
            buffer[1] = functionCode; //功能码
            buffer[2] = BitConverter.GetBytes(writeAddress)[1];
            buffer[3] = BitConverter.GetBytes(writeAddress)[0];//寄存器地址
            buffer[4] = (byte)(value ? 0xFF : 0x00);     //此处只可以是FF表示闭合00表示断开，其他数值非法
            buffer[5] = 0x00;
            return buffer;
        }

        #endregion

        #region IIoTBase
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="address">全写法"s=2;x=3;100"，对应站号，功能码，地址</param>
        public IoTResult<T> Read<T>(string address)
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

        public IoTResult<string> ReadString(string address, int length, Encoding encoding)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 读取多个
        /// </summary>
        /// <param name="address">全写法"s=2;x=3;100"，对应站号，功能码，地址</param>
        /// <param name="number">读取数量</param>
        public IoTResult<IEnumerable<T>> Read<T>(string address, int number)
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
        public IoTResult Write<T>(string address, T value)
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

        public IoTResult WriteString(string address, string value, int length, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 写入，内部循环，失败了就跳出
        /// </summary>
        /// <param name="address">全写法"s=2;x=3;100"，对应站号，功能码，地址</param>
        public IoTResult Write<T>(string address, params T[] value)
        {
            var result = ModbusInput.AddressAnalysis<T>(address, false, stationNumber);
            if (!result.IsSucceed)
                return result;

            var d = Convert.ToUInt16(result.Value.Address);
            var addNum = WordHelp.OccupyNum<T>();

            var tType = typeof(T);
            IoTResult readResut = new IoTResult();

            var isautoopen = false;
            if (!IsConnected)
            {
                Open();
                isautoopen = true;
            }
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
                if (isautoopen)
                    Close();

                throw new NotImplementedException("暂不支持的类型");
            }

            if (isautoopen)
                Close();
            return readResut;
        }

        #endregion
    }
}
