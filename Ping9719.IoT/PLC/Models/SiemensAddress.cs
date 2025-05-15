

using System.Collections.Generic;
using System;
using System.Linq;

namespace Ping9719.IoT.PLC
{
    /// <summary>
    /// 西门子解析后的地址信息
    /// </summary>
    public class SiemensAddress
    {
        /// <summary>
        /// 原地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public DataTypeEnum DataType { get; set; }

        /// <summary>
        /// 区域类型
        /// </summary>
        public byte TypeCode { get; set; }
        /// <summary>
        /// DB块编号
        /// </summary>
        public ushort DbBlock { get; set; }
        /// <summary>
        /// 地址索引1
        /// </summary>
        public int Address1 { get; set; }
        /// <summary>
        /// 地址索引2
        /// </summary>
        public int Address2 { get; set; }
        /// <summary>
        /// 开始地址(西门子plc地址为8个位的长度，这里展开实际的开始地址。)
        /// </summary>
        public int BeginAddress { get; set; }
        /// <summary>
        /// 读取或写入长度
        /// </summary>
        public ushort Length { get; set; }
        /// <summary>
        /// 是否读取或写入bit类型
        /// </summary>
        public bool IsBit { get; set; } = false;

        /// <summary>
        /// 获取区域类型代码
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static SiemensAddress ConvertArg(string address)
        {
            try
            {
                //转换成大写
                address = address.ToUpper();
                var addressInfo = new SiemensAddress()
                {
                    Address = address,
                    DbBlock = 0,
                };
                switch (address[0])
                {
                    case 'I':
                        addressInfo.TypeCode = 0x81;
                        break;
                    case 'Q':
                        addressInfo.TypeCode = 0x82;
                        break;
                    case 'M':
                        addressInfo.TypeCode = 0x83;
                        break;
                    case 'D':
                        addressInfo.TypeCode = 0x84;
                        string[] adds = address.Split('.');
                        if (address[1] == 'B')
                            addressInfo.DbBlock = Convert.ToUInt16(adds[0].Substring(2));
                        else
                            addressInfo.DbBlock = Convert.ToUInt16(adds[0].Substring(1));
                        //TODO 
                        //addressInfo.BeginAddress = GetBeingAddress(address.Substring(address.IndexOf('.') + 1));
                        break;
                    case 'T':
                        addressInfo.TypeCode = 0x1D;
                        break;
                    case 'C':
                        addressInfo.TypeCode = 0x1C;
                        break;
                    case 'V':
                        addressInfo.TypeCode = 0x84;
                        addressInfo.DbBlock = 1;
                        break;
                }

                //if (address[0] != 'D' && address[1] != 'B')
                //    addressInfo.BeginAddress = GetBeingAddress(address.Substring(1));

                //DB块
                if (address[0] == 'D' && address[1] == 'B')
                {
                    //DB1.0.0、DB1.4（非PLC地址）
                    var indexOfpoint = address.IndexOf('.') + 1;
                    if (address[indexOfpoint] >= '0' && address[indexOfpoint] <= '9')
                        GetBeingAddress(address.Substring(indexOfpoint), addressInfo);
                    //DB1.DBX0.0、DB1.DBD4（标准PLC地址）
                    else
                        GetBeingAddress(address.Substring(address.IndexOf('.') + 4), addressInfo);
                }
                //非DB块
                else
                {
                    //I0.0、V1004的情况（非PLC地址）
                    if (address[1] >= '0' && address[1] <= '9')
                        GetBeingAddress(address.Substring(1), addressInfo);
                    //VB1004的情况（标准PLC地址）
                    else
                        GetBeingAddress(address.Substring(2), addressInfo);
                }
                return addressInfo;
            }
            catch (Exception ex)
            {
                throw new Exception($"地址[{address}]解析异常，ConvertArg Err:{ex.Message}");
            }
        }

        public static SiemensAddress[] ConvertArg(Dictionary<string, DataTypeEnum> addresses)
        {
            return addresses.Select(t =>
            {
                var item = ConvertArg(t.Key);
                item.DataType = t.Value;
                switch (t.Value)
                {
                    case DataTypeEnum.Bool:
                        item.Length = 1;
                        item.IsBit = true;
                        break;
                    case DataTypeEnum.Byte:
                        item.Length = 1;
                        break;
                    case DataTypeEnum.Int16:
                        item.Length = 2;
                        break;
                    case DataTypeEnum.UInt16:
                        item.Length = 2;
                        break;
                    case DataTypeEnum.Int32:
                        item.Length = 4;
                        break;
                    case DataTypeEnum.UInt32:
                        item.Length = 4;
                        break;
                    case DataTypeEnum.Int64:
                        item.Length = 8;
                        break;
                    case DataTypeEnum.UInt64:
                        item.Length = 8;
                        break;
                    case DataTypeEnum.Float:
                        item.Length = 4;
                        break;
                    case DataTypeEnum.Double:
                        item.Length = 8;
                        break;
                    default:
                        throw new Exception($"未定义数据类型：{t.Value}");
                }
                return item;
            }).ToArray();
        }

        /// <summary>
        /// 获取需要读取的长度
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        internal static int GetContentLength(byte[] head)
        {
            if (head?.Length >= 4)
                return head[2] * 256 + head[3] - 4;
            else
                throw new ArgumentException("请传入正确的参数");
        }

        /// <summary>
        /// 获取读取PLC地址的开始位置
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        internal static void GetBeingAddress(string address, SiemensAddress addressInfo)
        {
            //去掉V1025 前面的V
            //address = address.Substring(1);
            //I1.3地址的情况
            if (address.IndexOf('.') < 0)
            {
                addressInfo.BeginAddress = int.Parse(address) * 8;
                addressInfo.Address1 = addressInfo.BeginAddress;
                addressInfo.Address2 = 0;
            }
            else
            {
                string[] temp = address.Split('.');
                var a1 = Convert.ToInt32(temp[0]) * 8;
                var a2 = Convert.ToInt32(temp[1]);
                addressInfo.BeginAddress = a1 + a2;
                addressInfo.Address1 = addressInfo.BeginAddress / 8;
                addressInfo.Address2 = addressInfo.BeginAddress % 8;
            }
        }
    }


    /// <summary>
    /// 西门子[写]解析后的地址信息
    /// </summary>
    public class SiemensWriteAddress : SiemensAddress
    {
        public SiemensWriteAddress(SiemensAddress data)
        {
            Address = data.Address;
            DataType = data.DataType;
            TypeCode = data.TypeCode;
            DbBlock = data.DbBlock;
            BeginAddress = data.BeginAddress;
            Length = data.Length;
            IsBit = data.IsBit;
        }

        /// <summary>
        /// 要写入的数据
        /// </summary>
        public byte[] WriteData { get; set; }

        public static SiemensWriteAddress[] ConvertWriteArg(Dictionary<string, KeyValuePair<byte[], bool>> addresses)
        {
            return addresses.Select(t =>
            {
                var item = new SiemensWriteAddress(ConvertArg(t.Key));
                item.WriteData = t.Value.Key;
                item.IsBit = t.Value.Value;
                return item;
            }).ToArray();
        }
    }

}
