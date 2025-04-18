using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT
{
    /// <summary>
    /// 不推荐使用
    /// </summary>
    public interface IIoTBase
    {
        /// <summary>
        /// 字符串编码格式
        /// </summary>
        Encoding Encoding { get; set; }
        /// <summary>
        /// 是否是连接的
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// 打开连接（如果已经是连接状态会先关闭再打开）
        /// </summary>
        /// <returns></returns>
        IoTResult Open();
        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        IoTResult Close();

        /// <summary>
        /// 读取
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="address">地址</param>
        /// <returns>结果</returns>
        IoTResult<T> Read<T>(string address);
        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="length">长度</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        IoTResult<string> ReadString(string address, int length, Encoding encoding);
        /// <summary>
        /// 读取多个
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="address">地址</param>
        /// <param name="number">数量</param>
        /// <returns>结果</returns>
        IoTResult<IEnumerable<T>> Read<T>(string address, int number);

        /// <summary>
        /// 写入
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns>结果</returns>
        IoTResult Write<T>(string address, T value);
        /// <summary>
        /// 写入字符串
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <param name="length">长度</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        IoTResult WriteString(string address, string value, int length, Encoding encoding);
        /// <summary>
        /// 写入多个
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="address">地址</param>
        /// <param name="values">值</param>
        /// <returns>结果</returns>
        IoTResult Write<T>(string address, params T[] values);
    }
}
