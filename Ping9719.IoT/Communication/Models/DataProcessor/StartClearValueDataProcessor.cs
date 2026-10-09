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
        byte[] _dataStart = new byte[] { };
        /// <summary>
        /// 向开头移除固定的值。
        /// </summary>
        /// <param name="dataStart">要移除的内容</param>
        public StartClearValueDataProcessor(params byte[] dataStart)
        {
            _dataStart = dataStart ?? new byte[] { };
        }
        /// <summary>
        /// 向开头移除固定的值。
        /// </summary>
        /// <param name="dataStart">要移除的内容</param>
        /// <param name="encoding">字符串编码</param>
        public StartClearValueDataProcessor(string dataStart, Encoding encoding)
        {
            _dataStart = string.IsNullOrEmpty(dataStart) ? new byte[] { } : encoding.GetBytes(dataStart);
        }

        public byte[] DataProcess(byte[] data)
        {
            data ??= new byte[] { };
            if (_dataStart.Length == 0)
                return data;

            return data.StartsWith(_dataStart) ? data.Skip(_dataStart.Length).ToArray() : data;
        }
    }
}
