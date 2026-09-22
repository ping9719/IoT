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
            if (Client == null)
            {
                Client = client;
                return IoTResult.Create();
            }

            if (useOldSet)
            {
                client.ConnectionMode = Client.ConnectionMode;
                //client.IsAutoOpen = Client.IsAutoOpen;
                //client.IsAutoClose = Client.IsAutoClose;
                client.SendDataProcessors = Client.SendDataProcessors;
                client.ReceivedDataProcessors = Client.ReceivedDataProcessors;
                client.MaxReconnectionTime = Client.MaxReconnectionTime;
                client.ReceiveBufferSize = Client.ReceiveBufferSize;
                client.HeartbeatTime = Client.HeartbeatTime;
                client.HeartbeatReceiveTime = Client.HeartbeatReceiveTime;
                client.IsAutoDiscard = Client.IsAutoDiscard;
                client.Encoding = Client.Encoding;
                client.TimeOut = Client.TimeOut;
                client.ReceiveMode = Client.ReceiveMode;
                client.ReceiveModeReceived = Client.ReceiveModeReceived;

                client.Opening = Client.Opening;
                client.Opened = Client.Opened;
                client.Closing = Client.Closing;
                client.Closed = Client.Closed;
                client.Received = Client.Received;
                client.Heartbeat = Client.Heartbeat;

                Client.Opening = null;
                Client.Opened = null;
                Client.Closing = null;
                Client.Closed = null;
                Client.Received = null;
                Client.Heartbeat = null;
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
