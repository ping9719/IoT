using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Ping9719.IoT.Communication;

namespace Ping9719.IoT.PLC
{
    /// <summary>
    /// 三菱客户端（MC协议）.
    /// 已测试单个元素读写：bool,short,int32,float,double
    /// 已测试数组元素读写：bool(循环写入速度较慢),short,int32,float,double
    /// </summary>
    public class MitsubishiMcClient : IIoT
    {
        /// <summary>
        /// 版本
        /// </summary>
        public MitsubishiVersion Version { get; private set; }

        /// <summary>
        /// 字符串编码格式。默认ASCII
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.ASCII;

        public ClientBase Client { get; private set; }//通讯管道

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="client">客户端</param>
        public MitsubishiMcClient(MitsubishiVersion version, ClientBase client, int timeout = 1500)
        {
            Client = client;
            Client.TimeOut = timeout;
            Client.ReceiveMode = ReceiveMode.ParseByteAll();
            Client.Encoding = Encoding.ASCII;
            Client.ConnectionMode = ConnectionMode.AutoReconnection;

            this.Version = version;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口</param>
        public MitsubishiMcClient(MitsubishiVersion version, string ip, int port = 1500, int timeout = 1500) : this(version, new TcpClient(ip, port), timeout) { }

        #region 读

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="length"></param>
        /// <param name="isBit"></param>
        /// <returns></returns>
        private IoTResult<byte[]> Read(string address, ushort length, bool isBit = false)
        {
            var result = new IoTResult<byte[]>();
            try
            {
                //发送读取信息
                MitsubishiMCAddress arg = null;
                byte[] command = null;

                switch (Version)
                {
                    case MitsubishiVersion.A_1E:
                        arg = ConvertArg_A_1E(address);
                        command = GetReadCommand_A_1E(arg.BeginAddress, arg.TypeCode, length, isBit);
                        break;

                    case MitsubishiVersion.Qna_3E:
                        arg = ConvertArg_Qna_3E(address);
                        command = GetReadCommand_Qna_3E(arg.BeginAddress, arg.TypeCode, length, isBit);
                        break;
                }

                IoTResult<byte[]> sendResult = new IoTResult<byte[]>();
                switch (Version)
                {
                    case MitsubishiVersion.A_1E:
                        var lenght = command[10] + command[11] * 256;
                        if (isBit)
                            sendResult = Client.SendReceive(command, ReceiveMode.ParseByte((int)Math.Ceiling(lenght * 0.5) + 2));
                        else
                            sendResult = Client.SendReceive(command, ReceiveMode.ParseByte(lenght * 2 + 2));
                        break;

                    case MitsubishiVersion.Qna_3E:
                        sendResult = Client.SendReceive(command);
                        break;
                }
                if (!sendResult.IsSucceed) return sendResult;

                byte[] dataPackage = sendResult.Value;

                var bufferLength = length;
                byte[] responseValue = null;

                switch (Version)
                {
                    case MitsubishiVersion.A_1E:
                        responseValue = new byte[dataPackage.Length - 2];
                        Array.Copy(dataPackage, 2, responseValue, 0, responseValue.Length);
                        break;

                    case MitsubishiVersion.Qna_3E:

                        if (isBit)
                        {
                            bufferLength = (ushort)Math.Ceiling(bufferLength * 0.5);
                        }
                        responseValue = new byte[bufferLength];
                        Array.Copy(dataPackage, dataPackage.Length - bufferLength, responseValue, 0, bufferLength);
                        break;
                }

                result.Value = responseValue;
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 通用读取单个值的泛型方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="byteLength">每个元素的字节数</param>
        /// <param name="converter">字节转T的委托</param>
        /// <param name="isBit">是否位操作</param>
        /// <returns></returns>
        private IoTResult<T> ReadValue<T>(string address, int byteLength, Func<byte[], int, T> converter, bool isBit = false)
        {
            var readResut = Read(address, (ushort)byteLength, isBit);
            var result = new IoTResult<T>(readResut);
            if (result.IsSucceed)
                result.Value = converter(readResut.Value, 0);
            return result.ToEnd();
        }

        /// <summary>
        /// 通用批量读取的泛型方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="readNumber"></param>
        /// <param name="byteLength">每个元素的字节数</param>
        /// <param name="converter">字节转T的委托</param>
        /// <returns></returns>
        private IoTResult<List<T>> ReadValues<T>(string address, ushort readNumber, int byteLength, Func<byte[], int, T> converter)
        {
            var readResut = Read(address, (ushort)(byteLength * readNumber));
            var result = new IoTResult<List<T>>(readResut);
            if (result.IsSucceed)
            {
                var values = new List<T>();
                for (int i = 0; i < readNumber; i++)
                {
                    values.Add(converter(readResut.Value, i * byteLength));
                }
                result.Value = values;
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 读取Boolean
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        private IoTResult<bool> ReadBoolean(string address)
        {
            var readResut = Read(address, 1, isBit: true);
            var result = new IoTResult<bool>(readResut);
            if (result.IsSucceed)
                result.Value = (readResut.Value[0] & 0b00010000) != 0;
            return result.ToEnd();
        }

        /// <summary>
        /// 读取Boolean
        /// </summary>
        /// <param name="address"></param>
        /// <param name="readNumber"></param>
        /// <returns></returns>
        private IoTResult<List<bool>> ReadBoolean(string address, ushort readNumber)
        {
            var length = 1;
            var readResut = Read(address, Convert.ToUInt16(length * readNumber), isBit: true);
            var result = new IoTResult<List<bool>>(readResut);
            if (result.IsSucceed)
            {
                var values = new List<bool>();
                for (ushort i = 0; i < readNumber; i++)
                {
                    var index = i / 2;
                    var isoffset = i % 2 == 0;
                    bool value;
                    if (isoffset)
                        value = (readResut.Value[index] & 0b00010000) != 0;
                    else
                        value = (readResut.Value[index] & 0b00000001) != 0;
                    values.Add(value);
                }
                result.Value = values;
            }
            return result.ToEnd();
        }

        #endregion 读

        #region 写

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        private IoTResult Write(string address, bool value)
        {
            byte[] valueByte = new byte[1];
            if (value) valueByte[0] = 16;
            return Write(address, valueByte, true);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <param name="isBit"></param>
        /// <returns></returns>
        private IoTResult Write(string address, byte[] data, bool isBit = false)
        {
            IoTResult result = new IoTResult();
            try
            {
                Array.Reverse(data);

                //发送写入信息
                MitsubishiMCAddress arg = null;
                byte[] command = null;
                switch (Version)
                {
                    case MitsubishiVersion.A_1E:
                        arg = ConvertArg_A_1E(address);
                        command = GetWriteCommand_A_1E(arg.BeginAddress, arg.TypeCode, data, isBit);
                        break;

                    case MitsubishiVersion.Qna_3E:
                        arg = ConvertArg_Qna_3E(address);
                        command = GetWriteCommand_Qna_3E(arg.BeginAddress, arg.TypeCode, data, isBit);
                        break;
                }

                IoTResult<byte[]> sendResult = new IoTResult<byte[]>();
                switch (Version)
                {
                    case MitsubishiVersion.A_1E:
                        sendResult = Client.SendReceive(command, ReceiveMode.ParseByte(2));
                        break;

                    case MitsubishiVersion.Qna_3E:
                        sendResult = Client.SendReceive(command);
                        break;
                }
                if (!sendResult.IsSucceed)
                    return sendResult;

                byte[] dataPackage = sendResult.Value;
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 通用写入单个值的泛型方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <param name="converter">类型转字节数组的委托</param>
        /// <returns></returns>
        private IoTResult WriteValue<T>(string address, T value, Func<T, byte[]> converter)
        {
            return Write(address, converter(value), false);
        }

        /// <summary>
        /// 通用连续写入多个值的泛型方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="values"></param>
        /// <param name="converter">类型转字节数组的委托</param>
        /// <returns></returns>
        private IoTResult WriteValues<T>(string address, T[] values, Func<T, byte[]> converter)
        {
            if (values == null || values.Length == 0)
                return new IoTResult() { IsSucceed = false };

            // 计算单个元素的字节长度
            int elementLength = converter(values[0]).Length;
            byte[] allBytes = new byte[values.Length * elementLength];
            for (int i = 0; i < values.Length; i++)
            {
                var bytes = converter(values[i]);
                Buffer.BlockCopy(bytes, 0, allBytes, i * elementLength, elementLength);
            }
            // 直接调用底层批量写入
            return Write(address, allBytes, false);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        private IoTResult Write(string address, string value)
        {
            var valueBytes = Encoding.ASCII.GetBytes(value);
            var bytes = new byte[valueBytes.Length + 1];
            bytes[0] = (byte)valueBytes.Length;
            valueBytes.CopyTo(bytes, 1);
            Array.Reverse(bytes);
            return Write(address, bytes);
        }

        #endregion 写

        #region 生成报文命令

        /// <summary>
        /// 获取Qna_3E读取命令
        /// </summary>
        /// <param name="beginAddress"></param>
        /// <param name="typeCode"></param>
        /// <param name="length"></param>
        /// <param name="isBit"></param>
        /// <returns></returns>
        protected byte[] GetReadCommand_Qna_3E(int beginAddress, byte[] typeCode, ushort length, bool isBit)
        {
            if (!isBit) length = (ushort)(length / 2);

            byte[] command = new byte[21];
            command[0] = 0x50;
            command[1] = 0x00; //副头部
            command[2] = 0x00; //网络编号
            command[3] = 0xFF; //PLC编号
            command[4] = 0xFF;
            command[5] = 0x03; //IO编号
            command[6] = 0x00; //模块站号
            command[7] = (byte)((command.Length - 9) % 256);
            command[8] = (byte)((command.Length - 9) / 256); // 请求数据长度
            command[9] = 0x0A;
            command[10] = 0x00; //时钟
            command[11] = 0x01;
            command[12] = 0x04;//指令（0x01 0x04读 0x01 0x14写）
            command[13] = isBit ? (byte)0x01 : (byte)0x00;//子指令（位 或 字节为单位）
            command[14] = 0x00;
            command[15] = BitConverter.GetBytes(beginAddress)[0];// 起始地址的地位
            command[16] = BitConverter.GetBytes(beginAddress)[1];
            command[17] = BitConverter.GetBytes(beginAddress)[2];
            command[18] = typeCode[0]; //数据类型
            command[19] = (byte)(length % 256);
            command[20] = (byte)(length / 256); //长度
            return command;
        }

        /// <summary>
        /// 获取A_1E读取命令
        /// </summary>
        /// <param name="beginAddress"></param>
        /// <param name="typeCode"></param>
        /// <param name="length"></param>
        /// <param name="isBit"></param>
        /// <returns></returns>
        protected byte[] GetReadCommand_A_1E(int beginAddress, byte[] typeCode, ushort length, bool isBit)
        {
            if (!isBit)
                length = (ushort)(length / 2);
            byte[] command = new byte[12];
            command[0] = isBit ? (byte)0x00 : (byte)0x01;//副头部
            command[1] = 0xFF; //PLC编号
            command[2] = 0x0A;
            command[3] = 0x00;
            command[4] = BitConverter.GetBytes(beginAddress)[0]; //
            command[5] = BitConverter.GetBytes(beginAddress)[1]; // 开始读取的地址
            command[6] = 0x00;
            command[7] = 0x00;
            command[8] = typeCode[1];
            command[9] = typeCode[0];
            command[10] = (byte)(length % 256);//长度
            command[11] = (byte)(length / 256);
            return command;
        }

        /// <summary>
        /// 获取Qna_3E写入命令
        /// </summary>
        /// <param name="beginAddress"></param>
        /// <param name="typeCode"></param>
        /// <param name="data"></param>
        /// <param name="isBit"></param>
        /// <returns></returns>
        protected byte[] GetWriteCommand_Qna_3E(int beginAddress, byte[] typeCode, byte[] data, bool isBit)
        {
            var length = data.Length / 2;
            if (isBit) length = 1;

            byte[] command = new byte[21 + data.Length];
            command[0] = 0x50;
            command[1] = 0x00; //副头部
            command[2] = 0x00; //网络编号
            command[3] = 0xFF; //PLC编号
            command[4] = 0xFF;
            command[5] = 0x03; //IO编号
            command[6] = 0x00; //模块站号
            command[7] = (byte)((command.Length - 9) % 256);// 请求数据长度
            command[8] = (byte)((command.Length - 9) / 256);
            command[9] = 0x0A;
            command[10] = 0x00; //时钟
            command[11] = 0x01;
            command[12] = 0x14;//指令（0x01 0x04读 0x01 0x14写）
            command[13] = isBit ? (byte)0x01 : (byte)0x00;//子指令（位 或 字节为单位）
            command[14] = 0x00;
            command[15] = BitConverter.GetBytes(beginAddress)[0];// 起始地址的地位
            command[16] = BitConverter.GetBytes(beginAddress)[1];
            command[17] = BitConverter.GetBytes(beginAddress)[2];
            command[18] = typeCode[0];//数据类型
            command[19] = (byte)(length % 256);
            command[20] = (byte)(length / 256); //长度
            data.Reverse().ToArray().CopyTo(command, 21);
            return command;
        }

        /// <summary>
        /// 获取A_1E写入命令
        /// </summary>
        /// <param name="beginAddress"></param>
        /// <param name="typeCode"></param>
        /// <param name="data"></param>
        /// <param name="isBit"></param>
        /// <returns></returns>
        protected byte[] GetWriteCommand_A_1E(int beginAddress, byte[] typeCode, byte[] data, bool isBit)
        {
            var length = data.Length / 2;
            if (isBit) length = data.Length;

            byte[] command = new byte[12 + data.Length];
            command[0] = isBit ? (byte)0x02 : (byte)0x03;     //副标题
            command[1] = 0xFF;                             // PLC号
            command[2] = 0x0A;
            command[3] = 0x00;
            command[4] = BitConverter.GetBytes(beginAddress)[0];        //
            command[5] = BitConverter.GetBytes(beginAddress)[1];        //起始地址的地位
            command[6] = 0x00;
            command[7] = 0x00;
            command[8] = typeCode[1];        //
            command[9] = typeCode[0];        //数据类型
            command[10] = (byte)(length % 256);
            command[11] = (byte)(length / 256);
            data.Reverse().ToArray().CopyTo(command, 12);
            return command;
        }

        #endregion 生成报文命令

        #region private

        #region 地址解析

        /// <summary>
        /// Qna_3E地址解析
        /// </summary>
        /// <param name="address"></param>
        /// <param name="toUpper"></param>
        /// <returns></returns>
        private MitsubishiMCAddress ConvertArg_Qna_3E(string address, DataTypeEnum dataType = DataTypeEnum.None, bool toUpper = true)
        {
            if (toUpper) address = address.ToUpper();
            var addressInfo = new MitsubishiMCAddress()
            {
                DataTypeEnum = dataType
            };
            switch (address[0])
            {
                case 'M'://M中间继电器
                    {
                        addressInfo.TypeCode = new byte[] { 0x90 };
                        addressInfo.BitType = 0x01;
                        addressInfo.Format = 10;
                        addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                        addressInfo.TypeChar = address.Substring(0, 1);
                    }
                    break;

                case 'X':// X输入继电器
                    {
                        addressInfo.TypeCode = new byte[] { 0x9C };
                        addressInfo.BitType = 0x01;
                        addressInfo.Format = 16;
                        addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                        addressInfo.TypeChar = address.Substring(0, 1);
                    }
                    break;

                case 'Y'://Y输出继电器
                    {
                        addressInfo.TypeCode = new byte[] { 0x9D };
                        addressInfo.BitType = 0x01;
                        addressInfo.Format = 16;
                        addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                        addressInfo.TypeChar = address.Substring(0, 1);
                    }
                    break;

                case 'D'://D数据寄存器
                    {
                        addressInfo.TypeCode = new byte[] { 0xA8 };
                        addressInfo.BitType = 0x00;
                        addressInfo.Format = 10;
                        addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                        addressInfo.TypeChar = address.Substring(0, 1);
                    }
                    break;

                case 'W'://W链接寄存器
                    {
                        addressInfo.TypeCode = new byte[] { 0xB4 };
                        addressInfo.BitType = 0x00;
                        addressInfo.Format = 16;
                        addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                        addressInfo.TypeChar = address.Substring(0, 1);
                    }
                    break;

                case 'L'://L锁存继电器
                    {
                        addressInfo.TypeCode = new byte[] { 0x92 };
                        addressInfo.BitType = 0x01;
                        addressInfo.Format = 10;
                        addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                        addressInfo.TypeChar = address.Substring(0, 1);
                    }
                    break;

                case 'F'://F报警器
                    {
                        addressInfo.TypeCode = new byte[] { 0x93 };
                        addressInfo.BitType = 0x01;
                        addressInfo.Format = 10;
                        addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                        addressInfo.TypeChar = address.Substring(0, 1);
                    }
                    break;

                case 'V'://V边沿继电器
                    {
                        addressInfo.TypeCode = new byte[] { 0x94 };
                        addressInfo.BitType = 0x01;
                        addressInfo.Format = 10;
                        addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                        addressInfo.TypeChar = address.Substring(0, 1);
                    }
                    break;

                case 'B'://B链接继电器
                    {
                        addressInfo.TypeCode = new byte[] { 0xA0 };
                        addressInfo.BitType = 0x01;
                        addressInfo.Format = 16;
                        addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                        addressInfo.TypeChar = address.Substring(0, 1);
                    }
                    break;

                case 'R'://R文件寄存器
                    {
                        addressInfo.TypeCode = new byte[] { 0xAF };
                        addressInfo.BitType = 0x00;
                        addressInfo.Format = 10;
                        addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                        addressInfo.TypeChar = address.Substring(0, 1);
                    }
                    break;

                case 'S':
                    {
                        //累计定时器的线圈
                        if (address[1] == 'C')
                        {
                            addressInfo.TypeCode = new byte[] { 0xC6 };
                            addressInfo.BitType = 0x01;
                            addressInfo.Format = 10;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(2), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 2);
                        }
                        //累计定时器的触点
                        else if (address[1] == 'S')
                        {
                            addressInfo.TypeCode = new byte[] { 0xC7 };
                            addressInfo.BitType = 0x01;
                            addressInfo.Format = 10;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(2), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 2);
                        }
                        //累计定时器的当前值
                        else if (address[1] == 'N')
                        {
                            addressInfo.TypeCode = new byte[] { 0xC8 };
                            addressInfo.BitType = 0x00;
                            addressInfo.Format = 100;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(2), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 2);
                        }
                        // S步进继电器
                        else
                        {
                            addressInfo.TypeCode = new byte[] { 0x98 };
                            addressInfo.BitType = 0x01;
                            addressInfo.Format = 10;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 1);
                        }
                        break;
                    }
                case 'Z':
                    {
                        //文件寄存器ZR区
                        if (address[1] == 'R')
                        {
                            addressInfo.TypeCode = new byte[] { 0xB0 };
                            addressInfo.BitType = 0x00;
                            addressInfo.Format = 16;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(2), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 2);
                        }
                        //变址寄存器
                        else
                        {
                            addressInfo.TypeCode = new byte[] { 0xCC };
                            addressInfo.BitType = 0x00;
                            addressInfo.Format = 10;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 1);
                        }
                        break;
                    }
                case 'T':
                    {
                        // 定时器的当前值
                        if (address[1] == 'N')
                        {
                            addressInfo.TypeCode = new byte[] { 0xC2 };
                            addressInfo.BitType = 0x00;
                            addressInfo.Format = 10;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(2), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 2);
                        }
                        //定时器的触点
                        else if (address[1] == 'S')
                        {
                            addressInfo.TypeCode = new byte[] { 0xC1 };
                            addressInfo.BitType = 0x01;
                            addressInfo.Format = 10;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(2), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 2);
                        }
                        //定时器的线圈
                        else if (address[1] == 'C')
                        {
                            addressInfo.TypeCode = new byte[] { 0xC0 };
                            addressInfo.BitType = 0x01;
                            addressInfo.Format = 10;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(2), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 2);
                        }
                        break;
                    }
                case 'C':
                    {
                        //计数器的当前值
                        if (address[1] == 'N')
                        {
                            addressInfo.TypeCode = new byte[] { 0xC5 };
                            addressInfo.BitType = 0x00;
                            addressInfo.Format = 10;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(2), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 2);
                        }
                        //计数器的触点
                        else if (address[1] == 'S')
                        {
                            addressInfo.TypeCode = new byte[] { 0xC4 };
                            addressInfo.BitType = 0x01;
                            addressInfo.Format = 10;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(2), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 2);
                        }
                        //计数器的线圈
                        else if (address[1] == 'C')
                        {
                            addressInfo.TypeCode = new byte[] { 0xC3 };
                            addressInfo.BitType = 0x01;
                            addressInfo.Format = 10;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(2), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 2);
                        }
                        break;
                    }
            }
            return addressInfo;
        }

        /// <summary>
        /// A_1E地址解析
        /// </summary>
        /// <param name="address"></param>
        /// <param name="toUpper"></param>
        /// <returns></returns>
        private MitsubishiMCAddress ConvertArg_A_1E(string address, bool toUpper = true)
        {
            {
                if (toUpper) address = address.ToUpper();
                var addressInfo = new MitsubishiMCAddress();
                switch (address[0])
                {
                    case 'X'://X输入寄存器
                        {
                            addressInfo.TypeCode = new byte[] { 0x58, 0x20 };
                            addressInfo.BitType = 0x01;
                            addressInfo.Format = 8;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 1);
                        }
                        break;

                    case 'Y'://Y输出寄存器
                        {
                            addressInfo.TypeCode = new byte[] { 0x59, 0x20 };
                            addressInfo.BitType = 0x01;
                            addressInfo.Format = 8;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 1);
                        }
                        break;

                    case 'M'://M中间寄存器
                        {
                            addressInfo.TypeCode = new byte[] { 0x4D, 0x20 };
                            addressInfo.BitType = 0x01;
                            addressInfo.Format = 10;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 1);
                        }
                        break;

                    case 'S'://S状态寄存器
                        {
                            addressInfo.TypeCode = new byte[] { 0x53, 0x20 };
                            addressInfo.BitType = 0x01;
                            addressInfo.Format = 10;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 1);
                        }
                        break;

                    case 'D'://D数据寄存器
                        {
                            addressInfo.TypeCode = new byte[] { 0x44, 0x20 };
                            addressInfo.BitType = 0x00;
                            addressInfo.Format = 10;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 1);
                        }
                        break;

                    case 'R'://R文件寄存器
                        {
                            addressInfo.TypeCode = new byte[] { 0x52, 0x20 };
                            addressInfo.BitType = 0x00;
                            addressInfo.Format = 10;
                            addressInfo.BeginAddress = Convert.ToInt32(address.Substring(1), addressInfo.Format);
                            addressInfo.TypeChar = address.Substring(0, 1);
                        }
                        break;
                }
                return addressInfo;
            }
        }

        #endregion 地址解析

        ///// <summary>
        ///// 获取地址的区域类型
        ///// </summary>
        ///// <param name="address"></param>
        ///// <returns></returns>
        //private string GetAddressType(string address)
        //{
        //    if (address.Length < 2)
        //        throw new Exception("address格式不正确");

        //    if ((address[1] >= 'A' && address[1] <= 'Z') ||
        //        (address[1] >= 'a' && address[1] <= 'z'))
        //        return address.Substring(0, 2);
        //    else
        //        return address.Substring(0, 1);
        //}

        #endregion private

        #region IIoTBase

        public IoTResult<T> Read<T>(string address)
        {
            var tType = typeof(T);
            if (tType == typeof(bool))
            {
                var readResut = ReadBoolean(address);
                return new IoTResult<T>(readResut, (T)(object)readResut.Value);
            }
            if (tType == typeof(byte))
            {
                var r = ReadValue(address, 1, (b, i) => b[i]);
                return new IoTResult<T>(r, (T)(object)r.Value);
            }
            if (tType == typeof(sbyte))
            {
                var r = ReadValue(address, 1, (b, i) => (sbyte)b[i]);
                return new IoTResult<T>(r, (T)(object)r.Value);
            }
            if (tType == typeof(short))
            {
                var r = ReadValue(address, 2, BitConverter.ToInt16);
                return new IoTResult<T>(r, (T)(object)r.Value);
            }
            if (tType == typeof(ushort))
            {
                var r = ReadValue(address, 2, BitConverter.ToUInt16);
                return new IoTResult<T>(r, (T)(object)r.Value);
            }
            if (tType == typeof(int))
            {
                var r = ReadValue(address, 4, BitConverter.ToInt32);
                return new IoTResult<T>(r, (T)(object)r.Value);
            }
            if (tType == typeof(uint))
            {
                var r = ReadValue(address, 4, BitConverter.ToUInt32);
                return new IoTResult<T>(r, (T)(object)r.Value);
            }
            if (tType == typeof(long))
            {
                var r = ReadValue(address, 8, BitConverter.ToInt64);
                return new IoTResult<T>(r, (T)(object)r.Value);
            }
            if (tType == typeof(ulong))
            {
                var r = ReadValue(address, 8, BitConverter.ToUInt64);
                return new IoTResult<T>(r, (T)(object)r.Value);
            }
            if (tType == typeof(float))
            {
                var r = ReadValue(address, 4, BitConverter.ToSingle);
                return new IoTResult<T>(r, (T)(object)r.Value);
            }
            if (tType == typeof(double))
            {
                var r = ReadValue(address, 8, BitConverter.ToDouble);
                return new IoTResult<T>(r, (T)(object)r.Value);
            }
            throw new NotImplementedException("暂不支持的类型");
        }

        public IoTResult<IEnumerable<T>> Read<T>(string address, int number)
        {
            var tType = typeof(T);
            if (tType == typeof(bool))
            {
                var readResut = ReadBoolean(address, (ushort)number);
                return new IoTResult<IEnumerable<T>>(readResut, (IEnumerable<T>)(object)readResut.Value);
            }
            if (tType == typeof(byte))
            {
                var r = ReadValues(address, (ushort)number, 1, (b, i) => b[i]);
                return new IoTResult<IEnumerable<T>>(r, (IEnumerable<T>)(object)r.Value);
            }
            if (tType == typeof(sbyte))
            {
                var r = ReadValues(address, (ushort)number, 1, (b, i) => (sbyte)b[i]);
                return new IoTResult<IEnumerable<T>>(r, (IEnumerable<T>)(object)r.Value);
            }
            if (tType == typeof(short))
            {
                var r = ReadValues(address, (ushort)number, 2, BitConverter.ToInt16);
                return new IoTResult<IEnumerable<T>>(r, (IEnumerable<T>)(object)r.Value);
            }
            if (tType == typeof(ushort))
            {
                var r = ReadValues(address, (ushort)number, 2, BitConverter.ToUInt16);
                return new IoTResult<IEnumerable<T>>(r, (IEnumerable<T>)(object)r.Value);
            }
            if (tType == typeof(int))
            {
                var r = ReadValues(address, (ushort)number, 4, BitConverter.ToInt32);
                return new IoTResult<IEnumerable<T>>(r, (IEnumerable<T>)(object)r.Value);
            }
            if (tType == typeof(uint))
            {
                var r = ReadValues(address, (ushort)number, 4, BitConverter.ToUInt32);
                return new IoTResult<IEnumerable<T>>(r, (IEnumerable<T>)(object)r.Value);
            }
            if (tType == typeof(long))
            {
                var r = ReadValues(address, (ushort)number, 8, BitConverter.ToInt64);
                return new IoTResult<IEnumerable<T>>(r, (IEnumerable<T>)(object)r.Value);
            }
            if (tType == typeof(ulong))
            {
                var r = ReadValues(address, (ushort)number, 8, BitConverter.ToUInt64);
                return new IoTResult<IEnumerable<T>>(r, (IEnumerable<T>)(object)r.Value);
            }
            if (tType == typeof(float))
            {
                var r = ReadValues(address, (ushort)number, 4, BitConverter.ToSingle);
                return new IoTResult<IEnumerable<T>>(r, (IEnumerable<T>)(object)r.Value);
            }
            if (tType == typeof(double))
            {
                var r = ReadValues(address, (ushort)number, 8, BitConverter.ToDouble);
                return new IoTResult<IEnumerable<T>>(r, (IEnumerable<T>)(object)r.Value);
            }
            throw new NotImplementedException("暂不支持的类型");
        }

        public IoTResult<string> ReadString(string address, int length, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public IoTResult Write<T>(string address, T value)
        {
            if (value is bool boolv)
            {
                // bool 类型有特殊 bit 写法，单独处理
                return Write(address, boolv);
            }
            else if (value is byte bytev)
            {
                return WriteValue(address, bytev, v => BitConverter.GetBytes(v));
            }
            else if (value is sbyte sbytev)
            {
                return WriteValue(address, sbytev, v => BitConverter.GetBytes(v));
            }
            else if (value is float floatv)
            {
                return WriteValue(address, floatv, v => BitConverter.GetBytes(v));
            }
            else if (value is double doublev)
            {
                return WriteValue(address, doublev, BitConverter.GetBytes);
            }
            else if (value is short Int16v)
            {
                return WriteValue(address, Int16v, BitConverter.GetBytes);
            }
            else if (value is int Int32v)
            {
                return WriteValue(address, Int32v, BitConverter.GetBytes);
            }
            else if (value is long Int64v)
            {
                return WriteValue(address, Int64v, BitConverter.GetBytes);
            }
            else if (value is ushort UInt16v)
            {
                return WriteValue(address, UInt16v, BitConverter.GetBytes);
            }
            else if (value is uint UInt32v)
            {
                return WriteValue(address, UInt32v, BitConverter.GetBytes);
            }
            else if (value is ulong UInt64v)
            {
                return WriteValue(address, UInt64v, BitConverter.GetBytes);
            }
            else if (value is string Stringv)
            {
                return Write(address, Stringv);
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

        public IoTResult Write<T>(string address, params T[] value)
        {
            if (value == null || value.Length == 0)
                return new IoTResult() { IsSucceed = false };

            var tType = typeof(T);

            if (tType == typeof(bool))
            {
                var boolArray = value as bool[];
                if (boolArray == null)
                    return new IoTResult() { IsSucceed = false };

                IoTResult lastResult = null;
                for (int i = 0; i < boolArray.Length; i++)
                {
                    string nextAddress = GetOffsetAddress(address, i);
                    lastResult = Write(nextAddress, boolArray[i]);
                    if (!lastResult.IsSucceed)
                        return lastResult;
                }
                return lastResult ?? new IoTResult() { IsSucceed = false };
            }
            else
            {
                // ...existing code...
                Func<T, byte[]> converter = null;
                if (tType == typeof(byte))
                    converter = v => BitConverter.GetBytes((byte)(object)v);
                else if (tType == typeof(sbyte))
                    converter = v => BitConverter.GetBytes((sbyte)(object)v);
                else if (tType == typeof(short))
                    converter = v => BitConverter.GetBytes((short)(object)v);
                else if (tType == typeof(ushort))
                    converter = v => BitConverter.GetBytes((ushort)(object)v);
                else if (tType == typeof(int))
                    converter = v => BitConverter.GetBytes((int)(object)v);
                else if (tType == typeof(uint))
                    converter = v => BitConverter.GetBytes((uint)(object)v);
                else if (tType == typeof(long))
                    converter = v => BitConverter.GetBytes((long)(object)v);
                else if (tType == typeof(ulong))
                    converter = v => BitConverter.GetBytes((ulong)(object)v);
                else if (tType == typeof(float))
                    converter = v => BitConverter.GetBytes((float)(object)v);
                else if (tType == typeof(double))
                    converter = v => BitConverter.GetBytes((double)(object)v);
                else
                    throw new NotImplementedException("暂不支持的类型");

                return WriteValues(address, value, converter);
            }
        }

        /// <summary>
        /// 生成偏移后的PLC地址（如 M101, X1F 等）
        /// </summary>
        /// <param name="address">原始地址</param>
        /// <param name="offset">偏移量</param>
        /// <returns>偏移后的地址</returns>
        private string GetOffsetAddress(string address, int offset)
        {
            if (string.IsNullOrEmpty(address) || address.Length < 2)
                throw new ArgumentException("address格式不正确");

            // 处理两位前缀（如ZR、TN、TS、TC、CN、CS、CC）
            string prefix = address.Substring(0, 1);
            int startIndex = 1;
            if (address.Length >= 2 && char.IsLetter(address[1]))
            {
                prefix = address.Substring(0, 2);
                startIndex = 2;
            }
            string numPart = address.Substring(startIndex);

            // 判断进制
            int number = 0;
            int numberBase = 10;
            switch (prefix)
            {
                case "X":
                case "Y":
                case "B":
                case "W":
                case "ZR":
                    numberBase = 16;
                    break;

                default:
                    numberBase = 10;
                    break;
            }
            number = Convert.ToInt32(numPart, numberBase);
            int newNumber = number + offset;
            string newNumPart = numberBase == 16 ? newNumber.ToString("X") : newNumber.ToString();
            return prefix + newNumPart;
        }

        #endregion IIoTBase
    }
}