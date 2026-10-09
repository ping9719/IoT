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
        byte[] _dataStart = new byte[] { };

        /// <summary>
        /// 向开头添加固定的值。
        /// </summary>
        /// <param name="dataStart">添加的内容</param>
        public StartAddValueDataProcessor(params byte[] dataStart)
        {
            _dataStart = dataStart ?? new byte[] { };
        }

        /// <summary>
        /// 向开头添加固定的值。
        /// </summary>
        /// <param name="dataStart">添加的内容</param>
        /// <param name="encoding">字符串编码</param>
        public StartAddValueDataProcessor(string dataStart, Encoding encoding)
        {
            _dataStart = string.IsNullOrEmpty(dataStart) ? new byte[] { } : encoding.GetBytes(dataStart);
        }

        public byte[] DataProcess(byte[] data)
        {
            return _dataStart.Concat(data ?? new byte[] { }).ToArray();
        }
    }
}
