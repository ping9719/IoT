using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// 串口客户端
    /// </summary>
    public class SerialPortClient : ClientBase
    {
        /// <summary>
        /// 得到所有的设备
        /// </summary>
        public static string[] GetNames => System.IO.Ports.SerialPort.GetPortNames();
        public override bool IsOpen => base.IsOpen && (serialPort?.IsOpen ?? false);
        string portName; int baudRate; Parity parity = Parity.None; int dataBits = 8; StopBits stopBits = StopBits.One; Handshake handshake = Handshake.None;
        private System.IO.Ports.SerialPort serialPort;

        /// <summary>
        /// 初始化 串口客户端
        /// </summary>
        /// <param name="connectString">比如：COM7-19200-N-8-1。[串口号,COM开头]-[波特率,100以上]-[校验位,N,O,E,M,S]-[数据位,5-8]-[停止位,1,2,1.5/3]-[流控制,HN,HX,HR,HRX]</param>
        public SerialPortClient(string connectString)
        {
            this.portName = "COM0";
            this.baudRate = 9600;
            this.dataBits = 8;
            this.stopBits = StopBits.One;
            this.parity = Parity.None;
            this.handshake = Handshake.None;

            Ini();

            if (string.IsNullOrWhiteSpace(connectString))
                return;

            foreach (string item in connectString.Split(new char[] { '-', '-', ',', '，', ';', '；' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (item.ToUpper().StartsWith("COM"))
                    this.portName = item.ToUpper();
                else if (item.ToUpper().StartsWith("H"))
                {
                    if (item.ToUpper().StartsWith("HN"))
                        this.handshake = Handshake.None;
                    else if (item.ToUpper().StartsWith("HX"))
                        this.handshake = Handshake.XOnXOff;
                    else if (item.ToUpper().StartsWith("HR"))
                        this.handshake = Handshake.RequestToSend;
                    else if (item.ToUpper().StartsWith("HRX"))
                        this.handshake = Handshake.RequestToSendXOnXOff;
                }
                else if (item.ToUpper().StartsWith("N"))
                    this.parity = Parity.None;
                else if (item.ToUpper().StartsWith("O"))
                    this.parity = Parity.Odd;
                else if (item.ToUpper().StartsWith("E"))
                    this.parity = Parity.Even;
                else if (item.ToUpper().StartsWith("M"))
                    this.parity = Parity.Mark;
                else if (item.ToUpper().StartsWith("S"))
                    this.parity = Parity.Space;
                else if (double.TryParse(item.ToUpper(), out double douVal))
                {
                    if (douVal == 1)
                        this.stopBits = StopBits.One;
                    else if (douVal == 2)
                        this.stopBits = StopBits.Two;
                    else if (douVal == 1.5 || douVal == 3)
                        this.stopBits = StopBits.OnePointFive;
                    else if (douVal >= 5 && douVal <= 8)
                        this.dataBits = (int)douVal;
                    else if (douVal >= 100)
                        this.baudRate = (int)douVal;
                }

            }
        }

        /// <summary>
        /// 初始化 串口客户端
        /// </summary>
        /// <param name="portName">串口号</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">校验位</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="handshake">流控制</param>
        public SerialPortClient(string portName, int baudRate, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, Handshake handshake = Handshake.None)
        {
            this.portName = portName;
            this.baudRate = baudRate;
            this.dataBits = dataBits;
            this.stopBits = stopBits;
            this.parity = parity;
            this.handshake = handshake;

            Ini();
        }

        void Ini()
        {
            ConnectionMode = ConnectionMode.Manual;
            Encoding = Encoding.ASCII;
            ReceiveMode = ReceiveMode.ParseTime();
            ReceiveModeReceived = ReceiveMode.ParseTime();
        }

        public override IoTResult DiscardInBuffer()
        {
            try
            {
                base.DiscardInBuffer();
                serialPort?.DiscardInBuffer();
                return new IoTResult().ToEnd();
            }
            catch (Exception ex)
            {
                return new IoTResult().AddError(ex).ToEnd();
            }
        }

        protected override OpenClientData Open2()
        {
            serialPort = new System.IO.Ports.SerialPort(portName, baudRate, parity, dataBits, stopBits);
            serialPort.Encoding = Encoding;
            serialPort.ReadTimeout = TimeOut;
            serialPort.WriteTimeout = TimeOut;
            serialPort.Handshake = handshake;

            serialPort.Open();

            return new OpenClientData(serialPort.BaseStream);
        }

        protected override void Close2()
        {
            serialPort?.Close();
            serialPort?.Dispose();
        }
    }
}
