using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ping9719.IoT.Common;
using Ping9719.IoT.Communication.TCP;

namespace Ping9719.IoT.Robot
{
    /// <summary>
    /// 爱普生机器人
    /// </summary>
    public class EpsonRobot : SocketBase
    {
        public EpsonRobot(string ip, int port = 5000, int timeout = 3000)
        {
            if (socket == null)
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SetIpEndPoint(ip, port);
            this.timeout = timeout;
        }

        //bool isxt = false;
        //public IoTResult Open(string pwd = null)
        //{
        //    var aaa = Connect();
        //    if (!aaa.IsSucceed)
        //    {
        //        return aaa;
        //    }

        //    string comm = $"$Login{(pwd == null ? "" : "," + pwd)}\r\n";
        //    var bbb = SendPackageReliable(comm, Encoding.UTF8);

        //    //if (!isxt)
        //    //{
        //    //    isxt = true;
        //    //    Task.Run(() =>
        //    //    {
        //    //        while (true)
        //    //        {
        //    //            try
        //    //            {
        //    //                SendPackageSingle("$GetStatus\r\n", Encoding.UTF8);
        //    //                Thread.Sleep(1000);
        //    //            }
        //    //            catch (Exception)
        //    //            {

        //    //            }
        //    //        }
        //    //    });
        //    //}

        //    return bbb;
        //}

        /// <summary>
        /// 开始
        /// </summary>
        public IoTResult Start()
        {
            if (!IsConnected)
                Connect();

            var info = SendPackageSingle("$Login\r\n");
            if (info.IsSucceed && info.Value.StartsWith("#Login,0"))
            {
                Thread.Sleep(300);
                var returnms = SendPackageSingle("$Stop\r\n");
                Thread.Sleep(300);

                if (returnms.IsSucceed && returnms.Value.StartsWith("#Stop,0"))
                {
                    Thread.Sleep(300);
                    var mmooo = SendPackageSingle("$Start,0\r\n");
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
            var returnmes = SendPackageSingle("$Pause\r\n");
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
            var returnmes = SendPackageSingle("$Continue\r\n");
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
            var returnmes = SendPackageSingle("$Reset\r\n");
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
            var returnmes = SendPackageSingle("$Stop\r\n");
            if (returnmes.IsSucceed && returnmes.Value.StartsWith("#Stop"))
                return returnmes;

            returnmes.IsSucceed = false;
            return returnmes;
        }
    }
}
