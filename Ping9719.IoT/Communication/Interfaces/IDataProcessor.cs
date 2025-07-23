using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// 数据处理器
    /// </summary>
    public interface IDataProcessor
    {
        /// <summary>
        /// 数据处理
        /// </summary>
        /// <param name="rawData">源数据</param>
        /// <returns>处理后的数据</returns>
        byte[] DataProcess(byte[] rawData);
    }
}
