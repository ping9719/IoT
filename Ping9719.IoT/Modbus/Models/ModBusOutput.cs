using System;
using System.Collections.Generic;
using System.Text;

namespace Ping9719.IoT.Modbus.Models
{
    /// <summary>
    /// Modbus输出
    /// </summary>
    public class ModbusOutput
    {
        /// <summary>
        /// 地址
        /// </summary>
        public int Address { get; set; }
        /// <summary>
        /// 站号
        /// </summary>
        public byte StationNumber { get; set; }
        /// <summary>
        /// 功能码
        /// </summary>
        public byte FunctionCode { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public object Value { get; set; }
    }
}
