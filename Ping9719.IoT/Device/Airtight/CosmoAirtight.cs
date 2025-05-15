using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Airtight
{
    /// <summary>
    /// 科斯莫气密检测
    /// </summary>
    public class CosmoAirtight
    {
        public ClientBase Client { get; private set; }//通讯管道

        public CosmoAirtight(ClientBase client)
        {
            Client = client;
            Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            Client.ConnectionMode = ConnectionMode.AutoOpen;
        }

        public CosmoAirtight(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, Handshake handshake = Handshake.RequestToSend) : this(new SerialPortClient(portName, baudRate, parity, dataBits, stopBits, handshake)) { }

        /// <summary>
        /// 启动
        /// </summary>
        public IoTResult<string> Start()
        {
            string comm = $"STT\r\n";
            try
            {
                return Client.SendReceive(comm);
            }
            catch (Exception ex)
            {
                return IoTResult.Create<string>().AddError(ex);
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public IoTResult Stop()
        {
            string comm = $"STP\r\n";
            try
            {
                return Client.SendReceive(comm);
            }
            catch (Exception ex)
            {
                return IoTResult.Create().AddError(ex);
            }
        }

        /// <summary>
        /// 读取测试数据
        /// </summary>
        public IoTResult<string> ReadTestData()
        {
            string comm = $"RLD\r\n";
            try
            {
                return Client.SendReceive(comm);
            }
            catch (Exception ex)
            {
                return IoTResult.Create<string>().AddError(ex);
            }
        }

        /// <summary>
        /// 设置频道
        /// </summary>
        /// <param name="channel">0-15</param>
        /// <returns></returns>
        public IoTResult SetChannel(int channel)
        {
            string comm = $"WCHN_{channel.ToString().PadLeft(2, '0')}\r\n";
            try
            {
                return Client.SendReceive(comm);
            }
            catch (Exception ex)
            {
                return IoTResult.Create().AddError(ex);
            }
        }
    }
}
