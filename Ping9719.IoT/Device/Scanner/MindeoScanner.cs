using Ping9719.IoT;
using Ping9719.IoT.Communication.SerialPort;
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
    public class MindeoScanner : SerialPortBase, IScannerBase
    {
        byte[] stateCode = new byte[] { 0x16, 0x54, 0x0D };//开始扫描
        byte[] endCode = new byte[] { 0x16, 0x55, 0x0D };//取消扫描

        public MindeoScanner(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            if (serialPort == null)
                serialPort = new SerialPort();
            serialPort.PortName = portName;
            serialPort.BaudRate = baudRate;
            serialPort.DataBits = dataBits;
            serialPort.StopBits = stopBits;
            serialPort.Encoding = Encoding.ASCII;
            serialPort.Parity = parity;
        }

        /// <summary>
        /// 在主机模式下执行一次
        /// </summary>
        /// <param name="keepTime">保持时长（毫秒）</param>
        /// <returns></returns>
        public IoTResult<string> ReadOne(int timeout)
        {
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return new IoTResult<string>(conn);
            }

            serialPort.ReadTimeout = timeout;
            serialPort.WriteTimeout = timeout;
            var result = new IoTResult<string>();
            try
            {
                //清空字符,还原状态
                serialPort.DiscardInBuffer();
                var aaa = SendPackageReliable(stateCode);
                if (!aaa.IsSucceed)
                    return new IoTResult<string>(aaa);

                result.Value = Encoding.ASCII.GetString(aaa.Value);
            }
            catch (Exception ex)
            {
                result.IsSucceed = false;
                result.AddError(ex);
            }
            finally
            {
                if (isAutoOpen)
                    Dispose();
            }
            return result.ToEnd();
        }
    }
}
