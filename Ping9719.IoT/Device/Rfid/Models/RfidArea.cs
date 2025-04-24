using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Rfid
{
    /// <summary>
    /// rfid区域
    /// </summary>
    public enum RfidArea
    {
        /// <summary>
        /// 存储标签的保留区（Reserved Memory）。通常用于存储标签的制造商信息或其他保留数据。
        /// </summary>
        Retain,
        /// <summary>
        /// 存储标签的电子产品代码（Electronic Product Code）。通常用于唯一标识产品。
        /// </summary>
        EPC,
        /// <summary>
        /// 存储用户数据区（User Memory）。用于存储用户自定义的数据。
        /// </summary>
        User,
        /// <summary>
        /// 存储标签的唯一标识符（Unique Identifier）。通常是一个不可更改的值，用于唯一标识标签。
        /// </summary>
        TID,
        /// <summary>
        /// 存储标签的扩展区（Extended Memory）。用于存储额外的数据或信息。
        /// </summary>
        Ext,
        /// <summary>
        /// ISO15693数据块
        /// </summary>
        ISO15693,
        /// <summary>
        /// ISO14443A数据块
        /// </summary>
        ISO14443A,
    }
}
