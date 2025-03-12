using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.PLC.Enums
{
    public enum CipVariableType : byte
    {
        TIMER = 1,
        COUNTER,
        CHANNEL,
        UINT_BCD,
        UDINT_BCD,
        ULINT_BCD,
        ENUM,
        DATE_NSEC,
        TIME_NSEC,
        DATE_AND_TIME_NSEC,
        TIME_OF_DAY_NSEC,
        UNION,
        BOOL = 193,
        SINT,
        INT,
        DINT,
        LINT,
        USINT,
        UINT,
        UDINT,
        ULINT,
        REAL,
        LREAL,
        STRING = 208,
        BYTE,
        WORD,
        DWORD,
        LWORD,
        ASTRUCT = 160,
        STRUCT = 162,
        ARRAY
    }
}
