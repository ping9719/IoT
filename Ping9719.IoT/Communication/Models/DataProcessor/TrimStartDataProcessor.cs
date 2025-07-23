using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// 处理器：移除开头指定的匹配项。
    /// </summary>
    public class TrimStartDataProcessor : IDataProcessor
    {
        byte[] trimBytes = new byte[] { };
        /// <summary>
        /// 移除开头指定的匹配项。
        /// </summary>
        /// <param name="trimBytes"></param>
        public TrimStartDataProcessor(params byte[] trimBytes)
        {
            this.trimBytes = trimBytes ?? new byte[] { };
        }

        public byte[] DataProcess(byte[] data)
        {
            data ??= new byte[] { };
            if (trimBytes == null || trimBytes.Length == 0)
                return data;

            return TrimDataProcessor.TrimHelper(trimBytes, 0, data);
        }
    }
}
