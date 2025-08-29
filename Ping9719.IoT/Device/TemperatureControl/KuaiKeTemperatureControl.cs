using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ping9719.IoT.Algorithm;
using Ping9719.IoT.Communication;
using Ping9719.IoT;

namespace Ping9719.IoT.Device.TemperatureControl
{
    /// <summary>
    /// 快克温控（支持378FA等）（请注意站点地址！）
    /// 378FA通讯协议.docx
    /// 快克_通用串口通讯协议.docx
    /// </summary>
    public class KuaiKeTemperatureControl : IClient
    {
        protected EndianFormat format;
        private byte stationNumber = 1;

        public ClientBase Client { get; private set; }
        public KuaiKeTemperatureControl(ClientBase client, EndianFormat format = EndianFormat.BADC, byte stationNumber = 20)
        {
            Client = client;
            //Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            //Client.TimeOut = timeout;
            //Client.IsAutoOpen = true;
            Client.IsAutoDiscard = true;

            this.format = format;
            this.stationNumber = stationNumber;
        }
        public KuaiKeTemperatureControl(string ip, int port = 55256) : this(new TcpClient(ip, port)) { }
        public KuaiKeTemperatureControl(string portName, int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One) : this(new SerialPortClient(portName, baudRate, parity, dataBits, stopBits)) { }


        public IoTResult<byte[]> Read(int address, int number)
        {
            try
            {
                byte[] bytes = new byte[4];
                bytes[0] = stationNumber;
                bytes[1] = 0x67;
                bytes[2] = Convert.ToByte(address);
                bytes[3] = Convert.ToByte(number);
                var commandCRC16 = CRC.Crc16(bytes.ToArray());
                var sendResult = Client.SendReceive(commandCRC16);
                if (!sendResult.IsSucceed)
                {
                    sendResult.Value = new byte[] { };
                    return sendResult;
                }
                if (sendResult.Value.Length != 6 + number)
                {
                    sendResult.Value = new byte[] { };
                    sendResult.AddError("数据长度验证不合格");
                    return sendResult;
                }
                if (!CRC.CheckCrc16(sendResult.Value))
                {
                    sendResult.Value = new byte[] { };
                    sendResult.AddError("数据CRC16验证不合格");
                    return sendResult;
                }

                sendResult.Value = sendResult.Value.Skip(4).Take(number).ToArray();
                return sendResult;

            }
            catch (Exception ex)
            {
                return new IoTResult<byte[]>().AddError(ex);
            }
        }

        public IoTResult Write(int address, params byte[] values)
        {
            try
            {

                byte[] bytes = new byte[4 + values.Length];
                bytes[0] = stationNumber;
                bytes[1] = 0x68;
                bytes[2] = Convert.ToByte(address);
                bytes[3] = Convert.ToByte(values.Length);
                Array.Copy(values, 0, bytes, 4, values.Length);

                var commandCRC16 = CRC.Crc16(bytes.ToArray());
                var sendResult = Client.SendReceive(commandCRC16);

                if (!sendResult.IsSucceed)
                {
                    sendResult.Value = new byte[] { };
                    return sendResult;
                }
                if (sendResult.Value.Length != 6 + values.Length)
                {
                    sendResult.Value = new byte[] { };
                    sendResult.AddError("数据长度验证不合格");
                    return sendResult;
                }
                if (!CRC.CheckCrc16(sendResult.Value))
                {
                    sendResult.Value = new byte[] { };
                    sendResult.AddError("数据CRC16验证不合格");
                    return sendResult;
                }

                return sendResult;
            }
            catch (Exception ex)
            {
                return new IoTResult<byte[]>().AddError(ex);
            }
        }

        /// <summary>
        /// 读取温度信息
        /// </summary>
        /// <returns></returns>
        public IoTResult<KuaiKeTemperatureControlInfo_378FA> ReadTemperatureControlInfo()
        {
            var re = Read(2, 15);
            if (!re.IsSucceed)
                return new IoTResult<KuaiKeTemperatureControlInfo_378FA>(re);

            IoTResult<KuaiKeTemperatureControlInfo_378FA> result = new IoTResult<KuaiKeTemperatureControlInfo_378FA>() { Value = new KuaiKeTemperatureControlInfo_378FA() };

            var info1 = re.Value[0].ByteToBinaryBoolArray();
            result.Value.开机 = info1[0];
            result.Value.休眠 = !info1[1];

            var info2 = re.Value[1].ByteToBinaryBoolArray();
            result.Value.堵料报警 = info2[0];
            result.Value.传感器异常 = info2[1];
            result.Value.缺料报警 = info2[2];
            result.Value.发热芯异常 = info2[3];
            result.Value.温度高 = info2[5];
            result.Value.温度低 = info2[6];
            result.Value.发热芯过热保护 = info2[7];

            result.Value.当前温度 = BitConverter.ToInt16(re.Value, 2);
            result.Value.设定温度 = BitConverter.ToInt16(re.Value, 4);
            result.Value.密码 = BitConverter.ToInt32(re.Value, 6);
            result.Value.报警温度上限 = re.Value[10];
            result.Value.报警温度下限 = re.Value[11];
            result.Value.休眠时间 = re.Value[12];
            result.Value.关机时间 = re.Value[13];
            result.Value.工作模式 = re.Value[14];

            return result;
        }

        /// <summary>
        /// 写入温度
        /// </summary>
        /// <param name="temp">温度范围50-500</param>
        /// <returns></returns>
        public IoTResult WriteTemperature(int temp)
        {
            if (temp < 50 || temp > 500)
                return new IoTResult().AddError("设置温度范围50-500") ;

            return Write(0x06, BitConverter.GetBytes(Convert.ToInt16(temp)));
        }

        /// <summary>
        /// 设置开机休眠
        /// </summary>
        /// <param name="isXiu">true：开机休眠，false：开机不休眠</param>
        /// <returns></returns>
        public IoTResult WriteXiu(bool isXiu)
        {
            return Write(0x02, Convert.ToByte(isXiu ? 1 : 3));
        }

    }

    public class KuaiKeTemperatureControlInfo_378FA
    {
        public bool 开机 { get; set; }
        public bool 休眠 { get; set; }

        public bool 堵料报警 { get; set; }
        public bool 传感器异常 { get; set; }
        public bool 缺料报警 { get; set; }
        public bool 发热芯异常 { get; set; }
        public bool 温度高 { get; set; }
        public bool 温度低 { get; set; }
        public bool 发热芯过热保护 { get; set; }

        public int 当前温度 { get; set; }
        /// <summary>
        /// 温度范围：50-500摄氏度
        /// </summary>
        public int 设定温度 { get; set; }
        public int 密码 { get; set; }
        public int 报警温度上限 { get; set; }
        public int 报警温度下限 { get; set; }
        /// <summary>
        /// 0：不休眠；1-99：1到99分钟内不激活休眠；
        /// </summary>
        public int 休眠时间 { get; set; }
        /// <summary>
        /// 0：休眠后不关机；1-99：休眠后1到99分钟内不激活关机；
        /// </summary>
        public int 关机时间 { get; set; }
        /// <summary>
        /// 数据范围：00-05；10-15；
        /// </summary>
        public int 工作模式 { get; set; }

    }
}
