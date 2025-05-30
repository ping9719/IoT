using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// 网络
    /// </summary>
    public interface INetwork
    {
        /// <summary>
        /// 套接字
        /// </summary>
        Socket Socket { get; }
    }
}
