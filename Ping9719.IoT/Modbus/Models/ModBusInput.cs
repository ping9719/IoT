using Ping9719.IoT;
using Ping9719.IoT.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ping9719.IoT.Modbus.Models
{
    /// <summary>
    /// Modbus输入
    /// </summary>
    public class ModbusInput
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public DataTypeEnum DataType { get; set; }
        /// <summary>
        /// 站号
        /// </summary>
        public byte StationNumber { get; set; }
        /// <summary>
        /// 功能码
        /// </summary>
        public byte FunctionCode { get; set; }

        /// <summary>
        /// 解析地址
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address">全写法"s=2;x=3;100"，对应站号，功能码，地址</param>
        /// <param name="isRead">是否为读</param>
        /// <param name="stationNumber">默认站号</param>
        /// <returns></returns>
        public static IoTResult<ModbusInput> AddressAnalysis<T>(string address, bool isRead, byte stationNumber)
        {
            //s=2;x=3;100"，对应站号，功能码，地址
            var result = new IoTResult<ModbusInput>();

            byte s = stationNumber;
            byte x = 0;
            ushort a = 0;
            bool isaok = false;

            //解析地址
            var addressSplit = address.Split(new char[] { ';', '；' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in addressSplit)
            {
                var itemSplit = item.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (itemSplit.Length == 1)
                {
                    if (ushort.TryParse(itemSplit[0], out a))
                    {
                        isaok = true;
                    }
                    else
                    {
                        result.IsSucceed = false;
                        result.AddError($"address地址[{address}]格式不正确。无法解析其中的地址（0-65535）。");
                        
                        return result;
                    }
                }
                else if (itemSplit.Length == 2)
                {
                    if (itemSplit[0] == "s")
                    {
                        if (!byte.TryParse(itemSplit[1], out s))
                        {
                            result.IsSucceed = false;
                            result.AddError($"address地址[{address}]格式不正确。无法解析其中的站号（0-255）。");
                            
                            return result;
                        }
                    }
                    else if (itemSplit[0] == "x")
                    {
                        if (!byte.TryParse(itemSplit[1], out x))
                        {
                            result.IsSucceed = false;
                            result.AddError($"address地址[{address}]格式不正确。无法解析其中的功能码（1-255）。");
                            
                            return result;
                        }
                        if (x == 0)
                        {
                            result.IsSucceed = false;
                            result.AddError($"address地址[{address}]格式不正确。无法解析其中的功能码（1-255）。");
                            
                            return result;
                        }
                    }
                }
            }

            if (!isaok)
            {
                result.IsSucceed = false;
                result.AddError($"address地址[{address}]格式不正确。无法找到其中的地址（0-65535）。");
                
                return result;
            }

            //赋值功能码
            var tType = typeof(T);
            if (x == 0)
            {
                if (isRead)
                {
                    if (tType == typeof(bool))
                        x = 1;
                    else
                        x = 3;
                }
                else
                {
                    if (tType == typeof(bool))
                        x = 5;
                    else
                        x = 16;
                }
            }

            //赋值类型
            DataTypeEnum dataTypeEnum = DataTypeEnum.None;
            if (tType == typeof(bool))
                dataTypeEnum = DataTypeEnum.Bool;
            else if (tType == typeof(byte))
                dataTypeEnum = DataTypeEnum.Byte;
            else if (tType == typeof(short))
                dataTypeEnum = DataTypeEnum.Int16;
            else if (tType == typeof(ushort))
                dataTypeEnum = DataTypeEnum.UInt16;
            else if (tType == typeof(int))
                dataTypeEnum = DataTypeEnum.Int32;
            else if (tType == typeof(uint))
                dataTypeEnum = DataTypeEnum.UInt32;
            else if (tType == typeof(long))
                dataTypeEnum = DataTypeEnum.Int64;
            else if (tType == typeof(ulong))
                dataTypeEnum = DataTypeEnum.UInt64;
            else if (tType == typeof(float))
                dataTypeEnum = DataTypeEnum.Float;
            else if (tType == typeof(double))
                dataTypeEnum = DataTypeEnum.Double;
            else if (tType == typeof(string))
                dataTypeEnum = DataTypeEnum.String;
            else
                dataTypeEnum = DataTypeEnum.None;

            result.Value = new ModbusInput()
            {
                StationNumber = s,
                Address = a.ToString(),
                FunctionCode = x,
                DataType = dataTypeEnum,
            };
            return result;
        }
    }
}
