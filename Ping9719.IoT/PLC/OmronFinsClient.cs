using Ping9719.IoT.Common;
using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Ping9719.IoT.PLC
{
    /// <summary>
    /// 欧姆龙客户端（Fins协议）
    /// https://flat2010.github.io/2020/02/23/Omron-Fins%E5%8D%8F%E8%AE%AE/
    /// </summary>
    public class OmronFinsClient : IIoT
    {
        private EndianFormat endianFormat;

        /// <summary>
        /// 基础命令
        /// </summary>
        private byte[] BasicCommand = new byte[]
        {
            0x46, 0x49, 0x4E, 0x53,//Magic字段  0x46494E53 对应的ASCII码，即FINS
            0x00, 0x00, 0x00, 0x0C,//Length字段 表示其后所有字段的总长度
            0x00, 0x00, 0x00, 0x00,//Command字段 
            0x00, 0x00, 0x00, 0x00,//Error Code字段
            0x00, 0x00, 0x00, 0x0B //Client/Server Node Address字段
        };

        /// <summary>
        /// DA2(即Destination unit address，目标单元地址)
        /// 0x00：PC(CPU)
        /// 0xFE： SYSMAC NET Link Unit or SYSMAC LINK Unit connected to network；
        /// 0x10~0x1F：CPU总线单元 ，其值等于10 + 单元号(前端面板中配置的单元号)
        /// </summary>
        public byte UnitAddress { get; set; } = 0x00;

        /// <summary>
        /// SA1 客户端节点编号
        /// </summary>
        public byte SA1 { get; set; } = 0x00;

        /// <summary>
        /// DA1 服务器节点编号
        /// </summary>
        private byte DA1 { get; set; } = 0x01;

        public ClientBase Client { get; private set; }//通讯管道

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="format">数据格式</param>
        /// <param name="stationNumber">站号</param>
        public OmronFinsClient(ClientBase client, int timeout = 1500, EndianFormat endianFormat = EndianFormat.CDAB)
        {
            Client = client;
            Client.TimeOut = timeout;
            Client.ReceiveMode = ReceiveMode.ParseByteAll();
            Client.Encoding = Encoding.ASCII;
            Client.ConnectionMode = ConnectionMode.AutoReconnection;
            Client.IsAutoDiscard = true;
            Client.Opened = (a) =>
            {
                BasicCommand[19] = SA1;

                var info1 = Client.SendReceive(BasicCommand);
                if (!info1.IsSucceed)
                {
                    Client.Close();
                    throw new Exception(info1.ErrorText);
                }

                //4-7是Length字段 表示其后所有字段的总长度
                //byte[] buffer = new byte[4];
                //buffer[0] = head[7];
                //buffer[1] = head[6];
                //buffer[2] = head[5];
                //buffer[3] = head[4];
                //var length = BitConverter.ToInt32(buffer, 0);

                // 服务器节点编号
                if (info1.Value.Length >= 24)
                {
                    SA1 = info1.Value[19];
                    DA1 = info1.Value[23];
                }
                else
                {
                    SA1 = 0x0B;

                    if (Client is INetwork network)
                    {
                        //Socket.LocalEndPoint = {[::ffff:127.0.0.1]:60521}
                        var da = network?.Socket?.LocalEndPoint?.ToString()?.Split(']')?.ElementAtOrDefault(0)?.Split('.')?.LastOrDefault() ?? "0";
                        DA1 = Convert.ToByte(da);
                    }
                    else
                    {
                        DA1 = 0x00;
                    }
                }

            };

            this.endianFormat = endianFormat;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口</param>
        /// <param name="format">数据格式</param>
        /// <param name="stationNumber">站号</param>
        public OmronFinsClient(string ip, int port = 1500, int timeout = 1500, EndianFormat endianFormat = EndianFormat.CDAB) : this(new TcpClient(ip, port), timeout, endianFormat) { }

        #region Read
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="length"></param>
        /// <param name="isBit"></param>
        /// <param name="setEndian">返回值是否设置大小端</param>
        /// <returns></returns>
        public IoTResult<byte[]> Read(string address, ushort length, bool isBit = false, bool setEndian = true)
        {
            var result = new IoTResult<byte[]>();
            try
            {
                //发送读取信息
                var arg = ConvertArg(address, isBit: isBit);
                byte[] command = GetReadCommand(arg, length);
                result.Requests.Add(command);
                //发送命令 并获取响应报文
                var sendResult = Client.SendReceive(command);
                if (!sendResult.IsSucceed)
                    return sendResult;
                var dataPackage = sendResult.Value;

                byte[] responseData = new byte[length];
                Array.Copy(dataPackage, dataPackage.Length - length, responseData, 0, length);
                result.Responses.Add(dataPackage);
                if (setEndian)
                    result.Value = responseData.ToArray().ByteFormatting(endianFormat, false);
                else
                    result.Value = responseData.ToArray();
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 读取Boolean
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        public IoTResult<bool> ReadBoolean(string address)
        {
            var readResut = Read(address, 1, isBit: true);
            var result = new IoTResult<bool>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToBoolean(readResut.Value, 0);
            return result.ToEnd();
        }

        private IoTResult<bool> ReadBoolean(int startAddressInt, int addressInt, byte[] values)
        {
            try
            {
                var interval = addressInt - startAddressInt;
                var byteArry = values.Skip(interval * 1).Take(1).ToArray();
                return new IoTResult<bool>
                {
                    Value = BitConverter.ToBoolean(byteArry, 0)
                };
            }
            catch (Exception ex)
            {
                return new IoTResult<bool>().AddError(ex);
            }
        }

        /// <summary>
        /// 读取byte
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public IoTResult<byte> ReadByte(string address)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 读取Int16
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public IoTResult<short> ReadInt16(string address)
        {
            var readResut = Read(address, 2);
            var result = new IoTResult<short>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToInt16(readResut.Value, 0);
            return result.ToEnd();
        }

        private IoTResult<short> ReadInt16(int startAddressInt, int addressInt, byte[] values)
        {
            try
            {
                var interval = addressInt - startAddressInt;
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
        /// 读取UInt16
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        public IoTResult<ushort> ReadUInt16(string address)
        {
            var readResut = Read(address, 2);
            var result = new IoTResult<ushort>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToUInt16(readResut.Value, 0);
            return result.ToEnd();
        }

        private IoTResult<ushort> ReadUInt16(int startAddressInt, int addressInt, byte[] values)
        {
            try
            {
                var interval = addressInt - startAddressInt;
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
        /// 读取Int32
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        public IoTResult<int> ReadInt32(string address)
        {
            var readResut = Read(address, 4);
            var result = new IoTResult<int>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToInt32(readResut.Value, 0);
            return result.ToEnd();
        }

        private IoTResult<int> ReadInt32(int startAddressInt, int addressInt, byte[] values)
        {
            try
            {
                var interval = (addressInt - startAddressInt) / 2;
                var offset = (addressInt - startAddressInt) % 2 * 2;//取余 乘以2（每个地址16位，占两个字节）
                var byteArry = values.Skip(interval * 2 * 2 + offset).Take(2 * 2).ToArray().ByteFormatting(endianFormat, false);
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
        /// 读取UInt32
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        public IoTResult<uint> ReadUInt32(string address)
        {
            var readResut = Read(address, 4);
            var result = new IoTResult<uint>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToUInt32(readResut.Value, 0);
            return result.ToEnd();
        }

        private IoTResult<uint> ReadUInt32(int startAddressInt, int addressInt, byte[] values)
        {
            try
            {
                var interval = (addressInt - startAddressInt) / 2;
                var offset = (addressInt - startAddressInt) % 2 * 2;//取余 乘以2（每个地址16位，占两个字节）
                var byteArry = values.Skip(interval * 2 * 2 + offset).Take(2 * 2).ToArray().ByteFormatting(endianFormat, false);
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
        /// 读取Int64
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        public IoTResult<long> ReadInt64(string address)
        {
            var readResut = Read(address, 8);
            var result = new IoTResult<long>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToInt64(readResut.Value, 0);
            return result.ToEnd();
        }

        private IoTResult<long> ReadInt64(int startAddressInt, int addressInt, byte[] values)
        {
            try
            {
                var interval = (addressInt - startAddressInt) / 4;
                var offset = (addressInt - startAddressInt) % 4 * 2;
                var byteArry = values.Skip(interval * 2 * 4 + offset).Take(2 * 4).ToArray().ByteFormatting(endianFormat, false);
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
        /// 读取UInt64
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        public IoTResult<ulong> ReadUInt64(string address)
        {
            var readResut = Read(address, 8);
            var result = new IoTResult<ulong>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToUInt64(readResut.Value, 0);
            return result.ToEnd();
        }

        private IoTResult<ulong> ReadUInt64(int startAddressInt, int addressInt, byte[] values)
        {
            try
            {
                var interval = (addressInt - startAddressInt) / 4;
                var offset = (addressInt - startAddressInt) % 4 * 2;
                var byteArry = values.Skip(interval * 2 * 4 + offset).Take(2 * 4).ToArray().ByteFormatting(endianFormat, false);
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
        /// 读取Float
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        public IoTResult<float> ReadFloat(string address)
        {
            var readResut = Read(address, 4);
            var result = new IoTResult<float>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToSingle(readResut.Value, 0);
            return result.ToEnd();
        }

        public IoTResult<float> ReadFloat(int beginAddressInt, int addressInt, byte[] values)
        {
            try
            {
                var interval = (addressInt - beginAddressInt) / 2;
                var offset = (addressInt - beginAddressInt) % 2 * 2;//取余 乘以2（每个地址16位，占两个字节）
                var byteArry = values.Skip(interval * 2 * 2 + offset).Take(2 * 2).ToArray().ByteFormatting(endianFormat, false);
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
        /// 读取Double
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        public IoTResult<double> ReadDouble(string address)
        {
            var readResut = Read(address, 8);
            var result = new IoTResult<double>(readResut);
            if (result.IsSucceed)
                result.Value = BitConverter.ToDouble(readResut.Value, 0);
            return result.ToEnd();
        }

        public IoTResult<double> ReadDouble(int beginAddressInt, int addressInt, byte[] values)
        {
            try
            {
                var interval = (addressInt - beginAddressInt) / 4;
                var offset = (addressInt - beginAddressInt) % 4 * 2;
                var byteArry = values.Skip(interval * 2 * 4 + offset).Take(2 * 4).ToArray().ByteFormatting(endianFormat, false);
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
        #endregion

        #region Write

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="data">值</param>
        /// <param name="isBit">值</param>
        /// <returns></returns>
        public IoTResult Write(string address, byte[] data, bool isBit = false)
        {
            IoTResult<byte[]> sendResult = new IoTResult<byte[]>();
            try
            {
                data = data.Reverse().ToArray().ByteFormatting(endianFormat);
                var arg = ConvertArg(address, isBit: isBit);
                byte[] command = GetWriteCommand(arg, data);

                sendResult = Client.SendReceive(command);
                if (!sendResult.IsSucceed)
                    return sendResult;

                if (sendResult.Value.Length <= 0)
                {

                }
            }
            catch (Exception ex)
            {
                sendResult.AddError(ex);
            }
            return sendResult.ToEnd();
        }

        /// <summary>
        /// 写入数据(string专用)
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="data">值</param>
        /// <param name="isBit">值</param>
        /// <returns></returns>
        public IoTResult WriteString(string address, byte[] data, bool isBit = false)
        {
            IoTResult result = new IoTResult();
            try
            {
                //data = data.Reverse.ToArray().ByteFormatting2(endianFormat);
                //发送写入信息
                var arg = ConvertArg(address, isBit: isBit);
                byte[] command = GetWriteCommand(arg, data);

                var sendResult = Client.SendReceive(command);
                if (!sendResult.IsSucceed)
                    return sendResult;

                var dataPackage = sendResult.Value;

            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        public IoTResult Write(string address, byte[] data)
        {
            return Write(address, data, false);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public IoTResult Write(string address, bool value)
        {
            return Write(address, value ? new byte[] { 0x01 } : new byte[] { 0x00 }, true);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public IoTResult Write(string address, byte value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public IoTResult Write(string address, sbyte value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public IoTResult Write(string address, short value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public IoTResult Write(string address, ushort value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public IoTResult Write(string address, int value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public IoTResult Write(string address, uint value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public IoTResult Write(string address, long value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public IoTResult Write(string address, ulong value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public IoTResult Write(string address, float value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public IoTResult Write(string address, double value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }

        #endregion

        /// <summary>
        /// 地址信息解析
        /// </summary>
        /// <param name="address"></param>        
        /// <param name="dataType"></param> 
        /// <param name="isBit"></param> 
        /// <returns></returns>
        private OmronFinsAddress ConvertArg(string address, DataTypeEnum dataType = DataTypeEnum.None, bool isBit = false)
        {
            address = address.ToUpper();
            var addressInfo = new OmronFinsAddress()
            {
                DataTypeEnum = dataType,
                IsBit = isBit
            };
            switch (address[0])
            {
                case 'D'://DM区
                    {
                        addressInfo.BitCode = 0x02;
                        addressInfo.WordCode = 0x82;
                        addressInfo.TypeChar = address.Substring(0, 1);
                        addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1).Split('.')[0]);
                        break;
                    }
                case 'C'://CIO区
                    {
                        addressInfo.BitCode = 0x30;
                        addressInfo.WordCode = 0xB0;
                        addressInfo.TypeChar = address.Substring(0, 1);
                        addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1).Split('.')[0]);
                        break;
                    }
                case 'W'://WR区
                    {
                        addressInfo.BitCode = 0x31;
                        addressInfo.WordCode = 0xB1;
                        addressInfo.TypeChar = address.Substring(0, 1);
                        addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1).Split('.')[0]);
                        break;
                    }
                case 'H'://HR区
                    {
                        addressInfo.BitCode = 0x32;
                        addressInfo.WordCode = 0xB2;
                        addressInfo.TypeChar = address.Substring(0, 1);
                        addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1).Split('.')[0]);
                        break;
                    }
                case 'A'://AR区
                    {
                        addressInfo.BitCode = 0x33;
                        addressInfo.WordCode = 0xB3;
                        addressInfo.TypeChar = address.Substring(0, 1);
                        addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1).Split('.')[0]);
                        break;
                    }
                case 'E':
                    {
                        string[] address_split = address.Split('.');
                        int block_length = Convert.ToInt32(address_split[0].Substring(1), 16);
                        if (block_length < 16)
                        {
                            addressInfo.BitCode = (byte)(0x20 + block_length);
                            addressInfo.WordCode = (byte)(0xA0 + block_length);
                        }
                        else
                        {
                            addressInfo.BitCode = (byte)(0xE0 + block_length - 16);
                            addressInfo.WordCode = (byte)(0x60 + block_length - 16);
                        }

                        if (isBit)
                        {
                            // 位操作
                            ushort address_location = ushort.Parse(address_split[1]);
                            addressInfo.BitAddress = new byte[3];
                            addressInfo.BitAddress[0] = BitConverter.GetBytes(address_location)[1];
                            addressInfo.BitAddress[1] = BitConverter.GetBytes(address_location)[0];

                            if (address_split.Length > 2)
                            {
                                addressInfo.BitAddress[2] = byte.Parse(address_split[2]);
                                if (addressInfo.BitAddress[2] > 15)
                                    //输入的位地址只能在0-15之间
                                    throw new Exception("位地址数据异常");
                            }
                        }
                        else
                        {
                            // 字操作
                            ushort address_location = ushort.Parse(address_split[1]);
                            addressInfo.BitAddress = new byte[3];
                            addressInfo.BitAddress[0] = BitConverter.GetBytes(address_location)[1];
                            addressInfo.BitAddress[1] = BitConverter.GetBytes(address_location)[0];
                        }
                        break;
                    }
                default:
                    //类型不支持
                    throw new Exception("Address解析异常");
            }

            if (address[0] != 'E')
            {
                if (isBit)
                {
                    // 位操作
                    string[] address_split = address.Substring(1).Split('.');
                    ushort address_location = ushort.Parse(address_split[0]);
                    addressInfo.BitAddress = new byte[3];
                    addressInfo.BitAddress[0] = BitConverter.GetBytes(address_location)[1];
                    addressInfo.BitAddress[1] = BitConverter.GetBytes(address_location)[0];

                    if (address_split.Length > 1)
                    {
                        addressInfo.BitAddress[2] = byte.Parse(address_split[1]);
                        if (addressInfo.BitAddress[2] > 15)
                            //输入的位地址只能在0-15之间
                            throw new Exception("位地址数据异常");
                    }
                }
                else
                {
                    // 字操作
                    ushort address_location = ushort.Parse(address.Substring(1));
                    addressInfo.BitAddress = new byte[3];
                    addressInfo.BitAddress[0] = BitConverter.GetBytes(address_location)[1];
                    addressInfo.BitAddress[1] = BitConverter.GetBytes(address_location)[0];
                }
            }

            return addressInfo;
        }

        /// <summary>
        /// 获取Read命令
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected byte[] GetReadCommand(OmronFinsAddress arg, ushort length)
        {
            bool isBit = arg.IsBit;

            if (!isBit) length = (ushort)(length / 2);

            byte[] command = new byte[26 + 8];

            Array.Copy(BasicCommand, 0, command, 0, 4);
            byte[] tmp = BitConverter.GetBytes(command.Length - 8);
            Array.Reverse(tmp);
            tmp.CopyTo(command, 4);
            command[11] = 0x02;

            command[16] = 0x80; //ICF 信息控制字段
            command[17] = 0x00; //RSV 保留字段
            command[18] = 0x02; //GCT 网关计数
            command[19] = 0x00; //DNA 目标网络地址 00:表示本地网络  0x01~0x7F:表示远程网络
            command[20] = DA1; //DA1 目标节点编号 0x01~0x3E:SYSMAC LINK网络中的节点号 0x01~0x7E:YSMAC NET网络中的节点号 0xFF:广播传输
            command[21] = UnitAddress; //DA2 目标单元地址
            command[22] = 0x00; //SNA 源网络地址 取值及含义同DNA字段
            command[23] = SA1; //SA1 源节点编号 取值及含义同DA1字段
            command[24] = 0x00; //SA2 源单元地址 取值及含义同DA2字段
            command[25] = 0x00; //SID Service ID 取值0x00~0xFF，产生会话的进程的唯一标识

            command[26] = 0x01;
            command[27] = 0x01; //Command Code 内存区域读取
            command[28] = isBit ? arg.BitCode : arg.WordCode;
            arg.BitAddress.CopyTo(command, 29);
            command[32] = (byte)(length / 256);
            command[33] = (byte)(length % 256);

            return command;
        }

        /// <summary>
        /// 获取Write命令
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected byte[] GetWriteCommand(OmronFinsAddress arg, byte[] value)
        {
            bool isBit = arg.IsBit;
            byte[] command = new byte[26 + 8 + value.Length];

            Array.Copy(BasicCommand, 0, command, 0, 4);
            byte[] tmp = BitConverter.GetBytes(command.Length - 8);
            Array.Reverse(tmp);
            tmp.CopyTo(command, 4);
            command[11] = 0x02;

            command[16] = 0x80; //ICF 信息控制字段
            command[17] = 0x00; //RSV 保留字段
            command[18] = 0x02; //GCT 网关计数
            command[19] = 0x00; //DNA 目标网络地址 00:表示本地网络  0x01~0x7F:表示远程网络
            command[20] = DA1; //DA1 目标节点编号 0x01~0x3E:SYSMAC LINK网络中的节点号 0x01~0x7E:YSMAC NET网络中的节点号 0xFF:广播传输
            command[21] = UnitAddress; //DA2 目标单元地址
            command[22] = 0x00; //SNA 源网络地址 取值及含义同DNA字段
            command[23] = SA1; //SA1 源节点编号 取值及含义同DA1字段
            command[24] = 0x00; //SA2 源单元地址 取值及含义同DA2字段
            command[25] = 0x00; //SID Service ID 取值0x00~0xFF，产生会话的进程的唯一标识

            command[26] = 0x01;
            command[27] = 0x02; //Command Code 内存区域写入
            command[28] = isBit ? arg.BitCode : arg.WordCode;
            arg.BitAddress.CopyTo(command, 29);
            command[32] = isBit ? (byte)(value.Length / 256) : (byte)(value.Length / 2 / 256);
            command[33] = isBit ? (byte)(value.Length % 256) : (byte)(value.Length / 2 % 256);
            value.CopyTo(command, 34);

            return command;
        }

        /// <summary>
        /// 批量读取
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="batchNumber">此参数设置无实际效果</param>
        /// <returns></returns>
        public IoTResult<Dictionary<string, object>> BatchRead(Dictionary<string, DataTypeEnum> addresses, int batchNumber)
        {
            var result = new IoTResult<Dictionary<string, object>>();
            result.Value = new Dictionary<string, object>();

            var omronFinsAddresses = addresses.Select(t => ConvertArg(t.Key, t.Value)).ToList();
            var typeChars = omronFinsAddresses.Select(t => t.TypeChar).Distinct();
            foreach (var typeChar in typeChars)
            {
                var tempAddresses = omronFinsAddresses.Where(t => t.TypeChar == typeChar).ToList();
                var minAddress = tempAddresses.Select(t => t.BeginAddress).Min();
                var maxAddress = tempAddresses.Select(t => t.BeginAddress).Max();

                while (maxAddress >= minAddress)
                {
                    int readLength = 121;//TODO 分批读取的长度还可以继续调大

                    var tempAddress = tempAddresses.Where(t => t.BeginAddress >= minAddress && t.BeginAddress <= minAddress + readLength).ToList();
                    //如果范围内没有数据。按正确逻辑不存在这种情况。
                    if (!tempAddress.Any())
                    {
                        minAddress = minAddress + readLength;
                        continue;
                    }

                    var tempMax = tempAddress.OrderByDescending(t => t.BeginAddress).FirstOrDefault();
                    switch (tempMax.DataTypeEnum)
                    {
                        case DataTypeEnum.Bool:
                            throw new Exception("暂时不支持Bool类型批量读取");
                        case DataTypeEnum.Byte:
                            throw new Exception("暂时不支持Byte类型批量读取");
                        //readLength = tempMax.BeginAddress + 1 - minAddress;
                        //break;
                        case DataTypeEnum.Int16:
                        case DataTypeEnum.UInt16:
                            readLength = tempMax.BeginAddress * 2 + 2 - minAddress * 2;
                            break;
                        case DataTypeEnum.Int32:
                        case DataTypeEnum.UInt32:
                        case DataTypeEnum.Float:
                            readLength = tempMax.BeginAddress * 2 + 4 - minAddress * 2;
                            break;
                        case DataTypeEnum.Int64:
                        case DataTypeEnum.UInt64:
                        case DataTypeEnum.Double:
                            readLength = tempMax.BeginAddress * 2 + 8 - minAddress * 2;
                            break;
                        default:
                            throw new Exception("Err BatchRead 未定义类型 -1");
                    }

                    var tempResult = Read(typeChar + minAddress.ToString(), Convert.ToUInt16(readLength), false, setEndian: false);

                    if (!tempResult.IsSucceed)
                    {
                        return result.AddError(tempResult.Error).ToEnd();
                    }

                    var rValue = tempResult.Value.ToArray();
                    foreach (var item in tempAddress)
                    {
                        object tempVaue = null;

                        switch (item.DataTypeEnum)
                        {
                            case DataTypeEnum.Bool:
                                tempVaue = ReadBoolean(minAddress, item.BeginAddress, rValue).Value;
                                break;
                            case DataTypeEnum.Byte:
                                throw new Exception("Err BatchRead 未定义类型 -2");
                            case DataTypeEnum.Int16:
                                tempVaue = ReadInt16(minAddress, item.BeginAddress, rValue).Value;
                                break;
                            case DataTypeEnum.UInt16:
                                tempVaue = ReadUInt16(minAddress, item.BeginAddress, rValue).Value;
                                break;
                            case DataTypeEnum.Int32:
                                tempVaue = ReadInt32(minAddress, item.BeginAddress, rValue).Value;
                                break;
                            case DataTypeEnum.UInt32:
                                tempVaue = ReadUInt32(minAddress, item.BeginAddress, rValue).Value;
                                break;
                            case DataTypeEnum.Int64:
                                tempVaue = ReadInt64(minAddress, item.BeginAddress, rValue).Value;
                                break;
                            case DataTypeEnum.UInt64:
                                tempVaue = ReadUInt64(minAddress, item.BeginAddress, rValue).Value;
                                break;
                            case DataTypeEnum.Float:
                                tempVaue = ReadFloat(minAddress, item.BeginAddress, rValue).Value;
                                break;
                            case DataTypeEnum.Double:
                                tempVaue = ReadDouble(minAddress, item.BeginAddress, rValue).Value;
                                break;
                            default:
                                throw new Exception("Err BatchRead 未定义类型 -3");
                        }

                        result.Value.Add(item.TypeChar + item.BeginAddress.ToString(), tempVaue);
                    }
                    minAddress = minAddress + readLength / 2;

                    if (tempAddresses.Any(t => t.BeginAddress >= minAddress))
                        minAddress = tempAddresses.Where(t => t.BeginAddress >= minAddress).OrderBy(t => t.BeginAddress).FirstOrDefault().BeginAddress;
                }
            }
            return result.ToEnd();
        }

        public IoTResult BatchWrite(Dictionary<string, object> addresses, int batchNumber)
        {
            throw new NotImplementedException();
        }

        #region IIoTBase
        public IoTResult<T> Read<T>(string address)
        {
            var tType = typeof(T);
            if (tType == typeof(bool))
            {
                var readResut = ReadBoolean(address);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(byte))
            {
                var readResut = ReadByte(address);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(float))
            {
                var readResut = ReadFloat(address);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(double))
            {
                var readResut = ReadDouble(address);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(short))
            {
                var readResut = ReadInt16(address);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(int))
            {
                var readResut = ReadInt32(address);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(long))
            {
                var readResut = ReadInt64(address);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(ushort))
            {
                var readResut = ReadUInt16(address);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(uint))
            {
                var readResut = ReadUInt32(address);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else if (tType == typeof(ulong))
            {
                var readResut = ReadUInt64(address);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            else
            {
                throw new NotImplementedException("暂不支持的类型");
            }
        }

        public IoTResult<string> ReadString(string address, int length, Encoding encoding = null)
        {
            try
            {
                encoding = encoding ?? Encoding.ASCII;

                var readResut = Read(address, Convert.ToUInt16(length), false, false);
                var result = new IoTResult<string>(readResut);
                if (result.IsSucceed)
                {
                    result.Value = encoding.GetString(readResut.Value);
                    //高低置位
                    string High_Data = string.Empty;
                    string Low_Data = string.Empty;
                    string Allstring = string.Empty;
                    if (result.Value.Length > 0)
                    {
                        for (int i = 0; i < result.Value.Length / 2; i++)
                        {
                            High_Data = result.Value.Substring(i * 2, 1);
                            Low_Data = result.Value.Substring(i * 2 + 1, 1);
                            Allstring += Low_Data;
                            Allstring += High_Data;
                        }
                    }
                    result.Value = Allstring;
                }

                return result.ToEnd();
            }
            catch (Exception ex)
            {
                throw new Exception("ReadString 出错" + ex.Message);
            }
        }

        public IoTResult<IEnumerable<T>> Read<T>(string address, int number)
        {
            var tType = typeof(T);
            if (tType == typeof(byte))
            {
                var readResut = Read(address, Convert.ToUInt16(number));
                return new IoTResult<IEnumerable<T>>(readResut, (IEnumerable<T>)(object)readResut.Value);
            }
            else
            {
                throw new NotImplementedException("暂不支持的类型");
            }
        }

        public IoTResult Write<T>(string address, T value)
        {
            if (value is bool boolv)
            {
                return Write(address, boolv);
            }
            else if (value is byte bytev)
            {
                return Write(address, bytev);
            }
            else if (value is sbyte sbytev)
            {
                return Write(address, sbytev);
            }
            else if (value is float floatv)
            {
                return Write(address, floatv);
            }
            else if (value is double doublev)
            {
                return Write(address, doublev);
            }
            else if (value is short Int16v)
            {
                return Write(address, Int16v);
            }
            else if (value is int Int32v)
            {
                return Write(address, Int32v);
            }
            else if (value is long Int64v)
            {
                return Write(address, Int64v);
            }
            else if (value is ushort UInt16v)
            {
                return Write(address, UInt16v);
            }
            else if (value is uint UInt32v)
            {
                return Write(address, UInt32v);
            }
            else if (value is ulong UInt64v)
            {
                return Write(address, UInt64v);
            }
            else
            {
                throw new NotImplementedException("暂不支持的类型");
            }
        }

        public IoTResult WriteString(string address, string value, int length, Encoding encoding = null)
        {
            try
            {
                encoding = encoding ?? Encoding.ASCII;

                //高低置位
                string High_Data = string.Empty;
                string Low_Data = string.Empty;
                string Allstring = string.Empty;

                if (value.Length > 0)
                {
                    for (int i = 0; i < value.Length / 2; i++)
                    {
                        High_Data = value.Substring(i * 2, 1);
                        Low_Data = value.Substring(i * 2 + 1, 1);
                        Allstring += Low_Data;
                        Allstring += High_Data;
                    }
                }

                return WriteString(address, encoding.GetBytes(Allstring), false);
            }
            catch (Exception ex)
            {
                return new IoTResult().AddError(ex);
            }
        }

        public IoTResult Write<T>(string address, params T[] value)
        {
            if (value is byte[] bytev)
            {
                return Write(address, bytev, false);
            }
            else
            {
                throw new NotImplementedException("暂不支持的类型");
            }
        }

        #endregion
    }
}
