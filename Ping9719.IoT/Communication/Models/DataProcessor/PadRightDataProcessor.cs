using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// 处理器：向右侧（尾部）添加固定的值达到指定的长度。
    /// </summary>
    public class PadRightDataProcessor : IDataProcessor
    {
        int totalWidth;
        byte paddingByte;

        /// <summary>
        /// 向右侧（尾部）添加固定的值达到指定的长度。
        /// </summary>
        /// <param name="totalWidth">填充的长度</param>
        /// <param name="paddingByte">填充的内容</param>
        public PadRightDataProcessor(int totalWidth, byte paddingByte = 0)
        {
            this.totalWidth = totalWidth;
            this.paddingByte = paddingByte;
        }

        public byte[] DataProcess(byte[] data)
        {
            data ??= new byte[] { };
            return data.Length < totalWidth ? data.Concat(Enumerable.Repeat(paddingByte, totalWidth - data.Length)).ToArray() : data;
        }
    }
}
