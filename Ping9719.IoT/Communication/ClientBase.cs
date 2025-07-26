using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// 客户端基类
    /// </summary>
    public abstract class ClientBase
    {
        protected object obj1 = new object();
        protected bool IsOpen2 = false;
        protected bool IsUserClose = false;//是否用户关闭
        protected bool isSendReceive = false;//是否正在发送和接收中

        protected OpenClientData openData;
        protected QueueByteFixed dataEri;
        protected Task task;
        protected int ReconnectionCount = 0;
        protected CancellationTokenSource flushCts;

        ///// <summary>
        ///// 类的名称
        ///// </summary>
        //public string Name { get => this.GetType().Name; }
        /// <summary>
        /// 是否打开
        /// </summary>
        public abstract bool IsOpen { get; }
        /// <summary>
        /// 链接模式
        /// </summary>
        public ConnectionMode ConnectionMode { get; set; }
        /// <summary>
        /// 发送数据处理器
        /// </summary>
        public List<IDataProcessor> SendDataProcessors { get; set; } = new List<IDataProcessor>();
        /// <summary>
        /// 接受数据处理器
        /// </summary>
        public List<IDataProcessor> ReceivedDataProcessors { get; set; } = new List<IDataProcessor>();
        /// <summary>
        /// 断线重连，最大重连时间。默认10秒。
        /// </summary>
        public int MaxReconnectionTime { get; set; } = 10 * 1000;
        /// <summary>
        /// 接收区，缓冲区大小（默认1024 * 100）
        /// </summary>
        public int ReceiveBufferSize { get; set; } = 1024 * 100;

        /// <summary>
        /// 是否在发送和接收时丢弃来自缓冲区的数据（默认false）
        /// </summary>
        public virtual bool IsAutoDiscard { get; set; }
        /// <summary>
        /// 字符串编码，默认UTF8
        /// </summary>
        public virtual Encoding Encoding { get; set; } = Encoding.UTF8;
        /// <summary>
        /// 超时（发送、接收、链接）（毫秒）-1永久，默认3000
        /// </summary>
        public virtual int TimeOut { get; set; } = 3000;
        /// <summary>
        /// 接收数据的方式
        /// </summary>
        public virtual ReceiveMode ReceiveMode { get; set; } = ReceiveMode.ParseByteAll();
        /// <summary>
        /// 接收数据的方式，在事件 Received 下。注意 ReceiveModeEnum.Time 模式下时间设置太长或对方一直在发送消息，可能会死锁 ！
        /// </summary>
        public virtual ReceiveMode ReceiveModeReceived { get; set; } = ReceiveMode.ParseByteAll();

        /// <summary>
        /// 即将打开
        /// </summary>
        public Func<ClientBase, bool> Opening;
        /// <summary>
        /// 成功打开
        /// </summary>
        public Action<ClientBase> Opened;
        /// <summary>
        /// 即将断开连接。仅主动断开。
        /// </summary>
        public Func<ClientBase, bool> Closing;
        /// <summary>
        /// 断开连接，item2:是否手动断开
        /// </summary>
        public Action<ClientBase, bool> Closed;
        /// <summary>
        /// 接收到信息
        /// </summary>
        public Action<ClientBase, byte[]> Received;

        /// <summary>
        /// 打开
        /// </summary>
        public virtual IoTResult Open()
        {
            var result = new IoTResult();
            try
            {
                lock (obj1)
                {
                    var aa = Opening?.Invoke(this);
                    if (aa == false)
                        throw new Exception("用户已拒绝链接");

                    bool isOpenOk = false;

                    try
                    {
                        openData = Open2();
                        isOpenOk = true;
                    }
                    catch (Exception ex)
                    {
                        result.AddError(ex);
                    }

                    dataEri = new QueueByteFixed(ReceiveBufferSize, true);
                    IsOpen2 = isOpenOk;
                    IsUserClose = false;
                    ReconnectionCount = 0;
                    if (isOpenOk || ConnectionMode == ConnectionMode.AutoReconnection)
                        GoRun();
                    if (isOpenOk)
                        Opened?.Invoke(this);
                }
            }
            catch (Exception ex)
            {
                result.AddError(ex);
                IsOpen2 = false;
            }
            return result.ToEnd();
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public virtual IoTResult Close()
        {
            var result = new IoTResult();
            try
            {
                var aa = Closing?.Invoke(this);
                if (aa == false)
                    throw new Exception("用户已拒绝断开");

                IsUserClose = true;
                IsOpen2 = false;
                dataEri = null;
                Close2();
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            finally
            {
                Closed?.Invoke(this, true);
                task?.Wait();
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 清空接收缓存
        /// </summary>
        public virtual IoTResult DiscardInBuffer()
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

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="offset">开始位置</param>
        /// <param name="count">数量，-1全部</param>
        /// <returns></returns>
        public virtual IoTResult Send(byte[] data, int offset = 0, int count = -1)
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

                var d2 = DataProcessors(data,true);
                result.Requests.Add(d2);
                lock (obj1)
                {
                    Send2(d2, offset, count);
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
        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="charIndex">开始位置</param>
        /// <param name="charCount">数量，-1全部</param>
        /// <returns></returns>
        public virtual IoTResult Send(string data, int charIndex = 0, int charCount = -1)
        {
            if (data == null)
                return Send(null, offset: 0);

            charCount = charCount < 0 ? data.Length - charIndex : charCount;
            return Send(Encoding.GetBytes(data.ToArray(), charIndex, charCount));

            //else if (charIndex <= 0 && charCount < 0)
            //    return Send(Encoding.GetBytes(data));
            //else
            //    return Send(Encoding.GetBytes(data.Skip(charIndex <= 0 ? 0 : charIndex).Take((charCount < 0 ? int.MaxValue : charCount)).ToArray()));
        }

        /// <summary>
        /// 接收
        /// </summary>
        /// <returns></returns>
        public virtual IoTResult<byte[]> Receive(ReceiveMode receiveMode = null)
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

                    result.Value = DataProcessors( Receive2(receiveMode),false);
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
        /// <summary>
        /// 接收
        /// </summary>
        /// <param name="timeOut">重设接收数据模式的超时（毫秒，-1永久 -2默认）</param>
        /// <returns></returns>
        public virtual IoTResult<byte[]> Receive(int timeOut) => Receive(ReceiveMode.SetTimeOut(ReceiveMode, timeOut));
        /// <summary>
        /// 接收为字符串
        /// </summary>
        /// <returns></returns>
        public virtual IoTResult<string> ReceiveString(ReceiveMode receiveMode = null, Encoding encoding = null)
        {
            var isok = Receive(receiveMode);
            return isok.IsSucceed ? isok.ToVal((encoding ?? Encoding).GetString(isok.Value)) : isok.ToVal<string>();
        }
        /// <summary>
        /// 接收为字符串
        /// </summary>
        /// <param name="timeOut">重设接收数据模式的超时（毫秒，-1永久 -2默认）</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public virtual IoTResult<string> ReceiveString(int timeOut, Encoding encoding = null) => ReceiveString(ReceiveMode.SetTimeOut(ReceiveMode, timeOut), encoding);


        /// <summary>
        /// 发送并等待接收为字节
        /// </summary>
        /// <param name="data"></param>
        /// <param name="receiveMode"></param>
        /// <returns></returns>
        public virtual IoTResult<byte[]> SendReceive(byte[] data, ReceiveMode receiveMode = null)
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

                    var d1 = DataProcessors(data, true);
                    result.Requests.Add(d1);
                    Send2(d1);
                    result.Value = DataProcessors(Receive2(receiveMode), false);
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
        /// <summary>
        /// 发送并等待接收为字节
        /// </summary>
        /// <param name="data"></param>
        /// <param name="timeOut">重设接收数据模式的超时（毫秒，-1永久 -2默认）</param>
        /// <returns></returns>
        public virtual IoTResult<byte[]> SendReceive(byte[] data, int timeOut) => SendReceive(data, ReceiveMode.SetTimeOut(ReceiveMode, timeOut));
        /// <summary>
        /// 发送并等待接收为字符串
        /// </summary>
        public virtual IoTResult<string> SendReceive(string data, ReceiveMode receiveMode = null, Encoding encoding = null)
        {
            var isok = SendReceive((encoding ?? Encoding).GetBytes(data), receiveMode);
            return isok.IsSucceed ? isok.ToVal((encoding ?? Encoding).GetString(isok.Value)) : isok.ToVal<string>();
        }
        /// <summary>
        /// 发送并等待接收为字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="timeOut">重设接收数据模式的超时（毫秒，-1永久 -2默认）</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public virtual IoTResult<string> SendReceive(string data, int timeOut, Encoding encoding = null) => SendReceive(data, ReceiveMode.SetTimeOut(ReceiveMode, timeOut), encoding);


        /// <summary>
        /// 发送为字符串并等待结果为字节
        /// </summary>
        public virtual IoTResult<byte[]> SendReceiveToByte(string data, ReceiveMode receiveMode = null, Encoding encoding = null) => SendReceive((encoding ?? Encoding).GetBytes(data), receiveMode);
        /// <summary>
        /// 发送为字符串并等待结果为字节
        /// </summary>
        /// <param name="data"></param>
        /// <param name="timeOut">重设接收数据模式的超时（毫秒，-1永久 -2默认）</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public virtual IoTResult<byte[]> SendReceiveToByte(string data, int timeOut, Encoding encoding = null) => SendReceiveToByte(data, ReceiveMode.SetTimeOut(ReceiveMode, timeOut), encoding);
        /// <summary>
        /// 发送为字节并等待结果为字符串
        /// </summary>
        public virtual IoTResult<string> SendReceiveToString(byte[] data, ReceiveMode receiveMode = null, Encoding encoding = null)
        {
            var isok = SendReceive(data, receiveMode);
            return isok.IsSucceed ? isok.ToVal((encoding ?? Encoding).GetString(isok.Value)) : isok.ToVal<string>();
        }
        /// <summary>
        /// 发送为字节并等待结果为字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="timeOut">重设接收数据模式的超时（毫秒，-1永久 -2默认）</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public virtual IoTResult<string> SendReceiveToString(byte[] data, int timeOut, Encoding encoding = null) => SendReceiveToString(data, ReceiveMode.SetTimeOut(ReceiveMode, timeOut), encoding);

        /// <summary>
        /// 是否超时
        /// </summary>
        /// <param name="beginTime">开始时间</param>
        /// <param name="timeOut">超时（毫秒，-1永久 -2默认）</param>
        /// <returns></returns>
        public bool IsOutTime(DateTime beginTime, int timeOut)
        {
            var TimeOutVal = TimeOut;
            if (timeOut == -1 || timeOut < -2)
                TimeOutVal = -1;
            else if (timeOut == -2)
                TimeOutVal = TimeOut;
            else if (timeOut >= 0)
                TimeOutVal = timeOut;

            return TimeOutVal < 0 ? false : DateTime.Now - beginTime > TimeSpan.FromMilliseconds(TimeOutVal);
        }


        #region 其他
        protected virtual void GoRun()
        {
            task = Task.Factory.StartNew(async (a) =>
            {
                var cc = (ClientBase)a;
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
                                //var receiveResult1 = cc.stream.BeginRead(data, 0, data.Length, null, null);
                                //receiveResult1.AsyncWaitHandle.WaitOne();
                                //readLength = cc.stream.EndRead(receiveResult1);
                                readLength = await cc.openData.ReadAsync(data, 0, data.Length);
                            }
                            //catch (IOException ex) when ((ex.InnerException as SocketException)?.ErrorCode == (int)SocketError.OperationAborted || (ex.InnerException as SocketException)?.ErrorCode == 125 /* 操作取消（Linux） */)
                            //{
                            //    //警告：此错误代码（995）可能会更改。
                            //    //查看 https://docs.microsoft.com/en-us/windows/desktop/winsock/windows-sockets-error-codes-2
                            //    //注意：在Linux上观察到NativeErrorCode和ErrorCode 125。

                            //    //Message?.Invoke(this, new AsyncTcpEventArgs("本地连接已关闭", ex));
                            //    readLength = -1;
                            //}
                            //catch (IOException ex) when ((ex.InnerException as SocketException)?.ErrorCode == (int)SocketError.ConnectionAborted)
                            //{
                            //    //Message?.Invoke(this, new AsyncTcpEventArgs("连接失败", ex));
                            //    readLength = -1;
                            //}
                            //catch (IOException ex) when ((ex.InnerException as SocketException)?.ErrorCode == (int)SocketError.ConnectionReset)
                            //{
                            //    //Message?.Invoke(this, new AsyncTcpEventArgs("远程连接重置", ex));
                            //    readLength = -2;
                            //}
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
                                {
                                    IsOpen2 = false;
                                    dataEri = null;
                                    IsUserClose = false;
                                    cc.Close2();
                                    Closed?.Invoke(this, false);
                                }
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
                                                    cc.Received?.Invoke(this, DataProcessors(bytes, false));
                                            }, TaskContinuationOptions.ExecuteSynchronously);
                                        }
                                        else
                                        {
                                            var bytes = cc.Receive2(cc.ReceiveModeReceived, true);
                                            if (bytes != null && bytes.Length > 0)
                                                cc.Received?.Invoke(this, DataProcessors(bytes, false));
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

                            openData = cc.Open2();
                            dataEri = new QueueByteFixed(ReceiveBufferSize, true);
                            IsOpen2 = true;
                            ReconnectionCount = 0;
                            Opened?.Invoke(this);
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
            }, this, CancellationToken.None, (ConnectionMode == ConnectionMode.AutoReconnection ? TaskCreationOptions.LongRunning : TaskCreationOptions.None), TaskScheduler.Default);

            //task.Start();
        }

        protected abstract OpenClientData Open2();

        protected virtual void Close2()
        {
            openData.Close();
        }

        protected virtual void Send2(byte[] data, int offset = 0, int count = -1)
        {
            if (openData == null)
                throw new Exception("未链接");
            openData.Write(data, offset, count < 0 ? data.Length : count);
        }

        protected virtual byte[] Receive2(ReceiveMode receiveMode = null, bool isevent = false)
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

        byte[] DataProcessors(byte[] data, bool isSend)
        {
            var pro = isSend ? SendDataProcessors : ReceivedDataProcessors;
            var data2 = data;
            foreach (var item in pro)
            {
                data2 = item.DataProcess(data2);
            }
            return data2;
        }
    }

    /// <summary>
    /// 响应模式
    /// </summary>
    public class ReceiveMode
    {
        private ReceiveMode()
        {

        }

        /// <summary>
        /// 接收数据的方式
        /// </summary>
        public ReceiveModeEnum Type { get; private set; }
        /// <summary>
        /// 数据
        /// </summary>
        public object Data { get; private set; }
        /// <summary>
        /// 超时（毫秒，-1永久 -2默认）
        /// </summary>
        public int TimeOut { get; private set; }

        /// <summary>
        /// 读取指定的字节数量
        /// </summary>
        /// <param name="count">字节数量</param>
        /// <param name="timeOut">超时（毫秒，-1永久 -2默认）</param>
        /// <returns></returns>
        public static ReceiveMode ParseByte(int count, int timeOut = -2) => new ReceiveMode() { Type = ReceiveModeEnum.Byte, Data = count, TimeOut = timeOut };
        /// <summary>
        /// 读取所有立即可用的字节
        /// </summary>
        /// <param name="timeOut">超时（毫秒，-1永久 -2默认）</param>
        /// <returns></returns>
        public static ReceiveMode ParseByteAll(int timeOut = -2) => new ReceiveMode() { Type = ReceiveModeEnum.ByteAll, TimeOut = timeOut };
        /// <summary>
        /// 读取指定的字符数量
        /// </summary>
        /// <param name="count">字符数量</param>
        /// <param name="timeOut">超时（毫秒，-1永久 -2默认）</param>
        /// <returns></returns>
        public static ReceiveMode ParseChar(int count, int timeOut = -2) => new ReceiveMode() { Type = ReceiveModeEnum.Char, Data = count, TimeOut = timeOut };
        /// <summary>
        /// 读取达到指定的时间间隔后没有新消息后结束
        /// </summary>
        /// <param name="time">时间间隔，默认10毫秒</param>
        /// <param name="timeOut">超时（毫秒，-1永久 -2默认）</param>
        /// <returns></returns>
        public static ReceiveMode ParseTime(int time = 10, int timeOut = -2) => new ReceiveMode() { Type = ReceiveModeEnum.Time, Data = time, TimeOut = timeOut };
        /// <summary>
        /// 读取到指定的字符串后结束
        /// </summary>
        /// <param name="endString">为 null 时为 Environment.NewLine</param>
        /// <param name="timeOut">超时（毫秒，-1永久 -2默认）</param>
        /// <returns></returns>
        public static ReceiveMode ParseToString(string endString = null, int timeOut = -2) => new ReceiveMode() { Type = ReceiveModeEnum.ToString, Data = endString ?? Environment.NewLine, TimeOut = timeOut };

        /// <summary>
        /// 初始化一个新对象并重新设置超时
        /// </summary>
        /// <param name="mode">响应模式</param>
        /// <param name="timeOut">超时（毫秒，-1永久 -2默认）</param>
        /// <returns></returns>
        public static ReceiveMode SetTimeOut(ReceiveMode mode, int timeOut) => new ReceiveMode() { Type = mode.Type, Data = mode.Data, TimeOut = timeOut };
    }

    /// <summary>
    /// 接收数据的方式
    /// </summary>
    public enum ReceiveModeEnum
    {
        /// <summary>
        /// 读取指定的字节数量
        /// </summary>
        Byte = 10,
        /// <summary>
        /// 读取所有立即可用的字节
        /// </summary>
        ByteAll,
        /// <summary>
        /// 读取指定的字符数量
        /// </summary>
        Char = 20,
        /// <summary>
        /// 读取达到指定的时间隔间后没有新消息后结束
        /// </summary>
        Time = 30,
        /// <summary>
        /// 读取到指定的字符串后结束
        /// </summary>
        ToString = 40,
        //Regex = 50,
    }

    /// <summary>
    /// 连接模式
    /// </summary>
    public enum ConnectionMode
    {
        /// <summary>
        /// 手动。需要自己去打开和关闭，此方式比较灵活。
        /// </summary>
        Manual,
        /// <summary>
        /// 自动打开。没有执行Open()时每次发送和接收会自动打开和关闭，比较合适需要短链接的场景，如需要临时的长链接也可以调用Open()后在Close()。
        /// </summary>
        AutoOpen = 10,
        /// <summary>
        /// 自动断线重连。在执行了Open()后，如果检测到断开后会自动打开，比较合适需要长链接的场景。调用Close()将不再重连。
        /// </summary>
        AutoReconnection,
    }

    public class OpenClientData
    {
        System.IO.Stream stream;
        System.Net.Sockets.Socket socket;

        public OpenClientData(System.IO.Stream stream) => this.stream = stream;

        public OpenClientData(System.Net.Sockets.Socket socket) => this.socket = socket;

        public Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            if (stream != null)
            {
                return stream.ReadAsync(buffer, offset, count);
            }
            else
            {
                var receiveResult1 = socket.BeginReceive(buffer, offset, count, SocketFlags.None, null, null);
                receiveResult1.AsyncWaitHandle.WaitOne();
                return Task.FromResult<int>(socket.EndReceive(receiveResult1));
            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (stream != null)
            {
                stream.Write(buffer, offset, count);
            }
            else
            {
                socket.Send(buffer, offset, count, SocketFlags.None);
            }
        }

        public void Close()
        {
            if (stream != null)
            {
                stream?.Close();
                stream?.Dispose();
            }
            else
            {
                socket?.Shutdown(SocketShutdown.Both);
                socket?.Close();
            }
        }
    }
}
