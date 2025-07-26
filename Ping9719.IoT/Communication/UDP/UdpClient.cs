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

        string ip; int port;

        private System.Net.Sockets.UdpClient udpClient;

        public UdpClient(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        protected override OpenClientData Open2()
        {
            udpClient = new System.Net.Sockets.UdpClient(AddressFamily.InterNetworkV6);
            udpClient.Client.DualMode = true;

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