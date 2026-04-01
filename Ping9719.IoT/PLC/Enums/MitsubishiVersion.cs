using System;
using System.ComponentModel;

namespace Ping9719.IoT.PLC
{
    /// <summary>
    /// 三菱型号版本
    /// </summary>
    public enum MitsubishiVersion : byte
    {
        /// <summary>
        /// 未定义
        /// </summary>
        None = 0,
        /// <summary>
        /// 三菱 MC A-1E帧
        /// </summary>
        A_1E = 1,
        /// <summary>
        /// 三菱 MC Qna-3E帧
        /// </summary>
        Qna_3E = 2,
    }
}
