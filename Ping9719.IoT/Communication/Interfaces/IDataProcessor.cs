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

    /// <summary>
    /// 数据结尾添加处理器，可在数据的结尾添加固定的值。
    /// </summary>
    public class DataEndAddProcessor : IDataProcessor
    {
        byte[] _dataEnd = new byte[] { };
        /// <summary>
        /// 数据结尾添加处理器
        /// </summary>
        /// <param name="dataEnd"></param>
        public DataEndAddProcessor(params byte[] dataEnd)
        {
            _dataEnd = dataEnd ?? new byte[] { };
        }
        /// <summary>
        /// 数据结尾添加处理器
        /// </summary>
        /// <param name="dataEnd"></param>
        /// <param name="encoding"></param>
        public DataEndAddProcessor(string dataEnd, Encoding encoding)
        {
            _dataEnd = string.IsNullOrEmpty(dataEnd) ? new byte[] { } : encoding.GetBytes(dataEnd);
        }

        public byte[] DataProcess(byte[] data)
        {
            return (data ?? new byte[] { }).Concat(_dataEnd).ToArray();
        }
    }

    /// <summary>
    /// 数据结尾移除处理器，可在数据的结尾移除固定的值。
    /// </summary>
    public class DataEndClearProcessor : IDataProcessor
    {
        byte[] _dataEnd = new byte[] { };
        /// <summary>
        /// 数据结尾移除处理器
        /// </summary>
        /// <param name="dataEnd"></param>
        public DataEndClearProcessor(params byte[] dataEnd)
        {
            _dataEnd = dataEnd ?? new byte[] { };
        }
        /// <summary>
        /// 数据结尾移除处理器
        /// </summary>
        /// <param name="dataEnd"></param>
        /// <param name="encoding"></param>
        public DataEndClearProcessor(string dataEnd, Encoding encoding)
        {
            _dataEnd = string.IsNullOrEmpty(dataEnd) ? new byte[] { } : encoding.GetBytes(dataEnd);
        }

        public byte[] DataProcess(byte[] rawData)
        {
            return rawData.EndsWith(_dataEnd) ? rawData.Take(rawData.Length - _dataEnd.Length).ToArray() : rawData;
        }
    }

}
