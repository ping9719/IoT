using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT
{
    public interface IIoT
    {
        ClientBase Client { get; }

        /// <summary>
        /// 读取
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="address">地址</param>
        /// <returns>结果</returns>
        IoTResult<T> Read<T>(string address);
        /// <summary>
        /// 读取多个
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="address">地址</param>
        /// <param name="number">数量</param>
        /// <returns>结果</returns>
        IoTResult<IEnumerable<T>> Read<T>(string address, int number);
        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="length">长度</param>
        /// <param name="encoding">编码</param>
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
        /// 写入多个
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="address">地址</param>
        /// <param name="values">值</param>
        /// <returns>结果</returns>
        IoTResult Write<T>(string address, params T[] values);
        /// <summary>
        /// 写入字符串
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <param name="length">长度</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        IoTResult WriteString(string address, string value, int length, Encoding encoding);

    }
}
