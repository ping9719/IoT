using Ping9719.IoT.Common;
using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Weld
{
    /// <summary>
    /// JBC焊接台
    /// 下载驱动：https://www.silabs.com/software-and-tools/usb-to-uart-bridge-vcp-drivers
    /// </summary>
    public class JBCWeld : IClient
    {
        public ClientBase Client { get; private set; }//通讯管道

        static byte[] stx = new byte[] { 0x10, 0x02 };//帧头
        static byte[] etx = new byte[] { 0x10, 0x03 };//帧尾

        public byte host { get; set; } = 0x01;//主机地址
        public byte from { get; set; } = 0x00;//主机地址

        public JBCWeld(ClientBase client)
        {
            Client = client;
            //Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            //Client.ReceiveMode = ReceiveMode.ParseByteAll();
            //Client.ReceiveModeReceived = ReceiveMode.ParseByteAll();
            Client.Received += (s, e) =>
            {
                if (e == null || e.Length == 0)
                    return;
                else if (e.Length == 1 && e[0] == 0x15)
                    s.Send(new byte[] { 0x16 });
                else if (e.Length == 1 && e[0] == 0x06)
                    s.Send(new byte[] { 0x06 });
                else if (e.Length == 1 && e[0] == 0x01)
                    s.Send(new byte[] { 0x06, 0x10, 0x02, 0x01, 0x00, 0x21, 0x00, 0x21, 0x10, 0x03 });
            };
        }

        public JBCWeld(string portName, int baudRate = 500000, Parity parity = Parity.Even, int dataBits = 8, StopBits stopBits = StopBits.One, Handshake handshake = Handshake.None) : this(new SerialPortClient(portName, baudRate, parity, dataBits, stopBits, handshake)) { }

        /// <summary>
        /// 读取数据（此方法最好在0.1-1s内循环调用）
        /// </summary>
        /// <returns></returns>
        public IoTResult<JBCWeldInfo> ReadInfo()
        {
            try
            {
                var comm = GetComm(0x30, new byte[] { 0 });
                var aaa = Client.SendReceive(comm);
                if (!aaa.IsSucceed)
                    return IoTResult.Create<JBCWeldInfo>().AddError(aaa.Error);

                aaa.Value= aaa.Value.Replace(new byte[] { 0x10, 0x10 }, new byte[] { 0x10 });//处理转义
                if (aaa.Value.Length != 24)
                    return IoTResult.Create<JBCWeldInfo>().AddError("长度校验失败");
                if (!aaa.Value.StartsWith(stx) || !aaa.Value.EndsWith(etx))
                    return IoTResult.Create<JBCWeldInfo>().AddError("首位校验失败");

                JBCWeldInfo jBCWeldInfo = new JBCWeldInfo();
                jBCWeldInfo.Chan = aaa.Value[6 + 0];
                jBCWeldInfo.State = aaa.Value[6 + 1];
                jBCWeldInfo.Tempe = ((float)BitConverter.ToUInt16(aaa.Value, 6 + 2)) / 10.0f;
                jBCWeldInfo.Power = BitConverter.ToUInt16(aaa.Value, 6 + 6);
                jBCWeldInfo.RunState = aaa.Value[6 + 10];
                jBCWeldInfo.SleepCountdown = aaa.Value[6 + 12];

                return aaa.ToVal(jBCWeldInfo).ToEnd();
            }
            catch (Exception ex)
            {
                return IoTResult.Create<JBCWeldInfo>().AddError(ex);
            }
        }

        private byte[] GetComm(byte add, byte[] data = null)
        {
            data ??= new byte[] { };

            var aaaa = new List<byte>(9 + data.Length);
            aaaa.AddRange(stx);
            aaaa.Add(host);
            aaaa.Add(from);

            //按道理这里开始 如果发现有 0x10 应该 转义为 0x10 0x10 ，省略转义，后续优化
            aaaa.Add(add);
            aaaa.Add((byte)data.Length);
            aaaa.AddRange(data);
            aaaa.Add((byte)(aaaa.Skip(4).Sum(o => o)));

            aaaa.AddRange(etx);
            return aaaa.ToArray();
        }
    }

    public class JBCWeldInfo
    {
        /// <summary>
        /// 通道号，未激活为0
        /// </summary>
        public int Chan { get; set; }
        /// <summary>
        /// 状态 0正常 4无焊头
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 实时温度
        /// </summary>
        public float Tempe { get; set; }
        /// <summary>
        /// 输出功率
        /// </summary>
        public int Power { get; set; }
        /// <summary>
        /// 状态 0工作模式/有异常 1浅度休眠 2深度休眠
        /// </summary>
        public int RunState { get; set; }
        /// <summary>
        /// 倒计时（秒）
        /// </summary>
        public int SleepCountdown { get; set; }

        public override string ToString()
        {
            return $"通道:{Chan} 状态:{(State == 0 ? "正常" : $"异常({State})")} 温度:{Tempe} 功率:{Power} 状态:{RunStateStr()} 倒计时:{SleepCountdown}s";
        }

        private string RunStateStr()
        {
            if (RunState == 0)
                return "正常模式";
            else if (RunState == 1)
                return "浅度休眠";
            else if (RunState == 2)
                return "深度休眠";
            else
                return $"未知状态({RunState})";
        }
    }

}
