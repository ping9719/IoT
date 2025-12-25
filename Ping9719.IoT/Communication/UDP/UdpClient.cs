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
    /// Udp客户端
    /// </summary>
    public class UdpClient : ClientBase, INetwork
    {
        public override bool IsOpen => base.IsOpen && udpClient != null;
        public Socket Socket => udpClient?.Client;

        IPAddress ip; int port; int listeningPort;

        private System.Net.Sockets.UdpClient udpClient;
        /// <summary>
        /// Udp客户端
        /// </summary>
        /// <param name="ip">远程和本地的ip</param>
        /// <param name="port">远端的端口</param>
        /// <param name="listeningPort">监听的本机端口</param>
        public UdpClient(string ip, int port, int listeningPort)
        {
            this.ip = IPAddress.Parse(ip);
            this.port = port;
            this.listeningPort = listeningPort;
            Ini();
        }
        /// <summary>
        /// Udp客户端
        /// </summary>
        /// <param name="ip">远程和本地的ip</param>
        /// <param name="port">远端的端口</param>
        /// <param name="listeningPort">监听的本机端口</param>
        public UdpClient(IPAddress ip, int port, int listeningPort)
        {
            this.ip = ip;
            this.port = port;
            this.listeningPort = listeningPort;
            Ini();
        }

        void Ini()
        {
            ConnectionMode = ConnectionMode.Manual;
            Encoding = Encoding.UTF8;
            ReceiveMode = ReceiveMode.ParseByteAll();
            ReceiveModeReceived = ReceiveMode.ParseByteAll();
        }

        protected override OpenClientData Open2()
        {
            udpClient = new System.Net.Sockets.UdpClient(listeningPort);
            udpClient.Connect(ip, port);

            return new OpenClientData(udpClient.Client);
        }

        protected override void Close2()
        {
            udpClient?.Client?.Shutdown(SocketShutdown.Both);
            udpClient?.Close();
        }
    }
}