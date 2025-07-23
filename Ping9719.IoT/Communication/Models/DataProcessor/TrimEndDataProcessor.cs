using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// 处理器：移除结尾指定的匹配项。
    /// </summary>
    public class TrimEndDataProcessor : IDataProcessor
    {
        byte[] trimBytes = new byte[] { };
        /// <summary>
        /// 移除结尾指定的匹配项。
        /// </summary>
        /// <param name="trimBytes"></param>
        public TrimEndDataProcessor(params byte[] trimBytes)
        {
            this.trimBytes = trimBytes ?? new byte[] { };
        }

        public byte[] DataProcess(byte[] data)
        {
            data ??= new byte[] { };
            if (trimBytes == null || trimBytes.Length == 0)
                return data;

            return TrimDataProcessor.TrimHelper(trimBytes, 1, data);
        }
    }
}
