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
        public static string[] GetNames => System.IO.Ports.SerialPort.GetPortNames();
        public override bool IsOpen => base.IsOpen && (serialPort?.IsOpen ?? false);
        string portName; int baudRate; Parity parity = Parity.None; int dataBits = 8; StopBits stopBits = StopBits.One; Handshake handshake = Handshake.None;
        private System.IO.Ports.SerialPort serialPort;

        public SerialPortClient(string portName, int baudRate, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, Handshake handshake = Handshake.None)
        {
            this.portName = portName;
            this.baudRate = baudRate;
            this.dataBits = dataBits;
            this.stopBits = stopBits;
            this.parity = parity;
            this.handshake = handshake;

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
