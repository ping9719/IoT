using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
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
        /// <summary>
        /// 类的名称
        /// </summary>
        public string Name { get => this.GetType().Name; }
        /// <summary>
        /// 是否打开
        /// </summary>
        public abstract bool IsOpen { get; }
        /// <summary>
        /// 链接模式
        /// </summary>
        public ConnectionMode ConnectionMode { get; set; }
        /// <summary>
        /// 断线重连，最大重连时间。默认10秒。
        /// </summary>
        public int MaxReconnectionTime { get; set; } = 10 * 1000;
        /// <summary>
        /// 接受区，缓冲区大小（默认1024 * 100）
        /// </summary>
        public int ReceiveBufferSize { get; set; } = 1024 * 100;

        /// <summary>
        /// 是否在发送和接受时丢弃来自缓冲区的数据（默认false）
        /// </summary>
        public virtual bool IsAutoDiscard { get; set; }
        /// <summary>
        /// 字符串编码
        /// </summary>
        public virtual Encoding Encoding { get; set; } = Encoding.UTF8;
        /// <summary>
        /// 超时（发送、接受、链接）（毫秒）-1永久
        /// </summary>
        public virtual int TimeOut { get; set; } = 3000;
        /// <summary>
        /// 接受数据的方式
        /// </summary>
        public virtual ReceiveMode ReceiveMode { get; set; } = ReceiveMode.ParseByteAll();
        /// <summary>
        /// 接受数据的方式，在事件 Received 下。注意 ReceiveModeEnum.Time 模式下时间设置太长或对方一直在发送消息，可能会死锁 ！
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
        /// 断开连接，item2:是否自动断开
        /// </summary>
        public Action<ClientBase, bool> Closed;
        /// <summary>
        /// 接收到信息
        /// </summary>
        public Action<ClientBase, byte[]> Received;

        /// <summary>
        /// 打开
        /// </summary>
        public abstract IoTResult Open();
        /// <summary>
        /// 关闭
        /// </summary>
        public abstract IoTResult Close();

        /// <summary>
        /// 清空接受缓存
        /// </summary>
        public abstract IoTResult DiscardInBuffer();

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="offset">开始位置</param>
        /// <param name="count">数量，-1全部</param>
        /// <returns></returns>
        public abstract IoTResult Send(byte[] data, int offset = 0, int count = -1);
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
        /// 接受
        /// </summary>
        /// <returns></returns>
        public abstract IoTResult<byte[]> Receive(ReceiveMode receiveMode = null);
        /// <summary>
        /// 接受为字符串
        /// </summary>
        /// <returns></returns>
        public virtual IoTResult<string> ReceiveString(ReceiveMode receiveMode = null, Encoding encoding = null)
        {
            var isok = Receive(receiveMode);
            return isok.IsSucceed ? isok.ToVal((encoding ?? Encoding).GetString(isok.Value)) : isok.ToVal<string>();
        }
        /// <summary>
        /// 发送并等待接受为字节
        /// </summary>
        /// <param name="data"></param>
        /// <param name="receiveMode"></param>
        /// <returns></returns>
        public abstract IoTResult<byte[]> SendReceive(byte[] data, ReceiveMode receiveMode = null);
        /// <summary>
        /// 发送并等待接受为字符串
        /// </summary>
        public virtual IoTResult<string> SendReceive(string data, ReceiveMode receiveMode = null, Encoding encoding = null)
        {
            var isok = SendReceive((encoding ?? Encoding).GetBytes(data), receiveMode);
            return isok.IsSucceed ? isok.ToVal((encoding ?? Encoding).GetString(isok.Value)) : isok.ToVal<string>();
        }
        /// <summary>
        /// 发送为字符串并等待结果为字节
        /// </summary>
        public virtual IoTResult<byte[]> SendReceiveToByte(string data, ReceiveMode receiveMode = null, Encoding encoding = null)
        {
            return SendReceive((encoding ?? Encoding).GetBytes(data), receiveMode);
        }

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
    }

    /// <summary>
    /// 响应模式
    /// </summary>
    public class ReceiveMode
    {
        /// <summary>
        /// 接受数据的方式
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
    }

    /// <summary>
    /// 接受数据的方式
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
        /// 自动打开。没有执行Open()时每次发送和接受会自动打开和关闭，比较合适需要短链接的场景，如需要临时的长链接也可以调用Open()后在Close()。
        /// </summary>
        AutoOpen = 10,
        /// <summary>
        /// 自动断线重连。在执行了Open()后，如果检测到断开后会自动打开，比较合适需要长链接的场景。调用Close()将不再重连。
        /// </summary>
        AutoReconnection,
    }
}
