using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ping9719.IoT.Common;
using Ping9719.IoT.Communication;

namespace Ping9719.IoT.Device.Robot
{
    /// <summary>
    /// 爱普生机器人
    /// </summary>
    public class EpsonRobot : ClientHostBase
    {
        public EpsonRobot(ClientBase client)
        {
            Client = client;
            //Client.TimeOut = timeout;
            //Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            //Client.ConnectionMode = ConnectionMode.Manual;
            Client.IsAutoDiscard = true;
        }
        public EpsonRobot(string ip, int port = 5000) : this(new TcpClient(ip, port)) { }


        /// <summary>
        /// 开始
        /// </summary>
        public IoTResult Start()
        {
            var info = Client.SendReceive("$Login\r\n");
            if (info.IsSucceed && info.Value.StartsWith("#Login,0"))
            {
                Thread.Sleep(300);
                var stop = Client.SendReceive("$Stop\r\n");
                Thread.Sleep(300);

                if (stop.IsSucceed && stop.Value.StartsWith("#Stop,0"))
                {
                    Thread.Sleep(300);
                    var start = Client.SendReceive("$Start,0\r\n");
                    if (start.IsSucceed && start.Value.Contains("#Start,0"))
                        return start;

                    return start.AddError("机器人启动指令($Start)未得到确认");
                }
                return stop.AddError("机器人停止指令($Stop)未得到确认");
            }
            return info.AddError("机器人登录指令($Login)未得到确认");
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public IoTResult Pause()
        {
            var returnmes = Client.SendReceive("$Pause\r\n");
            if (returnmes.IsSucceed && returnmes.Value.StartsWith("#Pause"))
                return returnmes;

            return returnmes.AddError("机器人暂停指令($Pause)未得到确认");
        }

        /// <summary>
        /// 继续
        /// </summary>
        public IoTResult Continue()
        {
            var returnmes = Client.SendReceive("$Continue\r\n");
            if (returnmes.IsSucceed && returnmes.Value.StartsWith("#Continue"))
                return returnmes;

            return returnmes.AddError("机器人继续指令($Continue)未得到确认");
        }

        /// <summary>
        /// 复位
        /// </summary>
        public IoTResult Reset()
        {
            var returnmes = Client.SendReceive("$Reset\r\n");
            if (returnmes.IsSucceed && returnmes.Value.StartsWith("#Reset,0"))
                return returnmes;

            return returnmes.AddError("机器人复位指令($Reset)未得到确认");
        }

        /// <summary>
        /// 停止
        /// </summary>
        public IoTResult Stop()
        {
            var returnmes = Client.SendReceive("$Stop\r\n");
            if (returnmes.IsSucceed && returnmes.Value.StartsWith("#Stop"))
                return returnmes;

            return returnmes.AddError("机器人停止指令($Stop)未得到确认");
        }
    }
}
