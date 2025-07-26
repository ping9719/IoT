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

        internal TcpClient Get(System.Net.Sockets.TcpClient tcpClient, ServiceBase serviceBase)
        {
            ReceiveBufferSize = serviceBase.ReceiveBufferSize;
            IsAutoDiscard = serviceBase.IsAutoDiscard;
            Encoding = serviceBase.Encoding;
            TimeOut = serviceBase.TimeOut;
            ReceiveMode = serviceBase.ReceiveMode;
            ReceiveModeReceived = serviceBase.ReceiveModeReceived;

            dataEri = new QueueByteFixed(ReceiveBufferSize, true);
            tcpClient.ReceiveTimeout = TimeOut;
            tcpClient.SendTimeout = TimeOut;

            IsOpen2 = true;
            this.tcpClient = tcpClient;
            openData = new OpenClientData(tcpClient.Client);
            ReconnectionCount = 0;

            IsUserClose = false;
            GoRun();
            Opened?.Invoke(this);
            return this;
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