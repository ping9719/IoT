using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT
{
    /// <summary>
    /// 客户端接口
    /// </summary>
    public interface IClient
    {
        //EndianFormat EndianFormat { get; set; }

        /// <summary>
        /// 客户端
        /// </summary>
        ClientBase Client { get; }
    }
}
