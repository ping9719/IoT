using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ping9719.IoT.Common;
using Ping9719.IoT.Communication;

namespace Ping9719.IoT.Robot
{
    /// <summary>
    /// 爱普生机器人
    /// </summary>
    public class EpsonRobot : IClient
    {
        public ClientBase Client { get; private set; }
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
                var returnms = Client.SendReceive("$Stop\r\n");
                Thread.Sleep(300);

                if (returnms.IsSucceed && returnms.Value.StartsWith("#Stop,0"))
                {
                    Thread.Sleep(300);
                    var mmooo = Client.SendReceive("$Start,0\r\n");
                    if (mmooo.IsSucceed && mmooo.Value.Contains("#Start,0"))
                    {
                        return mmooo;
                    }
                }
            }
            info.IsSucceed = false;
            return info;
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public IoTResult Pause()
        {
            var returnmes = Client.SendReceive("$Pause\r\n");
            if (returnmes.IsSucceed && returnmes.Value.StartsWith("#Pause"))
                return returnmes;

            returnmes.IsSucceed = false;
            return returnmes;
        }

        /// <summary>
        /// 继续
        /// </summary>
        public IoTResult Continue()
        {
            var returnmes = Client.SendReceive("$Continue\r\n");
            if (returnmes.IsSucceed && returnmes.Value.StartsWith("#Continue"))
                return returnmes;

            returnmes.IsSucceed = false;
            return returnmes;
        }

        /// <summary>
        /// 复位
        /// </summary>
        public IoTResult Reset()
        {
            var returnmes = Client.SendReceive("$Reset\r\n");
            if (returnmes.IsSucceed && returnmes.Value.StartsWith("#Reset,0"))
                return returnmes;

            returnmes.IsSucceed = false;
            return returnmes;
        }

        /// <summary>
        /// 停止
        /// </summary>
        public IoTResult Stop()
        {
            var returnmes = Client.SendReceive("$Stop\r\n");
            if (returnmes.IsSucceed && returnmes.Value.StartsWith("#Stop"))
                return returnmes;

            returnmes.IsSucceed = false;
            return returnmes;
        }
    }
}
