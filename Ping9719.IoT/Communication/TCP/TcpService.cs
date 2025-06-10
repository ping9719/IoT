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
    public class TcpService : ServiceBase
    {
        string ip; int port;
        bool IsOpen2 = false;

        private System.Net.Sockets.TcpListener tcpListener;
        private System.Net.Sockets.Socket stream;

        Task task;
        //客户端+初始化时间
        ConcurrentDictionary<TcpClient, DateTime> clients = new ConcurrentDictionary<TcpClient, DateTime>();
        public override TcpClient[] Clients => clients.Keys.ToArray();

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
                        while (true)
                        {
                            System.Net.Sockets.TcpClient tcpClient;
                            TcpClient tcpClient1;
                            try
                            {
                                tcpClient = await tcpListener.AcceptTcpClientAsync();
                                tcpClient1 = new TcpClient("", 0).Get(tcpClient, cc);
                            }
                            catch (ObjectDisposedException) when (!IsOpen2)
                            {
                                //监听停止了
                                break;
                            }

                            //客户端链接
                            Opened?.Invoke(tcpClient1);
                            clients.TryAdd(tcpClient1, DateTime.Now);
                            tcpClient1.Closed += (a, b) =>
                            {
                                //客户端断开
                                Closed?.Invoke(a);
                            };
                            tcpClient1.Received += (a, b) =>
                            {
                                Received?.Invoke(a, b);
                            };
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {

                    }
                }
            }, this);

            //task.Start();
        }

        #endregion
    }
}