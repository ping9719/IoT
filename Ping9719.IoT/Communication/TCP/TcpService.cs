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
    public class TcpService : ServiceBase, INetwork
    {
        string ip; int port;
        bool IsOpen2 = false;

        private System.Net.Sockets.TcpListener tcpListener;
        private System.Net.Sockets.Socket stream;

        public Socket Socket => stream;

        Task task;
        //客户端+初始化时间
        ConcurrentDictionary<TcpClient, DateTime> clients = new ConcurrentDictionary<TcpClient, DateTime>();
        public override ClientBase[] Clients => clients.Keys.ToArray();

        public override bool IsOpen => IsOpen2;

        public TcpService(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public override IoTResult Open()
        {
            var result = new IoTResult();
            try
            {
                tcpListener = new TcpListener(IPAddress.Parse(ip), port);
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