using Ping9719.IoT.Common;
using System;
using System.IO.Ports;
using System.Linq;
using Ping9719.IoT.Algorithm;
using Ping9719.IoT;
using Ping9719.IoT.Communication;
using System.Collections.Generic;
using System.Text;

namespace Ping9719.IoT.Modbus
{
    /// <summary>
    /// ModbusRtu协议客户端
    /// </summary>
    public class ModbusRtuClient : IIoT
    {
        internal EndianFormat format;
        internal byte stationNumber = 1;

        public ClientBase Client { get; private set; }//通讯管道

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="format">数据格式</param>
        /// <param name="stationNumber">站号</param>
        public ModbusRtuClient(ClientBase client, EndianFormat format = EndianFormat.ABCD, byte stationNumber = 1)
        {
            Client = client;
            //Client.TimeOut = 1500;
            //Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            Client.ConnectionMode = ConnectionMode.AutoOpen;

            this.format = format;
            this.stationNumber = stationNumber;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudRate"></param>
        /// <param name="parity"></param>
        /// <param name="dataBits"></param>
        /// <param name="stopBits"></param>
        /// <param name="format">数据格式</param>
        /// <param name="stationNumber">站号</param>
        public ModbusRtuClient(string portName, int baudRate, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, EndianFormat format = EndianFormat.ABCD, byte stationNumber = 1)
            : this(new SerialPortClient(portName, baudRate, parity, dataBits, stopBits), format, stationNumber) { }
        
        #region IIoT
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="address">全写法"s=2;x=3;100"，对应站号，功能码，地址</param>
        public virtual IoTResult<T> Read<T>(string address)
        {
            try
            {
                var result = ModbusInfo.AddressAnalysis(address, stationNumber);
                if (!result.IsSucceed)
                    return result.ToVal<T>();

                var val = Read<T>(address, 1);
                if (!val.IsSucceed)
                    return val.ToVal<T>();

                if (!result.Value.Bit.HasValue)
                {
                    return val.ToVal<T>(val.Value.FirstOrDefault());
                }
                //取位
                else
                {
                    var val2 = Convert.ToInt64(val.Value.FirstOrDefault()?.ToString() ?? "0");
                    var vql3 = Convert.ToString(val2, 2).PadLeft(64, '0').Reverse().ElementAtOrDefault(result.Value.Bit.Value).ToString();
                    return val.ToVal<T>((T)Convert.ChangeType(vql3, typeof(T)));
                }
            }
            catch (Exception ex)
            {
                return new IoTResult<T>().AddError(ex);
            }
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="length">长度</param>
        /// <param name="encoding">编码。一般情况下，如果为null为16进制的字符串</param>
        /// <returns></returns>
        public virtual IoTResult<string> ReadString(string address, int length, Encoding encoding)
        {
            var result = ModbusInfo.AddressAnalysis(address, stationNumber);
            if (!result.IsSucceed)
                return result.ToVal<string>();

            try
            {
                var comm = result.Value.GetModbusRtuCommand<string>(Convert.ToUInt16(length), null, Client.Encoding, format);
                if (!comm.IsSucceed)
                    return comm.ToVal<string>();

                var sVal = CRC.Crc16Modbus(comm.Value);

                //获取响应报文
                var sendResult = Client.SendReceive(sVal);
                if (!sendResult.IsSucceed)
                    return sendResult.AddError($"读取 地址:{result.Value.Address} 站号:{result.Value.StationNumber} 功能码:{result.Value.FunctionCode} 失败。").ToVal<string>();

                //验证
                if (!CRC.CheckCrc16Modbus(sendResult.Value))
                    return sendResult.AddError($"读取 地址:{result.Value.Address} 站号:{result.Value.StationNumber} 功能码:{result.Value.FunctionCode} 失败。响应结果校验失败").ToVal<string>();
                if (ModbusErr.VerifyFunctionCode(comm.Value[1], sendResult.Value[1]))
                    return sendResult.AddError(ModbusErr.ErrMsg(sendResult.Value[2])).ToVal<string>();

                //数据
                var data = sendResult.Value.Skip(3).Take(sendResult.Value[2]).ToArray();
                string val2 = string.Empty;
                if (encoding == null)
                    val2 = data.ByteArrayToString("");
                else
                    val2 = encoding.GetString(data);

                return sendResult.ToVal<string>(val2);
            }
            catch (Exception ex)
            {
                return new IoTResult<string>().AddError(ex);
            }
        }

        /// <summary>
        /// 读取多个
        /// </summary>
        /// <param name="address">全写法"s=2;x=3;100"，对应站号，功能码，地址</param>
        /// <param name="number">读取数量</param>
        public virtual IoTResult<IEnumerable<T>> Read<T>(string address, int number)
        {
            var result = ModbusInfo.AddressAnalysis(address, stationNumber);
            if (!result.IsSucceed)
                return result.ToVal<IEnumerable<T>>();

            try
            {
                var comm = result.Value.GetModbusRtuCommand<T>(Convert.ToUInt16(number), null, Client.Encoding, format);
                if (!comm.IsSucceed)
                    return comm.ToVal<IEnumerable<T>>();

                var sVal = CRC.Crc16Modbus(comm.Value);

                //获取响应报文
                var sendResult = Client.SendReceive(sVal);
                if (!sendResult.IsSucceed)
                    return sendResult.AddError($"读取 地址:{result.Value.Address} 站号:{result.Value.StationNumber} 功能码:{result.Value.FunctionCode} 失败。").ToVal<IEnumerable<T>>();

                //验证
                if (!CRC.CheckCrc16Modbus(sendResult.Value))
                    return sendResult.AddError($"读取 地址:{result.Value.Address} 站号:{result.Value.StationNumber} 功能码:{result.Value.FunctionCode} 失败。响应结果校验失败").ToVal<IEnumerable<T>>();
                if (ModbusErr.VerifyFunctionCode(comm.Value[1], sendResult.Value[1]))
                    return sendResult.AddError(ModbusErr.ErrMsg(sendResult.Value[2])).ToVal<IEnumerable<T>>();

                //数据
                var data = sendResult.Value.Skip(3).Take(sendResult.Value[2]).ToArray();
                var tType = typeof(T);
                IEnumerable<T> val2 = null;
                if (tType == typeof(bool))
                {
                    val2 = data.SelectMany(o => Convert.ToString(o, 2).PadLeft(8, '0').Reverse()).Select(o => (T)(object)(o == '1')).Take(number);
                }
                else if (tType == typeof(byte))
                {
                    val2 = data.Select(o => (T)(object)(o)).Take(number);
                }
                else if (tType == typeof(short))
                {
                    val2 = data.SplitBlock(2, true).Select(o => (T)(object)BitConverter.ToInt16(o.EndianIotToNet(format), 0)).Take(number);
                }
                else if (tType == typeof(ushort))
                {
                    val2 = data.SplitBlock(2, true).Select(o => (T)(object)BitConverter.ToUInt16(o.EndianIotToNet(format), 0)).Take(number);
                }
                else if (tType == typeof(int))
                {
                    val2 = data.SplitBlock(4, true).Select(o => (T)(object)BitConverter.ToInt32(o.EndianIotToNet(format), 0)).Take(number);
                }
                else if (tType == typeof(uint))
                {
                    val2 = data.SplitBlock(4, true).Select(o => (T)(object)BitConverter.ToUInt32(o.EndianIotToNet(format), 0)).Take(number);
                }
                else if (tType == typeof(long))
                {
                    val2 = data.SplitBlock(8, true).Select(o => (T)(object)BitConverter.ToInt64(o.EndianIotToNet(format), 0)).Take(number);
                }
                else if (tType == typeof(ulong))
                {
                    val2 = data.SplitBlock(8, true).Select(o => (T)(object)BitConverter.ToUInt64(o.EndianIotToNet(format), 0)).Take(number);
                }
                else if (tType == typeof(float))
                {
                    val2 = data.SplitBlock(4, true).Select(o => (T)(object)BitConverter.ToSingle(o.EndianIotToNet(format), 0)).Take(number);
                }
                else if (tType == typeof(double))
                {
                    val2 = data.SplitBlock(8, true).Select(o => (T)(object)BitConverter.ToDouble(o.EndianIotToNet(format), 0)).Take(number);
                }
                //else if (tType == typeof(string))
                //{
                //}
                else
                    return sendResult.AddError($"不支持类型{tType.Name}").ToVal<IEnumerable<T>>(val2);

                return sendResult.ToVal<IEnumerable<T>>(val2);
            }
            catch (Exception ex)
            {
                return new IoTResult<IEnumerable<T>>().AddError(ex);
            }
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">全写法"s=2;x=3;100"，对应站号，功能码，地址</param>
        public virtual IoTResult Write<T>(string address, T value)
        {
            return Write<T>(address, new[] { value });
        }

        /// <summary>
        /// 写入字符串
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <param name="length">长度。一般用于补充的长度</param>
        /// <param name="encoding">编码。一般情况下，如果为null为16进制的字符串</param>
        /// <returns></returns>
        public virtual IoTResult WriteString(string address, string value, int length, Encoding encoding)
        {
            try
            {
                var val2 = new byte[] { };
                if (encoding == null)
                    val2 = value.StringToByteArray();
                else
                    val2 = encoding.GetBytes(value);

                if (length > 0 && val2.Length < length * 2)
                    val2 = val2.Concat(Enumerable.Repeat<byte>(0, length * 2 - val2.Length)).ToArray();
                if (val2.Length % 2 != 0)
                    val2 = val2.Concat(new byte[] { 0 }).ToArray();

                return Write(address, val2);
            }
            catch (Exception ex)
            {
                return new IoTResult().AddError(ex);
            }
        }

        /// <summary>
        /// 写入多个
        /// </summary>
        /// <param name="address">全写法"s=2;x=3;100"，对应站号，功能码，地址</param>
        public virtual IoTResult Write<T>(string address, params T[] value)
        {
            var result = ModbusInfo.AddressAnalysis(address, stationNumber);
            if (!result.IsSucceed)
                return result;

            try
            {
                var comm = result.Value.GetModbusRtuCommand<T>(0, value, Client.Encoding, format);
                if (!comm.IsSucceed)
                    return comm;

                var sVal = CRC.Crc16Modbus(comm.Value);

                //获取响应报文
                var sendResult = Client.SendReceive(sVal);
                if (!sendResult.IsSucceed)
                    return sendResult.AddError($"读取 地址:{result.Value.Address} 站号:{result.Value.StationNumber} 功能码:{result.Value.FunctionCode} 失败。");

                //验证
                if (!CRC.CheckCrc16Modbus(sendResult.Value))
                    return sendResult.AddError($"读取 地址:{result.Value.Address} 站号:{result.Value.StationNumber} 功能码:{result.Value.FunctionCode} 失败。响应结果校验失败");
                if (ModbusErr.VerifyFunctionCode(comm.Value[1], sendResult.Value[1]))
                    return sendResult.AddError(ModbusErr.ErrMsg(sendResult.Value[2]));

                return sendResult;
            }
            catch (Exception ex)
            {
                return new IoTResult<IEnumerable<T>>().AddError(ex);
            }
        }
        #endregion 
    }
}
