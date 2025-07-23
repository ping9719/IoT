using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// 处理器：向开头添加固定的值。
    /// </summary>
    public class StartAddValueDataProcessor : IDataProcessor
    {
        byte[] _dataEnd = new byte[] { };

        /// <summary>
        /// 向开头添加固定的值。
        /// </summary>
        /// <param name="dataEnd"></param>
        public StartAddValueDataProcessor(params byte[] dataEnd)
        {
            _dataEnd = dataEnd ?? new byte[] { };
        }

        /// <summary>
        /// 向开头添加固定的值。
        /// </summary>
        /// <param name="dataEnd"></param>
        /// <param name="encoding"></param>
        public StartAddValueDataProcessor(string dataEnd, Encoding encoding)
        {
            _dataEnd = string.IsNullOrEmpty(dataEnd) ? new byte[] { } : encoding.GetBytes(dataEnd);
        }

        public byte[] DataProcess(byte[] data)
        {
            return _dataEnd.Concat(data ?? new byte[] { }).ToArray();
        }
    }
}
