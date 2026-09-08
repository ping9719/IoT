using Ping9719.IoT.Communication;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT
{
    /// <summary>
    /// 对接口 <see cref="IReadWrite"/> 的扩展实现
    /// </summary>
    public abstract class ReadWriteBase : IReadWrite
    {
        public EndianFormat EndianFormat { get; set; } = EndianFormat.DCBA;
        #region IReadWrite
        public abstract IoTResult<T> Read<T>(string address);
        public abstract IoTResult<IEnumerable<T>> Read<T>(string address, int number);
        public abstract IoTResult<string> ReadString(string address, int length, Encoding encoding);
        public abstract IoTResult Write<T>(string address, T value);
        public abstract IoTResult Write<T>(string address, IEnumerable<T> values);
        public abstract IoTResult WriteString(string address, string value, int length, Encoding encoding);
        #endregion

        #region 对字符串类型的实现
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="type">不区分大小写的类型。（bool，byte，int16，int32，int64，uint16，uint32，uint64，float，double，string，datatime，timespan，char，）</param>
        /// <param name="address">地址</param>
        /// <returns>结果</returns>
        public virtual IoTResult<object> Read(string type, string address)
        {
            try
            {
                var ts = type.Trim().ToLower();
                switch (ts)
                {
                    case "bool":
                        return Read<bool>(address).ToVal<object>(o => (object)o, true);
                    case "byte":
                        return Read<byte>(address).ToVal<object>(o => (object)o, true);
                    case "int16":
                    case "short":
                        return Read<Int16>(address).ToVal<object>(o => (object)o, true);
                    case "int32":
                    case "int":
                        return Read<Int32>(address).ToVal<object>(o => (object)o, true);
                    case "int64":
                    case "long":
                        return Read<Int64>(address).ToVal<object>(o => (object)o, true);
                    case "uint16":
                    case "ushort":
                        return Read<UInt16>(address).ToVal<object>(o => (object)o, true);
                    case "uint32":
                    case "uint":
                        return Read<UInt32>(address).ToVal<object>(o => (object)o, true);
                    case "uint64":
                    case "ulong":
                        return Read<UInt64>(address).ToVal<object>(o => (object)o, true);
                    case "float":
                    case "single":
                        return Read<float>(address).ToVal<object>(o => (object)o, true);
                    case "double":
                        return Read<double>(address).ToVal<object>(o => (object)o, true);
                    case "string":
                        return Read<string>(address).ToVal<object>(o => (object)o, true);
                    case "datatime":
                        return Read<DateTime>(address).ToVal<object>(o => (object)o, true);
                    case "timespan":
                        return Read<TimeSpan>(address).ToVal<object>(o => (object)o, true);
                    case "char":
                        return Read<Char>(address).ToVal<object>(o => (object)o, true);
                    default:
                        return IoTResult.Create<object>().AddError($"不支持的类型[{type}]").ToEnd();
                }
            }
            catch (Exception ex)
            {
                return IoTResult.Create<object>().AddError(ex);
            }
        }
        /// <summary>
        /// 读取多个
        /// </summary>
        /// <param name="type">不区分大小写的类型。（bool，byte，int16，int32，int64，uint16，uint32，uint64，float，double，string，datatime，timespan，char，）</param>
        /// <param name="address">地址</param>
        /// <param name="number">数量</param>
        /// <returns>结果</returns>
        public virtual IoTResult<IEnumerable<object>> Read(string type, string address, int number)
        {
            try
            {
                var ts = type.Trim().ToLower();
                switch (ts)
                {
                    case "bool":
                        return Read<bool>(address, number).ToVal<IEnumerable<object>>(o => o.Select(o2 => (object)o2), true);
                    case "byte":
                        return Read<byte>(address, number).ToVal<IEnumerable<object>>(o => o.Select(o2 => (object)o2), true);
                    case "int16":
                    case "short":
                        return Read<Int16>(address, number).ToVal<IEnumerable<object>>(o => o.Select(o2 => (object)o2), true);
                    case "int32":
                    case "int":
                        return Read<Int32>(address, number).ToVal<IEnumerable<object>>(o => o.Select(o2 => (object)o2), true);
                    case "int64":
                    case "long":
                        return Read<Int64>(address, number).ToVal<IEnumerable<object>>(o => o.Select(o2 => (object)o2), true);
                    case "uint16":
                    case "ushort":
                        return Read<UInt16>(address, number).ToVal<IEnumerable<object>>(o => o.Select(o2 => (object)o2), true);
                    case "uint32":
                    case "uint":
                        return Read<UInt32>(address, number).ToVal<IEnumerable<object>>(o => o.Select(o2 => (object)o2), true);
                    case "uint64":
                    case "ulong":
                        return Read<UInt64>(address, number).ToVal<IEnumerable<object>>(o => o.Select(o2 => (object)o2), true);
                    case "float":
                    case "single":
                        return Read<float>(address, number).ToVal<IEnumerable<object>>(o => o.Select(o2 => (object)o2), true);
                    case "double":
                        return Read<double>(address, number).ToVal<IEnumerable<object>>(o => o.Select(o2 => (object)o2), true);
                    case "string":
                        return Read<string>(address, number).ToVal<IEnumerable<object>>(o => o.Select(o2 => (object)o2), true);
                    case "datatime":
                        return Read<DateTime>(address, number).ToVal<IEnumerable<object>>(o => o.Select(o2 => (object)o2), true);
                    case "timespan":
                        return Read<TimeSpan>(address, number).ToVal<IEnumerable<object>>(o => o.Select(o2 => (object)o2), true);
                    case "char":
                        return Read<Char>(address, number).ToVal<IEnumerable<object>>(o => o.Select(o2 => (object)o2), true);
                    default:
                        return IoTResult.Create<IEnumerable<object>>().AddError($"不支持的类型[{type}]").ToEnd();
                }
            }
            catch (Exception ex)
            {
                return IoTResult.Create<IEnumerable<object>>().AddError(ex);
            }

        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="type">不区分大小写的类型。（bool，byte，int16，int32，int64，uint16，uint32，uint64，float，double，string，datatime，timespan，char，）</param>
        /// <param name="address">地址</param>
        /// <param name="value">写入的值。大部分进行<see cref="Convert"/>转换</param>
        /// <returns>结果</returns>
        public virtual IoTResult Write(string type, string address, object value)
        {
            try
            {
                //是否为数组或集合
                if (value is IEnumerable enumerable && !(value is string))
                    return WriteIn(type, address, enumerable.Cast<object>());

                var ts = type.Trim().ToLower();
                switch (ts)
                {
                    case "bool":
                        return Write<bool>(address, Convert.ToBoolean(value));
                    case "byte":
                        return Write<byte>(address, Convert.ToByte(value));
                    case "int16":
                    case "short":
                        return Write<Int16>(address, Convert.ToInt16(value));
                    case "int32":
                    case "int":
                        return Write<Int32>(address, Convert.ToInt32(value));
                    case "int64":
                    case "long":
                        return Write<Int64>(address, Convert.ToInt64(value));
                    case "uint16":
                    case "ushort":
                        return Write<UInt16>(address, Convert.ToUInt16(value));
                    case "uint32":
                    case "uint":
                        return Write<UInt32>(address, Convert.ToUInt32(value));
                    case "uint64":
                    case "ulong":
                        return Write<UInt64>(address, Convert.ToUInt64(value));
                    case "float":
                    case "single":
                        return Write<float>(address, Convert.ToSingle(value));
                    case "double":
                        return Write<double>(address, Convert.ToDouble(value));
                    case "string":
                        return Write<string>(address, Convert.ToString(value));
                    case "datatime":
                        return Write<DateTime>(address, Convert.ToDateTime(value));
                    case "timespan":
                        return Write<TimeSpan>(address, (TimeSpan)value);
                    case "char":
                        return Write<Char>(address, Convert.ToChar(value));
                    default:
                        return IoTResult.Create<object>().AddError($"不支持的类型[{type}]").ToEnd();
                }
            }
            catch (Exception ex)
            {
                return IoTResult.Create().AddError(ex);
            }
        }
        /// <summary>
        /// 写入多个
        /// </summary>
        /// <param name="type">不区分大小写的类型。（bool，byte，int16，int32，int64，uint16，uint32，uint64，float，double，string，datatime，timespan，char，）</param>
        /// <param name="address">地址</param>
        /// <param name="values">写入的值。大部分进行<see cref="Convert"/>转换</param>
        /// <returns>结果</returns>
        public virtual IoTResult Write(string type, string address, IEnumerable<object> values)
        {
            return WriteIn(type, address, values);
        }

        private IoTResult WriteIn(string type, string address, IEnumerable<object> values)
        {
            try
            {
                var ts = type.Trim().ToLower();
                switch (ts)
                {
                    case "bool":
                        return Write<bool>(address, values.Select(o => Convert.ToBoolean(o)).ToArray());
                    case "byte":
                        return Write<byte>(address, values.Select(o => Convert.ToByte(o)).ToArray());
                    case "int16":
                    case "short":
                        return Write<Int16>(address, values.Select(o => Convert.ToInt16(o)).ToArray());
                    case "int32":
                    case "int":
                        return Write<Int32>(address, values.Select(o => Convert.ToInt32(o)).ToArray());
                    case "int64":
                    case "long":
                        return Write<Int64>(address, values.Select(o => Convert.ToInt64(o)).ToArray());
                    case "uint16":
                    case "ushort":
                        return Write<UInt16>(address, values.Select(o => Convert.ToUInt16(o)).ToArray());
                    case "uint32":
                    case "uint":
                        return Write<UInt32>(address, values.Select(o => Convert.ToUInt32(o)).ToArray());
                    case "uint64":
                    case "ulong":
                        return Write<UInt64>(address, values.Select(o => Convert.ToUInt64(o)).ToArray());
                    case "float":
                    case "single":
                        return Write<float>(address, values.Select(o => Convert.ToSingle(o)).ToArray());
                    case "double":
                        return Write<double>(address, values.Select(o => Convert.ToDouble(o)).ToArray());
                    case "string":
                        return Write<string>(address, values.Select(o => Convert.ToString(o)).ToArray());
                    case "datatime":
                        return Write<DateTime>(address, values.Select(o => Convert.ToDateTime(o)).ToArray());
                    case "timespan":
                        return Write<TimeSpan>(address, values.Select(o => (TimeSpan)o).ToArray());
                    case "char":
                        return Write<Char>(address, values.Select(o => Convert.ToChar(o)).ToArray());
                    default:
                        return IoTResult.Create<IEnumerable<object>>().AddError($"不支持的类型[{type}]").ToEnd();
                }
            }
            catch (Exception ex)
            {
                return IoTResult.Create().AddError(ex);
            }
        }
        #endregion
    }
}
