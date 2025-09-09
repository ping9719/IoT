using Ping9719.IoT.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// TCP服务端
    /// </summary>
    public class TcpService : ServiceBase, INetwork
    {
        IPAddress localaddr; int port;
        bool IsOpen2 = false;

        private System.Net.Sockets.TcpListener tcpListener;
        private System.Net.Sockets.Socket stream;

        public Socket Socket => stream;

        Task task;
        //客户端+初始化时间
        ConcurrentDictionary<TcpClient, DateTime> clients = new ConcurrentDictionary<TcpClient, DateTime>();
        public override ClientBase[] Clients => clients.Keys.ToArray();

        public override bool IsOpen => IsOpen2;

        /// <summary>
        /// 初始化TCP服务端。监听所有的ip
        /// </summary>
        public TcpService(int port)
        {
            this.localaddr = IPAddress.Any;
            this.port = port;
        }

        /// <summary>
        /// 初始化TCP服务端
        /// </summary>
        public TcpService(string ip, int port)
        {
            this.localaddr = IPAddress.Parse(ip);
            this.port = port;
        }

        /// <summary>
        /// 初始化TCP服务端
        /// </summary>
        public TcpService(IPAddress localaddr, int port)
        {
            this.localaddr = localaddr;
            this.port = port;
        }

        public override IoTResult Open()
        {
            var result = new IoTResult();
            try
            {
                tcpListener = new TcpListener(localaddr, port);
                //tcpListener.Server.DualMode = true;
                tcpListener.Start();

                IsOpen2 = true;
                stream = tcpListener.Server;

                GoRun();
            }
            catch (Exception ex)
            {
                result.AddError(ex);
                IsOpen2 = false;
            }
            return result.ToEnd();
        }

        public override IoTResult Close()
        {
            var result = new IoTResult();
            try
            {
                IsOpen2 = false;
                //IsUserClose = isUser;
                //dataEri = null;

                tcpListener?.Server?.Shutdown(SocketShutdown.Both);
                tcpListener?.Stop();
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            finally
            {
                //Closed?.Invoke(this, isUser);

                //if (isUser)
                task?.Wait();
            }
            return result.ToEnd();
        }

        #region 内部
        void GoRun()
        {
            task = Task.Factory.StartNew(async (a) =>
            {
                var cc = (TcpService)a;
                byte[] data = new byte[ReceiveBufferSize];
                while (true)
                {
                    try
                    {

                        if (!IsOpen)
                        {
                            break;
                        }

                        TcpClient tcpClientMy;
                        try
                        {
                            var tcpClient = await tcpListener.AcceptTcpClientAsync();
                            tcpClientMy = TcpClient.Get(tcpClient, cc);
                            cc.clients.TryAdd(tcpClientMy, DateTime.Now);
                        }
                        catch (Exception ex)
                        {
                            //监听停止了
                            continue;
                        }

                        //客户端链接
                        Opened?.Invoke(tcpClientMy);
                        tcpClientMy.Closed += (a, b) =>
                        {
                            cc.Closed?.Invoke(a);
                            cc.clients.TryRemove(tcpClientMy, out DateTime dt);
                        };
                        tcpClientMy.Received += (a, b) =>
                        {
                            cc.Received?.Invoke(a, b);
                        };

                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {

                    }
                }
            },
            this,
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default).Unwrap();

            //task.Start();
        }

        #endregion
    }
}