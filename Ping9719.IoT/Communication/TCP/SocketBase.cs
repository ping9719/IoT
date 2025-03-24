using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// Socket基类
    /// </summary>
    public abstract class SocketBase
    {
        /// <summary>
        /// 警告日志委托
        /// 为了可用性，会对异常网络进行重试。此类日志通过委托接口给出去。
        /// </summary>
        public Action<Exception> WarningLog { get; set; }
        /// <summary>
        /// 连接状态
        /// </summary>
        public virtual bool IsConnected => socket?.Connected ?? false;
        /// <summary>
        /// 分批缓冲区大小
        /// </summary>
        protected const int BufferSize = 4096;

        /// <summary>
        /// Socket实例
        /// </summary>
        protected Socket socket;
        /// <summary>
        /// 读写，连接时超时
        /// </summary>
        protected int timeout = -1;
        /// <summary>
        /// 连接的ip，端口
        /// </summary>
        protected IPEndPoint ipEndPoint;

        /// <summary>
        /// 是否自动打开关闭
        /// </summary>
        protected bool isAutoOpen = true;

        /// <summary>
        /// 连接（如果已经是连接状态会先关闭再打开）
        /// </summary>
        /// <returns></returns>
        protected virtual IoTResult Connect()
        {
            var result = new IoTResult();
            SafeClose();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.ReceiveTimeout = timeout;
                socket.SendTimeout = timeout;

                //连接
                //socket.Connect(ipEndPoint);
                IAsyncResult connectResult = socket.BeginConnect(ipEndPoint, null, null);
                //阻塞当前线程           
                if (!connectResult.AsyncWaitHandle.WaitOne(timeout))
                    throw new TimeoutException("连接超时");
                socket.EndConnect(connectResult);
            }
            catch (Exception ex)
            {
                SafeClose();
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        protected void SetIpEndPoint(string ip, int port)
        {
            if (!IPAddress.TryParse(ip, out IPAddress address))
                address = Dns.GetHostEntry(ip).AddressList?.FirstOrDefault();
            ipEndPoint = new IPEndPoint(address, port);
        }

        /// <summary>
        /// 打开连接（如果已经是连接状态会先关闭再打开）
        /// </summary>
        /// <returns></returns>
        public IoTResult Open()
        {
            isAutoOpen = false;
            return Connect();
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        protected virtual IoTResult Dispose()
        {
            IoTResult result = new IoTResult();
            try
            {
                SafeClose();
                return result;
            }
            catch (Exception ex)
            {
                return result.AddError(ex);
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        public IoTResult Close()
        {
            isAutoOpen = true;
            return Dispose();
        }

        /// <summary>
        /// 安全关闭
        /// </summary>
        /// <param name="socket"></param>
        public void SafeClose()
        {
            try
            {
                if (socket?.Connected ?? false) socket?.Shutdown(SocketShutdown.Both);//正常关闭连接
            }
            catch { }

            try
            {
                socket?.Close();
            }
            catch { }
        }

        /// <summary>
        /// Socket读取
        /// </summary>
        /// <param name="socket">socket</param>
        /// <param name="receiveCount">读取长度</param>          
        /// <returns></returns>
        protected IoTResult<byte[]> SocketRead(int receiveCount)
        {
            var result = new IoTResult<byte[]>();
            if (receiveCount < 0)
            {
                return result.AddError($"读取长度[receiveCount]为{receiveCount}");
            }

            byte[] receiveBytes = new byte[receiveCount];
            int receiveFinish = 0;
            while (receiveFinish < receiveCount)
            {
                // 分批读取
                int receiveLength = receiveCount - receiveFinish >= BufferSize ? BufferSize : receiveCount - receiveFinish;
                try
                {
                    var readLeng = socket.Receive(receiveBytes, receiveFinish, receiveLength, SocketFlags.None);
                    if (readLeng == 0)
                    {
                        SafeClose();
                        return result.AddError($"连接被断开");
                    }
                    receiveFinish += readLeng;
                }
                catch (SocketException ex)
                {
                    SafeClose();
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                        result.AddError($"连接超时：{ex.Message}");
                    else
                        result.AddError($"连接被断开，{ex.Message}");
                    return result;
                }
            }
            result.Value = receiveBytes;
            return result.ToEnd();
        }

        /// <summary>
        /// Socket读取
        /// </summary>
        public IoTResult<byte[]> SocketRead()
        {
            //从发送命令到读取响应为最小单元，避免多线程执行串数据（可线程安全执行）
            lock (this)
            {
                IoTResult<byte[]> result = new IoTResult<byte[]>();
                try
                {
                    DateTime beginTime = DateTime.Now;
                    var tempBufferLength = socket.Available;
                    //在(没有取到数据或BytesToRead在继续读取)且没有超时的情况，延时处理
                    while ((socket.Available == 0 || tempBufferLength != socket.Available) && DateTime.Now - beginTime <= TimeSpan.FromMilliseconds(socket.ReceiveTimeout))
                    {
                        tempBufferLength = socket.Available;
                        //延时处理
                        Thread.Sleep(20);
                    }
                    byte[] buffer = new byte[socket.Available];
                    var receiveFinish = 0;
                    while (receiveFinish < buffer.Length)
                    {
                        var readLeng = socket.Receive(buffer, buffer.Length, SocketFlags.None);
                        if (readLeng == 0)
                        {
                            result.Value = null;
                            return result.ToEnd();
                        }
                        receiveFinish += readLeng;
                    }
                    result.Value = buffer;
                    return result.ToEnd();
                }
                catch (Exception ex)
                {
                    return result.AddError(ex).ToEnd();
                }
            }
        }

        /// <summary>
        /// 发送报文，并获取响应报文
        /// </summary>
        /// <param name="command">发送命令</param>
        /// <returns></returns>
        public virtual IoTResult<byte[]> SendPackageSingle(byte[] command)
        {
            IoTResult<byte[]> result = new IoTResult<byte[]>();
            try
            {
                socket.Send(command);
            }
            catch (Exception ex)
            {
                return result.AddError(ex).ToEnd();
            }

            return SocketRead();
        }

        public virtual IoTResult<string> SendPackageSingle(string command, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.ASCII;

            var info = SendPackageSingle(encoding.GetBytes(command));
            if (info.IsSucceed)
                return new IoTResult<string>(info, encoding.GetString(info.Value));

            return new IoTResult<string>(info);
        }

        /// <summary>
        /// 发送报文，并获取响应报文（如果网络异常，会自动进行一次重试）
        /// TODO 重试机制应改成用户主动设置
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual IoTResult<byte[]> SendPackageReliable(byte[] command)
        {
            try
            {
                var result = SendPackageSingle(command);
                if (!result.IsSucceed)
                {
                    WarningLog?.Invoke(result.Error.FirstOrDefault());
                    //如果出现异常，则进行一次重试         
                    var conentResult = Connect();
                    if (!conentResult.IsSucceed)
                        return new IoTResult<byte[]>(conentResult);

                    return SendPackageSingle(command);
                }
                else
                    return result;
            }
            catch (Exception ex)
            {
                try
                {
                    WarningLog?.Invoke( ex);
                    //如果出现异常，则进行一次重试                
                    var conentResult = Connect();
                    if (!conentResult.IsSucceed)
                        return new IoTResult<byte[]>(conentResult);

                    return SendPackageSingle(command);
                }
                catch (Exception ex2)
                {
                    IoTResult<byte[]> result = new IoTResult<byte[]>();
                    return result.AddError(ex2).ToEnd();
                }
            }
        }

        public virtual IoTResult<string> SendPackageReliable(string command, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.ASCII;

            var info = SendPackageReliable(encoding.GetBytes(command));
            if (info.IsSucceed)
                return new IoTResult<string>(info, encoding.GetString(info.Value));

            return new IoTResult<string>(info);
        }
    }
}
