using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Algorithm
{
    /// <summary>
    /// 线性回归
    /// </summary>
    public class LinearRegression
    {
        /// <summary>
        /// 斜率
        /// </summary>
        public double Slope { get; private set; }
        /// <summary>
        /// 截距
        /// </summary>
        public double Intercept { get; private set; }
        /// <summary>
        /// 相关系数平方。R²值（0-1）1.0为完美线性
        /// </summary>
        public double RSquare { get; private set; }

        /// <summary>
        /// 使用最小二乘法拟合
        /// </summary>
        /// <param name="xValues">点，逗号分隔</param>
        /// <param name="yValues">点，逗号分隔</param>
        public static LinearRegression Fit(string xValues, string yValues)
        {
           return Fit(xValues.Split(new char[] { ',', '，' }).Select(o => double.Parse(o)), yValues.Split(new char[] { ',', '，' }).Select(o => double.Parse(o)));
        }
        /// <summary>
        /// 使用最小二乘法拟合
        /// </summary>
        /// <param name="xValues">点</param>
        /// <param name="yValues">点</param>
        /// <exception cref="ArgumentException">长度不同</exception>
        public static LinearRegression Fit(IEnumerable<double> xValues, IEnumerable<double> yValues)
        {
            if (xValues.Count() != yValues.Count())
                throw new ArgumentException("xValues 和 yValues 长度必须相同");

            LinearRegression lR = new LinearRegression();
            int n = xValues.Count();
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0;

            for (int i = 0; i < n; i++)
            {
                sumX += xValues.ElementAt(i);
                sumY += yValues.ElementAt(i);
                sumXY += xValues.ElementAt(i) * yValues.ElementAt(i);
                sumX2 += xValues.ElementAt(i) * xValues.ElementAt(i);
                sumY2 += yValues.ElementAt(i) * yValues.ElementAt(i);
            }

            lR.Slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            lR.Intercept = (sumY - lR.Slope * sumX) / n;

            double numerator = n * sumXY - sumX * sumY;
            double denominator = Math.Sqrt((n * sumX2 - sumX * sumX) * (n * sumY2 - sumY * sumY));
            lR.RSquare = denominator == 0 ? 0 : Math.Pow(numerator / denominator, 2);
            return lR;
        }

        /// <summary>
        /// 预测
        /// </summary>
        /// <param name="x">x值</param>
        /// <returns></returns>
        public double Project(double x)
        {
            return Slope * x + Intercept;
        }

        /// <summary>
        /// 预测
        /// </summary>
        /// <param name="xValues">x值</param>
        /// <returns></returns>
        public double[] Project(double[] xValues)
        {
            return xValues.Select(x => Slope * x + Intercept).ToArray();
        }
    }
}
