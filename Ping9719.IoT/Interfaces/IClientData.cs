using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT
{
    /// <summary>
    /// 支持读或写的客户端
    /// </summary>
    public interface IClientData : IClient, IReadWrite
    {

    }
}
