using Ping9719.IoT;
using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Fct
{
    /// <summary>
    /// 盟讯电子协议（v2.0）
    /// </summary>
    public class MengXunFct
    {
        int version = 2;

        public ClientBase Client { get; private set; }
        public MengXunFct(ClientBase client, int timeout = 1500, int version = 2)
        {
            Client = client;
            Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            Client.TimeOut = timeout;
            Client.ConnectionMode = ConnectionMode.AutoOpen;
        }
        public MengXunFct(string ip, int port) : this(new TcpClient(ip, port)) { }
        public MengXunFct(string portName, int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One) : this(new SerialPortClient(portName, baudRate, parity, dataBits, stopBits)) { }


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
                try
                {
                    byte[] order = new byte[] { 0x23, 0x3A, 0x08, Convert.ToByte(addr), 0x0A, Convert.ToByte(road), 0x0B, 0x7E };
                    var receive = Client.SendReceive(order);
                    if (!receive.IsSucceed)
                        return receive.ToVal<List<bool>>().ToEnd();

                    if (receive.Value == null || receive.Value.Length != 11)
                        return receive.AddError("长度效验失败").ToVal<List<bool>>().ToEnd();

                    var databyte = new byte[] { receive.Value[7], receive.Value[6] };
                    var data16 = BitConverter.ToInt16(databyte, 0);
                    string data2 = Convert.ToString(data16, 2).PadLeft(16, '0');

                    var receive2 = receive.ToVal<List<bool>>();
                    receive2.Value = new List<bool>();
                    foreach (var item in data2.Reverse())
                    {
                        receive2.Value.Add(item == '1');
                    }
                    return receive2.ToEnd();
                }
                catch (Exception ex)
                {
                    return new IoTResult<List<bool>>().AddError(ex).ToEnd();
                }
            }
            else
            {
                return new IoTResult<List<bool>>().AddError("不支持此版本").ToEnd();
            }

        }
    }
}