using System;
using System.ComponentModel;

namespace Ping9719.IoT.PLC
{
    /// <summary>
    /// 西门子型号版本
    /// </summary>
    public enum SiemensVersion : byte
    {
        /// <summary>
        /// 未定义
        /// </summary>
        None = 0,
        /// <summary>
        /// 西门子S7-200 【需要配置网络模块】
        /// </summary>
        S7_200 = 1,
        /// <summary>
        /// 西门子S7-200Smar
        /// </summary>
        S7_200Smart = 2,
        /// <summary>
        /// 西门子S7-300
        /// </summary>
        S7_300 = 3,
        /// <summary>
        /// 西门子S7-400
        /// </summary>
        S7_400 = 4,
        /// <summary>
        /// 西门子S7-1200
        /// </summary>
        S7_1200 = 5,
        /// <summary>
        /// 西门子S7-1500
        /// </summary>
        S7_1500 = 6,
    }
}
