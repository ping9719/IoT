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
    /// 霍尼韦尔扫码器（支持HF800，等...）
    /// 必须去下载软件“DataMax”去设置‘工作模式’为‘外部触发’；解码设置：触发超时时间设置为5000。
    /// 官网：https://sps.honeywell.com/us/en/products/productivity/barcode-scanners/fixed-mount/hf800
    /// 文档：https://prod-edam.honeywell.com/content/dam/honeywell-edam/sps/ppr/en-us/public/products/barcode-scanners/fixed-mount/hf800/sps-ppr-hf800-en-qs.pdf?download=false
    /// 下载软件：https://hsmftp.honeywell.com/
    /// </summary>
    public class HoneywellScanner : IScannerBase, IClient
    {
        public ClientBase Client { get; private set; }
        public HoneywellScanner(ClientBase client)
        {
            Client = client;
            //Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            //Client.TimeOut = timeout;
            //Client.ConnectionMode = ConnectionMode.AutoOpen;
        }
        public HoneywellScanner(string ip, int port = 55256) : this(new TcpClient(ip, port)) { }
        public HoneywellScanner(string portName, int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One) : this(new SerialPortClient(portName, baudRate, parity, dataBits, stopBits)) { }


        /// <summary>
        /// 在外部触发模式下执行一次
        /// </summary>
        /// <param name="timeout">保持时长（毫秒），填写解码设置：触发超时时间设置的值</param>
        /// <returns></returns>
        public IoTResult<string> ReadOne()
        {
            return Client.SendReceive("TRIGGER");
        }

        /// <summary>
        /// 取消扫描
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public IoTResult<string> ReadCancel()
        {
            return Client.SendReceive("UNTRIG");
        }
    }
}
