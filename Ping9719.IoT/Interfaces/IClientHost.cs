using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT
{
    /// <summary>
    /// 客户端接口
    /// </summary>
    public interface IClientHost
    {
        /// <summary>
        /// 客户端
        /// </summary>
        ClientBase Client { get; }
    }

    /// <summary>
    /// 客户端接口的实现
    /// </summary>
    public class ClientHostBase : IClientHost
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public ClientHostBase() { }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="client"></param>
        public ClientHostBase(ClientBase client)
        {
            Client = client;
        }

        /// <summary>
        /// 客户端
        /// </summary>
        public ClientBase Client { get; protected set; }

        /// <summary>
        /// 切换客户端
        /// </summary>
        /// <param name="client">新的客户端</param>
        /// <param name="useOldSet">是否采用旧客户端的设置</param>
        /// <param name="isAutoOpen">true：如果旧客户端已打开，就尝试打开</param>
        /// <returns></returns>
        public IoTResult SetClient(ClientBase client, bool useOldSet = true, bool isAutoOpen = true)
        {
            if (client == null)
                return IoTResult.Create().AddError("新的客户端不能为null");

            if (Client == null)
            {
                Client = client;
                return IoTResult.Create();
            }

            if (ReferenceEquals(Client, client))
                return IoTResult.Create();

            var oldClient = Client;
            if (useOldSet)
            {
                client.ConnectionMode = oldClient.ConnectionMode;
                //client.IsAutoOpen = oldClient.IsAutoOpen;
                //client.IsAutoClose = oldClient.IsAutoClose;
                client.SendDataProcessors = oldClient.SendDataProcessors;
                client.ReceivedDataProcessors = oldClient.ReceivedDataProcessors;
                client.MaxReconnectionTime = oldClient.MaxReconnectionTime;
                client.ReceiveBufferSize = oldClient.ReceiveBufferSize;
                client.HeartbeatTime = oldClient.HeartbeatTime;
                client.HeartbeatReceiveTime = oldClient.HeartbeatReceiveTime;
                client.IsAutoDiscard = oldClient.IsAutoDiscard;
                client.Encoding = oldClient.Encoding;
                client.TimeOut = oldClient.TimeOut;
                client.ReceiveMode = oldClient.ReceiveMode;
                client.ReceiveModeReceived = oldClient.ReceiveModeReceived;

                client.Opening = oldClient.Opening;
                client.Opened = oldClient.Opened;
                client.Closing = oldClient.Closing;
                client.Closed = oldClient.Closed;
                client.Received = oldClient.Received;
                client.Heartbeat = oldClient.Heartbeat;

                oldClient.Opening = null;
                oldClient.Opened = null;
                oldClient.Closing = null;
                oldClient.Closed = null;
                oldClient.Received = null;
                oldClient.Heartbeat = null;
            }

            bool isOpen = false;
            if (client.ConnectionMode == ConnectionMode.Manual && isAutoOpen)
            {
                isOpen = Client.IsOpen;
            }
            //else if (client.ConnectionMode == ConnectionMode.AutoOpen)
            //{
            //    isOpen = false;
            //}
            else if (client.ConnectionMode == ConnectionMode.AutoReconnection && isAutoOpen)
            {
            /*
                         打开  用户关闭 重连中
             IsOpen       o       x       x
             IsUserClose  x       o       x
             */
                //isOpen = !Client.IsUserClose;
                isOpen = (Client.IsOpen && !Client.IsUserClose) || (!Client.IsOpen && !Client.IsUserClose);
            }

            var close = Client?.Close();
            Client = null;
            Client = client;//切换

            if (isOpen)
                return Client?.Open();

            return close ?? IoTResult.Create();
        }
    }
}
