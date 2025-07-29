using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// 处理器：向开头移除固定的值。
    /// </summary>
    public class StartClearValueDataProcessor : IDataProcessor
    {
        byte[] _dataEnd = new byte[] { };
        /// <summary>
        /// 向开头移除固定的值。
        /// </summary>
        /// <param name="dataEnd"></param>
        public StartClearValueDataProcessor(params byte[] dataEnd)
        {
            _dataEnd = dataEnd ?? new byte[] { };
        }
        /// <summary>
        /// 向开头移除固定的值。
        /// </summary>
        /// <param name="dataEnd"></param>
        /// <param name="encoding"></param>
        public StartClearValueDataProcessor(string dataEnd, Encoding encoding)
        {
            _dataEnd = string.IsNullOrEmpty(dataEnd) ? new byte[] { } : encoding.GetBytes(dataEnd);
        }

        public byte[] DataProcess(byte[] data)
        {
            return data.StartsWith(_dataEnd) ? data.Skip(_dataEnd.Length).ToArray() : data;
        }
    }
}
