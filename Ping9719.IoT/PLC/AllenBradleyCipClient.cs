using Ping9719.IoT.Common;
using Ping9719.IoT.Communication;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ping9719.IoT.PLC
{
    /// <summary>
    /// (AB)罗克韦尔客户端
    /// https://blog.csdn.net/lishiming0308/article/details/85243041
    /// https://www.cnblogs.com/ChuFeiFan/p/10868241.html
    /// </summary>
    public class AllenBradleyCipClient : ReadWriteBase, IClientData
    {
        /// <summary>
        /// 插槽
        /// </summary>
        public readonly byte Slot;

        ushort num = 0;

        /// <summary>
        /// 字符串编码格式。默认ASCII
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public byte[] BoolTrueByteVal = new byte[] { 0xFF, 0xFF };

        public ClientBase Client { get; private set; }
        public AllenBradleyCipClient(ClientBase client, byte slot = 0)
        {
            Client = client;
            //Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.UTF8;
            //Client.TimeOut = timeout;
            Client.ConnectionMode = ConnectionMode.AutoReconnection;
            Client.IsAutoDiscard = true;
            Client.Opened = (a) =>
            {
                //注册命令
                byte[] RegisteredCommand = new byte[28] {
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
                    throw new Exception("打开cip获取会话句柄失败。");
                }

                //打开扩展请求
                {
                    byte[] command = new byte[]
                    {
                        0x6f, 0x00,
                        0x44, 0x00,
                        SessionByte[0], SessionByte[1], SessionByte[2], SessionByte[3],
                        0x00, 0x00, 0x00, 0x00,
                        0x41, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00,
                        0x02, 0x00,
                        0x00, 0x00,
                        0x00, 0x00,
                        0xb2, 0x00,
                        0x34, 0x00,
                        0x5b, 0x02,
                        0x20, 0x06,
                        0x24, 0x01,
                        0x05, 0xf7,
                        0x02, 0x00, 0x00, 0x80,
                        0x01, 0x00, 0xfe, 0x80,
                        0x02, 0x00,
                        0x1b, 0x05,
                        0xc0, 0x80, 0xa7, 0x02, 0x02, 0x00, 0x00, 0x00, 0x80, 0x84, 0x1e, 0x00, 0xa0, 0x0f, 0x00, 0x42, 0x80, 0x84, 0x1e, 0x00, 0xa0, 0x0f, 0x00, 0x42, 0xa3, 0x03, 0x01, 0x00,
                        0x20, 0x02, 0x24, 0x01
                    };
                    var aaa = Client.SendReceive(command);
                    if (!aaa.IsSucceed || aaa.Value.Length < 43)
                    {
                        throw new Exception("打开扩展请求失败。");
                    }
                    if (BitConverter.ToUInt16(aaa.Value, 41) != 0)
                    {
                        throw new Exception("打开扩展请求失败，错误码：" + BitConverter.ToUInt16(aaa.Value, 41));
                    }
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
        public AllenBradleyCipClient(string ip, int port = 44818, byte slot = 0) : this(new TcpClient(ip, port), slot) { }


        /// <summary>
        /// 会话句柄
        /// </summary>
        public readonly byte[] SessionByte = new byte[] { 0, 0, 0, 0 };

        /// <summary>
        /// 会话句柄(由AB PLC生成)
        /// </summary>
        public uint Session => BitConverter.ToUInt32(SessionByte, 0);

        #region Read
        public IoTResult<object> Read(string address, int len = 1)
        {
            var result = new IoTResult<object>();
            try
            {
                var cpiadd = AllenBradleyAddress.Parse(address);
                //var command = GetReadCommand(address, (UInt16)address.Length);
                var command = GetReadCommand2(cpiadd);

                //发送命令 并获取响应报文
                var sendResult = Client.SendReceive(command);
                if (!sendResult.IsSucceed)
                    return new IoTResult<object>() { IsSucceed = false };
                var dataPackage = sendResult.Value;


                //int16 10
                //0A 00 02 00 CC 00 00 00 C3 00 0A 00
                //bool t
                //0A 00 03 00 CC 00 00 00 C1 00 FF FF
                //int16 1-5
                //12 00 05 00 CC 00 00 00 C3 00 01 00 02 00 03 00 04 00 05 00

                //70 00 1E 00 AB 63 9D 51 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 0A 00 02 00 A1 00 04 00 01 00 FE 80 B1 00
                //0A 00 09 00 CC 00 00 00 C3 00 0A 00
                //70 00 1C 00 AB 63 9D 51 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 0A 00 02 00 A1 00 04 00 01 00 FE 80 B1 00
                //08 00 07 00 00 00 04 00 00 00

                var count = BitConverter.ToUInt16(dataPackage, 42);//数据总长度
                var hNum = BitConverter.ToUInt16(dataPackage, 44);//发送编号
                var isok = BitConverter.ToUInt16(dataPackage, 47);//合格
                var dTypt = (CipVariableType)dataPackage.ElementAtOrDefault(50);//类型
                //var dLeng = (CipVariableType)dataPackage.ElementAtOrDefault(51);//长度或格式
                var data = dataPackage.Skip(52).ToArray();//数据

                if (num != hNum)
                    throw new Exception("编号不一致" + hNum);
                if (isok != 0)
                    throw new Exception("读取失败，错误代码" + isok);

                if (cpiadd.Index.Count() >= 2)
                    throw new Exception("不支持一维数组以上数组");

                var staIndex = cpiadd.Index.FirstOrDefault();

                if (dTypt == CipVariableType.BOOL)
                {
                    var v1 = data.Chunk(2).Select(o => BitConverter.ToBoolean(o.ToArray(), 0)).Skip(staIndex);
                    result.Value = len <= 1 ? v1.First() : v1.Take(len).ToArray();
                }
                else if (dTypt == CipVariableType.BYTE)
                {
                    var v1 = data.Skip(staIndex);
                    result.Value = len <= 1 ? v1.First() : v1.Take(len).ToArray();
                }
                else if (dTypt == CipVariableType.REAL)
                {
                    var v1 = data.Chunk(4).Select(o => BitConverter.ToSingle(o.ToArray(), 0)).Skip(staIndex);
                    result.Value = len <= 1 ? v1.First() : v1.Take(len).ToArray();
                }
                else if (dTypt == CipVariableType.LREAL)
                {
                    var v1 = data.Chunk(8).Select(o => BitConverter.ToDouble(o.ToArray(), 0)).Skip(staIndex);
                    result.Value = len <= 1 ? v1.First() : v1.Take(len).ToArray();
                }
                else if (dTypt == CipVariableType.INT)
                {
                    var v1 = data.Chunk(2).Select(o => BitConverter.ToInt16(o.ToArray(), 0)).Skip(staIndex);
                    result.Value = len <= 1 ? v1.First() : v1.Take(len).ToArray();
                }
                else if (dTypt == CipVariableType.DINT)
                {
                    var v1 = data.Chunk(4).Select(o => BitConverter.ToInt32(o.ToArray(), 0)).Skip(staIndex);
                    result.Value = len <= 1 ? v1.First() : v1.Take(len).ToArray();
                }
                else if (dTypt == CipVariableType.LINT)
                {
                    var v1 = data.Chunk(8).Select(o => BitConverter.ToInt64(o.ToArray(), 0)).Skip(staIndex);
                    result.Value = len <= 1 ? v1.First() : v1.Take(len).ToArray();
                }
                else if (dTypt == CipVariableType.UINT)
                {
                    var v1 = data.Chunk(2).Select(o => BitConverter.ToUInt16(o.ToArray(), 0)).Skip(staIndex);
                    result.Value = len <= 1 ? v1.First() : v1.Take(len).ToArray();
                }
                else if (dTypt == CipVariableType.UDINT)
                {
                    var v1 = data.Chunk(4).Select(o => BitConverter.ToUInt32(o.ToArray(), 0)).Skip(staIndex);
                    result.Value = len <= 1 ? v1.First() : v1.Take(len).ToArray();
                }
                else if (dTypt == CipVariableType.ULINT)
                {
                    var v1 = data.Chunk(8).Select(o => BitConverter.ToUInt64(o.ToArray(), 0)).Skip(staIndex);
                    result.Value = len <= 1 ? v1.First() : v1.Take(len).ToArray();
                }
                else if (dTypt == CipVariableType.STRING)
                {
                    var v1 = data.Chunk(100).Select(o => Encoding.GetString(o.ToArray(), 2, BitConverter.ToUInt16(o.ToArray(), 0))).Skip(staIndex);
                    result.Value = len <= 1 ? v1.First() : v1.Take(len).ToArray();
                }
                else
                    throw new Exception("不支持的类型" + dTypt.ToString());


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
        protected byte[] GetReadCommand(string address, ushort length)
        {
            //if (!isBit)
            //length = (ushort)(length / 2);

            var address_ASCII = Encoding.GetBytes(address);
            if (address_ASCII.Length % 2 == 1)
            {
                address_ASCII = new byte[address_ASCII.Length + 1];
                Encoding.GetBytes(address).CopyTo(address_ASCII, 0);
            }

            byte[] command = new byte[9 + 26 + address_ASCII.Length + 1 + 24];

            command[0] = 0x6F;//命令
            command[2] = BitConverter.GetBytes((ushort)(command.Length - 24))[0];
            command[3] = BitConverter.GetBytes((ushort)(command.Length - 24))[1];//长度
            command[4] = SessionByte[0];
            command[5] = SessionByte[1];
            command[6] = SessionByte[2];
            command[7] = SessionByte[3];//会话句柄

            command[0 + 24] = 0x00;
            command[1 + 24] = 0x00;
            command[2 + 24] = 0x00;
            command[3 + 24] = 0x00;//接口句柄，默认为0x00000000（CIP）
            command[4 + 24] = 0x01;
            command[5 + 24] = 0x00;//超时（0x0001）
            command[6 + 24] = 0x02;
            command[7 + 24] = 0x00;//项数（0x0002）
            command[8 + 24] = 0x00;
            command[9 + 24] = 0x00;//空地址项（0x0000）
            command[10 + 24] = 0x00;
            command[11 + 24] = 0x00;//长度（0x0000）
            command[12 + 24] = 0xB2;
            command[13 + 24] = 0x00;//未连接数据项（0x00b2）
            command[14 + 24] = BitConverter.GetBytes((short)(command.Length - 16 - 24))[0]; // 后面数据包的长度，等全部生成后在赋值
            command[15 + 24] = BitConverter.GetBytes((short)(command.Length - 16 - 24))[1];
            command[16 + 24] = 0x52;//服务类型（0x03请求服务列表，0x52请求标签数据）
            command[17 + 24] = 0x02;//请求路径大小
            command[18 + 24] = 0x20;
            command[19 + 24] = 0x06;//请求路径(0x0620)
            command[20 + 24] = 0x24;
            command[21 + 24] = 0x01;//请求路径(0x0124)
            command[22 + 24] = 0x0A;
            command[23 + 24] = 0xF0;
            command[24 + 24] = BitConverter.GetBytes((short)(6 + address_ASCII.Length))[0];     // CIP指令长度
            command[25 + 24] = BitConverter.GetBytes((short)(6 + address_ASCII.Length))[1];

            command[0 + 24 + 26] = 0x4C;//读取数据
            command[1 + 24 + 26] = (byte)((address_ASCII.Length + 2) / 2);
            command[2 + 24 + 26] = 0x91;
            command[3 + 24 + 26] = (byte)address.Length;
            address_ASCII.CopyTo(command, 4 + 24 + 26);
            command[4 + 24 + 26 + address_ASCII.Length] = BitConverter.GetBytes(length)[0];
            command[5 + 24 + 26 + address_ASCII.Length] = BitConverter.GetBytes(length)[1];

            command[6 + 24 + 26 + address_ASCII.Length] = 0x01;
            command[7 + 24 + 26 + address_ASCII.Length] = 0x00;
            command[8 + 24 + 26 + address_ASCII.Length] = 0x01;
            command[9 + 24 + 26 + address_ASCII.Length] = Slot;

            return command;
        }

        /// <summary>
        /// 获取Read命令
        /// </summary>
        /// <param name="address"></param>
        /// <param name="slot"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected byte[] GetReadCommand2(AllenBradleyAddress address)
        {
            byte[] aadd;
            {
                //70 00 1E 00 98 A5 45 94 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
                //00 00 00 00 00 0A 00 02 00 A1 00 04 00 C1 09 41 5F B1 00 0A 00 21 00
                //4C 02 91 01 41 00 01 00

                //70 00 1E 00 5C 82 C0 84 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
                //00 00 00 00 00 0A 00 02 00 A1 00 04 00 01 00 FE 80 B1 00 0A 00 FF FF
                //4C 02 91 01 41 00 01 00

                num = (ushort)(num + 2);
                var numbyte = BitConverter.GetBytes(num);
                byte[] Header = new byte[24]
                {
                    0x70,0x00,//命令 2byte
　　                0x28,0x00,//长度 2byte（总长度-Header的长度）=40 
　　                SessionByte[0],SessionByte[1],SessionByte[2],SessionByte[3],//会话句柄 4byte
　　                0x00,0x00,0x00,0x00,//状态默认0 4byte
　　                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,//发送方描述默认0 8byte
　　                0x00,0x00,0x00,0x00,//选项默认0 4byte
                };
                //byte[] CommandSpecificData = new byte[16]
                //{
                //  0x00,0x00,0x00,0x00,//接口句柄 CIP默认为0x00000000 4byte
　　              //0x01,0x00,//超时默认0x0001 4byte
　　              //0x02,0x00,//项数默认0x0002 4byte
　　              //0x00,0x00,//空地址项默认0x0000 2byte
　　              //0x00,0x00,//长度默认0x0000 2byte
　　              //0xb2,0x00,//未连接数据项默认为 0x00b2
　　              //0x18,0x00,//后面数据包的长度 24个字节(总长度-Header的长度-CommandSpecificData的长度)
                //};
                byte[] CommandSpecificData = new byte[]//16
                {
                    0x00,0x00,0x00,0x00,//接口句柄 CIP默认为0x00000000 4byte
　　                0x0A,0x00,//超时 默认0x0001 4byte
　　                0x02,0x00,//项数 默认0x0002 4byte
　　                0xA1,0x00,//连接的地址项 空地址项默认0x0000 2byte
　　                0x04,0x00,//长度 默认0x0000 2byte
　　                0xC1,0x09,0x41,0x5F,//连接标识 未连接数据项默认为 0x00b2
                    0xb1,0x00,//连接的数据项
　　                0x18,0x00,// 后面数据包的长度 24个字节(总长度-Header的长度-CommandSpecificData的长度)
                    numbyte[0],numbyte[1],// 序号 从1开始
                };
                List<byte> CipMessage = new List<byte>()
                {
                    0x4C,//服务代码 读0x4C
                    0x00,//路径长度 2byte  规律为 (标签名的长度+1/2)+1
                    //0x91,//扩展符号 默认为 0x91
                    //length,//标签名的长度
                    //0x54,0x41,0x47,0x31,//标签名 ：TAG1转换成ASCII字节 当标签名的长度为奇数时，需要在末尾补0  比如TAG转换成ASCII为0x54,0x41,0x47，需要在末尾补0 变成 0x54,0x41,0x47，0
                    //0x01,0x00,//读取数量
                };
                CipMessage.AddRange(address.GetCip(Encoding));//标签名
                CipMessage.AddRange(new byte[] { 0x01, 0x00 });//读取数量
                                                               //长度
                Header[2] = BitConverter.GetBytes((short)(CommandSpecificData.Length + CipMessage.Count))[0];
                Header[3] = BitConverter.GetBytes((short)(CommandSpecificData.Length + CipMessage.Count))[1];
                CommandSpecificData[18] = BitConverter.GetBytes((short)(CipMessage.Count + 2))[0];
                CommandSpecificData[19] = BitConverter.GetBytes((short)(CipMessage.Count + 2))[1];
                CipMessage[1] = (byte)((CipMessage.Count - 4) / 2);

                aadd = Header.Concat(CommandSpecificData).Concat(CipMessage).ToArray();
            }
            return aadd;
        }

        /// <summary>
        /// 获取Read命令
        /// </summary>
        /// <param name="address"></param>
        /// <param name="slot"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected byte[] GetReadCommand3(string address)
        {
            var addData = Encoding.GetBytes(address).ToList();
            byte length = (byte)addData.Count;
            if (length % 2 == 1)
            {
                addData.Add(0);
            }

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
                byte[] CommandSpecificData = new byte[16]
                {
                    0x00,0x00,0x00,0x00,//接口句柄 CIP默认为0x00000000 4byte
　　                0x01,0x00,//超时默认0x0001 4byte
　　                0x02,0x00,//项数默认0x0002 4byte
　　                0x00,0x00,//空地址项默认0x0000 2byte
　　                0x00,0x00,//长度默认0x0000 2byte
　　                0xb2,0x00,//未连接数据项默认为 0x00b2
　　                0x18,0x00,//后面数据包的长度 24个字节(总长度-Header的长度-CommandSpecificData的长度)
                };
                List<byte> CipMessage = new List<byte>(20 + addData.Count)
                {
                    0x52,0x02,//服务默认0x52  请求路径大小 默认2
                    0x20,0x06,0x24,0x01,//请求路径 默认0x01240622 4byte
                    0x0A,0xF0,//超时默认0xF00A 4byte
                    0x0A,0x00,//Cip指令长度  服务标识到服务命令指定数据的长度 
                    0x4C,//服务标识固定为0x4C 1byte  
                    0x03,// 节点长度 2byte  规律为 (标签名的长度+1/2)+1
                    0x91,//扩展符号 默认为 0x91
                    0x04,//标签名的长度
                    //0x54,0x41,0x47,0x31,//标签名 ：TAG1转换成ASCII字节 当标签名的长度为奇数时，需要在末尾补0  比如TAG转换成ASCII为0x54,0x41,0x47，需要在末尾补0 变成 0x54,0x41,0x47，0
                    //0x01,0x00,//服务命令指定数据　默认为0x0001
                    //0x01,0x00,0x01,0x00//最后一位是PLC的槽号
                };
                CipMessage.AddRange(addData);//标签名
                CipMessage.AddRange(new byte[] { 0x01, 0x00 });//服务命令指定数据
                CipMessage.AddRange(new byte[] { 0x01, 0x00, 0x01, Slot });//服务命令指定数据
                                                                           //长度
                Header[2] = BitConverter.GetBytes((short)(CommandSpecificData.Length + CipMessage.Count))[0];
                Header[3] = BitConverter.GetBytes((short)(CommandSpecificData.Length + CipMessage.Count))[1];
                CommandSpecificData[14] = BitConverter.GetBytes((short)CipMessage.Count)[0];
                CommandSpecificData[15] = BitConverter.GetBytes((short)CipMessage.Count)[1];
                CipMessage[8] = BitConverter.GetBytes((short)(6 + addData.Count))[0];
                CipMessage[9] = BitConverter.GetBytes((short)(6 + addData.Count))[1];
                CipMessage[11] = (byte)((addData.Count + 2) / 2);
                CipMessage[13] = length;

                aadd = Header.Concat(CommandSpecificData).Concat(CipMessage).ToArray();
            }
            return aadd;
        }

        #endregion

        #region Write
        public IoTResult Write(string address, CipVariableType typeCode, byte[] data)
        {
            IoTResult result = new IoTResult();
            try
            {
                //Array.Reverse(data);
                //发送写入信息
                //var arg = ConvertWriteArg(address, data, false);
                byte[] command = GetWriteCommand2(address, typeCode, data);

                var sendResult = Client.SendReceive(command);
                if (!sendResult.IsSucceed)
                    return sendResult;

                var dataPackage = sendResult.Value;


                //var count = BitConverter.ToUInt16(dataPackage, 38);//数据总长度
                var isok = BitConverter.ToUInt16(dataPackage, 42);//合格
                //var dTypt = (CipVariableType)BitConverter.ToUInt16(dataPackage, 44);//类型
                //var data22 = dataPackage.Skip(46).Take(count - 6).ToArray();//数据

                if (isok != 0)
                    throw new Exception("读取失败，错误代码" + isok);
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 获取Write命令
        /// </summary>
        /// <param name="address"></param>
        /// <param name="typeCode"></param>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected byte[] GetWriteCommand(string address, ushort typeCode, byte[] value, int length)
        {
            var address_ASCII = Encoding.GetBytes(address);
            if (address_ASCII.Length % 2 == 1)
            {
                address_ASCII = new byte[address_ASCII.Length + 1];
                Encoding.GetBytes(address).CopyTo(address_ASCII, 0);
            }
            byte[] command = new byte[8 + 26 + address_ASCII.Length + value.Length + 4 + 24];

            command[0] = 0x6F;//命令
            command[2] = BitConverter.GetBytes((ushort)(command.Length - 24))[0];
            command[3] = BitConverter.GetBytes((ushort)(command.Length - 24))[1];//长度
            command[4] = BitConverter.GetBytes(Session)[0];
            command[5] = BitConverter.GetBytes(Session)[1];
            command[6] = BitConverter.GetBytes(Session)[2];
            command[7] = BitConverter.GetBytes(Session)[3];//会话句柄

            command[0 + 24] = 0x00;
            command[1 + 24] = 0x00;
            command[2 + 24] = 0x00;
            command[3 + 24] = 0x00;//接口句柄，默认为0x00000000（CIP）
            command[4 + 24] = 0x01;
            command[5 + 24] = 0x00;//超时（0x0001）
            command[6 + 24] = 0x02;
            command[7 + 24] = 0x00;//项数（0x0002）
            command[8 + 24] = 0x00;
            command[9 + 24] = 0x00;
            command[10 + 24] = 0x00;
            command[11 + 24] = 0x00;//空地址项（0x0000）
            command[12 + 24] = 0xB2;
            command[13 + 24] = 0x00;//未连接数据项（0x00b2）
            command[14 + 24] = BitConverter.GetBytes((short)(command.Length - 16 - 24))[0]; // 后面数据包的长度，等全部生成后在赋值
            command[15 + 24] = BitConverter.GetBytes((short)(command.Length - 16 - 24))[1];
            command[16 + 24] = 0x52;//服务类型（0x03请求服务列表，0x52请求标签数据）
            command[17 + 24] = 0x02;//请求路径大小
            command[18 + 24] = 0x20;
            command[19 + 24] = 0x06;//请求路径(0x0620)
            command[20 + 24] = 0x24;
            command[21 + 24] = 0x01;//请求路径(0x0124)
            command[22 + 24] = 0x0A;
            command[23 + 24] = 0xF0;
            command[24 + 24] = BitConverter.GetBytes((short)(8 + value.Length + address_ASCII.Length))[0];     // CIP指令长度
            command[25 + 24] = BitConverter.GetBytes((short)(8 + value.Length + address_ASCII.Length))[1];

            command[0 + 26 + 24] = 0x4D;//写数据
            command[1 + 26 + 24] = (byte)((address_ASCII.Length + 2) / 2);
            command[2 + 26 + 24] = 0x91;
            command[3 + 26 + 24] = (byte)address.Length;
            address_ASCII.CopyTo(command, 4 + 26 + 24);
            command[4 + 26 + 24 + address_ASCII.Length] = BitConverter.GetBytes(typeCode)[0];
            command[5 + 26 + 24 + address_ASCII.Length] = BitConverter.GetBytes(typeCode)[1];
            command[6 + 26 + 24 + address_ASCII.Length] = BitConverter.GetBytes(length)[0];//TODO length ??
            command[7 + 26 + 24 + address_ASCII.Length] = BitConverter.GetBytes(length)[1];
            value.CopyTo(command, 8 + 26 + 24 + address_ASCII.Length);

            command[8 + 26 + 24 + address_ASCII.Length + value.Length] = 0x01;
            command[9 + 26 + 24 + address_ASCII.Length + value.Length] = 0x00;
            command[10 + 26 + 24 + address_ASCII.Length + value.Length] = 0x01;
            command[11 + 26 + 24 + address_ASCII.Length + value.Length] = Slot;
            return command;

        }

        /// <summary>
        /// 获取Write命令
        /// </summary>
        /// <param name="address"></param>
        /// <param name="typeCode"></param>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected byte[] GetWriteCommand2(string address, CipVariableType typeCode, byte[] value)
        {
            var addData = Encoding.GetBytes(address).ToList();
            byte length = (byte)addData.Count;
            if (length % 2 == 1)
            {
                addData.Add(0);
            }

            byte[] aadd;
            {
                num = (ushort)(num + 2);
                var numbyte = BitConverter.GetBytes(num);

                byte[] Header = new byte[24]
                {
                    0x70,0x00,//命令 2byte
　　                0x28,0x00,//长度 2byte（总长度-Header的长度）=40 
　　                SessionByte[0],SessionByte[1],SessionByte[2],SessionByte[3],//会话句柄 4byte
　　                0x00,0x00,0x00,0x00,//状态默认0 4byte
　　                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,//发送方描述默认0 8byte
　　                0x00,0x00,0x00,0x00,//选项默认0 4byte
                };
                byte[] CommandSpecificData = new byte[]
                {
                    0x00,0x00,0x00,0x00,//接口句柄 CIP默认为0x00000000 4byte
　　                0x0A,0x00,//超时 默认0x0001 4byte
　　                0x02,0x00,//项数 默认0x0002 4byte
　　                0xA1,0x00,//连接的地址项 空地址项默认0x0000 2byte
　　                0x04,0x00,//长度 默认0x0000 2byte
　　                0xC1,0x09,0x41,0x5F,//连接标识 未连接数据项默认为 0x00b2
                    0xb1,0x00,//连接的数据项
　　                0x18,0x00,// 后面数据包的长度 24个字节(总长度-Header的长度-CommandSpecificData的长度)
                    numbyte[0],numbyte[1],// 序号 从1开始
                };
                List<byte> CipMessage = new List<byte>()
                {
                    0x52,0x02,//服务默认0x52  请求路径大小 默认2
                    0x22,0x06,0x24,0x01,//请求路径 默认0x01240622 4byte
                    0x0A,0xF0,//超时默认0xF00A 4byte
                    0x0A,0x00,//Cip指令长度  服务标识到服务命令指定数据的长度 
                    0x4D,//服务标识固定为0x4D 1byte  写
                    0x03,// 节点长度 2byte  规律为 (标签名的长度+1/2)+1
                    0x91,//扩展符号 默认为 0x91
                    0x04,//标签名的长度
                    //0x54,0x41,0x47,0x31,//标签名 ：TAG1转换成ASCII字节 当标签名的长度为奇数时，需要在末尾补0  比如TAG转换成ASCII为0x54,0x41,0x47，需要在末尾补0 变成 0x54,0x41,0x47，0
                    //0xC1,0x00,//数据类型
                    //0x01,0x00,//服务命令指定数据　默认为0x0001
                    //0x01,0x00,数据
                    //0x01,0x00,0x01,0x00//最后一位是PLC的槽号
                };
                CipMessage.AddRange(addData);//标签名
                CipMessage.AddRange(BitConverter.GetBytes((ushort)typeCode).Reverse());//数据类型
                CipMessage.AddRange(new byte[] { 0x01, 0x00 });//服务命令指定数据
                CipMessage.AddRange(value);//数据
                CipMessage.AddRange(new byte[] { 0x01, 0x00, 0x01, Slot });//服务命令指定数据
                                                                           //长度
                Header[2] = BitConverter.GetBytes((short)(CommandSpecificData.Length + CipMessage.Count))[0];
                Header[3] = BitConverter.GetBytes((short)(CommandSpecificData.Length + CipMessage.Count))[1];
                CommandSpecificData[14] = BitConverter.GetBytes((short)CipMessage.Count)[0];
                CommandSpecificData[15] = BitConverter.GetBytes((short)CipMessage.Count)[1];
                CipMessage[8] = BitConverter.GetBytes((short)(8 + value.Length + addData.Count))[0];
                CipMessage[9] = BitConverter.GetBytes((short)(8 + value.Length + addData.Count))[1];
                CipMessage[11] = (byte)((addData.Count + 2) / 2);
                CipMessage[13] = length;

                aadd = Header.Concat(CommandSpecificData).Concat(CipMessage).ToArray();
            }
            return aadd;
        }

        protected byte[] GetWriteCommand3(string address, CipVariableType typeCode, byte[] value)
        {
            var addData = Encoding.GetBytes(address).ToList();
            byte length = (byte)addData.Count;
            if (length % 2 == 1)
            {
                addData.Add(0);
            }

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
                byte[] CommandSpecificData = new byte[16]
                {
                    0x00,0x00,0x00,0x00,//接口句柄 CIP默认为0x00000000 4byte
　　                0x01,0x00,//超时默认0x0001 4byte
　　                0x02,0x00,//项数默认0x0002 4byte
　　                0x00,0x00,//空地址项默认0x0000 2byte
　　                0x00,0x00,//长度默认0x0000 2byte
　　                0xb2,0x00,//未连接数据项默认为 0x00b2
　　                0x18,0x00,//后面数据包的长度 24个字节(总长度-Header的长度-CommandSpecificData的长度)
                };
                List<byte> CipMessage = new List<byte>(22 + addData.Count + value.Length)
                {
                    0x52,0x02,//服务默认0x52  请求路径大小 默认2
                    0x22,0x06,0x24,0x01,//请求路径 默认0x01240622 4byte
                    0x0A,0xF0,//超时默认0xF00A 4byte
                    0x0A,0x00,//Cip指令长度  服务标识到服务命令指定数据的长度 
                    0x4D,//服务标识固定为0x4D 1byte  写
                    0x03,// 节点长度 2byte  规律为 (标签名的长度+1/2)+1
                    0x91,//扩展符号 默认为 0x91
                    0x04,//标签名的长度
                    //0x54,0x41,0x47,0x31,//标签名 ：TAG1转换成ASCII字节 当标签名的长度为奇数时，需要在末尾补0  比如TAG转换成ASCII为0x54,0x41,0x47，需要在末尾补0 变成 0x54,0x41,0x47，0
                    //0xC1,0x00,//数据类型
                    //0x01,0x00,//服务命令指定数据　默认为0x0001
                    //0x01,0x00,数据
                    //0x01,0x00,0x01,0x00//最后一位是PLC的槽号
                };
                CipMessage.AddRange(addData);//标签名
                CipMessage.AddRange(BitConverter.GetBytes((ushort)typeCode).Reverse());//数据类型
                CipMessage.AddRange(new byte[] { 0x01, 0x00 });//服务命令指定数据
                CipMessage.AddRange(value);//数据
                CipMessage.AddRange(new byte[] { 0x01, 0x00, 0x01, Slot });//服务命令指定数据
                                                                           //长度
                Header[2] = BitConverter.GetBytes((short)(CommandSpecificData.Length + CipMessage.Count))[0];
                Header[3] = BitConverter.GetBytes((short)(CommandSpecificData.Length + CipMessage.Count))[1];
                CommandSpecificData[14] = BitConverter.GetBytes((short)CipMessage.Count)[0];
                CommandSpecificData[15] = BitConverter.GetBytes((short)CipMessage.Count)[1];
                CipMessage[8] = BitConverter.GetBytes((short)(8 + value.Length + addData.Count))[0];
                CipMessage[9] = BitConverter.GetBytes((short)(8 + value.Length + addData.Count))[1];
                CipMessage[11] = (byte)((addData.Count + 2) / 2);
                CipMessage[13] = length;

                aadd = Header.Concat(CommandSpecificData).Concat(CipMessage).ToArray();
            }
            return aadd;
        }
        #endregion

        #region IIoTBase
        public override IoTResult<T> Read<T>(string address)
        {
            var aaa = Read(address, 1);
            try
            {
                return aaa.IsSucceed ? new IoTResult<T>(aaa, (T)aaa.Value) : new IoTResult<T>(aaa);
            }
            catch (Exception ex)
            {
                var bbb = new IoTResult<T>(aaa);
                bbb.IsSucceed = false;
                bbb.AddError(ex.Message);
                return bbb;
            }
        }

        public override IoTResult<string> ReadString(string address, int length, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public override IoTResult<IEnumerable<T>> Read<T>(string address, int number)
        {
            var aaa = Read(address, number);
            try
            {
                return aaa.IsSucceed ? new IoTResult<IEnumerable<T>>(aaa, ((IEnumerable)aaa.Value).Cast<T>()) : new IoTResult<IEnumerable<T>>(aaa);
            }
            catch (Exception ex)
            {
                var bbb = new IoTResult<IEnumerable<T>>(aaa);
                bbb.IsSucceed = false;
                bbb.AddError(ex.Message);
                return bbb;
            }
        }

        public override IoTResult Write<T>(string address, T value)
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
                var valueBytes = Encoding.GetBytes(stringv);
                var data = BitConverter.GetBytes((ushort)valueBytes.Length).Concat(valueBytes).ToArray();
                return Write(address, CipVariableType.STRING, data);
            }
            else
                throw new NotImplementedException("暂不支持的类型");
        }

        public override IoTResult WriteString(string address, string value, int length, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public override IoTResult Write<T>(string address, IEnumerable<T> value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
