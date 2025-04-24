using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Enums
{
    /// <summary>
    /// 字符串编码
    /// </summary>
    public enum EncodingEnum
    {
        Hex16,
        ASCII,
        UTF8,
        Unicode,
        UTF32,
    }
    public static class EncodingEnumEx
    {
        public static Encoding GetEncoding(this EncodingEnum encodingEnum)
        {
            if (encodingEnum == EncodingEnum.ASCII)
                return Encoding.ASCII;
           else if (encodingEnum == EncodingEnum.UTF8)
                return Encoding.UTF8;
            else if (encodingEnum == EncodingEnum.Unicode)
                return Encoding.Unicode;
            else if (encodingEnum == EncodingEnum.UTF32)
                return Encoding.UTF32;

            return null;
        }
    }
}
