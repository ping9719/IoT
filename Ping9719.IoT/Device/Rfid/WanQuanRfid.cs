using Ping9719.IoT;
using Ping9719.IoT.Algorithm;
using Ping9719.IoT.Common;
using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Rfid
{
    /// <summary>
    /// 万全Rfid
    /// </summary>
    public class WanQuanRfid
    {
        //设备地址 
        private byte stationNumber = 1;

        public ClientBase Client { get; private set; }
        public WanQuanRfid(ClientBase client, byte stationNumber = 0x01, int timeout = 1500)
        {
            Client = client;
            Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            Client.TimeOut = timeout;
            Client.ConnectionMode = ConnectionMode.AutoOpen;

            this.stationNumber = stationNumber;
        }
        public WanQuanRfid(string ip, int port) : this(new TcpClient(ip, port)) { }
        public WanQuanRfid(string portName, int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One) : this(new SerialPortClient(portName, baudRate, parity, dataBits, stopBits)) { }

        /// <summary>
        /// 【命令模式】读取标签 传入天线号 返回对应天线读取的EPC编号字符串
        /// </summary>
        /// <param name="antNumber"></param>
        /// <returns></returns>
        public IoTResult<string> Read(int antNumber = 1)
        {
            var startAddress = 5100;
            switch (antNumber)
            {
                case 1:
                    startAddress = 5100;
                    break;
                case 2:
                    startAddress = 5200;
                    break;
                case 3:
                    startAddress = 5300;
                    break;
                case 4:
                    startAddress = 5400;
                    break;
            }

            byte[] bytes = new byte[6];
            bytes[0] = stationNumber;//设备地址
            bytes[1] = 0x03;//功能码
            bytes[2] = BitConverter.GetBytes(startAddress)[1];
            bytes[3] = BitConverter.GetBytes(startAddress)[0];//寄存器地址
            bytes[4] = BitConverter.GetBytes(34)[1];
            bytes[5] = BitConverter.GetBytes(34)[0];//表示request 寄存器的长度(寄存器个数)
            var commandCRC16 = CRC.Crc16(bytes.ToArray());
            var sendResult = Client.SendReceive(commandCRC16);
            if (!sendResult.IsSucceed)
            {
                sendResult.Value = new byte[] { };
                return new IoTResult<string>().AddError("读取失败");
            }
            //返回数据设备地址（1 byte）  功能码（1 byte） 字节数（2 byte） 寄存器值（64 bytes）   CRC（2 bytes）
            //&& sendResult.Value[2] != number * 2
            if (sendResult.Value.Length != 5 + 34 * 2)
            {
                return new IoTResult<string>().AddError("读取失败,设备返回数据长度验证不合格");
            }
            if (!CRC.CheckCrc16(sendResult.Value))
            {
                return new IoTResult<string>().AddError("读取失败,设备返回数据CRC验证不合格");
            }
            sendResult.Value = sendResult.Value.Skip(4).Take(64).ToArray();
            var data = sendResult.Value;
            if (data[0] == 0x00)
            {
                return new IoTResult<string>().AddError("读取失败,设备未读取到有效标签");
            }
            var epcLen = (int)data[2];/*BitConverter.ToInt16(new byte[] { data[2], data[3] }, 0);*/
            var epcData = data.Skip(3).Take(epcLen).ToArray();
            var epcStr = epcData.ByteArrayToString();
            return new IoTResult<string>
            {
                IsSucceed = true,
                Value = epcStr
            };
        }

        /// <summary>
        /// 写入标签
        /// </summary>
        /// <param name="values">需要写入的数据 长度应为4的倍数；每个字符都是16进制的数字或字母</param>
        /// <param name="antNumber">写标签的天线号 天线号由控制器接口决定</param>
        /// <param name="length">写入数据长度（字节） 如果不传默认为传入数据长度的一半</param>
        /// <param name="startAdress"></param>
        /// <returns></returns>
        public IoTResult Write(string values, int antNumber, int length = -1, int startAdress = 4)
        {
            //values值校验 长度应为4的倍数；每个字符都是16进制的数字或字母
            if (string.IsNullOrEmpty(values) || values.Length % 4 != 0)
            {
                var result = new IoTResult<string>();
                result.AddError("输入长度错误，应为4的倍数");
                return result;
            }

            string str = "[0-9a-fA-F]$";
            if (!Regex.IsMatch(values, str))
            {
                var result = new IoTResult<string>();
                result.AddError("输入字符错误,应为0~9或A~F");
                return result;
            }
            if (length == -1)
            {
                length = values.Length / 2;
            }

            byte[] value = values.StringToByteArray(false);
            byte[] bytes = new byte[19 + value.Length];
            bytes[0] = 0x01;//设备地址
            bytes[1] = 0x10;//功能码
            bytes[2] = 0x10;//寄存器地址4100
            bytes[3] = 0x04;//寄存器地址4100
            int writeLen = value.Length / 2 + 6;
            bytes[4] = BitConverter.GetBytes(writeLen)[1];
            bytes[5] = BitConverter.GetBytes(writeLen)[0];//写入长度
            bytes[6] = Convert.ToByte(writeLen * 2);//数据长度
            bytes[7] = BitConverter.GetBytes(antNumber)[1]; //写入天线
            bytes[8] = BitConverter.GetBytes(antNumber)[0]; //写入天线

            bytes[9] = BitConverter.GetBytes(0)[3]; //标签访问密码
            bytes[10] = BitConverter.GetBytes(0)[2]; //标签访问密码
            bytes[11] = BitConverter.GetBytes(0)[1]; //标签访问密码
            bytes[12] = BitConverter.GetBytes(0)[0]; //标签访问密码
            bytes[13] = 0x00;
            bytes[14] = 0x01;//写入EPC
            bytes[15] = BitConverter.GetBytes(startAdress)[1];//写入起始地址
            bytes[16] = BitConverter.GetBytes(startAdress)[0];//写入起始地址
            bytes[17] = BitConverter.GetBytes(length)[1];//写入长度
            bytes[18] = BitConverter.GetBytes(length)[0];//写入长度
            for (int i = 0; i < value.Length; i++)
            {
                bytes[19 + i] = value[i];
            }

            var commandCRC16 = CRC.Crc16(bytes.ToArray());
            var sendResult = Client.SendReceive(commandCRC16);
            if (!sendResult.IsSucceed)
            {
                sendResult.Value = new byte[] { };
                return sendResult;
            }
            if (sendResult.Value.Length != 8)
            {
                return sendResult.AddError("数据长度验证不合格");
            }
            if (!CRC.CheckCrc16(sendResult.Value))
            {
                return sendResult.AddError("数据CRC16验证不合格");
            }
            //前6字节与发送一致
            for (int i = 0; i < 6; i++)
            {
                if (sendResult.Value[i] != commandCRC16[i])
                {
                    sendResult.Value = new byte[] { };
                    return sendResult.AddError("写入失败");
                }
            }
            //写入成功
            return sendResult;
        }
    }
}
