using Ping9719.IoT;
using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Scanner
{
    /// <summary>
    /// 民德扫码器（支持一维码，二维码，主机模式，等...）
    /// </summary>
    public class MindeoScanner : IScannerBase
    {
        int Ver = 1;
        public ClientBase Client { get; private set; }
        public MindeoScanner(ClientBase client, int timeout = 1500, int ver = 1)
        {
            Client = client;
            Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            Client.TimeOut = timeout;
            Client.ConnectionMode = ConnectionMode.AutoOpen;

            Ver = ver;
        }
        public MindeoScanner(string ip, int port = 8899) : this(new TcpClient(ip, port)) { }
        public MindeoScanner(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One) : this(new SerialPortClient(portName, baudRate, parity, dataBits, stopBits)) { }

        /// <summary>
        /// 在外部触发模式下执行一次
        /// </summary>
        /// <param name="timeout">保持时长（毫秒），填写解码设置：触发超时时间设置的值</param>
        /// <returns></returns>
        public IoTResult<string> ReadOne()
        {
            var info = Ver == 1 ? new byte[] { 0x16, 0x54, 0x0D } : new byte[] { 0x16, 0x4D, 0x0D, 0x16, 0x54, 0x0D, 0x2E };
            var aa = Client.SendReceive(info);
            return aa.IsSucceed ? aa.ToVal<string>(Encoding.ASCII.GetString(aa.Value)) : aa.ToVal<string>();
        }

        /// <summary>
        /// 取消扫描
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public IoTResult ReadCancel()
        {
            return Client.SendReceive(new byte[] { 0x16, 0x55, 0x0D });
        }
    }
}
