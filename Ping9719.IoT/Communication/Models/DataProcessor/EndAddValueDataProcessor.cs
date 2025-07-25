﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// 处理器：向结尾添加固定的值。比如结尾添加回车换行
    /// </summary>
    public class EndAddValueDataProcessor : IDataProcessor
    {
        byte[] _dataEnd = new byte[] { };
        /// <summary>
        /// 数据结尾添加处理器
        /// </summary>
        /// <param name="dataEnd"></param>
        public EndAddValueDataProcessor(params byte[] dataEnd)
        {
            _dataEnd = dataEnd ?? new byte[] { };
        }
        /// <summary>
        /// 数据结尾添加处理器
        /// </summary>
        /// <param name="dataEnd"></param>
        /// <param name="encoding"></param>
        public EndAddValueDataProcessor(string dataEnd, Encoding encoding)
        {
            _dataEnd = string.IsNullOrEmpty(dataEnd) ? new byte[] { } : encoding.GetBytes(dataEnd);
        }

        public byte[] DataProcess(byte[] data)
        {
            return (data ?? new byte[] { }).Concat(_dataEnd).ToArray();
        }
    }
}
