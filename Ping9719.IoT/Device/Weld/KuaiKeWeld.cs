using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Ping9719.IoT.Algorithm;
using Ping9719.IoT.Enums;
using Ping9719.IoT;
using Ping9719.IoT.Modbus;

namespace Ping9719.IoT.Device.Weld
{
    /// <summary>
    /// 快克焊接机
    /// PLC与主板通信持续整理.doc
    /// </summary>
    public class KuaiKeWeld : ModbusRtuClient
    {
        public KuaiKeWeld(string portName, int baudRate = 115200, int dataBits = 8, StopBits stopBits = StopBits.One, Parity parity = Parity.None, int timeout = 1500, EndianFormat format = EndianFormat.CDAB, byte stationNumber = 1, bool plcAddresses = false)
            : base(portName, baudRate, dataBits, stopBits, parity, timeout, format, stationNumber, plcAddresses)
        {

        }

        /// <summary>
        /// 读取状态信息
        /// </summary>
        /// <returns></returns>
        public IoTResult<KeyValuePair<int, string>> ReadStateInfo()
        {
            var result = new IoTResult<KeyValuePair<int, string>>();
            try
            {
                var data1 = Read<byte>("32;x=1");
                if (!data1.IsSucceed)
                    return new IoTResult<KeyValuePair<int, string>>(data1).ToEnd();

                switch (data1.Value)
                {
                    case 0x00:
                        result.Value = new KeyValuePair<int, string>(data1.Value, "停止状态");
                        break;
                    case 0x01:
                        result.Value = new KeyValuePair<int, string>(data1.Value, "加工状态");
                        break;
                    case 0x08:
                        result.Value = new KeyValuePair<int, string>(data1.Value, "停止状态（按下暂停后）");
                        break;
                    case 0x09:
                        result.Value = new KeyValuePair<int, string>(data1.Value, "加工状态（按下暂停后）");
                        break;
                    case 0x10:
                        result.Value = new KeyValuePair<int, string>(data1.Value, "加工过程中复位后的状态");
                        break;
                    case 0x11:
                        result.Value = new KeyValuePair<int, string>(data1.Value, "暂停状态（执行过程中暂停）");
                        break;
                    case 0x19:
                        result.Value = new KeyValuePair<int, string>(data1.Value, "暂停状态（手动按下暂停）");
                        break;
                    default:
                        result.Value = new KeyValuePair<int, string>(-1, "未知状态");
                        break;
                }

                return result.ToEnd();
            }
            catch (Exception ex)
            {
                result.AddError(ex);
                return result.ToEnd();
            }

        }

        /// <summary>
        /// 查询当前运行程序的文件名
        /// </summary>
        /// <returns></returns>
        public IoTResult<string> ReadRunSoftInfo()
        {
            var aaaa1 = Read<long>("16;x=3");
            if (!aaaa1.IsSucceed)
                return new IoTResult<string>(aaaa1);

            IoTResult<string> result = new IoTResult<string>(aaaa1);
            try
            {
                var aaa2 = BitConverter.GetBytes(aaaa1.Value);
                result.Value = Encoding.ASCII.GetString(aaa2).Trim();
            }
            catch (Exception ex)
            {
                
                result.AddError( ex);
            }
            return result;
        }

        /// <summary>
        /// 修改当前加工文件
        /// </summary>
        /// <param name="name">8位数，数字或字母</param>
        /// <returns></returns>
        public IoTResult SetRunSoft(string name)
        {
            var aaa = name.Substring(0, name.Length > 8 ? 8 : name.Length).PadRight(8);
            var values = Encoding.ASCII.GetBytes(aaa);
            values = values.ByteFormatting(EndianFormat.BADC, true);
            List<byte> bytes = new List<byte>() { 0x01, 0x03, 0x00, 0x80 };
            bytes.AddRange(values);
            var commandCRC16 = CRC.Crc16(bytes.ToArray());
            var sendResult = SendPackageReliable(commandCRC16);
            if (!sendResult.IsSucceed || sendResult.Value == null || sendResult.Value.Length != 7)
                return sendResult;

            sendResult.IsSucceed = sendResult.Value[4] == 1;
            if (!sendResult.IsSucceed)
            {
                sendResult.AddError("未找到文件或正在加工中");
            }
            return sendResult.ToEnd();
        }

        /// <summary>
        /// 修改当前加工文件并开始加工
        /// </summary>
        /// <param name="name">8位数，数字或字母</param>
        /// <returns></returns>
        public IoTResult SetRunSoftIn(string name)
        {
            var aaa = name.Substring(0, name.Length > 8 ? 8 : name.Length).PadRight(8);
            var values = Encoding.ASCII.GetBytes(aaa);
            values = values.ByteFormatting(EndianFormat.BADC, true);
            List<byte> bytes = new List<byte>() { 0x01, 0x03, 0x00, 0x84 };
            bytes.AddRange(values);
            var commandCRC16 = CRC.Crc16(bytes.ToArray());
            var sendResult = SendPackageReliable(commandCRC16);
            if (!sendResult.IsSucceed || sendResult.Value == null || sendResult.Value.Length != 7)
                return sendResult;

            sendResult.IsSucceed = sendResult.Value[4] == 1;
            if (!sendResult.IsSucceed)
            {
                sendResult.AddError("未找到文件或正在加工中");
            }
            return sendResult.ToEnd();
        }

        /// <summary>
        /// 查询当前烙铁头使用次数
        /// </summary>
        /// <returns></returns>
        public IoTResult<int> ReadScrewInfo()
        {
            return Read<int>("4111;x=3");
        }

        /// <summary>
        /// 查询当前烙铁头设定的使用次数 
        /// </summary>
        /// <returns></returns>
        public IoTResult<int> ReadScrewSetInfo()
        {
            return Read<int>("4113;x=3");
        }

        /// <summary>
        /// 读焊台设定温度  
        /// </summary>
        /// <returns></returns>
        public IoTResult<int> ReadSetTemperatureInfo()
        {
            return Read<int>("665;x=3");
        }

        /// <summary>
        /// 设定焊台温度
        /// </summary>
        /// <returns></returns>
        public IoTResult SetTemperatureInfo(int temperature)
        {
            var values = BitConverter.GetBytes(Convert.ToInt16(temperature));
            values = values.ByteFormatting(EndianFormat.BADC, true);
            List<byte> bytes = new List<byte>() { 0x01, 0x06, 0x01, 0x99 };
            bytes.AddRange(values);
            var commandCRC16 = CRC.Crc16(bytes.ToArray());
            var sendResult = SendPackageReliable(commandCRC16);
            return sendResult;
        }

        /// <summary>
        /// 读焊台实时温度   
        /// </summary>
        /// <returns></returns>
        public IoTResult<int> ReadTemperatureInfo()
        {
            return Read<int>("667;x=3");
        }

        /// <summary>
        /// 查询 AOI 检测产品 NG、OK 状态，加工结束状态。此方法可能有问题
        /// </summary>
        /// <returns></returns>
        public IoTResult<KeyValuePair<int, string>> ReadAoiInfo()
        {
            var result = new IoTResult<KeyValuePair<int, string>>();
            var data1 = Read<int>("685;x=3");
            if (!data1.IsSucceed)
                return new IoTResult<KeyValuePair<int, string>>(data1).ToEnd();

            switch (data1.Value)
            {
                case 0x01:
                    result.Value = new KeyValuePair<int, string>(data1.Value, "加工完成；合格");
                    break;
                case 0x11:
                    result.Value = new KeyValuePair<int, string>(data1.Value, "加工完成；不合格");
                    break;
                default:
                    result.Value = new KeyValuePair<int, string>(0, "停止或加工中");
                    break;
            }

            return result.ToEnd();
        }

        /// <summary>
        /// 复位
        /// </summary>
        /// <param name="isFast">是否快速复位</param>
        /// <returns></returns>
        public IoTResult Reset(bool isFast = false)
        {
            if (!isFast)
                return Write("10;x=5", true);
            else
                return Write("11;x=5", true);
        }

        /// <summary>
        /// 暂停
        /// </summary>
        /// <returns></returns>
        public IoTResult Suspend()
        {
            return Write("1;x=5", true);
        }

        /// <summary>
        /// 加工重新开始
        /// </summary>
        /// <returns></returns>
        public IoTResult Continue()
        {
            return Write("2;x=5", true);
        }

        /// <summary>
        /// 加工停止
        /// </summary>
        /// <returns></returns>
        public IoTResult Stop()
        {
            return Write("3;x=5", true);
        }
    }
}
