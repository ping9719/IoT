using System;
using System.Collections.Generic;
using System.Linq;

namespace Ping9719.IoT
{
    /// <summary>
    /// 请求结果
    /// </summary>
    public class IoTResult
    {
        /// <summary>
        /// 创建实例
        /// </summary>
        public static IoTResult Create() => new IoTResult();
        /// <summary>
        /// 创建实例
        /// </summary>
        public static IoTResult<T> Create<T>() => new IoTResult<T>();
        /// <summary>
        /// 创建实例
        /// </summary>
        public static IoTResult<T> Create<T>(T data) => new IoTResult<T>(data);

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSucceed { get; set; } = true;
        /// <summary>
        /// 请求报文（多组）
        /// </summary>
        public List<byte[]> Requests { get; set; } = new List<byte[]>();
        /// <summary>
        /// 请求报文字符串（回车+空格分割）
        /// </summary>
        public string RequestText { get => Requests == null ? string.Empty : string.Join(Environment.NewLine, Requests.Select(t => t == null ? string.Empty : string.Join(" ", t.Select(t2 => t2.ToString("X2"))))); }
        /// <summary>
        /// 响应报文（多组）
        /// </summary>
        public List<byte[]> Responses { get; set; } = new List<byte[]>();
        /// <summary>
        /// 响应报文字符串（回车+空格分割）
        /// </summary>
        public string ResponseText { get => Responses == null ? string.Empty : string.Join(Environment.NewLine, Responses.Select(t => t == null ? string.Empty : string.Join(" ", t.Select(t2 => t2.ToString("X2"))))); }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; protected set; } = DateTime.Now;
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; protected set; } = null;
        /// <summary>
        /// 耗时
        /// </summary>
        public TimeSpan? TimeConsuming { get => EndTime.HasValue ? (EndTime.Value - StartTime) : null; }
        /// <summary>
        /// 详细异常
        /// </summary>
        public List<Exception> Error { get; set; } = new List<Exception>();
        /// <summary>
        /// 以分号分割的异常文本信息
        /// </summary>
        public string ErrorText { get => Error == null ? string.Empty : string.Join(";", Error.Select(o => o.Message)); }

        /// <summary>
        /// 添加错误
        /// </summary>
        public IoTResult AddError(IEnumerable<Exception> error, bool? isSucceed = false)
        {
            if (isSucceed.HasValue)
                IsSucceed = isSucceed.Value;

            foreach (var err in error)
            {
                if (!Error.Any(o => o.Message == err.Message))
                    Error.Add(err);
            }
            return this;
        }

        /// <summary>
        /// 添加错误
        /// </summary>
        public IoTResult AddError(Exception error, bool? isSucceed = false) => AddError(new[] { error }, isSucceed);

        /// <summary>
        /// 添加错误
        /// </summary>
        public IoTResult AddError(string error, bool? isSucceed = false) => AddError(new Exception(error), isSucceed);

        /// <summary>
        /// 设为结束
        /// </summary>
        public IoTResult ToEnd(DateTime? endTime = null)
        {
            endTime ??= DateTime.Now;
            EndTime = endTime;
            return this;
        }

        /// <summary>
        /// 转为有值的结果
        /// </summary>
        public IoTResult<T> ToVal<T>() => new IoTResult<T>(this);

        /// <summary>
        /// 转为有值的结果
        /// </summary>
        public IoTResult<T> ToVal<T>(T data) => new IoTResult<T>(this, data);
    }

    /// <summary>
    /// 请求结果
    /// </summary>
    public class IoTResult<T> : IoTResult
    {
        public IoTResult() { }
        public IoTResult(T data) : this(null, data) { }
        public IoTResult(IoTResult result) : this(result, default) { }
        public IoTResult(IoTResult result, T data)
        {
            if (result != null)
            {
                IsSucceed = result.IsSucceed;
                Requests = result.Requests.ToList();
                Responses = result.Responses.ToList();
                StartTime = result.StartTime;
                EndTime = result.EndTime;
                Error = result.Error.ToList();
            }

            Value = data;
        }

        /// <summary>
        /// 数据结果
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 添加错误
        /// </summary>
        public new IoTResult<T> AddError(IEnumerable<Exception> error, bool? isSucceed = false)
        {
            base.AddError(error, isSucceed);
            return this;
        }

        /// <summary>
        /// 添加错误
        /// </summary>
        public new IoTResult<T> AddError(Exception error, bool? isSucceed = false) => AddError(new[] { error }, isSucceed);

        /// <summary>
        /// 添加错误
        /// </summary>
        public new IoTResult<T> AddError(string error, bool? isSucceed = false) => AddError(new Exception(error), isSucceed);

        /// <summary>
        /// 标记为结束
        /// </summary>
        public new IoTResult<T> ToEnd(DateTime? endTime = null)
        {
            base.ToEnd(endTime);
            return this;
        }

        /// <summary>
        /// 转为有值的结果
        /// </summary>
        /// <typeparam name="T1">转换的类型</typeparam>
        /// <param name="func">把原来的值转为新的值</param>
        /// <returns></returns>
        public IoTResult<T1> ToVal<T1>(Func<T, T1> func) => new IoTResult<T1>(this, func.Invoke(Value));
    }
}
