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

namespace Ping9719.IoT.Communication.TCP
{
    /// <summary>
    /// Tcp客户端
    /// </summary>
    public class TcpClient : ClientBase
    {
        public override bool IsOpen => tcpClient?.Connected ?? false;

        string ip;
        int port;
        object obj1 = new object();
        bool IsOpen2 = false;
        bool isSendReceive = false;//是否正在发送和接受中

        private System.Net.Sockets.TcpClient tcpClient;
        private System.Net.Sockets.NetworkStream stream;
        QueueByteFixed dataEri;

        public TcpClient(string ip, int port)
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
                if (IsOpen)
                {
                    var aa = Closing?.Invoke(this);
                    if (aa == false)
                        throw new Exception("用户已拒绝断开");

                    Close2(true);
                }
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
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
                    result.Value = Receive2(receiveMode);
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
                    {
                        dataEri.Clear();
                    }

                    Send2(data);
                    result.Value = Receive2(receiveMode);
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
            Task.Run(() =>
            {
                byte[] data = new byte[ReceiveBufferSize];
                while (true)
                {
                    try
                    {
                        if (IsOpen2 && IsOpen)
                        {
                            int readLength;
                            try
                            {
                                var receiveResult1 = stream.BeginRead(data, 0, data.Length, null, null);
                                receiveResult1.AsyncWaitHandle.WaitOne();
                                readLength = stream.EndRead(receiveResult1);
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

                            //断开
                            if (readLength <= 0)
                            {
                                if (readLength == 0)
                                {
                                    //Message?.Invoke(this, new AsyncTcpEventArgs("远程关闭连接"));
                                }
                                //closedTcs.TrySetResult(true);
                                //OnClosed(readLength != -1);
                                Close2(false);
                                //return;
                            }
                            //收到消息
                            else
                            {
                                dataEri.Enqueue(data, 0, readLength);
                                if (Received != null && !isSendReceive)
                                {
                                    lock (obj1)
                                    {
                                        var bytes = Receive2(ReceiveModeReceived, true);
                                        if (bytes != null && bytes.Length > 0)
                                            Received?.Invoke(this, bytes);
                                    }
                                }
                            }

                        }
                        else if (ConnectionMode == ConnectionMode.AutoReconnection)
                        {
                            Open2(true);
                        }
                        else
                        {
                            return;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {

                    }
                }
            });
        }

        void Open2(bool IsReconnection)
        {
            dataEri = new QueueByteFixed(ReceiveBufferSize, true);
            tcpClient = new System.Net.Sockets.TcpClient(AddressFamily.InterNetworkV6);
            tcpClient.Client.DualMode = true;
            tcpClient.ReceiveTimeout = TimeOut;
            tcpClient.SendTimeout = TimeOut;

            var connectResult = tcpClient.BeginConnect(ip, port, null, null);
            if (!connectResult.AsyncWaitHandle.WaitOne(TimeOut))//阻塞当前线程
                throw new TimeoutException("连接超时");
            tcpClient.EndConnect(connectResult);

            IsOpen2 = true;
            stream = tcpClient.GetStream();
            
            if (!IsReconnection)
                GoRun();
            Opened?.Invoke(this);
        }

        void Close2(bool isUser)
        {
            try
            {
                dataEri = null;
                tcpClient?.Client.Shutdown(SocketShutdown.Both);
                tcpClient?.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                IsOpen2 = false;
                Closed?.Invoke(this, isUser);
            }
        }

        void Send2(byte[] data, int offset = 0, int count = -1)
        {
            stream.Write(data, offset, count < 0 ? data.Length : count);
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
                var countMax = (int)receiveMode.Data*2;
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