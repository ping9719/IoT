using Ping9719.IoT;
using Ping9719.IoT.Algorithm;
using Ping9719.IoT.Common;
using Ping9719.IoT.Communication;
using Ping9719.IoT.Modbus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Rfid
{
    /// <summary>
    /// 万全Rfid
    /// </summary>
    public class WanQuanRfid : ModbusTcpClient, IIoT
    {
        WanQuanRfidVer ver;
        public ClientBase Client { get; private set; }
        public WanQuanRfid(WanQuanRfidVer ver, ClientBase client, byte stationNumber = 0x01, int timeout = 1500) : base(client, stationNumber: stationNumber)
        {
            Client = client;
            Client.TimeOut = timeout;
            Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            Client.TimeOut = timeout;
            Client.ConnectionMode = ConnectionMode.AutoOpen;

            this.ver = ver;
        }
        public WanQuanRfid(WanQuanRfidVer ver, string ip, int port = 502) : this(ver, new TcpClient(ip, port)) { }
        public WanQuanRfid(WanQuanRfidVer ver, string portName, int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One) : this(ver, new SerialPortClient(portName, baudRate, parity, dataBits, stopBits)) { }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="address">地址。比如“EPC”，“EPC;a=1;p=00 00 00 00”</param>
        /// <param name="length">读取长度</param>
        /// <param name="encoding">编码，null为16机制表示的字符串</param>
        /// <returns></returns>
        public IoTResult<string> ReadString(string address, int length = -1, Encoding encoding = null)
        {
            try
            {
                var add = RfidAddress.GetRfidAddress(address);

                if (ver == WanQuanRfidVer.unknown1)
                {
                    if (add.Area == RfidArea.EPC)
                    {
                        var startAddress = 5000 + 100 * add.AntennaNum;

                        byte[] bytes = new byte[6];
                        bytes[0] = stationNumber;//设备地址
                        bytes[1] = 0x03;//功能码
                        bytes[2] = BitConverter.GetBytes(startAddress)[1];
                        bytes[3] = BitConverter.GetBytes(startAddress)[0];//寄存器地址
                        bytes[4] = BitConverter.GetBytes(34)[1];
                        bytes[5] = BitConverter.GetBytes(34)[0];//表示request 寄存器的长度(寄存器个数)
                        var commandCRC16 = CRC.Crc16(bytes.ToArray());
                        var sendResult = Client.SendReceive(commandCRC16);
                        if (!sendResult.IsSucceed)
                            return sendResult.ToVal<string>();

                        //返回数据设备地址（1 byte）  功能码（1 byte） 字节数（2 byte） 寄存器值（64 bytes）   CRC（2 bytes）
                        //&& sendResult.Value[2] != number * 2
                        if (sendResult.Value.Length != 5 + 34 * 2)
                        {
                            return sendResult.ToVal<string>().AddError("读取失败,设备返回数据长度验证不合格");
                        }
                        if (!CRC.CheckCrc16(sendResult.Value))
                        {
                            return sendResult.ToVal<string>().AddError("读取失败,设备返回数据CRC验证不合格");
                        }
                        sendResult.Value = sendResult.Value.Skip(4).Take(64).ToArray();
                        var data = sendResult.Value;
                        if (data[0] == 0x00)
                        {
                            return sendResult.ToVal<string>().AddError("读取失败,设备未读取到有效标签");
                        }
                        var epcLen = (int)data[2];/*BitConverter.ToInt16(new byte[] { data[2], data[3] }, 0);*/
                        var epcData = data.Skip(3).Take(epcLen).ToArray();
                        var epcStr = epcData.ByteArrayToString();
                        return new IoTResult<string>(epcStr);
                    }
                    else
                    {
                        return new IoTResult<string>().AddError("不支持的区域" + add.Area.ToString());
                    }
                }
                else if (ver == WanQuanRfidVer.unknown2)
                {
                    var startAddress = 5000 + 100 * add.AntennaNum;
                    string retStr = "";
                    if (5100 == startAddress)
                    {
                        var ret = base.Read<Int16>(startAddress.ToString(), 10);
                        if (!ret.IsSucceed)
                            return ret.ToVal<string>();

                        var tmp1 = ret.Value.ToArray()[3];
                        var tmp2 = ret.Value.ToArray()[4];
                        var tmp3 = ret.Value.ToArray()[5];
                        var tmp4 = ret.Value.ToArray()[6];
                        byte[] nums = new byte[8];
                        nums[0] = (byte)(tmp1 >> 8);
                        nums[1] = (byte)(tmp1 & 0xFF);
                        nums[2] = (byte)(tmp2 >> 8);
                        nums[3] = (byte)(tmp2 & 0xFF);
                        nums[4] = (byte)(tmp3 >> 8);
                        nums[5] = (byte)(tmp3 & 0xFF);
                        nums[6] = (byte)(tmp4 >> 8);
                        nums[7] = (byte)(tmp4 & 0xFF);
                        retStr = DataConvert.ByteArrayToString(nums);
                    }
                    else if (5200 == startAddress)
                    {
                        var ret = base.Read<Int16>(startAddress.ToString(), 2);
                        if (!ret.IsSucceed)
                            return ret.ToVal<string>();

                        var tmp1 = ret.Value.ToArray()[0];
                        var tmp2 = ret.Value.ToArray()[1];
                        byte[] nums = new byte[4];
                        nums[0] = (byte)(tmp1 >> 8);
                        nums[1] = (byte)(tmp1 & 0xFF);
                        nums[2] = (byte)(tmp2 >> 8);
                        nums[3] = (byte)(tmp2 & 0xFF);
                        retStr = Encoding.ASCII.GetString(nums);
                    }
                    else
                    {
                        return new IoTResult<string>().AddError($"不支持的天线号：{startAddress}");
                    }

                    return new IoTResult<string>(retStr);
                }
                else if (ver == WanQuanRfidVer.IR610P_HF)
                {
                    var aa = base.Read<Int16>("5200", length);
                    if (!aa.IsSucceed)
                        return aa.ToVal<string>();

                    if (encoding == null)
                    {
                        var val = string.Join("", aa.Value.Select(t => t.ToString("X2").PadLeft(4, '0')));
                        return aa.ToVal<string>(val);
                    }
                    else
                    {
                        var val = aa.Value.SelectMany(t => BitConverter.GetBytes(t).Reverse()).ToArray();
                        var val2 = encoding.GetString(val);
                        return aa.ToVal<string>(val2);
                    }
                }
                else
                {
                    return new IoTResult<string>().AddError("不支持的版本" + ver.ToString());
                }
            }
            catch (Exception ex)
            {
                return new IoTResult<string>().AddError(ex);
            }
        }

        public IoTResult WriteString(string address, string value, int length = -1, Encoding encoding = null)
        {
            try
            {
                var add = RfidAddress.GetRfidAddress(address);

                if (ver == WanQuanRfidVer.unknown1)
                {
                    if (add.Area == RfidArea.EPC)
                    {
                        var values = value;
                        var antNumber = add.AntennaNum;

                        //values值校验 长度应为4的倍数；每个字符都是16进制的数字或字母
                        if (string.IsNullOrEmpty(values) || values.Length % 4 != 0)
                        {
                            var result = new IoTResult<string>();
                            result.AddError("输入长度错误，应为4的倍数");
                            return result;
                        }

                        string str = "[0-9a-fA-F]$";
                        if (!Regex.IsMatch(values, str))
                        {
                            var result = new IoTResult<string>();
                            result.AddError("输入字符错误,应为0~9或A~F");
                            return result;
                        }
                        if (length == -1)
                        {
                            length = values.Length / 2;
                        }

                        byte[] value11 = values.StringToByteArray();
                        byte[] bytes = new byte[19 + value11.Length];
                        bytes[0] = 0x01;//设备地址
                        bytes[1] = 0x10;//功能码
                        bytes[2] = 0x10;//寄存器地址4100
                        bytes[3] = 0x04;//寄存器地址4100
                        int writeLen = value11.Length / 2 + 6;
                        bytes[4] = BitConverter.GetBytes(writeLen)[1];
                        bytes[5] = BitConverter.GetBytes(writeLen)[0];//写入长度
                        bytes[6] = Convert.ToByte(writeLen * 2);//数据长度
                        bytes[7] = BitConverter.GetBytes(antNumber)[1]; //写入天线
                        bytes[8] = BitConverter.GetBytes(antNumber)[0]; //写入天线

                        bytes[9] = add.Pass[0]; //标签访问密码
                        bytes[10] = add.Pass[1]; //标签访问密码
                        bytes[11] = add.Pass[2]; //标签访问密码
                        bytes[12] = add.Pass[3]; //标签访问密码
                        bytes[13] = 0x00;
                        bytes[14] = 0x01;//写入EPC
                        bytes[15] = BitConverter.GetBytes(4)[1];//写入起始地址
                        bytes[16] = BitConverter.GetBytes(4)[0];//写入起始地址
                        bytes[17] = BitConverter.GetBytes(length)[1];//写入长度
                        bytes[18] = BitConverter.GetBytes(length)[0];//写入长度
                        for (int i = 0; i < value11.Length; i++)
                        {
                            bytes[19 + i] = value11[i];
                        }

                        var commandCRC16 = CRC.Crc16(bytes.ToArray());
                        var sendResult = Client.SendReceive(commandCRC16);
                        if (!sendResult.IsSucceed)
                        {
                            sendResult.Value = new byte[] { };
                            return sendResult;
                        }
                        if (sendResult.Value.Length != 8)
                        {
                            return sendResult.AddError("数据长度验证不合格");
                        }
                        if (!CRC.CheckCrc16(sendResult.Value))
                        {
                            return sendResult.AddError("数据CRC16验证不合格");
                        }
                        //前6字节与发送一致
                        for (int i = 0; i < 6; i++)
                        {
                            if (sendResult.Value[i] != commandCRC16[i])
                            {
                                sendResult.Value = new byte[] { };
                                return sendResult.AddError("写入失败");
                            }
                        }
                        //写入成功
                        return sendResult;
                    }
                    else
                    {
                        return new IoTResult<string>().AddError("不支持的区域" + add.Area.ToString());
                    }
                }
                else if (ver == WanQuanRfidVer.unknown2)
                {
                    return new IoTResult<string>().AddError("未实现");
                }
                else if (ver == WanQuanRfidVer.IR610P_HF)
                {
                    if (add.Area == RfidArea.ISO15693)
                    {
                        byte[] val = new byte[0];
                        if (encoding == null)
                        {
                            val = DataConvert.StringToByteArray(value);
                        }
                        else
                        {
                            val = encoding.GetBytes(value);
                        }

                        var aa = base.Write<byte>("5200", val);
                        if (!aa.IsSucceed)
                            return aa.ToVal<string>();

                        return new IoTResult<string>().AddError("不支持的区域" + add.Area.ToString());
                    }
                    else
                    {
                        return new IoTResult<string>().AddError("不支持的区域" + add.Area.ToString());
                    }
                }
                else
                {
                    return new IoTResult<string>().AddError("不支持的版本" + ver.ToString());
                }
            }
            catch (Exception ex)
            {
                return new IoTResult<string>().AddError(ex);
            }
        }
    }

    public enum WanQuanRfidVer
    {
        IR610P_HF,
        unknown1,//何海兵
        unknown2,//刘林
    }
}
