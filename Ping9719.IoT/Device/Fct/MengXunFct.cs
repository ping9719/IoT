using Ping9719.IoT;
using Ping9719.IoT.Communication.SerialPort;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Fct
{
    /// <summary>
    /// 盟讯电子协议（v2.0）
    /// </summary>
    public class MengXunFct : SerialPortBase
    {
        int version = 2;

        public MengXunFct(string portName, int baudRate = 115200, int dataBits = 8, StopBits stopBits = StopBits.One, Parity parity = Parity.None, int timeout = 10000, int version = 2)
        {
            if (serialPort == null) serialPort = new SerialPort();
            serialPort.PortName = portName;
            serialPort.BaudRate = baudRate;
            serialPort.DataBits = dataBits;
            serialPort.StopBits = stopBits;
            serialPort.Encoding = Encoding.ASCII;
            serialPort.Parity = parity;

            serialPort.ReadTimeout = timeout;
            serialPort.WriteTimeout = timeout;
            this.version = version;
        }

        /// <summary>
        /// 通断检测
        /// </summary>
        /// <param name="addr">地址</param>
        /// <param name="road">通道</param>
        /// <returns></returns>
        public IoTResult<List<bool>> OnOffCheck(int addr, int road)
        {
            if (version == 2)
            {
                if (isAutoOpen)
                {
                    var conn = Connect();
                    if (!conn.IsSucceed)
                        return new IoTResult<List<bool>>(conn).ToEnd();
                }

                var result = new IoTResult<List<bool>>() { Value = new List<bool>(16) };
                try
                {
                    byte[] order = new byte[] { 0x23, 0x3A, 0x08, Convert.ToByte(addr), 0x0A, Convert.ToByte(road), 0x0B, 0x7E };
                    var receive = SendPackageReliable(order);
                    if (!receive.IsSucceed)
                        return new IoTResult<List<bool>>(receive).ToEnd();

                    if (receive.Value.Length != 11)
                    {
                        result.IsSucceed = false;
                        result.AddError("长度效验失败");
                        return result;
                    }

                    var databyte = new byte[] { receive.Value[7], receive.Value[6] };
                    var data16 = BitConverter.ToInt16(databyte, 0);
                    string data2 = Convert.ToString(data16, 2).PadLeft(16, '0');

                    foreach (var item in data2.Reverse())
                    {
                        result.Value.Add(item == '1');
                    }
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
            else
            {
                return new IoTResult<List<bool>>().AddError("不支持此版本") .ToEnd();
            }

        }
    }
}