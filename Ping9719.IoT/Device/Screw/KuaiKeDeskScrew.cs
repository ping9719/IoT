using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Ping9719.IoT.Algorithm;
using Ping9719.IoT;
using Ping9719.IoT.Modbus;

namespace Ping9719.IoT.Device.Screw
{
    /// <summary>
    /// 快克螺丝机（新协议）（桌面式）
    /// 螺丝机运动平台通讯协议(对外)-20200106.pdf
    /// </summary>
    public class KuaiKeDeskScrew : ModbusRtuClient, IIoT
    {
        public KuaiKeDeskScrew(string portName, int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, EndianFormat format = EndianFormat.CDAB, byte stationNumber = 1)
            : base(portName, baudRate, parity, dataBits, stopBits, format, stationNumber)
        {

        }

        /// <summary>
        /// 查询报警
        /// </summary>
        /// <returns></returns>
        public IoTResult<bool> ReadErr()
        {
            var result = new IoTResult<bool>();
            try
            {
                var data1 = Read<int>("637");
                if (!data1.IsSucceed)
                    return new IoTResult<bool>(data1).ToEnd();

                result.Value = data1.Value > 0;
                return result.ToEnd();
            }
            catch (Exception ex)
            {
                
                result.AddError(ex);
                return result.ToEnd();
            }
        }

        /// <summary>
        /// 查询机械状态
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
            var sendResult = Client.SendReceive(commandCRC16);
            if (!sendResult.IsSucceed || sendResult.Value == null || sendResult.Value.Length != 7)
                return sendResult;

            sendResult.IsSucceed = sendResult.Value[4] == 1;
            if (!sendResult.IsSucceed)
            {
                sendResult.AddError ( "未找到文件或正在加工中");
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
            var sendResult = Client.SendReceive(commandCRC16);
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
