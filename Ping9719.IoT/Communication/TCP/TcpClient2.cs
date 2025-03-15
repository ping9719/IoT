using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication.TCP
{
    [Obsolete]
    public class TcpClient2 : ClientBase
    {
        public Socket Socket { get; private set; }
        public override bool IsOpen => Socket?.Connected ?? false;
        /// <summary>
        /// 是否链接
        /// </summary>
        public bool IsConnect = false;
        /// <summary>
        /// 是否开启Poll检测断线，默认false
        /// </summary>
        public bool IsPoll { get; set; }

        string ip;
        int port;
        object obj1 = new object();
        bool isSendReceive = false;//是否正在发送和接受中

        public TcpClient2(string ip, int port)
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
                    if (IsAutoDiscard && Socket.Available > 0)
                    {
                        var bytes = new byte[Socket.Available];
                        Socket.Receive(bytes);
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

        void Open2(bool IsReconnection)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.ReceiveTimeout = TimeOut;
            Socket.SendTimeout = TimeOut;

            var connectResult = Socket.BeginConnect(ip, port, null, null);
            if (!connectResult.AsyncWaitHandle.WaitOne(TimeOut))//阻塞当前线程
                throw new TimeoutException("连接超时");
            Socket.EndConnect(connectResult);

            Opened?.BeginInvoke(this, null, null);
            IsConnect = true;
            if (!IsReconnection)
                Monitor2();
        }

        /// <summary>
        /// 断开
        /// </summary>
        /// <param name="isUser">是否用户自己断开的</param>
        void Close2(bool isUser)
        {
            try
            {
                Socket?.Shutdown(SocketShutdown.Both);
                Socket?.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                IsConnect = false;
                Closed?.BeginInvoke(this, isUser, null, null);
            }
        }

        void Monitor2()
        {
            Task.Run(() =>
            {
                byte[] data = new byte[0];
                while (true)
                {
                    //System.Threading.Thread.Sleep(1);
                    try
                    {
                        if (IsConnect && IsOpen)
                        {
                            //等待消息
                            var receiveResult = Socket.BeginReceive(data, 0, 0, SocketFlags.None, null, null);
                            receiveResult.AsyncWaitHandle.WaitOne();
                            Socket.EndReceive(receiveResult);

                            if (IsConnect && Socket.Available == 0 && !isSendReceive)//可能已经断开
                            {
                                Close2(false);
                            }
                            else if (IsConnect && Received != null && Socket.Available > 0 && !isSendReceive)//有新信息
                            {
                                lock (obj1)
                                {
                                    var bytes = Receive2(ReceiveModeReceived);
                                    Received?.Invoke(this, bytes);
                                }
                            }

                            ////可能已经断开
                            //if (IsPoll && IsConnect && Socket.Poll(1000, SelectMode.SelectRead) && Socket.Available == 0)
                            //{
                            //    Close2(false);
                            //}
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

        void Send2(byte[] data, int offset = 0, int count = -1)
        {
            var count2 = Socket.Send(data, offset, count < 0 ? data.Length : count, SocketFlags.None);
        }

        byte[] Receive2(ReceiveMode receiveMode = null)
        {
            receiveMode ??= ReceiveMode;

            byte[] value = new byte[0];
            DateTime beginTime = DateTime.Now;
            if (receiveMode.Type == ReceiveModeEnum.Byte)
            {
                var count = 0;
                var countMax = (int)receiveMode.Data;
                value = new byte[countMax];

                while (countMax - count > 0)
                {
                    if (IsOutTime(beginTime, receiveMode.TimeOut))
                        throw new TimeoutException("已超时");

                    count += Socket.Receive(value, count, countMax - count, SocketFlags.None);
                }
            }
            else if (receiveMode.Type == ReceiveModeEnum.ByteAll)
            {
                while (Socket.Available == 0)
                {
                    if (IsOutTime(beginTime, receiveMode.TimeOut))
                        throw new TimeoutException("已超时");
                    Thread.Sleep(10);
                }
                value = new byte[Socket.Available];
                var count = Socket.Receive(value);
            }
            else if (receiveMode.Type == ReceiveModeEnum.Char)
            {
                var count = 0;
                var countMax = (int)receiveMode.Data * 2;
                value = new byte[countMax];

                while (countMax - count > 0)
                {
                    if (IsOutTime(beginTime, receiveMode.TimeOut))
                        throw new TimeoutException("已超时");

                    count += Socket.Receive(value, count, countMax - count, SocketFlags.None);
                }
            }
            else if (receiveMode.Type == ReceiveModeEnum.Time)
            {
                var countMax = (int)receiveMode.Data;
                var tempBufferLength = Socket.Available;
                while (Socket.Available == 0 || tempBufferLength != Socket.Available)
                {
                    if (IsOutTime(beginTime, receiveMode.TimeOut))
                        throw new TimeoutException("已超时");

                    tempBufferLength = Socket.Available;
                    Thread.Sleep(countMax);
                }
                value = new byte[Socket.Available];
                var count = Socket.Receive(value, Socket.Available, SocketFlags.None);
            }
            else if (receiveMode.Type == ReceiveModeEnum.ToString)
            {
                //value = new byte[0];
                var zfc = Encoding.GetBytes(receiveMode.Data.ToString());
                List<byte> buffer = new List<byte>();
                while (true)
                {
                    if (Socket.Available == 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    if (IsOutTime(beginTime, receiveMode.TimeOut))
                        throw new TimeoutException("已超时");

                    var a1 = new byte[Socket.Available];
                    var count = Socket.Receive(a1);
                    buffer.AddRange(a1);
                    if (buffer.ToArray().EndsWith(zfc))
                    {
                        break;
                    }
                }
                value = buffer.ToArray();
            }

            return value;
        }
        #endregion
    }
}