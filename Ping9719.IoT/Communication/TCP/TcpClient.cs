using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// Tcp客户端
    /// </summary>
    public class TcpClient : ClientBase, INetwork
    {
        public override bool IsOpen => base.IsOpen && (tcpClient?.Connected ?? false);
        public Socket Socket => tcpClient?.Client;
        string ip; int port;
        private System.Net.Sockets.TcpClient tcpClient;

        public TcpClient(string ip, int port)
        {
            this.ip = ip;
            this.port = port;

            ConnectionMode = ConnectionMode.Manual;
            Encoding = Encoding.ASCII;
            ReceiveMode = ReceiveMode.ParseByteAll();
            ReceiveModeReceived = ReceiveMode.ParseByteAll();
        }

        internal static TcpClient Get(System.Net.Sockets.TcpClient tcpClient, ServiceBase serviceBase)
        {
            TcpClient tcpClient1 = new TcpClient("", 0);
            tcpClient1.ReceiveBufferSize = serviceBase.ReceiveBufferSize;
            tcpClient1.IsAutoDiscard = serviceBase.IsAutoDiscard;
            tcpClient1.Encoding = serviceBase.Encoding;
            tcpClient1.TimeOut = serviceBase.TimeOut;
            tcpClient1.ReceiveMode = serviceBase.ReceiveMode;
            tcpClient1.ReceiveModeReceived = serviceBase.ReceiveModeReceived;

            tcpClient1.dataEri = new QueueByteFixed(tcpClient1.ReceiveBufferSize, true);
            tcpClient.ReceiveTimeout = serviceBase.TimeOut;
            tcpClient.SendTimeout = serviceBase.TimeOut;

            tcpClient1.IsOpen2 = true;
            tcpClient1.tcpClient = tcpClient;
            tcpClient1.openData = new OpenClientData(tcpClient.Client);
            tcpClient1.ReconnectionCount = 0;

            tcpClient1.IsUserClose = false;
            tcpClient1.GoRun();
            //tcpClient1.Opened?.Invoke(tcpClient1);
            return tcpClient1;
        }

        protected override OpenClientData Open2()
        {
            tcpClient = new System.Net.Sockets.TcpClient(AddressFamily.InterNetworkV6);
            tcpClient.Client.DualMode = true;
            tcpClient.ReceiveTimeout = TimeOut;
            tcpClient.SendTimeout = TimeOut;

            var connectResult = tcpClient.BeginConnect(ip, port, null, null);
            if (!connectResult.AsyncWaitHandle.WaitOne(TimeOut))//阻塞当前线程
                throw new TimeoutException("连接超时");
            tcpClient.EndConnect(connectResult);

            return new OpenClientData(tcpClient.Client);
        }

        protected override void Close2()
        {
            tcpClient?.Client?.Shutdown(SocketShutdown.Both);
            tcpClient?.Close();
        }
    }
}