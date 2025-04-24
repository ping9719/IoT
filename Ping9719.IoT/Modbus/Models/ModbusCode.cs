using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Modbus
{
    /// <summary>
    /// 功能码
    /// </summary>
    public enum ModbusCode : byte
    {
        读线圈 = 1,
        读只读线圈,
        读寄存器,
        读只读寄存器,

        写单个线圈,
        写单个寄存器,

        写多个线圈 = 0x0f,
        写多个寄存器 = 0x10,
    }
}
