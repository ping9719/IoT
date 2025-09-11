using Ping9719.IoT.Common;
using Ping9719.IoT.Communication;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Ping9719.IoT.PLC
{
    /// <summary>
    /// 欧姆龙客户端（Cip协议）
    /// </summary>
    public class OmronCipClient : ReadWriteBase, IClientData
    {
        /// <summary>
        /// 插槽
        /// </summary>
        public readonly byte Slot;
        //object objLock = new object();

        /// <summary>
        /// 字符串编码格式。默认ASCII
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        byte[] BoolTrueByteVal = new byte[] { 0x01, 0x00 };

        public ClientBase Client { get; private set; }
        public OmronCipClient(ClientBase client, byte slot = 0)
        {
            Client = client;
            //Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.UTF8;
            //Client.TimeOut = timeout;
            //Client.ConnectionMode = ConnectionMode.AutoReconnection;
            Client.IsAutoDiscard = true;
            Client.Opened = (a) =>
            {
                //注册命令
                byte[] RegisteredCommand = new byte[] {
                    0x65,0x00,//注册请求
                    0x04,0x00,//命令数据长度(单位字节)
                    0x00,0x00,0x00,0x00,//会话句柄,初始值为0x00000000
                    0x00,0x00,0x00,0x00,//状态，初始值为0x00000000（状态好）
                    0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,//请求通信一方的说明
                    0x00,0x00,0x00,0x00,//选项，默认为0x00000000
                    0x01,0x00,//协议版本（0x0001）
                    0x00,0x00,//选项标记（0x0000
                    };

                var socketReadResul = Client.SendReceive(RegisteredCommand);
                if (!socketReadResul.IsSucceed || socketReadResul.Value == null || socketReadResul.Value.Length < 8)
                {
                    SessionByte[0] = 0;
                    SessionByte[1] = 0;
                    SessionByte[2] = 0;
                    SessionByte[3] = 0;
                    Client.Close();
                    throw new Exception("打开cip获取会话句柄失败。");
                }

                var response = socketReadResul.Value;
                //会话句柄
                SessionByte[0] = response[4];
                SessionByte[1] = response[5];
                SessionByte[2] = response[6];
                SessionByte[3] = response[7];

            };
            Client.Closing = (a) =>
            {
                try
                {
                    byte[] command = new byte[] { 0x66, 0x00, 0x00, 0x00, SessionByte[0], SessionByte[1], SessionByte[2], SessionByte[3], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    Client.Send(command);
                }
                catch (Exception)
                {

                    throw;
                }
                return true;
            };

            Slot = slot;
        }
        public OmronCipClient(string ip, int port = 44818, byte slot = 0) : this(new TcpClient(ip, port), slot) { }

        /// <summary>
        /// 会话句柄
        /// </summary>
        public readonly byte[] SessionByte = new byte[] { 0, 0, 0, 0 };

        /// <summary>
        /// 会话句柄(由AB PLC生成)
        /// </summary>
        public uint Session => BitConverter.ToUInt32(SessionByte, 0);

        #region Read
        public IoTResult<IEnumerable<object>> Read(string address, Encoding encoding = null)
        {
            var result = new IoTResult<IEnumerable<object>>();
            try
            {
                //lock (objLock)
                {
                    var cpiadd = AllenBradleyAddress.Parse(address);
                    var command = GetReadCommand(cpiadd);

                    //发送命令 并获取响应报文
                    var sendResult = Client.SendReceive(command);
                    if (!sendResult.IsSucceed)
                        return sendResult.ToVal<IEnumerable<object>>();
                    var dataPackage = sendResult.Value;

                    result = sendResult.ToVal<IEnumerable<object>>();
                    try
                    {
                        //解析
                        var count = BitConverter.ToUInt16(dataPackage, 38);//数据总长度
                        var isok = BitConverter.ToUInt16(dataPackage, 42);//合格
                        if (isok != 0)
                        {

                            result.AddError("读取失败，错误代码：" + isok);
                            return result;
                        }

                        var dTypt = (CipVariableType)BitConverter.ToUInt16(dataPackage, 44);//类型
                        var data = dataPackage.Skip(46).Take(count - 6).ToArray();//数据
                        if (dTypt == CipVariableType.BOOL)
                        {
                            result.Value = DataConvert.ByteToBinaryBoolArray(data).Select(o => (object)o);
                        }
                        else if (dTypt == CipVariableType.BYTE)
                        {
                            result.Value = data.Select(o => (object)o);
                        }
                        else if (dTypt == CipVariableType.REAL)
                        {
                            result.Value = data.Chunk(4).Select(o => (object)BitConverter.ToSingle(o.ToArray(), 0));
                        }
                        else if (dTypt == CipVariableType.LREAL)
                        {
                            result.Value = data.Chunk(8).Select(o => (object)BitConverter.ToDouble(o.ToArray(), 0));
                        }
                        else if (dTypt == CipVariableType.INT)
                        {
                            result.Value = data.Chunk(2).Select(o => (object)BitConverter.ToInt16(o.ToArray(), 0));
                        }
                        else if (dTypt == CipVariableType.DINT)
                        {
                            result.Value = data.Chunk(4).Select(o => (object)BitConverter.ToInt32(o.ToArray(), 0));
                        }
                        else if (dTypt == CipVariableType.LINT)
                        {
                            result.Value = data.Chunk(8).Select(o => (object)BitConverter.ToInt64(o.ToArray(), 0));
                        }
                        else if (dTypt == CipVariableType.UINT)
                        {
                            result.Value = data.Chunk(2).Select(o => (object)BitConverter.ToUInt16(o.ToArray(), 0));
                        }
                        else if (dTypt == CipVariableType.UDINT)
                        {
                            result.Value = data.Chunk(4).Select(o => (object)BitConverter.ToUInt32(o.ToArray(), 0));
                        }
                        else if (dTypt == CipVariableType.ULINT)
                        {
                            result.Value = data.Chunk(8).Select(o => (object)BitConverter.ToUInt64(o.ToArray(), 0));
                        }
                        else if (dTypt == CipVariableType.STRING)
                        {
                            encoding ??= Encoding;
                            var sl = BitConverter.ToUInt16(data, 0);
                            if (sl > 0)
                                result.Value = new string[] { encoding.GetString(data, 2, sl) };
                            else
                                result.Value = new string[] { };
                        }
                        else if (dTypt == CipVariableType.DATE_AND_TIME_NSEC)
                        {
                            result.Value = data.Chunk(8).Select(o => (object)DateTime.FromBinary(BitConverter.ToInt64(o.ToArray(), 0) / 100).AddYears(1969));
                        }
                        else
                        {

                            result.AddError("不支持的类型" + dTypt.ToString());
                            return result;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.AddError("解析失败" + ex.Message);
                        return result;
                    }

                }

            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 获取Read命令
        /// </summary>
        /// <param name="address"></param>
        /// <param name="slot"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected byte[] GetReadCommand(AllenBradleyAddress address)
        {
            byte[] aadd;
            {
                byte[] Header = new byte[24]
                {
                    0x6F,0x00,//命令 2byte
　　                0x28,0x00,//长度 2byte（总长度-Header的长度）=40 
　　                SessionByte[0],SessionByte[1],SessionByte[2],SessionByte[3],//会话句柄 4byte
　　                0x00,0x00,0x00,0x00,//状态默认0 4byte
　　                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,//发送方描述默认0 8byte
　　                0x00,0x00,0x00,0x00,//选项默认0 4byte
                };
                byte[] CommandSpecificData = new byte[]
                {
                    0x00,0x00,0x00,0x00,//接口句柄 CIP默认为0x00000000 4byte
　　                0x0A,0x00,//超时默认0x0001 4byte
　　                0x02,0x00,//项数默认0x0002 4byte
　　                0x00,0x00,//空地址项默认0x0000 2byte
　　                0x00,0x00,//长度默认0x0000 2byte
　　                0xb2,0x00,//未连接数据项默认为 0x00b2
　　                0x00,0x00,//后面数据包的长度 24个字节(总长度-Header的长度-CommandSpecificData的长度)
                    //以下是cip
                    0x52,0x02,//服务默认0x52  请求路径大小 默认2
                    0x20,0x06,0x24,0x01,//请求路径 默认0x01240622 4byte
                    0x0A,0xF0,//超时默认0xF00A 4byte
                };
                List<byte> CipMessage = new List<byte>()
                {
                    0x0A,0x00,//Cip指令长度  服务标识到服务命令指定数据的长度 
                    0x4C,//服务标识固定为0x4C 1byte  
                    0x03,// 节点长度 2byte  规律为 (标签名的长度+1/2)+1
                    //0x91,//扩展符号 默认为 0x91
                    //0x04,//标签名的长度
                    //0x54,0x41,0x47,0x31,//标签名 ：TAG1转换成ASCII字节 当标签名的长度为奇数时，需要在末尾补0  比如TAG转换成ASCII为0x54,0x41,0x47，需要在末尾补0 变成 0x54,0x41,0x47，0
                    //0x01,0x00,//服务命令指定数据　默认为0x0001
                    //0x01,0x00,0x01,0x00//最后一位是PLC的槽号
                };
                //补充数据
                var cipmess = address.GetCip(Encoding);
                CipMessage.AddRange(cipmess);//标签名
                CipMessage.AddRange(new byte[] { 0x01, 0x00 });//服务命令指定数据
                CipMessage.AddRange(new byte[] { 0x01, 0x00, 0x01, Slot });//服务命令指定数据

                //长度
                Header[2] = BitConverter.GetBytes((short)(CommandSpecificData.Length + CipMessage.Count))[0];
                Header[3] = BitConverter.GetBytes((short)(CommandSpecificData.Length + CipMessage.Count))[1];
                CommandSpecificData[14] = BitConverter.GetBytes((short)CipMessage.Count + 8)[0];
                CommandSpecificData[15] = BitConverter.GetBytes((short)CipMessage.Count + 8)[1];
                CipMessage[0] = BitConverter.GetBytes((short)(cipmess.Length + 4))[0];
                CipMessage[1] = BitConverter.GetBytes((short)(cipmess.Length + 4))[1];
                CipMessage[3] = (byte)(cipmess.Length / 2);

                aadd = Header.Concat(CommandSpecificData).Concat(CipMessage).ToArray();
            }

            var aaaa = string.Join(" ", aadd.Select(t => t.ToString("X2")));
            return aadd;
        }
        #endregion

        #region Write
        public IoTResult Write(string address, CipVariableType typeCode, byte[] data, ushort num = 1, Encoding encoding = null)
        {
            IoTResult result = new IoTResult();
            try
            {
                //lock (objLock)
                {
                    var abadd = AllenBradleyAddress.Parse(address);
                    byte[] command = GetWriteCommand(abadd, typeCode, data, num);

                    var sendResult = Client.SendReceive(command);
                    if (!sendResult.IsSucceed)
                        return sendResult;

                    var dataPackage = sendResult.Value;


                    if (dataPackage.Length < 43)
                    {

                        result.AddError("写入失败，数据长度验证失败");
                        return result;
                    }
                    var isok = BitConverter.ToUInt16(dataPackage, 42);//合格
                    if (isok != 0)
                    {

                        result.AddError("写入失败，错误代码：" + isok);
                        return result;
                    }
                }

            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        protected byte[] GetWriteCommand(AllenBradleyAddress address, CipVariableType typeCode, byte[] value, ushort num)
        {
            byte[] aadd;
            {
                //6F 00 34 00 74 01 0B 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
                //00 00 00 00 0A 00 02 00 00 00 00 00 B2 00 24 00 52 02 20 06 24 01 0A F0
                //16 00 4D 05 91 07 E6 B5 8B E8 AF 95 35 00 D0 00 01 00 03 00 61 2E 62 00 01 00 01 00

                //6F 00 34 00 24 C3 29 19 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
                //00 00 00 00 0A 00 02 00 00 00 00 00 B2 00 24 00 52 02 20 06 24 01 0A F0
                //16 00 4D 05 91 07 E6 B5 8B E8 AF 95 35 00 D0 00 01 00 04 00 61 2E 62 00 01 00 01 00
                value = (value.Length % 2 == 0 ? value : value.Concat(new byte[] { 0 })).ToArray();
                byte[] Header = new byte[24]
                {
                    0x6F,0x00,//命令 2byte
　　                0x00,0x00,//长度 2byte（总长度-Header的长度）=40 
　　                SessionByte[0],SessionByte[1],SessionByte[2],SessionByte[3],//会话句柄 4byte
　　                0x00,0x00,0x00,0x00,//状态默认0 4byte
　　                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,//发送方描述默认0 8byte
　　                0x00,0x00,0x00,0x00,//选项默认0 4byte
                };
                byte[] CommandSpecificData = new byte[]
                {
                    0x00,0x00,0x00,0x00,//接口句柄 CIP默认为0x00000000 4byte
　　                0x0A,0x00,//超时默认0x0001 4byte
　　                0x02,0x00,//项数默认0x0002 4byte
　　                0x00,0x00,//空地址项默认0x0000 2byte
　　                0x00,0x00,//长度默认0x0000 2byte
　　                0xB2,0x00,//未连接数据项默认为 0x00b2
　　                0x00,0x00,//后面数据包的长度 24个字节(总长度-Header的长度-CommandSpecificData的长度)
                    //以下为cip
                    0x52,0x02,//服务默认0x52  请求路径大小 默认2
                    0x20,0x06,0x24,0x01,//请求路径 默认0x01240622 4byte
                    0x0A,0xF0,//超时默认0xF00A 4byte
                };

                List<byte> CipMessage = new List<byte>()
                {
                    0x00,0x00,//Cip指令长度  服务标识到服务命令指定数据的长度 
                    0x4D,//服务标识固定为0x4C 1byte  
                    0x00,// 节点长度 2byte  规律为 (标签名的长度+1/2)+1
                    //0x91,//扩展符号 默认为 0x91
                    //0x04,//标签名的长度
                    //0x54,0x41,0x47,0x31,//标签名 ：TAG1转换成ASCII字节 当标签名的长度为奇数时，需要在末尾补0  比如TAG转换成ASCII为0x54,0x41,0x47，需要在末尾补0 变成 0x54,0x41,0x47，0
                    //0x01,0x00,//服务命令指定数据　默认为0x0001
                    //0x01,0x00,0x01,0x00//最后一位是PLC的槽号
                };
                //补充数据
                var cipmess = address.GetCip(Encoding);
                CipMessage.AddRange(cipmess);//标签名
                CipMessage.AddRange(new byte[] { (byte)typeCode, 0 });//数据类型
                CipMessage.AddRange(BitConverter.GetBytes(num));//写入数量
                CipMessage.AddRange(value);//数据
                CipMessage.AddRange(new byte[] { 0x01, 0x00, 0x01, Slot });//服务命令指定数据

                //长度
                Header[2] = BitConverter.GetBytes((short)(CommandSpecificData.Length + CipMessage.Count))[0];
                Header[3] = BitConverter.GetBytes((short)(CommandSpecificData.Length + CipMessage.Count))[1];
                CommandSpecificData[14] = BitConverter.GetBytes((short)CipMessage.Count + 8)[0];
                CommandSpecificData[15] = BitConverter.GetBytes((short)CipMessage.Count + 8)[1];
                CipMessage[0] = BitConverter.GetBytes((short)(CipMessage.Count - 6))[0];
                CipMessage[1] = BitConverter.GetBytes((short)(CipMessage.Count - 6))[1];
                CipMessage[3] = (byte)(cipmess.Length / 2);

                aadd = Header.Concat(CommandSpecificData).Concat(CipMessage).ToArray();
            }
            return aadd;
        }
        #endregion

        #region IIoTBase
        public override IoTResult<T> Read<T>(string address)
        {
            var aaa = Read(address);
            if (!aaa.IsSucceed)
                return new IoTResult<T>(aaa);

            try
            {
                if (typeof(T).IsArray)
                {
                    var eType = typeof(T).GetElementType();
                    if (eType == typeof(bool))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (bool)o).ToArray());
                    else if (eType == typeof(byte))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (byte)o).ToArray());
                    else if (eType == typeof(float))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (float)o).ToArray());
                    else if (eType == typeof(double))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (double)o).ToArray());
                    else if (eType == typeof(short))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (short)o).ToArray());
                    else if (eType == typeof(int))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (int)o).ToArray());
                    else if (eType == typeof(long))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (long)o).ToArray());
                    else if (eType == typeof(ushort))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (ushort)o).ToArray());
                    else if (eType == typeof(uint))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (uint)o).ToArray());
                    else if (eType == typeof(ulong))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (ulong)o).ToArray());
                    //else if (eType == typeof(string))
                    //    return new IoTResult<T>(aaa, (T)(object)(aaa.Value.Select(o => (string)o).ToArray()));
                    else if (eType == typeof(DateTime))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (DateTime)o).ToArray());
                    else
                        return new IoTResult<T>(aaa).AddError("此类型不支持数组");
                }
                else if (typeof(ICollection).IsAssignableFrom(typeof(T)) && typeof(T) != typeof(string))
                {
                    var eType = typeof(T).GetGenericArguments().First();
                    if (eType == typeof(bool))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (bool)o).ToList());
                    else if (eType == typeof(byte))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (byte)o).ToList());
                    else if (eType == typeof(float))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (float)o).ToList());
                    else if (eType == typeof(double))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (double)o).ToList());
                    else if (eType == typeof(short))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (short)o).ToList());
                    else if (eType == typeof(int))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (int)o).ToList());
                    else if (eType == typeof(long))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (long)o).ToList());
                    else if (eType == typeof(ushort))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (ushort)o).ToList());
                    else if (eType == typeof(uint))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (uint)o).ToList());
                    else if (eType == typeof(ulong))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (ulong)o).ToList());
                    //else if (eType == typeof(string))
                    //    return new IoTResult<T>(aaa, (T)(object)(aaa.Value.Select(o => (string)o).ToArray()));
                    else if (eType == typeof(DateTime))
                        return new IoTResult<T>(aaa, (T)(object)aaa.Value.Select(o => (DateTime)o).ToList());
                    else
                        return new IoTResult<T>(aaa).AddError("此类型不支持集合");
                }
                else if (typeof(T) == typeof(string))
                {
                    return new IoTResult<T>(aaa, (T)(aaa.Value.FirstOrDefault() ?? string.Empty));
                }

                return new IoTResult<T>(aaa, (T)aaa.Value.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return aaa.AddError(ex).ToVal<T>();
            }

        }

        public override IoTResult<string> ReadString(string address, int length, Encoding encoding)
        {
            var aaa = Read(address, encoding);
            return aaa.IsSucceed ? new IoTResult<string>(aaa, aaa.Value.Cast<string>().FirstOrDefault()) : new IoTResult<string>(aaa);
        }

        public override IoTResult<IEnumerable<T>> Read<T>(string address, int number)
        {
            var aaa = Read(address);
            if (!aaa.IsSucceed)
                return new IoTResult<IEnumerable<T>>(aaa);

            return number >= 0 ? new IoTResult<IEnumerable<T>>(aaa, aaa.Value.Cast<T>().Take(number)) : new IoTResult<IEnumerable<T>>(aaa, aaa.Value.Cast<T>());
        }

        public override IoTResult Write<T>(string address, T value)
        {
            try
            {
                if (value is bool boolv)
                {
                    return Write(address, CipVariableType.BOOL, boolv ? BoolTrueByteVal : new byte[] { 0x00, 0x00 });
                }
                else if (value is byte bytev)
                {
                    return Write(address, CipVariableType.BYTE, new byte[] { bytev, 0x00 });
                }
                else if (value is float Singlev)
                {
                    return Write(address, CipVariableType.REAL, BitConverter.GetBytes(Singlev));
                }
                else if (value is double doublev)
                {
                    return Write(address, CipVariableType.LREAL, BitConverter.GetBytes(doublev));
                }
                else if (value is short Int16v)
                {
                    return Write(address, CipVariableType.INT, BitConverter.GetBytes(Int16v));
                }
                else if (value is int Int32v)
                {
                    return Write(address, CipVariableType.DINT, BitConverter.GetBytes(Int32v));
                }
                else if (value is long Int64v)
                {
                    return Write(address, CipVariableType.LINT, BitConverter.GetBytes(Int64v));
                }
                else if (value is ushort UInt16v)
                {
                    return Write(address, CipVariableType.UINT, BitConverter.GetBytes(UInt16v));
                }
                else if (value is uint UInt32v)
                {
                    return Write(address, CipVariableType.UDINT, BitConverter.GetBytes(UInt32v));
                }
                else if (value is ulong UInt64v)
                {
                    return Write(address, CipVariableType.ULINT, BitConverter.GetBytes(UInt64v));
                }
                else if (value is string stringv)
                {
                    return Write(address, CipVariableType.STRING, GetStringByte(stringv));
                }
                else if (value is DateTime DateTimev)
                {
                    return Write(address, CipVariableType.DATE_AND_TIME_NSEC, GetDateTimeByte(DateTimev));
                }
                else
                    throw new NotImplementedException("暂不支持的类型");
            }
            catch (Exception ex)
            {
                return new IoTResult().AddError(ex).ToEnd();
            }
        }

        public override IoTResult WriteString(string address, string value, int length, Encoding encoding)
        {
            return Write(address, CipVariableType.STRING, GetStringByte(value, encoding), 1, encoding);
        }

        public override IoTResult Write<T>(string address, IEnumerable<T> value)
        {
            if (value is IEnumerable<bool> boolv)
            {
                return Write(address, CipVariableType.BOOL, boolv.SelectMany(o => o ? BoolTrueByteVal.ToList() : new List<byte> { 0x00, 0x00 }).ToArray(), (ushort)value.Count());
            }
            else if (value is IEnumerable<byte> bytev)
            {
                return Write(address, CipVariableType.BYTE, bytev.ToArray(), (ushort)value.Count());
            }
            else if (value is IEnumerable<float> Singlev)
            {
                return Write(address, CipVariableType.REAL, Singlev.SelectMany(o => BitConverter.GetBytes(o)).ToArray(), (ushort)value.Count());
            }
            else if (value is IEnumerable<double> doublev)
            {
                return Write(address, CipVariableType.LREAL, doublev.SelectMany(o => BitConverter.GetBytes(o)).ToArray(), (ushort)value.Count());
            }
            else if (value is IEnumerable<short> Int16v)
            {
                return Write(address, CipVariableType.INT, Int16v.SelectMany(o => BitConverter.GetBytes(o)).ToArray(), (ushort)value.Count());
            }
            else if (value is IEnumerable<int> Int32v)
            {
                return Write(address, CipVariableType.DINT, Int32v.SelectMany(o => BitConverter.GetBytes(o)).ToArray(), (ushort)value.Count());
            }
            else if (value is IEnumerable<long> Int64v)
            {
                return Write(address, CipVariableType.LINT, Int64v.SelectMany(o => BitConverter.GetBytes(o)).ToArray(), (ushort)value.Count());
            }
            else if (value is IEnumerable<ushort> UInt16v)
            {
                return Write(address, CipVariableType.UINT, UInt16v.SelectMany(o => BitConverter.GetBytes(o)).ToArray(), (ushort)value.Count());
            }
            else if (value is IEnumerable<uint> UInt32v)
            {
                return Write(address, CipVariableType.UDINT, UInt32v.SelectMany(o => BitConverter.GetBytes(o)).ToArray(), (ushort)value.Count());
            }
            else if (value is IEnumerable<ulong> UInt64v)
            {
                return Write(address, CipVariableType.ULINT, UInt64v.SelectMany(o => BitConverter.GetBytes(o)).ToArray(), (ushort)value.Count());
            }
            else if (value is IEnumerable<string> stringv)
            {
                if (stringv != null && stringv.Count() != 1)
                    throw new NotImplementedException("字符串类型长度只能为1");
                return Write(address, CipVariableType.STRING, GetStringByte(stringv.First()));
            }
            else if (value is IEnumerable<DateTime> DateTimev)
            {
                return Write(address, CipVariableType.DATE_AND_TIME_NSEC, DateTimev.SelectMany(o => GetDateTimeByte(o)).ToArray(), (ushort)value.Count());
            }
            else
                throw new NotImplementedException("暂不支持的类型");
        }

        #endregion

        private byte[] GetStringByte(string info, Encoding encoding = null)
        {
            encoding ??= Encoding;
            var valueBytes = string.IsNullOrEmpty(info) ? new byte[] { } : encoding.GetBytes(info);
            valueBytes = (valueBytes.Length % 2 == 0 ? valueBytes : valueBytes.Concat(new byte[] { 0 })).ToArray();
            return BitConverter.GetBytes((ushort)valueBytes.Length).Concat(valueBytes).ToArray();
        }

        private byte[] GetDateTimeByte(DateTime dt)
        {
            var aaa = dt.AddYears(-1969).Ticks * 100;
            return BitConverter.GetBytes(aaa);
        }
    }
}
