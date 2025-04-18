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
    /// 服务端基类
    /// </summary>
    public abstract class ServiceBase
    {
        /// <summary>
        /// 是否打开
        /// </summary>
        public abstract bool IsOpen { get; }
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
        public virtual int TimeOut { get; set; } = 5000;
        /// <summary>
        /// 接受数据的方式
        /// </summary>
        public virtual ReceiveMode ReceiveMode { get; set; } = ReceiveMode.ParseByteAll();
        /// <summary>
        /// 接受数据的方式，在事件 Received 下
        /// </summary>
        public virtual ReceiveMode ReceiveModeReceived { get; set; } = ReceiveMode.ParseByteAll();

        /// <summary>
        /// 客户端即将链接
        /// </summary>
        public Func<ClientBase, bool> Opening;
        /// <summary>
        /// 客户端成功链接
        /// </summary>
        public Action<ClientBase> Opened;
        /// <summary>
        /// 客户端正在断开链接。仅主动断开。
        /// </summary>
        public Func<ClientBase, bool> Closing;
        /// <summary>
        /// 客户端断开链接，item2:是否自动断开
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
        public abstract IoTResult ClearAcceptCache();

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
        /// 发送并等待接受
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
}
