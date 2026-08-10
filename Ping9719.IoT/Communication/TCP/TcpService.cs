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
        /// <summary>
        /// 客户端成功链接
        /// </summary>
        public Action<ClientBase> Opened;
        /// <summary>
        /// 断开连接。item2：关闭代码。
        /// 0：用户关闭/正常关闭；
        /// -1：主动断开/未知错误/通用错误；
        /// -2：主动心跳验证失败；
        /// -3：被动心跳超时；
        /// 其他：请参考系统错误代码
        /// </summary>
        public Action<ClientBase, int> Closed;
        /// <summary>
        /// 接收到信息
        /// </summary>
        public Action<ClientBase, byte[]> Received;

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
        /// 初始化客户端
        /// </summary>
        /// <param name="connectString">比如：127.0.0.1:502。</param>
        public TcpService(string connectString)
        {
            this.localaddr = IPAddress.Parse("127.0.0.1");
            this.port = 502;

            if (string.IsNullOrWhiteSpace(connectString))
                return;

            foreach (string item in connectString.Split(new char[] { ':', '：' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(item.ToUpper(), out int intVal))
                    this.port = intVal;
                else
                    this.localaddr = IPAddress.Parse(item);
            }
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

                tcpListener?.Stop();

                foreach (var item in clients)
                {
                    item.Key.Close();
                }
                clients.Clear();
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            finally
            {
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
                while (true)
                {
                    try
                    {

                        if (!IsOpen)
                        {
                            break;
                        }

                        System.Net.Sockets.TcpClient tcpClient = null;
                        TcpClient tcpClientMy = null;
                        try
                        {
                            tcpClient = await tcpListener.AcceptTcpClientAsync();
                            tcpClientMy = TcpClient.Get(tcpClient, cc);
                        }
                        catch (Exception)
                        {
                            try
                            {
                                tcpClientMy?.Close();
                                tcpClientMy = null;

                                tcpClient?.Client?.Shutdown(SocketShutdown.Both);
                                tcpClient?.Close();
                                tcpClient = null;

                                //进行下一个监听
                                continue;
                            }
                            catch { }
                        }

                        cc.clients.TryAdd(tcpClientMy, DateTime.Now);
                        //客户端链接
                        Opened?.Invoke(tcpClientMy);
                        tcpClientMy.Closed += (a, b) =>
                        {
                            cc.Closed?.Invoke(a, b);
                            cc.clients.TryRemove(tcpClientMy, out DateTime dt);
                        };
                        tcpClientMy.Received += (a, b) =>
                        {
                            cc.Received?.Invoke(a, b);
                        };

                    }
                    catch (Exception)
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