using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Algorithm
{
    /// <summary>
    /// 仿射变换坐标转换器
    /// 支持相机坐标和机器人坐标之间的双向转换
    /// </summary>
    public class AffineTransform
    {
        private struct CoordinatePair
        {
            public double SourceX { get; set; }
            public double SourceY { get; set; }
            public double TargetX { get; set; }
            public double TargetY { get; set; }

            public CoordinatePair(double sourceX, double sourceY, double targetX, double targetY)
            {
                SourceX = sourceX;
                SourceY = sourceY;
                TargetX = targetX;
                TargetY = targetY;
            }
        }

        /// <summary>
        /// 仿射变换参数类
        /// </summary>
        public class TransformParameters
        {
            /// <summary>
            /// X轴缩放和旋转参数
            /// </summary>
            public double A { get; set; }
            /// <summary>
            /// Y轴对X轴变换的影响参数
            /// </summary>
            public double B { get; set; }
            /// <summary>
            /// X轴对Y轴变换的影响参数
            /// </summary>
            public double C { get; set; }
            /// <summary>
            /// Y轴缩放和旋转参数
            /// </summary>
            public double D { get; set; }
            /// <summary>
            /// X轴平移量（水平偏移）
            /// </summary>
            public double Tx { get; set; }
            /// <summary>
            /// Y轴平移量（垂直偏移）
            /// </summary>
            public double Ty { get; set; }
            /// <summary>
            /// 字符串
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return $"A={A:F6}, B={B:F6}, C={C:F6}, D={D:F6}, Tx={Tx:F6}, Ty={Ty:F6}";
            }
        }

        private double[,] transformationMatrix;
        private List<CoordinatePair> calibrationPairs;

        /// <summary>
        /// 变换参数
        /// </summary>
        public TransformParameters Parameters { get; private set; }
        /// <summary>
        /// 标定状态
        /// </summary>
        public bool IsCalibrated { get; private set; }
        /// <summary>
        /// 标定对数量
        /// </summary>
        public int CalibrationPairCount => calibrationPairs?.Count ?? 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AffineTransform()
        {
            Parameters = new TransformParameters();
            transformationMatrix = new double[2, 3];
            calibrationPairs = new List<CoordinatePair>();
            IsCalibrated = false;
        }

        /// <summary>
        /// 添加标定坐标对
        /// </summary>
        /// <param name="sourceX">源X坐标</param>
        /// <param name="sourceY">源Y坐标</param>
        /// <param name="targetX">目标X坐标</param>
        /// <param name="targetY">目标Y坐标</param>
        public void AddCalibration(double sourceX, double sourceY, double targetX, double targetY)
        {
            calibrationPairs.Add(new CoordinatePair(sourceX, sourceY, targetX, targetY));
        }
        /// <summary>
        /// 清空标定
        /// </summary>
        public void ClearCalibration()
        {
            calibrationPairs.Clear();
            IsCalibrated = false;
        }

        /// <summary>
        /// 执行标定
        /// </summary>
        /// <returns>标定是否成功</returns>
        public bool Calibrate()
        {
            if (calibrationPairs.Count < 3)
            {
                throw new InvalidOperationException($"至少需要3组坐标对进行标定，当前只有 {calibrationPairs.Count} 组");
            }

            try
            {
                // 构建最小二乘问题的矩阵
                int pointCount = calibrationPairs.Count;
                double[,] A = new double[2 * pointCount, 6];
                double[] B = new double[2 * pointCount];

                for (int i = 0; i < pointCount; i++)
                {
                    var pair = calibrationPairs[i];
                    double x = pair.SourceX;
                    double y = pair.SourceY;

                    // X 方向的方程: x' = a*x + b*y + tx
                    A[2 * i, 0] = x;
                    A[2 * i, 1] = y;
                    A[2 * i, 2] = 1;
                    A[2 * i, 3] = 0;
                    A[2 * i, 4] = 0;
                    A[2 * i, 5] = 0;
                    B[2 * i] = pair.TargetX;

                    // Y 方向的方程: y' = c*x + d*y + ty
                    A[2 * i + 1, 0] = 0;
                    A[2 * i + 1, 1] = 0;
                    A[2 * i + 1, 2] = 0;
                    A[2 * i + 1, 3] = x;
                    A[2 * i + 1, 4] = y;
                    A[2 * i + 1, 5] = 1;
                    B[2 * i + 1] = pair.TargetY;
                }

                // 使用最小二乘法求解
                double[] solution = SolveLeastSquares(A, B);

                // 设置变换参数
                Parameters.A = solution[0];
                Parameters.B = solution[1];
                Parameters.Tx = solution[2];
                Parameters.C = solution[3];
                Parameters.D = solution[4];
                Parameters.Ty = solution[5];

                // 设置变换矩阵
                transformationMatrix[0, 0] = Parameters.A;
                transformationMatrix[0, 1] = Parameters.B;
                transformationMatrix[0, 2] = Parameters.Tx;
                transformationMatrix[1, 0] = Parameters.C;
                transformationMatrix[1, 1] = Parameters.D;
                transformationMatrix[1, 2] = Parameters.Ty;

                IsCalibrated = true;
                return true;
            }
            catch (Exception ex)
            {
                IsCalibrated = false;
                return false;
            }
        }

        /// <summary>
        /// 坐标转换（源坐标 => 目标坐标）
        /// </summary>
        /// <param name="sourceX">源X坐标</param>
        /// <param name="sourceY">源Y坐标</param>
        /// <returns>转换后的坐标点</returns>
        public Tuple<double, double> Transform(double sourceX, double sourceY)
        {
            if (!IsCalibrated)
            {
                throw new InvalidOperationException("请先进行标定后再进行坐标转换");
            }

            double newX = transformationMatrix[0, 0] * sourceX + transformationMatrix[0, 1] * sourceY + transformationMatrix[0, 2];
            double newY = transformationMatrix[1, 0] * sourceX + transformationMatrix[1, 1] * sourceY + transformationMatrix[1, 2];

            return new Tuple<double, double>(newX, newY);
        }
        /// <summary>
        /// 反向坐标转换（目标坐标 => 源坐标）
        /// </summary>
        public Tuple<double, double> TransformInverse(double targetX, double targetY)
        {
            if (!IsCalibrated)
            {
                throw new InvalidOperationException("请先进行标定后再进行坐标转换");
            }

            // 计算逆矩阵
            double det = Parameters.A * Parameters.D - Parameters.B * Parameters.C;
            if (Math.Abs(det) < 1e-10)
            {
                throw new InvalidOperationException("变换矩阵不可逆，无法进行反向转换");
            }

            double invA = Parameters.D / det;
            double invB = -Parameters.B / det;
            double invC = -Parameters.C / det;
            double invD = Parameters.A / det;
            double invTx = (Parameters.B * Parameters.Ty - Parameters.D * Parameters.Tx) / det;
            double invTy = (Parameters.C * Parameters.Tx - Parameters.A * Parameters.Ty) / det;

            double originalX = invA * targetX + invB * targetY + invTx;
            double originalY = invC * targetX + invD * targetY + invTy;

            return new Tuple<double, double>(originalX, originalY);
        }

        /// <summary>
        /// 最小二乘法求解
        /// </summary>
        private double[] SolveLeastSquares(double[,] A, double[] B)
        {
            int rows = A.GetLength(0);
            int cols = A.GetLength(1);

            // A^T * A
            double[,] ATA = new double[cols, cols];
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    for (int k = 0; k < rows; k++)
                    {
                        ATA[i, j] += A[k, i] * A[k, j];
                    }
                }
            }

            // A^T * B
            double[] ATB = new double[cols];
            for (int i = 0; i < cols; i++)
            {
                for (int k = 0; k < rows; k++)
                {
                    ATB[i] += A[k, i] * B[k];
                }
            }

            // 求解线性方程组 (A^T * A) * X = A^T * B
            return SolveLinearSystem(ATA, ATB);
        }

        /// <summary>
        /// 求解线性方程组
        /// </summary>
        private double[] SolveLinearSystem(double[,] matrix, double[] vector)
        {
            int n = vector.Length;
            double[] solution = new double[n];

            // 使用高斯消元法
            for (int i = 0; i < n; i++)
            {
                // 寻找主元
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(matrix[k, i]) > Math.Abs(matrix[maxRow, i]))
                    {
                        maxRow = k;
                    }
                }

                // 交换行
                for (int k = i; k < n; k++)
                {
                    double temp = matrix[i, k];
                    matrix[i, k] = matrix[maxRow, k];
                    matrix[maxRow, k] = temp;
                }
                double tempVector = vector[i];
                vector[i] = vector[maxRow];
                vector[maxRow] = tempVector;

                // 消元
                for (int k = i + 1; k < n; k++)
                {
                    double factor = matrix[k, i] / matrix[i, i];
                    for (int j = i; j < n; j++)
                    {
                        matrix[k, j] -= factor * matrix[i, j];
                    }
                    vector[k] -= factor * vector[i];
                }
            }

            // 回代
            for (int i = n - 1; i >= 0; i--)
            {
                solution[i] = vector[i];
                for (int j = i + 1; j < n; j++)
                {
                    solution[i] -= matrix[i, j] * solution[j];
                }
                solution[i] /= matrix[i, i];
            }

            return solution;
        }

        /// <summary>
        /// 计算标定平均误差
        /// </summary>
        public double CalibrationAverageError()
        {
            double totalError = 0;
            foreach (var pair in calibrationPairs)
            {
                var transformed = Transform(pair.SourceX, pair.SourceY);
                double error = Math.Sqrt(
                    Math.Pow(transformed.Item1 - pair.TargetX, 2) +
                    Math.Pow(transformed.Item2 - pair.TargetY, 2)
                );
                totalError += error;
            }
            return totalError / calibrationPairs.Count;
        }

        /// <summary>
        /// 计算标定最大误差
        /// </summary>
        /// <returns>索引，最大误差</returns>
        public Tuple<int, double> CalibrationMaxError()
        {
            if (!IsCalibrated)
                return new Tuple<int, double>(-1, 0);

            double maxError = 0;
            int maxErrorIndex = 0;

            for (int i = 0; i < calibrationPairs.Count; i++)
            {
                var pair = calibrationPairs[i];
                var transformed = Transform(pair.SourceX, pair.SourceY);
                double error = Math.Sqrt(Math.Pow(transformed.Item1 - pair.TargetX, 2) + Math.Pow(transformed.Item2 - pair.TargetY, 2));

                if (error > maxError)
                {
                    maxError = error;
                    maxErrorIndex = i;
                }
            }
            return new Tuple<int, double>(maxErrorIndex, maxError);
        }
    }

}
