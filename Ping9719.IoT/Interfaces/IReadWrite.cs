using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT
{
    /// <summary>
    /// 基础的读和写接口
    /// </summary>
    public interface IReadWrite
    {
        /// <summary>
        /// 读取
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="address">地址</param>
        /// <returns>结果</returns>
        IoTResult<T> Read<T>(string address);
        /// <summary>
        /// 根据指定的类型读取
        /// </summary>
        /// <param name="type">类型。</param>
        /// <param name="address">地址</param>
        /// <returns>结果</returns>
        IoTResult<object> Read(string type, string address);
        /// <summary>
        /// 读取多个
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="address">地址</param>
        /// <param name="number">数量</param>
        /// <returns>结果</returns>
        IoTResult<IEnumerable<T>> Read<T>(string address, int number);
        /// <summary>
        /// 根据指定的类型读取多个
        /// </summary>
        /// <param name="type">类型。</param>
        /// <param name="address">地址</param>
        /// <param name="number">数量</param>
        /// <returns>结果</returns>
        IoTResult<IEnumerable<object>> Read(string type, string address, int number);
        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="length">长度</param>
        /// <param name="encoding">编码。一般情况下，如果为null为16进制的字符串</param>
        /// <returns></returns>
        IoTResult<string> ReadString(string address, int length, Encoding encoding);


        /// <summary>
        /// 写入
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns>结果</returns>
        IoTResult Write<T>(string address, T value);
        /// <summary>
        /// 根据指定的类型写入
        /// </summary>
        /// <param name="type">类型。</param>
        /// <param name="address">地址</param>
        /// <param name="value">写入的值。</param>
        /// <returns>结果</returns>
        IoTResult Write(string type, string address, object value);
        /// <summary>
        /// 写入多个
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="address">地址</param>
        /// <param name="values">值</param>
        /// <returns>结果</returns>
        IoTResult Write<T>(string address, IEnumerable<T> values);
        /// <summary>
        /// 根据指定的类型写入多个
        /// </summary>
        /// <param name="type">类型。</param>
        /// <param name="address">地址</param>
        /// <param name="values">写入的值。</param>
        /// <returns>结果</returns>
        IoTResult Write(string type, string address, IEnumerable<object> values);
        /// <summary>
        /// 写入字符串
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <param name="length">长度。一般用于补充的长度</param>
        /// <param name="encoding">编码。一般情况下，如果为null为16进制的字符串</param>
        /// <returns></returns>
        IoTResult WriteString(string address, string value, int length, Encoding encoding);
    }
}
