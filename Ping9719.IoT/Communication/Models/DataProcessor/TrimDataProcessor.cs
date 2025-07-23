using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// 处理器：移除前后指定的匹配项。
    /// </summary>
    public class TrimDataProcessor : IDataProcessor
    {
        byte[] trimBytes = new byte[] { };
        /// <summary>
        /// 移除前后指定的匹配项。
        /// </summary>
        /// <param name="trimBytes"></param>
        public TrimDataProcessor(params byte[] trimBytes)
        {
            this.trimBytes = trimBytes ?? new byte[] { };
        }

        public byte[] DataProcess(byte[] data)
        {
            data ??= new byte[] { };
            if (trimBytes == null || trimBytes.Length == 0)
                return data;

            return TrimDataProcessor.TrimHelper(trimBytes, 2, data);
        }

        internal static byte[] TrimHelper(byte[] trimChars, int trimType, byte[] data)
        {
            int num = data.Length - 1;
            int i = 0;
            if (trimType != 1)
            {
                for (i = 0; i < data.Length; i++)
                {
                    var num2 = 0;
                    var c = data[i];
                    for (num2 = 0; num2 < trimChars.Length && trimChars[num2] != c; num2++)
                    {
                    }

                    if (num2 == trimChars.Length)
                    {
                        break;
                    }
                }
            }

            if (trimType != 0)
            {
                for (num = data.Length - 1; num >= i; num--)
                {
                    var num3 = 0;
                    var c2 = data[num];
                    for (num3 = 0; num3 < trimChars.Length && trimChars[num3] != c2; num3++)
                    {
                    }

                    if (num3 == trimChars.Length)
                    {
                        break;
                    }
                }
            }

            var start = i;
            var end = num;

            int num1 = end - start + 1;
            if (num1 == data.Length)
            {
                return data;
            }

            if (num1 == 0)
            {
                return new byte[] { };
            }

            return data.Skip(start).Take(num1).ToArray();

        }
    }
}
