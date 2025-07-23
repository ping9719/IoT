using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Algorithm
{
    /// <summary>
    /// 平均点位算法
    /// </summary>
    public static class AveragePoint
    {
        /// <summary>
        /// 开始计算
        /// </summary>
        /// <param name="begin">开始的点位，以逗号、分号、空格，分割的字符串，如2，4；6</param>
        /// <param name="end">结束的点位，以逗号、分号、空格，分割的字符串，如8，10；20</param>
        /// <param name="num">加上开始和结束一共的数量，需要大于2</param>
        /// <returns>包含开始点结束点的全部平均数据</returns>
        public static List<double[]> Start(string begin, string end, int num)
        {
            return Start(begin.Split(new char[] { ' ', ',', '，', ';', '；' }, StringSplitOptions.RemoveEmptyEntries).Select(o => Convert.ToDouble(o)), end.Split(new char[] { ' ', ',', '，', ';', '；' }, StringSplitOptions.RemoveEmptyEntries).Select(o => Convert.ToDouble(o)), num);
        }

        /// <summary>
        /// 开始计算
        /// </summary>
        /// <param name="begin">开始的点位</param>
        /// <param name="end">结束的点位</param>
        /// <param name="num">加上开始和结束一共的数量，需要大于2</param>
        /// <returns>包含开始点结束点的全部平均数据</returns>
        public static double[] Start(double begin, double end, int num)
        {
            return Start(new double[] { begin }, new double[] { end }, num).SelectMany(o => o).ToArray();
        }

        /// <summary>
        /// 开始计算
        /// </summary>
        /// <param name="begin">开始的点位，如2，4；6</param>
        /// <param name="end">结束的点位，如8，10；20</param>
        /// <param name="num">加上开始和结束一共的数量，需要大于2</param>
        /// <returns>包含开始点结束点的全部平均数据</returns>
        public static List<double[]> Start(IEnumerable<double> begin, IEnumerable<double> end, int num)
        {
            if (begin.Count() != end.Count())
                throw new Exception("点位数量需要相等");
            if (num < 2)
                throw new Exception("数量需要>=2");

            List<double[]> sb1 = new List<double[]>(num);
            for (var i = 0; i < num; i++)//数量
            {
                double[] sb = new double[begin.Count()];
                for (var j = 0; j < begin.Count(); j++)//长度
                {
                    var da1 = (end.ElementAt(j) - begin.ElementAt(j)) / (num - 1) * i + begin.ElementAt(j);
                    sb[j] = da1;
                }
                sb1.Add(sb);
            }

            return sb1;
        }
    }
}
