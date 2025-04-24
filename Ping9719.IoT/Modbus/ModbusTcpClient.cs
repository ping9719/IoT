using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Ping9719.IoT.Communication;
using Ping9719.IoT;

namespace Ping9719.IoT.Modbus
{
    /// <summary>
    /// ModbusTcp协议客户端
    /// </summary>
    public class ModbusTcpClient : IIoT
    {
        internal EndianFormat format;
        internal bool plcAddresses;
        internal byte stationNumber = 1;

        private UInt16 transactionId_ = 0;
        private UInt16 TransactionId
        {
            get
            {
                transactionId_++;
                if (transactionId_ <= 0)
                    transactionId_++;
                return transactionId_;
            }
        }


        ///// <summary>
        ///// 字符串编码格式。默认ASCII
        ///// </summary>
        //public Encoding Encoding { get; set; } = Encoding.ASCII;

        ///// <summary>
        ///// 是否是连接的
        ///// </summary>
        //public override bool IsConnected => socket?.Connected ?? false;

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="ipAndPoint"></param>
        ///// <param name="timeout">超时时间（毫秒）</param>
        ///// <param name="format">大小端设置</param>
        ///// <param name="plcAddresses">PLC地址</param>
        ///// <param name="plcAddresses">PLC地址</param>
        //public ModbusTcpClient(IPEndPoint ipAndPoint, int timeout = 1500, EndianFormat format = EndianFormat.ABCD, byte stationNumber = 1, bool plcAddresses = false)
        //{
        //    this.timeout = timeout;
        //    ipEndPoint = ipAndPoint;
        //    this.format = format;
        //    this.plcAddresses = plcAddresses;
        //    this.stationNumber = stationNumber;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="ip"></param>
        ///// <param name="port"></param>
        ///// <param name="timeout">超时时间（毫秒）</param>
        ///// <param name="format">大小端设置</param>
        ///// <param name="plcAddresses">PLC地址</param>
        //public ModbusTcpClient(string ip, int port, int timeout = 1500, EndianFormat format = EndianFormat.ABCD, byte stationNumber = 1, bool plcAddresses = false)
        //{
        //    this.timeout = timeout;
        //    SetIpEndPoint(ip, port);
        //    this.format = format;
        //    this.plcAddresses = plcAddresses;
        //    this.stationNumber = stationNumber;
        //}

        public ClientBase Client { get; private set; }//通讯管道

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <param name="format">大小端设置</param>
        /// <param name="plcAddresses">PLC地址</param>
        public ModbusTcpClient(ClientBase client, int timeout = 1500, EndianFormat format = EndianFormat.ABCD, byte stationNumber = 1, bool plcAddresses = false)
        {
            Client = client;
            Client.TimeOut = timeout;
            Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            Client.ConnectionMode = ConnectionMode.AutoOpen;

            this.format = format;
            this.plcAddresses = plcAddresses;
            this.stationNumber = stationNumber;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <param name="format">大小端设置</param>
        /// <param name="plcAddresses">PLC地址</param>
        public ModbusTcpClient(string ip, int port = 1500, int timeout = 1500, EndianFormat format = EndianFormat.ABCD, byte stationNumber = 1, bool plcAddresses = false) : this(new TcpClient(ip, port), timeout, format, stationNumber, plcAddresses) { }//默认使用TcpClient

        #region 获取命令

        /// <summary>
        /// 获取随机校验头
        /// </summary>
        /// <returns></returns>
        private byte[] GetCheckHead(int seed)
        {
            var random = new Random(DateTime.Now.Millisecond + seed);
            return new byte[] { (byte)random.Next(255), (byte)random.Next(255) };
        }

        #endregion

        #region IIoTBase
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

        public virtual IoTResult<string> ReadString(string address, int length, Encoding encoding)
        {
            var result = ModbusInfo.AddressAnalysis(address, stationNumber);
            if (!result.IsSucceed)
                return result.ToVal<string>();

            try
            {
                var comm = result.Value.GetModbusTcpCommand<string>(Convert.ToUInt16(length), null, TransactionId, Client.Encoding, format);
                if (!comm.IsSucceed)
                    return comm.ToVal<string>();

                //获取响应报文
                var sendResult = Client.SendReceive(comm.Value);
                if (!sendResult.IsSucceed)
                    return result.AddError($"读取 地址:{result.Value.Address} 站号:{result.Value.StationNumber} 功能码:{result.Value.FunctionCode} 失败。").ToVal<string>();

                //验证
                if (comm.Value[0] != sendResult.Value[0] || comm.Value[1] != sendResult.Value[1] || comm.Value[7] != sendResult.Value[7])
                    return sendResult.AddError($"读取 地址:{result.Value.Address} 站号:{result.Value.StationNumber} 功能码:{result.Value.FunctionCode} 失败。响应结果校验失败").ToVal<string>();
                if (ModbusHelper.VerifyFunctionCode(comm.Value[7], sendResult.Value[7]))
                    return sendResult.AddError(ModbusHelper.ErrMsg(sendResult.Value[7])).ToVal<string>();

                //数据
                var data = sendResult.Value.Skip(9).Take(sendResult.Value[8]).ToArray();
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
                var comm = result.Value.GetModbusTcpCommand<T>(Convert.ToUInt16(number), null, TransactionId, Client.Encoding, format);
                if (!comm.IsSucceed)
                    return comm.ToVal<IEnumerable<T>>();

                //获取响应报文
                var sendResult = Client.SendReceive(comm.Value);
                if (!sendResult.IsSucceed)
                    return result.AddError($"读取 地址:{result.Value.Address} 站号:{result.Value.StationNumber} 功能码:{result.Value.FunctionCode} 失败。").ToVal<IEnumerable<T>>();

                //验证
                if (comm.Value[0] != sendResult.Value[0] || comm.Value[1] != sendResult.Value[1] || comm.Value[7] != sendResult.Value[7])
                    return sendResult.AddError($"读取 地址:{result.Value.Address} 站号:{result.Value.StationNumber} 功能码:{result.Value.FunctionCode} 失败。响应结果校验失败").ToVal<IEnumerable<T>>();
                if (ModbusHelper.VerifyFunctionCode(comm.Value[7], sendResult.Value[7]))
                    return sendResult.AddError(ModbusHelper.ErrMsg(sendResult.Value[7])).ToVal<IEnumerable<T>>();

                //数据
                var data = sendResult.Value.Skip(9).Take(sendResult.Value[8]).ToArray();
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

        public virtual IoTResult WriteString(string address, string value, int length, Encoding encoding)
        {
            try
            {
                var val2 = new byte[] { };
                if (encoding == null)
                    val2 = value.StringToByteArray();
                else
                    val2 = encoding.GetBytes(value);

                if (length > 0 && val2.Length  < length * 2)
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
                var comm = result.Value.GetModbusTcpCommand<T>(0, value, TransactionId, Client.Encoding, format);
                if (!comm.IsSucceed)
                    return comm;

                //获取响应报文
                var sendResult = Client.SendReceive(comm.Value);
                if (!sendResult.IsSucceed)
                    return result.AddError($"读取 地址:{result.Value.Address} 站号:{result.Value.StationNumber} 功能码:{result.Value.FunctionCode} 失败。");

                //验证
                if (comm.Value[0] != sendResult.Value[0] || comm.Value[1] != sendResult.Value[1] || comm.Value[7] != sendResult.Value[7])
                    return sendResult.AddError($"读取 地址:{result.Value.Address} 站号:{result.Value.StationNumber} 功能码:{result.Value.FunctionCode} 失败。响应结果校验失败");
                if (ModbusHelper.VerifyFunctionCode(comm.Value[7], sendResult.Value[7]))
                    return sendResult.AddError(ModbusHelper.ErrMsg(sendResult.Value[7]));

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
