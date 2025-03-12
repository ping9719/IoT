using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Common
{
    public class WordHelp
    {
        /// <summary>
        /// 指定类型占用的数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>0为无法判断</returns>
        public static ushort OccupyNum<T>()
        {
            var tType = typeof(T);
            if (tType == typeof(bool) || tType == typeof(byte) || tType == typeof(short) || tType == typeof(ushort))
                return 1;
            else if (tType == typeof(int) || tType == typeof(uint) || tType == typeof(float))
                return 2;
            else if (tType == typeof(double) || tType == typeof(long) || tType == typeof(ulong))
                return 4;
            else
                return 0;
        }

        /// <summary>
        /// 指定类型占用的字节数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>0为无法判断</returns>
        public static ushort OccupyBitNum<T>()
        {
            var tType = typeof(T);
            if (tType == typeof(bool) || tType == typeof(byte))
                return 1;
            else if (tType == typeof(short) || tType == typeof(ushort))
                return 2;
            else if (tType == typeof(int) || tType == typeof(uint) || tType == typeof(float))
                return 4;
            else if (tType == typeof(double) || tType == typeof(long) || tType == typeof(ulong))
                return 8;
            else
                return 0;
        }

        /// <summary>
        /// 分块
        /// </summary>
        /// <param name="sumNum">总数量</param>
        /// <param name="blockNum">块数量</param>
        /// <returns>【块1】+【块2】...=总数量</returns>
        public static int[] SplitBlock(int sumNum, int blockNum)
        {
            if (sumNum <= blockNum)
                return new int[] { sumNum };

            var cs = sumNum % blockNum == 0 ? sumNum / blockNum : sumNum / blockNum + 1;
            var jg = new int[cs];
            for (var i = 0; i < cs; i++)
            {
                jg[i] = blockNum;
            }

            if (sumNum % blockNum != 0)
            {
                jg[cs - 1] = sumNum % blockNum;
            }
            return jg;
        }

        /// <summary>
        /// 分块
        /// </summary>
        /// <param name="objNum">对象总数量</param>
        /// <param name="blockSize">块大小</param>
        /// <param name="addFir">第一个对象地址</param>
        /// <param name="objSpace">每个对象的间距。x=startAdd+i*倍数</param>
        /// <returns>1地址 2数量</returns>
        public static Dictionary<int, int> SplitBlock(int objNum, int blockSize, int addFir, int objSpace = 1)
        {
            if (objNum <= blockSize)
                return new Dictionary<int, int> { { addFir, objNum } };

            var cs = objNum % blockSize == 0 ? objNum / blockSize : objNum / blockSize + 1;
            var jg = new Dictionary<int, int>(cs);
            for (var i = 0; i < cs; i++)
            {
                var aaa = blockSize * i + addFir;
                jg.Add((aaa - addFir) * objSpace + addFir, blockSize);
            }

            if (objNum % blockSize != 0)
            {
                jg[jg.Last().Key] = objNum % blockSize;
            }
            return jg;
        }
    }
}
