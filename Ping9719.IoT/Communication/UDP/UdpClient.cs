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
        public override bool IsOpen => udpClient != null && IsOpen2;
        public Socket Socket => stream;

		string ip; int port;

        object obj1 = new object();
        bool IsOpen2 = false;
        bool IsUserClose = false;//是否用户关闭
        bool isSendReceive = false;//是否正在发送和接收中

        private System.Net.Sockets.UdpClient udpClient;
        private System.Net.Sockets.Socket stream;
        QueueByteFixed dataEri;
        Task task;
        int ReconnectionCount = 0;
		CancellationTokenSource flushCts;

        public UdpClient(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public override IoTResult Open()
        {
            var result = new IoTResult();
            try
            {
                lock (obj1)
                {
                    var aa = Opening?.Invoke(this);
                    if (aa == false)
                        throw new Exception("用户已拒绝链接");

                    Open2(false);
                }
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
                //if (IsOpen)
                //{
                var aa = Closing?.Invoke(this);
                if (aa == false)
                    throw new Exception("用户已拒绝断开");

                Close2(true);
                //}
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        public override IoTResult DiscardInBuffer()
        {
            try
            {
                dataEri?.Clear();
                return new IoTResult().ToEnd();
            }
            catch (Exception ex)
            {
                return new IoTResult().AddError(ex).ToEnd();
            }
        }

        public override IoTResult Send(byte[] data, int offset = 0, int count = -1)
        {
            var result = new IoTResult();
            var isHmOpen = false;
            isSendReceive = true;
            try
            {
                if (!IsOpen && ConnectionMode == ConnectionMode.AutoOpen)
                { result = Open(); isHmOpen = true; }
                if (!result.IsSucceed)
                    return result;

                result.Requests.Add(data);

                lock (obj1)
                {
                    Send2(data, offset, count);
                }
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            finally
            {
                if (IsOpen && ConnectionMode == ConnectionMode.AutoOpen && isHmOpen)
                    Close();

                isSendReceive = false;
            }
            return result.ToEnd();
        }

        public override IoTResult<byte[]> Receive(ReceiveMode receiveMode = null)
        {
            var result = new IoTResult<byte[]>();
            var isHmOpen = false;
            isSendReceive = true;
            try
            {
                if (!IsOpen && ConnectionMode == ConnectionMode.AutoOpen)
                { result = Open().ToVal<byte[]>(); isHmOpen = true; }
                if (!result.IsSucceed)
                    return result;

                lock (obj1)
                {
                    if (IsAutoDiscard)
                        DiscardInBuffer();

                    result.Value = Receive2(receiveMode);
                    result.Responses.Add(result.Value);
                }

            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            finally
            {
                if (IsOpen && ConnectionMode == ConnectionMode.AutoOpen && isHmOpen)
                    Close();

                isSendReceive = false;
            }
            return result.ToEnd();
        }

        public override IoTResult<byte[]> SendReceive(byte[] data, ReceiveMode receiveMode = null)
        {
            var result = new IoTResult<byte[]>();
            var isHmOpen = false;
            isSendReceive = true;
            try
            {
                if (!IsOpen && ConnectionMode == ConnectionMode.AutoOpen)
                { result = Open().ToVal<byte[]>(); isHmOpen = true; }
                if (!result.IsSucceed)
                    return result;

                lock (obj1)
                {
                    if (IsAutoDiscard)
                        DiscardInBuffer();

                    result.Requests.Add(data);
                    Send2(data);
                    result.Value = Receive2(receiveMode);
                    result.Responses.Add(result.Value);
                }
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            finally
            {
                if (IsOpen && ConnectionMode == ConnectionMode.AutoOpen && isHmOpen)
                    Close();

                isSendReceive = false;
            }
            return result.ToEnd();
        }

        #region 内部
        void GoRun()
        {
            task = Task.Factory.StartNew(async (a) =>
            {
                var cc = (UdpClient)a;
                byte[] data = new byte[ReceiveBufferSize];
                while (true)
                {
                    try
                    {
                        if (IsUserClose)
                        {
                            break;
                        }
                        else if (cc.IsOpen2 && cc.IsOpen)
                        {
                            int readLength;
                            try
                            {
                                var receiveResult1 = cc.stream.BeginReceive(data, 0, data.Length, SocketFlags.None, null, null);
                                receiveResult1.AsyncWaitHandle.WaitOne();
                                readLength = cc.stream.EndReceive(receiveResult1);
                            }
                            catch (IOException ex) when ((ex.InnerException as SocketException)?.ErrorCode == (int)SocketError.OperationAborted || (ex.InnerException as SocketException)?.ErrorCode == 125 /* 操作取消（Linux） */)
                            {
                                //警告：此错误代码（995）可能会更改。
                                //查看 https://docs.microsoft.com/en-us/windows/desktop/winsock/windows-sockets-error-codes-2
                                //注意：在Linux上观察到NativeErrorCode和ErrorCode 125。

                                //Message?.Invoke(this, new AsyncTcpEventArgs("本地连接已关闭", ex));
                                readLength = -1;
                            }
                            catch (IOException ex) when ((ex.InnerException as SocketException)?.ErrorCode == (int)SocketError.ConnectionAborted)
                            {
                                //Message?.Invoke(this, new AsyncTcpEventArgs("连接失败", ex));
                                readLength = -1;
                            }
                            catch (IOException ex) when ((ex.InnerException as SocketException)?.ErrorCode == (int)SocketError.ConnectionReset)
                            {
                                //Message?.Invoke(this, new AsyncTcpEventArgs("远程连接重置", ex));
                                readLength = -2;
                            }
                            catch (Exception ex)
                            {
                                //其他原因
                                readLength = -3;
                            }

                            //断开
                            if (readLength <= 0)
                            {
                                if (readLength == 0)
                                {
                                    //Message?.Invoke(this, new AsyncTcpEventArgs("远程关闭连接"));
                                }


                                if (cc.IsOpen2 && cc.IsOpen)
                                    cc.Close2(false);
                            }
                            //收到消息
                            else
                            {
                                cc.dataEri.Enqueue(data, 0, readLength);
                                if (cc.Received != null && !cc.isSendReceive)
                                {
                                    lock (cc.obj1)
                                    {
                                        if (cc.ReceiveModeReceived.Type == ReceiveModeEnum.Time)
                                        {
                                            // 取消之前的延迟刷新
                                            flushCts?.Cancel();
                                            flushCts = new CancellationTokenSource();

                                            var countMax = (int)cc.ReceiveModeReceived.Data;
                                            Task.Delay(countMax, flushCts.Token).ContinueWith(t =>
                                            {
                                                if (t.IsCanceled)
                                                    return;

                                                var bytes = dataEri.DequeueAll();
                                                if (bytes != null && bytes.Length > 0)
                                                    cc.Received?.Invoke(this, bytes);
                                            }, TaskContinuationOptions.ExecuteSynchronously);
                                        }
                                        else
                                        {
                                            var bytes = cc.Receive2(cc.ReceiveModeReceived, true);
                                            if (bytes != null && bytes.Length > 0)
                                                cc.Received?.Invoke(this, bytes);
                                        }
                                    }
                                }
                            }

                        }
                        else if (cc.ConnectionMode == ConnectionMode.AutoReconnection)
                        {
                            ReconnectionCount++;
                            var tz = Math.Min(ReconnectionCount * 1000, cc.MaxReconnectionTime);
                            System.Threading.Thread.Sleep(tz);

                            if (IsUserClose)
                                break;

                            cc.Open2(true);
                        }
                        else
                        {
                            break;
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

        void Open2(bool IsReconnection)
        {
            dataEri = new QueueByteFixed(ReceiveBufferSize, true);
            udpClient = new System.Net.Sockets.UdpClient(AddressFamily.InterNetworkV6);
            udpClient.Client.DualMode = true;
            //udpClient.ReceiveTimeout = TimeOut;
            //udpClient.SendTimeout = TimeOut;

            udpClient.Connect(ip, port);

            IsOpen2 = true;
            stream = udpClient.Client;
            ReconnectionCount = 0;

            if (!IsReconnection)
            {
                IsUserClose = false;
                GoRun();
            }
            Opened?.Invoke(this);
        }

        void Close2(bool isUser)
        {
            try
            {
                IsOpen2 = false;
                IsUserClose = isUser;
                dataEri = null;

                udpClient?.Client?.Shutdown(SocketShutdown.Both);
                udpClient?.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Closed?.Invoke(this, isUser);

                if (isUser)
                    task?.Wait();
            }
        }

        void Send2(byte[] data, int offset = 0, int count = -1)
        {
            stream.Send(data, offset, count < 0 ? data.Length : count, SocketFlags.None);
        }

        byte[] Receive2(ReceiveMode receiveMode = null, bool isevent = false)
        {
            receiveMode ??= ReceiveMode;
            byte[] value = null;
            DateTime beginTime = DateTime.Now;
            if (receiveMode.Type == ReceiveModeEnum.Byte)
            {
                var countMax = (int)receiveMode.Data;
                if (isevent)
                {
                    if (dataEri.Count >= countMax)
                    {
                        value = dataEri.Dequeue(countMax);
                    }
                }
                else
                {
                    while (dataEri.Count < countMax)
                    {
                        if (!IsOpen2)
                            throw new Exception("链接被断开");
                        if (IsOutTime(beginTime, receiveMode.TimeOut))
                            throw new TimeoutException("已超时");

                        Thread.Sleep(10);
                    }
                    value = dataEri.Dequeue(countMax);
                }
            }
            else if (receiveMode.Type == ReceiveModeEnum.ByteAll)
            {
                if (isevent)
                {
                    value = dataEri.DequeueAll();
                }
                else
                {
                    while (dataEri.Count == 0)
                    {
                        if (!IsOpen2)
                            throw new Exception("链接被断开");
                        if (IsOutTime(beginTime, receiveMode.TimeOut))
                            throw new TimeoutException("已超时");

                        Thread.Sleep(10);
                    }
                    value = dataEri.DequeueAll();
                }
            }
            else if (receiveMode.Type == ReceiveModeEnum.Char)
            {
                var countMax = (int)receiveMode.Data * 2;
                if (isevent)
                {
                    if (dataEri.Count >= countMax)
                    {
                        value = dataEri.Dequeue(countMax);
                    }
                }
                else
                {
                    while (dataEri.Count < countMax)
                    {
                        if (!IsOpen2)
                            throw new Exception("链接被断开");
                        if (IsOutTime(beginTime, receiveMode.TimeOut))
                            throw new TimeoutException("已超时");

                        Thread.Sleep(10);
                    }
                    value = dataEri.Dequeue(countMax);
                }
            }
            else if (receiveMode.Type == ReceiveModeEnum.Time)
            {
                var countMax = (int)receiveMode.Data;
                if (isevent)
                {
                    value = dataEri.DequeueAll();
                }
                else
                {
                    var tempBufferLength = dataEri.Count;
                    while (dataEri.Count == 0 || tempBufferLength != dataEri.Count)
                    {
                        if (!IsOpen2)
                            throw new Exception("链接被断开");
                        if (IsOutTime(beginTime, receiveMode.TimeOut))
                            throw new TimeoutException("已超时");

                        tempBufferLength = dataEri.Count;
                        Thread.Sleep(countMax);
                    }
                    value = dataEri.DequeueAll();
                }
            }
            else if (receiveMode.Type == ReceiveModeEnum.ToString)
            {
                var zfc = Encoding.GetBytes(receiveMode.Data.ToString());
                if (isevent)
                {
                    if (dataEri.ToArray().EndsWith(zfc))
                    {
                        value = dataEri.DequeueAll();
                    }
                }
                else
                {

                    while (dataEri.Count == 0 || !dataEri.ToArray().EndsWith(zfc))
                    {
                        if (!IsOpen2)
                            throw new Exception("链接被断开");
                        if (IsOutTime(beginTime, receiveMode.TimeOut))
                            throw new TimeoutException("已超时");

                        Thread.Sleep(10);
                    }
                    value = dataEri.DequeueAll();
                }
            }

            return value;
        }
        #endregion
    }
}