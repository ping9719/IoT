using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device
{
    /// <summary>
    /// 无协议设备
    /// </summary>
    public class RawDevice : ClientHostBase
    {
        public RawDevice() : base() { }
        public RawDevice(ClientBase client) : base(client) { }
    }
}
