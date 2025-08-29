using Ping9719.IoT;
using Ping9719.IoT.Common;
using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Rfid
{
    /// <summary>
    /// 泰和森
    /// </summary>
    public class TaiHeSenRfid : IClient
    {
        /// <summary>
        /// 字节格式
        /// </summary>
        public EndianFormat format { get; set; } = EndianFormat.BADC;
        public ClientBase Client { get; private set; }
        public TaiHeSenRfid(ClientBase client)
        {
            Client = client;
            //Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            //Client.TimeOut = timeout;
            Client.ConnectionMode = ConnectionMode.AutoOpen;
        }
        public TaiHeSenRfid(string ip, int port = 4000) : this(new TcpClient(ip, port)) { }
        public TaiHeSenRfid(string portName, int baudRate, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One) : this(new SerialPortClient(portName, baudRate, parity, dataBits, stopBits)) { }

        /// <summary>
        /// 读取
        /// </summary>
        /// <returns></returns>
        public IoTResult<T> Read<T>()
        {
            byte[] ReadLabelMessage = new byte[7] { 0xAA, 0xFF, 0x22, 0x00, 0x00, 0x21, 0xBB };//发送读值指令，固定编码格式
            IoTResult<T> result = new IoTResult<T>();
            try
            {
                var retValue_Send = Client.SendReceive(ReadLabelMessage);
                if (!retValue_Send.IsSucceed)
                    return new IoTResult<T>(retValue_Send).ToEnd();

                if (retValue_Send.Value.Length <= 8 || retValue_Send.Value[2] == 0xFF)  //根据ths协议，读取失败有固定的编码格式
                {
                    return new IoTResult<T>(retValue_Send).AddError("读取失败，未读取到RFID信息").ToEnd();
                }

                int datalength = retValue_Send.Value[4] - 5;//实际数据长度
                Byte[] byte1 = retValue_Send.Value.Skip(8).Take(datalength).ToArray();

                if (typeof(T) == typeof(byte[]))
                    result.Value = (T)(object)byte1;
                else if (typeof(T) == typeof(Int16))
                    result.Value = (T)(object)BitConverter.ToInt16(byte1.ToByteFormat(format), 0);
                else if (typeof(T) == typeof(UInt16))
                    result.Value = (T)(object)BitConverter.ToUInt16(byte1.ToByteFormat(format), 0);
                else if (typeof(T) == typeof(Int32))
                    result.Value = (T)(object)BitConverter.ToInt32(byte1.ToByteFormat(format), 0);
                else if (typeof(T) == typeof(UInt32))
                    result.Value = (T)(object)BitConverter.ToUInt32(byte1.ToByteFormat(format), 0);
                else if (typeof(T) == typeof(string))
                    result.Value = (T)(object)Client.Encoding.GetString(byte1);
                else
                {
                    result.IsSucceed = false;
                    result.AddError("不支持的类型");
                }
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 写入
        /// </summary>
        public IoTResult Write<T>(T value)
        {
            IoTResult result = new IoTResult();
            byte[] byte1 = null;
            if (value is string vstr)
            {
                if (vstr.Length > 12 || vstr.Length < 2)
                {
                    return result.AddError("写入字符串长度必须为2-12字");
                }
                if (vstr.Length % 2 != 0)
                {
                    return result.AddError("写入字符串长度必须为偶数");
                }
                byte1 = Client.Encoding.GetBytes(vstr);
            }
            else if (value is byte[] bytes)
            {
                if (bytes.Length > 12 || bytes.Length < 2)
                {
                    return result.AddError("写入字符串长度必须为2-12字");
                }
                if (bytes.Length % 2 != 0)
                {
                    return result.AddError("写入字符串长度必须为偶数");
                }
                byte1 = bytes;
            }
            else if (value is Int16 Int16)
                byte1 = BitConverter.GetBytes(Int16);
            else if (value is UInt16 UInt16)
                byte1 = BitConverter.GetBytes(UInt16);
            else if (value is Int32 Int32)
                byte1 = BitConverter.GetBytes(Int32);
            else if (value is UInt32 UInt32)
                byte1 = BitConverter.GetBytes(UInt32);
            else
            {
                return result.AddError("不支持的类型");
            }


            //组合成发送指令码
            List<byte> WriteLabelMessage = new List<byte>
            {
                0xAA, 0xFF, 0x51, //命令
                0x00, (byte)(11+byte1.Length), //长度
                0x00, 0x00, 0x00, 0x00,//密码
                0x01, //EPC
                0x00, 0x01, 0x00, (byte)(byte1.Length/2+1),//偏移地址+数据长度
                (byte)(byte1.Length*4), 0x00, //位数
            };  //组合标签信息

            WriteLabelMessage.AddRange(byte1);
            var aaa = (byte)WriteLabelMessage.Skip(1).Sum(o => o);//检验吗
            WriteLabelMessage.Add(aaa);
            WriteLabelMessage.Add(0xBB);

            try
            {
                var retValue = Client.SendReceive(WriteLabelMessage.ToArray());
                if (!retValue.IsSucceed)
                    return retValue.ToEnd();

                if (retValue.Value.Length <= 8 || retValue.Value[2] == 0xFF)  //根据ths协议，写失败有固定的编码格式
                {
                    return new IoTResult<string>(retValue).AddError("写入失败").ToEnd();
                }
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

    }
}
