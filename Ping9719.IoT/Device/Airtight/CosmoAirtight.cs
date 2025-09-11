using Ping9719.IoT.Common;
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
    public class CosmoAirtight : IClient
    {
        static byte[] OkByte = new byte[] { 0x06, 0x0d };
        //异常
        //#00 00 00 80:BB
        //#{机号} 00 {频号} {数据，错误}:{校验}
        public ClientBase Client { get; private set; }//通讯管道

        public CosmoAirtight(ClientBase client)
        {
            Client = client;
            //Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            //Client.ConnectionMode = ConnectionMode.AutoOpen;
        }

        public CosmoAirtight(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, Handshake handshake = Handshake.RequestToSend) : this(new SerialPortClient(portName, baudRate, parity, dataBits, stopBits, handshake)) { }

        /// <summary>
        /// 启动
        /// </summary>
        /// <returns></returns>
        public IoTResult Start()
        {
            string comm = $"STT\r\n";
            try
            {
                var aa = Client.SendReceive(Client.Encoding.GetBytes(comm));
                if (!aa.IsSucceed)
                    return aa;
                aa.IsSucceed = aa.Value.ArrayEquals(OkByte);
                return aa;
            }
            catch (Exception ex)
            {
                return IoTResult.Create<string>().AddError(ex);
            }
        }

        /// <summary>
        /// 启动并等待返回数据
        /// </summary>
        /// <returns>t1:错误信息，t2：正确的值</returns>
        public IoTResult<Tuple<string, double>> StartWait(int time = 60000)
        {
            string comm = $"STT\r\n";
            try
            {
                //#00 00 00 10:C2//已启动
                //06 0D
                var aa = Client.SendReceive(comm);
                if (!aa.IsSucceed)
                    return aa.ToVal<Tuple<string, double>>();

                //#00 00 D +0.000:26
                return Analysis(Client.ReceiveString(ReceiveMode.ParseTime(10, time)));
            }
            catch (Exception ex)
            {
                return IoTResult.Create<Tuple<string, double>>().AddError(ex);
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
                //06 0D
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
        /// <returns>t1:错误信息，t2：正确的值</returns>
        public IoTResult<Tuple<string, double>> ReadTestData()
        {
            string comm = $"RLD\r\n";
            try
            {
                //#00 00 0 +0.000:3A
                //#00 00 D +0.000:26
                return Analysis(Client.SendReceive(comm));
            }
            catch (Exception ex)
            {
                return IoTResult.Create<Tuple<string, double>>().AddError(ex);
            }
        }

        /// <summary>
        /// 设置频道
        /// </summary>
        /// <param name="channel">0-15</param>
        /// <returns></returns>
        public IoTResult SetChannel(int channel)
        {
            string comm = $"WCHN {channel.ToString().PadLeft(2, '0')}\r\n";
            try
            {
                //#00 00 00 01:C2
                var aa = Client.SendReceive(Client.Encoding.GetBytes(comm));
                if (!aa.IsSucceed)
                    return aa;
                aa.IsSucceed = aa.Value.ArrayEquals(OkByte);
                return aa;
            }
            catch (Exception ex)
            {
                return IoTResult.Create().AddError(ex);
            }
        }

        private IoTResult<Tuple<string, double>> Analysis(IoTResult<string> str)
        {
            //#00 00 D +0.000:26
            //#00 00 9 -0999.:14
            if (!str.IsSucceed)
                return str.ToVal<Tuple<string, double>>();

            var aa = str.Value.Split(new char[] { ' ', ':' });
            if (aa.Length <= 4)
                return str.ToVal<Tuple<string, double>>().AddError("返回数据长度不足");

            if (aa[2] == "2" || aa[2] == "GOOD")
            {
                double.TryParse(aa[3], out double bbb);
                return str.ToVal<Tuple<string, double>>(new Tuple<string, double>("", bbb));
            }
            else
            {
                string err = "未知错误";
                if (aa[2] == "0")
                    err = "未判断"; 
                else if (aa[2] == "1")
                    err = "Lo NG";
                //else if (aa[2] == "2")
                //    err = "GOOD";
                else if (aa[2] == "3")
                    err = "Hi NG";
                else if (aa[2] == "4")
                    err = "LL NG";
                else if (aa[2] == "C")
                    err = "HH NG";
                else if (aa[2] == "D")
                    err = "ERROR";

                double.TryParse(aa[3], out double bbb);
                return str.ToVal<Tuple<string, double>>(new Tuple<string, double>(err, bbb));
            }
        }
    }
}
