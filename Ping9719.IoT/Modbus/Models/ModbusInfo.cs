using Ping9719.IoT.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Modbus
{
    /// <summary>
    /// Modbus信息
    /// </summary>
    public class ModbusInfo
    {
        /// <summary>
        /// 地址 (0-65535)
        /// </summary>
        public UInt16 Address { get; set; }
        /// <summary>
        /// 地址位 （0-16）
        /// </summary>
        public byte? Bit { get; set; }
        /// <summary>
        /// 站号
        /// </summary>
        public byte StationNumber { get; set; }
        /// <summary>
        /// 功能码
        /// </summary>
        public ModbusCode? FunctionCode { get; set; }

        /// <summary>
        /// 解析地址
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address">全写法"s=2;x=3;100"，对应站号，功能码，地址</param>
        /// <param name="isRead">是否为读</param>
        /// <param name="stationNumber">默认站号</param>
        /// <returns></returns>
        public static IoTResult<ModbusInfo> AddressAnalysis(string address, byte stationNumber)
        {
            //s=2;x=3;100"，对应站号，功能码，地址
            var result = new IoTResult<ModbusInfo>() { Value = new ModbusInfo() };
            result.Value.StationNumber = stationNumber;

            //解析地址
            var addressSplit = address.Split(new char[] { ';', '；' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in addressSplit)
            {
                var itemSplit = item.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (itemSplit.Length == 1)
                {
                    var astr = itemSplit[0];
                    if (itemSplit[0].Contains("."))
                    {
                        var astrsp = itemSplit[0].Split('.');
                        astr = astrsp[0];
                        if (!byte.TryParse(astrsp[1], out byte bb))
                            return result.AddError($"address地址[{address}]格式不正确。无法解析其中的地址位（0-255）。");

                        result.Value.Bit = bb;
                    }

                    if (!ushort.TryParse(astr, out UInt16 aa))
                        return result.AddError($"address地址[{address}]格式不正确。无法解析其中的地址（0-65535）。");

                    result.Value.Address = aa;
                }
                else if (itemSplit.Length == 2)
                {
                    if (itemSplit[0] == "s")
                    {
                        if (!byte.TryParse(itemSplit[1], out byte ss))
                            return result.AddError($"address地址[{address}]格式不正确。无法解析其中的站号（0-255）。");

                        result.Value.StationNumber = ss;
                    }
                    else if (itemSplit[0] == "x")
                    {
                        if (!byte.TryParse(itemSplit[1], out byte xx))
                            return result.AddError($"address地址[{address}]格式不正确。无法解析其中的功能码（1-255）。");
                        if (xx == 0)
                            return result.AddError($"address地址[{address}]格式不正确。无法解析其中的功能码（1-255）。");

                        result.Value.FunctionCode = (ModbusCode)xx;
                    }
                }
            }

            ////赋值功能码
            //var tType = typeof(T);
            //if (!result.Value.FunctionCode.HasValue)
            //{
            //    if (isRead)
            //    {
            //        result.Value.FunctionCode = tType == typeof(bool) ? ModbusCode.读线圈 : ModbusCode.读寄存器;
            //    }
            //    //else
            //    //{
            //    //    if (tType == typeof(bool))
            //    //        x = Writevalue.Length == 1 ? (byte)5 : (byte)0x0f;
            //    //    else
            //    //        x = 0x10;
            //    //}
            //}

            //byte[] list1 = new byte[] { };
            ////赋值类型
            //DataTypeEnum dataTypeEnum = DataTypeEnum.None;
            //if (tType == typeof(bool))
            //{
            //    dataTypeEnum = DataTypeEnum.Bool;
            //    if (Writevalue.Length == 1)
            //    {
            //        list1 = (bool)(object)Writevalue[0] == true ? new byte[] { 0xFF, 0x00 } : new byte[] { 0x00, 0x00 };
            //    }
            //    else
            //    {
            //        list1 = WordHelp.SplitBlock(Writevalue.Select(o => ((bool)(object)o) ? 1 : 0), 8, 1, 0).Select(o => Convert.ToByte(string.Join("", o), 2)).ToArray();
            //    }
            //}
            //else if (tType == typeof(byte))
            //{ dataTypeEnum = DataTypeEnum.Byte; list1 = Writevalue.Select(o => (byte)(object)o).ToArray(); }
            //else if (tType == typeof(short))
            //{ dataTypeEnum = DataTypeEnum.Int16; list1 = Writevalue.SelectMany(o =>BitConverter.GetBytes((short)(object)o)).ToArray(); }
            //else if (tType == typeof(ushort))
            //{ dataTypeEnum = DataTypeEnum.UInt16; list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((ushort)(object)o)).ToArray(); }
            //else if (tType == typeof(int))
            //{ dataTypeEnum = DataTypeEnum.Int32; list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((int)(object)o)).ToArray(); }
            //else if (tType == typeof(uint))
            //{ dataTypeEnum = DataTypeEnum.UInt32; list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((uint)(object)o)).ToArray(); }
            //else if (tType == typeof(long))
            //{ dataTypeEnum = DataTypeEnum.Int64; list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((long)(object)o)).ToArray(); }
            //else if (tType == typeof(ulong))
            //{ dataTypeEnum = DataTypeEnum.UInt64; list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((ulong)(object)o)).ToArray(); }
            //else if (tType == typeof(float))
            //{ dataTypeEnum = DataTypeEnum.Float; list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((float)(object)o)).ToArray(); }
            //else if (tType == typeof(double))
            //{ dataTypeEnum = DataTypeEnum.Double; list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((double)(object)o)).ToArray(); }
            //else if (tType == typeof(string))
            //    dataTypeEnum = DataTypeEnum.String;
            //else
            //    dataTypeEnum = DataTypeEnum.None;

            //result.Value = new ModbusInfo()
            //{
            //    Address = a,
            //    Bit = isb ? b : null,
            //    StationNumber = s,
            //    FunctionCode = (ModbusCode)x,
            //};

            return result;
        }

        /// <summary>
        /// 得到ModbusTcp读或写的命令
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="readCount">读的数量</param>
        /// <param name="Writevalue">写的值</param>
        /// <param name="transactionId">事务标识</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public IoTResult<byte[]> GetModbusTcpCommand<T>(UInt16 readCount, T[] Writevalue, UInt16 transactionId, Encoding encoding, EndianFormat format)
        {
            IoTResult<byte[]> ioTResult = new IoTResult<byte[]>() { Value = new byte[] { } };
            try
            {
                if (readCount == 0 && (Writevalue == null || Writevalue.Count() == 0))
                    return ioTResult.AddError("读或写需有一个有数据");

                var tType = typeof(T);
                List<byte> result = new List<byte>(12);
                var swf = BitConverter.GetBytes(transactionId);
                var dz = BitConverter.GetBytes(Address);
                result.AddRange(new byte[]
                {
                    swf[1],swf[0],
                    0x00,0x00,//协议标识符,表示 Modbus TCP 协议
                    0x00,0x06,//长度字段,后续字节总数
                    StationNumber,//站号
                    0x00,//功能码，正常响应时与请求一致，错误时最高1
                    dz[1],dz[0],
                });
                //读
                if (readCount > 0)
                {
                    ModbusCode functionCode;
                    if (FunctionCode.HasValue)
                        functionCode = FunctionCode.Value;
                    else
                        functionCode = tType == typeof(bool) ? ModbusCode.读线圈 : ModbusCode.读寄存器;

                    //计算真实数量
                    var rCount = WordHelp.OccupyNum<T>();
                    rCount = Convert.ToUInt16(rCount == 0 ? readCount : readCount * rCount);

                    result.AddRange(BitConverter.GetBytes(rCount).Reverse());
                    result[7] = (byte)functionCode;
                    FunctionCode = functionCode;
                }
                //写
                else
                {
                    UInt16 yCount = Convert.ToUInt16(Writevalue.Count());//原本数量
                    UInt16 vCount = 0;//线圈/寄存器数量
                    ModbusCode? functionCode = null;
                    bool? isDan = null;
                    byte[] list1 = null;

                    if (tType == typeof(bool))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = yCount == 1 ? ModbusCode.写单个线圈 : ModbusCode.写多个线圈;

                        if (functionCode == ModbusCode.写单个线圈)
                        {
                            isDan = true;
                            list1 = (bool)(object)Writevalue[0] == true ? new byte[] { 0xFF, 0x00 } : new byte[] { 0x00, 0x00 };
                        }
                        else if (functionCode == ModbusCode.写多个线圈)
                        {
                            isDan = false;
                            list1 = WordHelp.SplitBlock(Writevalue.Select(o => ((bool)(object)o) ? 1 : 0), 8, 2, 0, true).Select(o => Convert.ToByte(string.Join("", o), 2)).ToArray();
                            vCount = yCount;
                        }
                    }
                    else if (tType == typeof(byte))
                    {
                        if (yCount % 2 != 0)
                            return ioTResult.AddError("byte类型数量必须为2的倍数");
                        if (!FunctionCode.HasValue)
                            functionCode = yCount == 2 ? ModbusCode.写单个寄存器 : ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写单个寄存器)
                        {
                            isDan = true;
                            list1 = Writevalue.Select(o => (byte)(object)o).Take(2).ToArray();
                        }
                        else if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.Select(o => (byte)(object)o).ToArray();
                            vCount = Convert.ToUInt16(yCount / 2);
                        }
                    }
                    else if (tType == typeof(short))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = yCount == 1 ? ModbusCode.写单个寄存器 : ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写单个寄存器)
                        {
                            isDan = true;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((short)(object)o)).Take(2).EndianNetToIot(format).ToArray();
                        }
                        else if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((short)(object)o).EndianNetToIot(format)).ToArray();
                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else if (tType == typeof(ushort))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = yCount == 1 ? ModbusCode.写单个寄存器 : ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写单个寄存器)
                        {
                            isDan = true;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((ushort)(object)o)).Take(2).EndianNetToIot(format).ToArray();
                        }
                        else if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((ushort)(object)o).EndianNetToIot(format)).ToArray();
                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else if (tType == typeof(int))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((int)(object)o).EndianNetToIot(format)).ToArray();
                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else if (tType == typeof(uint))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((uint)(object)o).EndianNetToIot(format)).ToArray();
                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else if (tType == typeof(long))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((long)(object)o).EndianNetToIot(format)).ToArray();
                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else if (tType == typeof(ulong))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((ulong)(object)o).EndianNetToIot(format)).ToArray();
                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else if (tType == typeof(float))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((float)(object)o).EndianNetToIot(format)).ToArray();
                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else if (tType == typeof(double))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((double)(object)o).EndianNetToIot(format)).ToArray();
                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else if (tType == typeof(string))
                    {
                        if (yCount != 1)
                            return ioTResult.AddError($"string只能写入一条数据");
                        if (!FunctionCode.HasValue)
                            functionCode = ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            if (encoding == null)
                                list1 = ((string)(object)Writevalue[0]).StringToByteArray();
                            else
                                list1 = encoding.GetBytes((string)(object)Writevalue[0]).ToArray();

                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else
                        return ioTResult.AddError($"不支持类型{tType.Name}");

                    if (isDan == true)
                    {
                        result.AddRange(list1);
                        result[7] = (byte)functionCode;
                    }
                    else if (isDan == false)
                    {
                        result.AddRange(BitConverter.GetBytes(vCount).Reverse());
                        result.Add(Convert.ToByte(list1.Length));
                        result.AddRange(list1);
                        var sl2 = BitConverter.GetBytes(Convert.ToUInt16(result.Count() - 6));
                        result[4] = sl2[1];
                        result[5] = sl2[0];
                        result[7] = (byte)functionCode;
                    }
                    else
                    {
                        return ioTResult.AddError($"无法解析出功能码等写入命令");
                    }
                    FunctionCode = functionCode;
                }

                ioTResult.Value = result.ToArray();
                return ioTResult;
            }
            catch (Exception ex)
            {
                return ioTResult.AddError(ex);
            }
        }

        /// <summary>
        /// 得到ModbusTcp读或写的命令
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="readCount">读的数量</param>
        /// <param name="Writevalue">写的值</param>
        /// <param name="encoding">编码</param>
        /// <returns>不带2位校验位</returns>
        public IoTResult<byte[]> GetModbusRtuCommand<T>(UInt16 readCount, T[] Writevalue, Encoding encoding, EndianFormat format)
        {
            IoTResult<byte[]> ioTResult = new IoTResult<byte[]>() { Value = new byte[] { } };
            try
            {
                if (readCount == 0 && (Writevalue == null || Writevalue.Count() == 0))
                    return ioTResult.AddError("读或写需有一个有数据");

                var tType = typeof(T);
                List<byte> result = new List<byte>(6);
                var dz = BitConverter.GetBytes(Address);
                result.AddRange(new byte[]
                {
                    StationNumber,//站号
                    0x00,//功能码
                    dz[1],dz[0],
                });
                //读
                if (readCount > 0)
                {
                    ModbusCode functionCode;
                    if (FunctionCode.HasValue)
                        functionCode = FunctionCode.Value;
                    else
                        functionCode = tType == typeof(bool) ? ModbusCode.读线圈 : ModbusCode.读寄存器;

                    //计算真实数量
                    var rCount = WordHelp.OccupyNum<T>();
                    rCount = Convert.ToUInt16(rCount == 0 ? readCount : readCount * rCount);

                    result.AddRange(BitConverter.GetBytes(rCount).Reverse());
                    result[1] = (byte)functionCode;
                    FunctionCode = functionCode;
                }
                //写
                else
                {
                    UInt16 yCount = Convert.ToUInt16(Writevalue.Count());//原本数量
                    UInt16 vCount = 0;//线圈/寄存器数量
                    ModbusCode? functionCode = null;
                    bool? isDan = null;
                    byte[] list1 = null;

                    if (tType == typeof(bool))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = yCount == 1 ? ModbusCode.写单个线圈 : ModbusCode.写多个线圈;

                        if (functionCode == ModbusCode.写单个线圈)
                        {
                            isDan = true;
                            list1 = (bool)(object)Writevalue[0] == true ? new byte[] { 0xFF, 0x00 } : new byte[] { 0x00, 0x00 };
                        }
                        else if (functionCode == ModbusCode.写多个线圈)
                        {
                            isDan = false;
                            list1 = WordHelp.SplitBlock(Writevalue.Select(o => ((bool)(object)o) ? 1 : 0), 8, 2, 0, true).Select(o => Convert.ToByte(string.Join("", o), 2)).ToArray();
                            vCount = yCount;
                        }
                    }
                    else if (tType == typeof(byte))
                    {
                        if (yCount % 2 != 0)
                            return ioTResult.AddError("byte类型数量必须为2的倍数");
                        if (!FunctionCode.HasValue)
                            functionCode = yCount == 2 ? ModbusCode.写单个寄存器 : ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写单个寄存器)
                        {
                            isDan = true;
                            list1 = Writevalue.Select(o => (byte)(object)o).Take(2).ToArray();
                        }
                        else if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.Select(o => (byte)(object)o).ToArray();
                            vCount = Convert.ToUInt16(yCount / 2);
                        }
                    }
                    else if (tType == typeof(short))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = yCount == 1 ? ModbusCode.写单个寄存器 : ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写单个寄存器)
                        {
                            isDan = true;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((short)(object)o)).Take(2).EndianNetToIot(format).ToArray();
                        }
                        else if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((short)(object)o).EndianNetToIot(format)).ToArray();
                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else if (tType == typeof(ushort))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = yCount == 1 ? ModbusCode.写单个寄存器 : ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写单个寄存器)
                        {
                            isDan = true;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((ushort)(object)o)).Take(2).EndianNetToIot(format).ToArray();
                        }
                        else if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((ushort)(object)o).EndianNetToIot(format)).ToArray();
                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else if (tType == typeof(int))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((int)(object)o).EndianNetToIot(format)).ToArray();
                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else if (tType == typeof(uint))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((uint)(object)o).EndianNetToIot(format)).ToArray();
                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else if (tType == typeof(long))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((long)(object)o).EndianNetToIot(format)).ToArray();
                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else if (tType == typeof(ulong))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((ulong)(object)o).EndianNetToIot(format)).ToArray();
                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else if (tType == typeof(float))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((float)(object)o).EndianNetToIot(format)).ToArray();
                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else if (tType == typeof(double))
                    {
                        if (!FunctionCode.HasValue)
                            functionCode = ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            list1 = Writevalue.SelectMany(o => BitConverter.GetBytes((double)(object)o).EndianNetToIot(format)).ToArray();
                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else if (tType == typeof(string))
                    {
                        if (yCount != 1)
                            return ioTResult.AddError($"string只能写入一条数据");
                        if (!FunctionCode.HasValue)
                            functionCode = ModbusCode.写多个寄存器;

                        if (functionCode == ModbusCode.写多个寄存器)
                        {
                            isDan = false;
                            if (encoding == null)
                                list1 = ((string)(object)Writevalue[0]).StringToByteArray();
                            else
                                list1 = encoding.GetBytes((string)(object)Writevalue[0]).ToArray();

                            vCount = Convert.ToUInt16(list1.Length / 2);
                        }
                    }
                    else
                        return ioTResult.AddError($"不支持类型{tType.Name}");

                    if (isDan == true)
                    {
                        result.AddRange(list1);
                        result[1] = (byte)functionCode;
                    }
                    else if (isDan == false)
                    {
                        result.AddRange(BitConverter.GetBytes(vCount).Reverse());
                        result.Add(Convert.ToByte(list1.Length));
                        result.AddRange(list1);
                        var sl2 = BitConverter.GetBytes(Convert.ToUInt16(result.Count() - 6));
                        result[1] = (byte)functionCode;
                    }
                    else
                    {
                        return ioTResult.AddError($"无法解析出功能码等写入命令");
                    }
                    FunctionCode = functionCode;
                }

                ioTResult.Value = result.ToArray();
                return ioTResult;
            }
            catch (Exception ex)
            {
                return ioTResult.AddError(ex);
            }
        }

    }
}
