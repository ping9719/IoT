using Ping9719.IoT;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Scanner
{
    /// <summary>
    /// 扫码枪
    /// </summary>
    public interface IScannerBase
    {
        /// <summary>
        /// 进行一次扫码
        /// </summary>
        /// <returns>结果</returns>
        IoTResult<string> ReadOne();
    }
}
