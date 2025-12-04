using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Algorithm
{
    /// <summary>
    /// 傅立叶变换的滤波算法
    /// </summary>
    public class FFTFilter
    {
        /// <summary>
        /// 对指定的原始数据进行滤波，并返回成功的数据值
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="filter">滤波值：最大值为1，不能低于0，越接近1，滤波强度越强，也可能会导致失去真实信号，为0时没有滤波效果。</param>
        /// <param name="maxDegreeOfParallelism">最大并行度，默认6，设为1则禁用并行计算</param>
        /// <returns>滤波后的数据值</returns>
        public static double[] FilterFFT(double[] source, double filter, int maxDegreeOfParallelism = 6)
        {
            if (source == null || source.Length == 0)
            {
                return new double[] { };
            }

            filter = Math.Max(0.0, Math.Min(1.0, filter));

            int fillLength;
            List<double> filledData = FillDataArray(new List<double>(source), out fillLength);

            // 根据数据大小和并行度设置选择计算方法
            Complex[] fftResult;
            bool useParallel = maxDegreeOfParallelism > 1 && filledData.Count > 10000;

            if (useParallel)
            {
                fftResult = ParallelIterativeFFT(filledData.ToArray(), maxDegreeOfParallelism);
            }
            else
            {
                fftResult = IterativeFFT(filledData.ToArray());
            }

            // 应用滤波
            ApplyFilterWithParallelism(fftResult, filter, maxDegreeOfParallelism);

            // 计算逆FFT
            Complex[] ifftResult;
            if (useParallel)
            {
                ifftResult = ParallelIFFT(fftResult, maxDegreeOfParallelism);
            }
            else
            {
                ifftResult = IFFTFromFFTResult(fftResult);
            }

            // 提取结果
            double[] result = new double[source.Length];
            int startIndex = fillLength;

            if (useParallel)
            {
                ParallelExtractResult(result, ifftResult, startIndex, maxDegreeOfParallelism);
            }
            else
            {
                for (int i = 0; i < source.Length; i++)
                {
                    result[i] = ifftResult[i + startIndex].Real;
                }
            }

            return result;
        }

        /// <summary>
        /// 对指定的数据进行填充，方便的进行傅立叶计算
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="putLength">输出的长度</param>
        /// <returns>填充结果</returns>
        private static List<double> FillDataArray(List<double> source, out int putLength)
        {
            int n = source.Count;

            // 计算最近的2的幂次方
            int nextPowerOfTwo = 1;
            while (nextPowerOfTwo < n)
            {
                nextPowerOfTwo <<= 1;
            }

            int num = (nextPowerOfTwo - n) / 2 + 1;
            putLength = num;

            if (num <= 0)
            {
                return source;
            }

            double first = source[0];
            double last = source[n - 1];

            // 使用InsertRange和AddRange提高性能
            List<double> padded = new List<double>(nextPowerOfTwo);
            for (int i = 0; i < num; i++)
            {
                padded.Add(first);
            }

            padded.AddRange(source);

            for (int i = 0; i < num; i++)
            {
                padded.Add(last);
            }

            return padded;
        }

        /// <summary>
        /// 并行迭代FFT（利用多核CPU）
        /// </summary>
        /// <param name="input">输入数据</param>
        /// <param name="maxDegreeOfParallelism">最大并行度</param>
        /// <returns>FFT结果</returns>
        private static Complex[] ParallelIterativeFFT(double[] input, int maxDegreeOfParallelism)
        {
            int n = input.Length;

            // 确保长度是2的幂次方
            if ((n & (n - 1)) != 0)
            {
                int newSize = 1;
                while (newSize < n)
                {
                    newSize <<= 1;
                }

                double[] padded = new double[newSize];
                Array.Copy(input, 0, padded, 0, n);
                input = padded;
                n = newSize;
            }

            Complex[] data = new Complex[n];

            // 并行初始化
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
            Parallel.For(0, n, parallelOptions, i =>
            {
                data[i] = new Complex(input[i], 0);
            });

            // 位反转置换（串行部分）
            for (int i = 1, j = 0; i < n; i++)
            {
                int bit = n >> 1;
                for (; (j & bit) != 0; bit >>= 1)
                {
                    j ^= bit;
                }
                j ^= bit;

                if (i < j)
                {
                    Complex temp = data[i];
                    data[i] = data[j];
                    data[j] = temp;
                }
            }

            // 迭代FFT，外层循环并行化
            for (int len = 2; len <= n; len <<= 1)
            {
                double angle = -2.0 * Math.PI / len;
                Complex wlen = new Complex(Math.Cos(angle), Math.Sin(angle));

                // 并行处理外层循环
                Parallel.For(0, n / len, parallelOptions, k =>
                {
                    int i = k * len;
                    Complex w = Complex.One;

                    for (int j = 0; j < len / 2; j++)
                    {
                        Complex u = data[i + j];
                        Complex v = w * data[i + j + len / 2];

                        data[i + j] = u + v;
                        data[i + j + len / 2] = u - v;

                        w *= wlen;
                    }
                });
            }

            return data;
        }

        /// <summary>
        /// 并行IFFT
        /// </summary>
        private static Complex[] ParallelIFFT(Complex[] data, int maxDegreeOfParallelism)
        {
            int n = data.Length;
            Complex[] result = new Complex[n];

            // 取共轭（并行）
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
            Parallel.For(0, n, parallelOptions, i =>
            {
                result[i] = Complex.Conjugate(data[i]);
            });

            // 对共轭数据执行FFT（并行迭代版）
            // 位反转置换（串行部分）
            for (int i = 1, j = 0; i < n; i++)
            {
                int bit = n >> 1;
                for (; (j & bit) != 0; bit >>= 1)
                {
                    j ^= bit;
                }
                j ^= bit;

                if (i < j)
                {
                    Complex temp = result[i];
                    result[i] = result[j];
                    result[j] = temp;
                }
            }

            // 迭代FFT
            for (int len = 2; len <= n; len <<= 1)
            {
                double angle = -2.0 * Math.PI / len;
                Complex wlen = new Complex(Math.Cos(angle), Math.Sin(angle));

                // 并行处理
                Parallel.For(0, n / len, parallelOptions, k =>
                {
                    int i = k * len;
                    Complex w = Complex.One;

                    for (int j = 0; j < len / 2; j++)
                    {
                        Complex u = result[i + j];
                        Complex v = w * result[i + j + len / 2];

                        result[i + j] = u + v;
                        result[i + j + len / 2] = u - v;

                        w *= wlen;
                    }
                });
            }

            // 取共轭并缩放（并行）
            double scale = 1.0 / n;
            Parallel.For(0, n, parallelOptions, i =>
            {
                result[i] = Complex.Conjugate(result[i]) * scale;
            });

            return result;
        }

        /// <summary>
        /// 带并行度控制的滤波器应用
        /// </summary>
        private static void ApplyFilterWithParallelism(Complex[] fftData, double filterThreshold, int maxDegreeOfParallelism)
        {
            if (filterThreshold <= 0.0)
            {
                return; // 不进行滤波
            }

            int n = fftData.Length;

            // 计算幅度
            double[] magnitudes = new double[n];
            double maxMagnitude = 0.0;

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };

            // 并行计算幅度
            Parallel.For(0, n, parallelOptions, i =>
            {
                double magnitude = fftData[i].Magnitude;
                magnitudes[i] = magnitude;

                // 使用锁更新最大值
                if (magnitude > maxMagnitude)
                {
                    lock (magnitudes)
                    {
                        if (magnitude > maxMagnitude)
                        {
                            maxMagnitude = magnitude;
                        }
                    }
                }
            });

            // 应用阈值滤波
            double threshold = maxMagnitude * filterThreshold;
            Parallel.For(0, n, parallelOptions, i =>
            {
                if (magnitudes[i] < threshold)
                {
                    fftData[i] = Complex.Zero;
                }
            });
        }

        /// <summary>
        /// 并行提取结果
        /// </summary>
        private static void ParallelExtractResult(double[] result, Complex[] ifftResult, int startIndex, int maxDegreeOfParallelism)
        {
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
            Parallel.For(0, result.Length, parallelOptions, i =>
            {
                result[i] = ifftResult[i + startIndex].Real;
            });
        }

        /// <summary>
        /// 迭代FFT（适合大数据量）
        /// </summary>
        /// <param name="input">输入数据</param>
        /// <returns>FFT结果</returns>
        private static Complex[] IterativeFFT(double[] input)
        {
            int n = input.Length;

            // 确保长度是2的幂次方
            if ((n & (n - 1)) != 0)
            {
                int newSize = 1;
                while (newSize < n)
                {
                    newSize <<= 1;
                }

                double[] padded = new double[newSize];
                Array.Copy(input, 0, padded, 0, n);
                input = padded;
                n = newSize;
            }

            Complex[] data = new Complex[n];
            for (int i = 0; i < n; i++)
            {
                data[i] = new Complex(input[i], 0);
            }

            // 位反转置换
            for (int i = 1, j = 0; i < n; i++)
            {
                int bit = n >> 1;
                for (; (j & bit) != 0; bit >>= 1)
                {
                    j ^= bit;
                }
                j ^= bit;

                if (i < j)
                {
                    Complex temp = data[i];
                    data[i] = data[j];
                    data[j] = temp;
                }
            }

            // 迭代FFT
            for (int len = 2; len <= n; len <<= 1)
            {
                double angle = -2.0 * Math.PI / len;
                Complex wlen = new Complex(Math.Cos(angle), Math.Sin(angle));

                for (int i = 0; i < n; i += len)
                {
                    Complex w = Complex.One;

                    for (int j = 0; j < len / 2; j++)
                    {
                        Complex u = data[i + j];
                        Complex v = w * data[i + j + len / 2];

                        data[i + j] = u + v;
                        data[i + j + len / 2] = u - v;

                        w *= wlen;
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// 从FFT结果计算IFFT
        /// </summary>
        private static Complex[] IFFTFromFFTResult(Complex[] data)
        {
            int n = data.Length;
            Complex[] result = new Complex[n];

            for (int i = 0; i < n; i++)
            {
                result[i] = Complex.Conjugate(data[i]);
            }

            // 使用迭代FFT
            // 位反转置换
            for (int i = 1, j = 0; i < n; i++)
            {
                int bit = n >> 1;
                for (; (j & bit) != 0; bit >>= 1)
                {
                    j ^= bit;
                }
                j ^= bit;

                if (i < j)
                {
                    Complex temp = result[i];
                    result[i] = result[j];
                    result[j] = temp;
                }
            }

            // 迭代FFT
            for (int len = 2; len <= n; len <<= 1)
            {
                double angle = -2.0 * Math.PI / len;
                Complex wlen = new Complex(Math.Cos(angle), Math.Sin(angle));

                for (int i = 0; i < n; i += len)
                {
                    Complex w = Complex.One;

                    for (int j = 0; j < len / 2; j++)
                    {
                        Complex u = result[i + j];
                        Complex v = w * result[i + j + len / 2];

                        result[i + j] = u + v;
                        result[i + j + len / 2] = u - v;

                        w *= wlen;
                    }
                }
            }

            // 取共轭并缩放
            double scale = 1.0 / n;
            for (int i = 0; i < n; i++)
            {
                result[i] = Complex.Conjugate(result[i]) * scale;
            }

            return result;
        }
    }
}
