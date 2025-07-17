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
        ///// <summary>
        ///// 类的名称
        ///// </summary>
        //public string Name { get => this.GetType().Name; }
        /// <summary>
        /// 是否打开
        /// </summary>
        public abstract bool IsOpen { get; }
        /// <summary>
        /// 所有的客户端
        /// </summary>
        public abstract TcpClient[] Clients { get; }

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
        public Action<ClientBase> Closed;
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

    }
}
