using Ping9719.IoT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ping9719.IoT.Communication;

namespace Ping9719.IoT.PLC
{
    /// <summary>
    /// 西门子客户端（S7协议）
    /// http://www.360doc.cn/mip/763580999.html
    /// </summary>
    public class SiemensS7Client : IIoT
    {
        /// <summary>
        /// CPU版本
        /// </summary>
        public SiemensVersion Version { get; private set; }

        /// <summary>
        /// 插槽号 
        /// </summary>
        public byte Slot { get; private set; }

        /// <summary>
        /// 机架号
        /// </summary>
        public byte Rack { get; private set; }

        /// <summary>
        /// 字节格式
        /// </summary>
        public EndianFormat Format { get; set; } = EndianFormat.DCBA;

        /// <summary>
        /// 超长时读写Byte采用循环的最小的步数
        /// </summary>
        public ushort ReadWriteByteNum { get; set; } = 200;

        public ClientBase Client { get; private set; }
        /// <summary>
        /// 西门子客户端
        /// </summary>
        /// <param name="version">版本</param>
        /// <param name="client">客户端</param>
        /// <param name="slot">插槽号</param>
        /// <param name="rack">机架号</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        public SiemensS7Client(SiemensVersion version, ClientBase client, byte slot = 0x00, byte rack = 0x00)
        {
            this.Version = version;
            Client = client;
            Slot = slot;
            Rack = rack;
            //Client.TimeOut = timeout;
            //Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            Client.ConnectionMode = ConnectionMode.AutoReconnection;
            Client.IsAutoDiscard = true;

            Client.Opened = (a) =>
            {
                var Command1 = SiemensConstant.Command1;
                var Command2 = SiemensConstant.Command2;

                switch (version)
                {
                    case SiemensVersion.S7_200:
                        Command1 = SiemensConstant.Command1_200;
                        Command2 = SiemensConstant.Command2_200;
                        break;
                    case SiemensVersion.S7_200Smart:
                        Command1 = SiemensConstant.Command1_200Smart;
                        Command2 = SiemensConstant.Command2_200Smart;
                        break;
                    case SiemensVersion.S7_300:
                        Command1[21] = (byte)(Rack * 0x20 + Slot); //0x02;
                        break;
                    case SiemensVersion.S7_400:
                        Command1[21] = (byte)(Rack * 0x20 + Slot); //0x03;
                        Command1[17] = 0x00;
                        break;
                    case SiemensVersion.S7_1200:
                        Command1[21] = (byte)(Rack * 0x20 + Slot); //0x00;
                        break;
                    case SiemensVersion.S7_1500:
                        Command1[21] = (byte)(Rack * 0x20 + Slot); //0x00;
                        break;
                    default:
                        Command1[18] = 0x00;
                        break;
                }

                var socketReadResul = Client.SendReceive(Command1);
                if (!socketReadResul.IsSucceed || socketReadResul.Value.Length <= SiemensConstant.InitHeadLength)
                    throw new Exception("打开S7第一次握手失败。");

                socketReadResul = Client.SendReceive(Command2);
                if (!socketReadResul.IsSucceed || socketReadResul.Value.Length <= SiemensConstant.InitHeadLength)
                    throw new Exception("打开S7第二次握手失败。");

            };
            Client.Closing = (a) =>
            {
                //try
                //{
                //    byte[] command = new byte[] { 0x66, 0x00, 0x00, 0x00, SessionByte[0], SessionByte[1], SessionByte[2], SessionByte[3], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                //    Client.Send(command);
                //}
                //catch (Exception)
                //{

                //    throw;
                //}
                return true;
            };
        }

        /// <summary>
        /// 西门子客户端,以网络的方式
        /// </summary>
        /// <param name="version">版本</param>
        /// <param name="ip">ip</param>
        /// <param name="port">端口</param>
        /// <param name="slot">插槽号</param>
        /// <param name="rack">机架号</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        public SiemensS7Client(SiemensVersion version, string ip, int port = 102, byte slot = 0x00, byte rack = 0x00) : this(version, new TcpClient(ip, port), slot, rack) { }

        #region Read 
        /// <summary>
        /// 读取字节数组
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="length">读取长度</param>
        /// <param name="isBit">是否Bit类型</param>        
        /// <returns></returns>
        public IoTResult<byte[]> Read(string address, ushort length, bool isBit = false)
        {
            //if (!socket?.Connected ?? true)
            //{
            //    var connectResult = Connect();
            //    if (!connectResult.IsSucceed)
            //    {
            //        return new IoTResult<byte[]>(connectResult).AddError($"读取{address}失败，{connectResult.ErrorText}");
            //    }
            //}
            var result = new IoTResult<byte[]>();
            result.Value = new byte[] { };
            try
            {
                var arg = SiemensAddress.ConvertArg(address);
                var cs = WordHelp.SplitBlock(length, ReadWriteByteNum, arg.BeginAddress, 8);
                foreach (var item in cs)
                {
                    //发送读取信息
                    arg.BeginAddress = item.Key;
                    arg.Length = Convert.ToUInt16(item.Value);
                    arg.IsBit = isBit;

                    byte[] command = GetReadCommand(arg);

                    //发送命令 并获取响应报文
                    var sendResult = Client.SendReceive(command);
                    if (!sendResult.IsSucceed)
                    {
                        return result.AddError(sendResult.Error).ToEnd();
                    }

                    var dataPackage = sendResult.Value;
                    byte[] responseData = new byte[arg.Length];
                    Array.Copy(dataPackage, dataPackage.Length - arg.Length, responseData, 0, arg.Length);

                    result.Requests.Add(sendResult.Requests.FirstOrDefault());
                    result.Responses.Add(sendResult.Responses.FirstOrDefault());
                    result.Value = result.Value.Concat(responseData).ToArray();

                    //0x04 读 0x01 读取一个长度 //如果是批量读取，批量读取方法里面有验证
                    if (dataPackage[19] == 0x04 && dataPackage[20] == 0x01)
                    {
                        if (dataPackage[21] == 0x0A && dataPackage[22] == 0x00)
                        {

                            result.AddError($"读取{address}失败，请确认是否存在地址{address}");
                            return result;
                        }
                        else if (dataPackage[21] == 0x05 && dataPackage[22] == 0x00)
                        {

                            result.AddError($"读取{address}失败，请确认是否存在地址{address}");
                            return result;
                        }
                        else if (dataPackage[21] != 0xFF)
                        {

                            result.AddError($"读取{address}失败，异常代码[{21}]:{dataPackage[21]}");
                            return result;
                        }
                    }
                }
            }
            //catch (SocketException ex)
            //{

            //    if (ex.SocketErrorCode == SocketError.TimedOut)
            //    {
            //        result.AddError($"读取{address}失败，连接超时");
            //    }
            //    else
            //    {
            //        result.AddError($"读取{address}失败，{ex.Message}");
            //    }
            //    SafeClose();
            //}
            catch (Exception ex)
            {
                result.AddError(ex);
                //SafeClose();
            }
            //finally
            //{
            //    if (isAutoOpen) Dispose();
            //}
            return result.ToEnd();
        }

        /// <summary>
        /// 分批读取，默认按19个地址打包读取
        /// </summary>
        /// <param name="addresses">地址集合</param>
        /// <param name="batchNumber">批量读取数量</param>
        /// <returns></returns>
        public IoTResult<Dictionary<string, object>> BatchRead(Dictionary<string, DataTypeEnum> addresses, int batchNumber = 19)
        {
            var result = new IoTResult<Dictionary<string, object>>();
            result.Value = new Dictionary<string, object>();

            var batchCount = Math.Ceiling((float)addresses.Count / batchNumber);
            for (int i = 0; i < batchCount; i++)
            {
                var tempAddresses = addresses.Skip(i * batchNumber).Take(batchNumber).ToDictionary(t => t.Key, t => t.Value);
                var tempResult = BatchRead(tempAddresses);
                if (!tempResult.IsSucceed)
                {
                    result.AddError(tempResult.Error);
                }

                if (tempResult.Value?.Any() ?? false)
                {
                    foreach (var item in tempResult.Value)
                    {
                        result.Value.Add(item.Key, item.Value);
                    }
                }

                //result.Requst = tempResult.Requst;
                //result.Response = tempResult.Response;
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 最多只能批量读取19个数据？        
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        private IoTResult<Dictionary<string, object>> BatchRead(Dictionary<string, DataTypeEnum> addresses)
        {
            //if (!socket?.Connected ?? true)
            //{
            //    var connectResult = Connect();
            //    if (!connectResult.IsSucceed)
            //    {
            //        return new IoTResult<Dictionary<string, object>>(connectResult);
            //    }
            //}
            var result = new IoTResult<Dictionary<string, object>>();
            result.Value = new Dictionary<string, object>();
            try
            {
                //发送读取信息
                var args = SiemensAddress.ConvertArg(addresses);
                byte[] command = GetReadCommand(args);

                //发送命令 并获取响应报文
                var sendResult = Client.SendReceive(command);
                if (!sendResult.IsSucceed)
                    return new IoTResult<Dictionary<string, object>>(sendResult);

                var dataPackage = sendResult.Value;

                //2021.5.27注释，直接使用【var length = dataPackage.Length - 21】代替。
                //DataType类型为Bool的时候需要读取两个字节
                //var length = args.Sum(t => t.ReadWriteLength == 1 ? 2 : t.ReadWriteLength) + args.Length * 4;
                //if (args.Last().ReadWriteLength == 1) length--;//最后一个如果是 ReadWriteLength == 1  ，结果会少一个字节。

                var length = dataPackage.Length - 21;

                byte[] responseData = new byte[length];

                Array.Copy(dataPackage, dataPackage.Length - length, responseData, 0, length);


                var cursor = 0;
                foreach (var item in args)
                {
                    object value;

                    var isSucceed = true;
                    if (responseData[cursor] == 0x0A && responseData[cursor + 1] == 0x00)
                    {
                        isSucceed = false;
                        result.AddError($"读取{item.Address}失败，请确认是否存在地址{item.Address}");

                    }
                    else if (responseData[cursor] == 0x05 && responseData[cursor + 1] == 0x00)
                    {
                        isSucceed = false;
                        result.AddError($"读取{item.Address}失败，请确认是否存在地址{item.Address}");
                    }
                    else if (responseData[cursor] != 0xFF)
                    {
                        isSucceed = false;
                        result.AddError($"读取{item.Address}失败，异常代码[{cursor}]:{responseData[cursor]}");
                    }

                    cursor += 4;

                    //如果本次读取有异常
                    if (!isSucceed)
                    {
                        result.IsSucceed = false;
                        continue;
                    }

                    var readResut = responseData.Skip(cursor).Take(item.Length).Reverse().ToArray();
                    cursor += item.Length == 1 ? 2 : item.Length;
                    switch (item.DataType)
                    {
                        case DataTypeEnum.Bool:
                            value = BitConverter.ToBoolean(readResut, 0) ? 1 : 0;
                            break;
                        case DataTypeEnum.Byte:
                            value = readResut[0];
                            break;
                        case DataTypeEnum.Int16:
                            value = BitConverter.ToInt16(readResut, 0);
                            break;
                        case DataTypeEnum.UInt16:
                            value = BitConverter.ToUInt16(readResut, 0);
                            break;
                        case DataTypeEnum.Int32:
                            value = BitConverter.ToInt32(readResut, 0);
                            break;
                        case DataTypeEnum.UInt32:
                            value = BitConverter.ToUInt32(readResut, 0);
                            break;
                        case DataTypeEnum.Int64:
                            value = BitConverter.ToInt64(readResut, 0);
                            break;
                        case DataTypeEnum.UInt64:
                            value = BitConverter.ToUInt64(readResut, 0);
                            break;
                        case DataTypeEnum.Float:
                            value = BitConverter.ToSingle(readResut, 0);
                            break;
                        case DataTypeEnum.Double:
                            value = BitConverter.ToDouble(readResut, 0);
                            break;
                        default:
                            throw new Exception($"未定义数据类型：{item.DataType}");
                    }
                    result.Value.Add(item.Address, value);
                }
            }
            //catch (SocketException ex)
            //{
            //    result.IsSucceed = false;
            //    if (ex.SocketErrorCode == SocketError.TimedOut)
            //    {
            //        result.AddError("连接超时");
            //    }
            //    else
            //    {
            //        result.AddError(ex);
            //    }
            //    SafeClose();
            //}
            catch (Exception ex)
            {
                result.AddError(ex);
                //SafeClose();
            }
            //finally
            //{
            //    if (isAutoOpen) Dispose();
            //}
            return result.ToEnd();
        }

        ///// <summary>
        ///// 读取Boolean
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <returns></returns>
        //public IoTResult<bool> ReadBoolean(string address)
        //{
        //    var readResut = Read(address, 1, isBit: true);
        //    var result = new IoTResult<bool>(readResut);
        //    if (result.IsSucceed)
        //        result.Value = BitConverter.ToBoolean(readResut.Value.ToByteFormat(Format), 0);
        //    return result.ToEnd();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="address"></param>
        ///// <returns></returns>
        //public IoTResult<byte> ReadByte(string address)
        //{
        //    var readResut = Read(address, 1);
        //    var result = new IoTResult<byte>(readResut);
        //    if (result.IsSucceed)
        //        result.Value = readResut.Value.ToByteFormat(Format)[0];
        //    return result.ToEnd();
        //}

        ///// <summary>
        ///// 读取Int16
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <returns></returns>
        //public IoTResult<short> ReadInt16(string address)
        //{
        //    var readResut = Read(address, 2);
        //    var result = new IoTResult<short>(readResut);
        //    if (result.IsSucceed)
        //        result.Value = BitConverter.ToInt16(readResut.Value.ToByteFormat(Format), 0);
        //    return result.ToEnd();
        //}

        ///// <summary>
        ///// 读取UInt16
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <returns></returns>
        //public IoTResult<ushort> ReadUInt16(string address)
        //{
        //    var readResut = Read(address, 2);
        //    var result = new IoTResult<ushort>(readResut);
        //    if (result.IsSucceed)
        //        result.Value = BitConverter.ToUInt16(readResut.Value.ToByteFormat(Format), 0);
        //    return result.ToEnd();
        //}

        ///// <summary>
        ///// 读取Int32
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <returns></returns>
        //public IoTResult<int> ReadInt32(string address)
        //{
        //    var readResut = Read(address, 4);
        //    var result = new IoTResult<int>(readResut);
        //    if (result.IsSucceed)
        //        result.Value = BitConverter.ToInt32(readResut.Value.ToByteFormat(Format), 0);
        //    return result.ToEnd();
        //}

        ///// <summary>
        ///// 读取UInt32
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <returns></returns>
        //public IoTResult<uint> ReadUInt32(string address)
        //{
        //    var readResut = Read(address, 4);
        //    var result = new IoTResult<uint>(readResut);
        //    if (result.IsSucceed)
        //        result.Value = BitConverter.ToUInt32(readResut.Value.ToByteFormat(Format), 0);
        //    return result.ToEnd();
        //}

        ///// <summary>
        ///// 读取Int64
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <returns></returns>
        //public IoTResult<long> ReadInt64(string address)
        //{
        //    var readResut = Read(address, 8);
        //    var result = new IoTResult<long>(readResut);
        //    if (result.IsSucceed)
        //        result.Value = BitConverter.ToInt64(readResut.Value.ToByteFormat(Format), 0);
        //    return result.ToEnd();
        //}

        ///// <summary>
        ///// 读取UInt64
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <returns></returns>
        //public IoTResult<ulong> ReadUInt64(string address)
        //{
        //    var readResut = Read(address, 8);
        //    var result = new IoTResult<ulong>(readResut);
        //    if (result.IsSucceed)
        //        result.Value = BitConverter.ToUInt64(readResut.Value.ToByteFormat(Format), 0);
        //    return result.ToEnd();
        //}

        ///// <summary>
        ///// 读取Float
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <returns></returns>
        //public IoTResult<float> ReadFloat(string address)
        //{
        //    var readResut = Read(address, 4);
        //    var result = new IoTResult<float>(readResut);
        //    if (result.IsSucceed)
        //        result.Value = BitConverter.ToSingle(readResut.Value.ToByteFormat(Format), 0);
        //    return result.ToEnd();
        //}

        ///// <summary>
        ///// 读取Double
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <returns></returns>
        //public IoTResult<double> ReadDouble(string address)
        //{
        //    var readResut = Read(address, 8);
        //    var result = new IoTResult<double>(readResut);
        //    if (result.IsSucceed)
        //        result.Value = BitConverter.ToDouble(readResut.Value.ToByteFormat(Format), 0);
        //    return result.ToEnd();
        //}
        #endregion

        #region Write

        /// <summary>
        /// 批量写入
        /// TODO 可以重构后面的Write 都走BatchWrite
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        private IoTResult BatchWrite(Dictionary<string, object> addresses)
        {
            //if (!socket?.Connected ?? true)
            //{
            //    var connectResult = Connect();
            //    if (!connectResult.IsSucceed)
            //    {
            //        return connectResult;
            //    }
            //}
            IoTResult result = new IoTResult();
            try
            {
                var newAddresses = new Dictionary<string, KeyValuePair<byte[], bool>>();
                foreach (var item in addresses)
                {
                    var tempData = new List<byte>();
                    switch (item.Value.GetType().Name)
                    {
                        case "Boolean":
                            tempData = (bool)item.Value ? new List<byte>() { 0x01 } : new List<byte>() { 0x00 };
                            break;
                        case "Byte":
                            tempData = new List<byte>() { (byte)item.Value };
                            break;
                        case "UInt16":
                            tempData = BitConverter.GetBytes((ushort)item.Value).ToList();
                            break;
                        case "Int16":
                            tempData = BitConverter.GetBytes((short)item.Value).ToList();
                            break;
                        case "UInt32":
                            tempData = BitConverter.GetBytes((uint)item.Value).ToList();
                            break;
                        case "Int32":
                            tempData = BitConverter.GetBytes((int)item.Value).ToList();
                            break;
                        case "UInt64":
                            tempData = BitConverter.GetBytes((ulong)item.Value).ToList();
                            break;
                        case "Int64":
                            tempData = BitConverter.GetBytes((long)item.Value).ToList();
                            break;
                        case "Single":
                            tempData = BitConverter.GetBytes((float)item.Value).ToList();
                            break;
                        case "Double":
                            tempData = BitConverter.GetBytes((double)item.Value).ToList();
                            break;
                        default:
                            throw new Exception($"暂未提供对{item.Value.GetType().Name}类型的写入操作。");
                    }
                    tempData.Reverse();
                    newAddresses.Add(item.Key, new KeyValuePair<byte[], bool>(tempData.ToArray(), item.Value.GetType().Name == "Boolean"));
                }
                var arg = SiemensWriteAddress.ConvertWriteArg(newAddresses);
                byte[] command = GetWriteCommand(arg);

                var sendResult = Client.SendReceive(command);
                if (!sendResult.IsSucceed)
                    return sendResult;

                var dataPackage = sendResult.Value;


                if (dataPackage.Length == arg.Length + 21)
                {
                    for (int i = 0; i < arg.Length; i++)
                    {
                        var offset = 21 + i;
                        if (dataPackage[offset] == 0x0A)
                        {

                            result.AddError($"写入{arg[i].Address}失败，请确认是否存在地址{arg[i].Address}，异常代码[{offset}]:{dataPackage[offset]}");
                        }
                        else if (dataPackage[offset] == 0x05)
                        {

                            result.AddError($"写入{arg[i].Address}失败，请确认是否存在地址{arg[i].Address}，异常代码[{offset}]:{dataPackage[offset]}");
                        }
                        else if (dataPackage[offset] != 0xFF)
                        {

                            result.AddError($"写入{string.Join(",", arg.Select(t => t.Address))}失败，异常代码[{offset}]:{dataPackage[offset]}");
                        }
                    }
                }
                else
                {

                    result.AddError($"写入数据数量和响应结果数量不一致，写入数据：{arg.Length} 响应数量：{dataPackage.Length - 21}");
                }
            }
            //catch (SocketException ex)
            //{
            //    result.IsSucceed = false;
            //    if (ex.SocketErrorCode == SocketError.TimedOut)
            //    {
            //        result.AddError("连接超时");
            //    }
            //    else
            //    {
            //        result.AddError(ex);
            //    }
            //    SafeClose();
            //}
            catch (Exception ex)
            {
                result.AddError(ex);
                //SafeClose();
            }
            //finally
            //{
            //    if (isAutoOpen) Dispose();
            //}
            return result.ToEnd();
        }

        /// <summary>
        /// 分批写入，默认按10个地址打包读取
        /// </summary>
        /// <param name="addresses">地址集合</param>
        /// <param name="batchNumber">批量读取数量</param>
        /// <returns></returns>
        public IoTResult BatchWrite(Dictionary<string, object> addresses, int batchNumber = 10)
        {
            var result = new IoTResult();
            var batchCount = Math.Ceiling((float)addresses.Count / batchNumber);
            for (int i = 0; i < batchCount; i++)
            {
                var tempAddresses = addresses.Skip(i * batchNumber).Take(batchNumber).ToDictionary(t => t.Key, t => t.Value);
                var tempResult = BatchWrite(tempAddresses);
                if (!tempResult.IsSucceed)
                {
                    result.AddError(tempResult.Error, tempResult.IsSucceed);

                }
                //result.Requst = tempResult.Requst;
                //result.Response = tempResult.Response;
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="data">值</param>
        /// <param name="isBit">值</param>
        /// <returns></returns>
        public IoTResult Write(string address, byte[] data, bool isBit = false)
        {
            IoTResult<byte[]> ioTResult = new IoTResult<byte[]>();
            try
            {
                var addr = SiemensWriteAddress.ConvertArg(address);
                var cs = WordHelp.SplitBlock(data.Length, ReadWriteByteNum, addr.BeginAddress, 8);
                for (var i = 0; i < cs.Count; i++)
                {
                    var item = cs.ElementAt(i);
                    SiemensWriteAddress arg = new SiemensWriteAddress(addr);
                    arg.BeginAddress = item.Key;
                    arg.WriteData = data.Skip(i * ReadWriteByteNum).Take(item.Value).ToArray();
                    arg.IsBit = isBit;

                    byte[] command = GetWriteCommand(arg);

                    var sendResult = Client.SendReceive(command);
                    ioTResult.Requests.Add(sendResult.Requests.FirstOrDefault());
                    ioTResult.Responses.Add(sendResult.Responses.FirstOrDefault());

                    if (!sendResult.IsSucceed)
                        return ioTResult.AddError(sendResult.Error);

                    var dataPackage = sendResult.Value;
                    var offset = dataPackage.Length - 1;
                    if (dataPackage[offset] == 0x0A)
                    {
                        return ioTResult.AddError($"写入{address}失败，请确认是否存在地址{address}，异常代码[{offset}]:{dataPackage[offset]}");
                    }
                    else if (dataPackage[offset] == 0x05)
                    {
                        return ioTResult.AddError($"写入{address}失败，请确认是否存在地址{address}，异常代码[{offset}]:{dataPackage[offset]}");
                    }
                    else if (dataPackage[offset] != 0xFF)
                    {
                        return ioTResult.AddError($"写入{address}失败，异常代码[{offset}]:{dataPackage[offset]}");
                    }
                }
            }
            catch (Exception ex)
            {
                ioTResult.AddError(ex);
            }
            return ioTResult.ToEnd();
        }

        ///// <summary>
        ///// 写入数据
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <param name="value">值</param>
        ///// <returns></returns>
        //public IoTResult Write(string address, bool value)
        //{
        //    return Write(address, new byte[1] { value ? (byte)1 : (byte)0 }.ToByteFormat(Format), true);
        //}

        ///// <summary>
        ///// 写入数据
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <param name="value">值</param>
        ///// <returns></returns>
        //public IoTResult Write(string address, byte value)
        //{
        //    return Write(address, new byte[1] { value }.ToByteFormat(Format), false);
        //}

        ///// <summary>
        ///// 写入数据
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <param name="value">值</param>
        ///// <returns></returns>
        //public IoTResult Write(string address, sbyte value)
        //{
        //    return Write(address, BitConverter.GetBytes(value).ToByteFormat(Format), false);
        //}

        ///// <summary>
        ///// 写入数据
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <param name="value">值</param>
        ///// <returns></returns>
        //public IoTResult Write(string address, short value)
        //{
        //    return Write(address, BitConverter.GetBytes(value).ToByteFormat(Format), false);
        //}

        ///// <summary>
        ///// 写入数据
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <param name="value">值</param>
        ///// <returns></returns>
        //public IoTResult Write(string address, ushort value)
        //{
        //    return Write(address, BitConverter.GetBytes(value).ToByteFormat(Format), false);
        //}

        ///// <summary>
        ///// 写入数据
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <param name="value">值</param>
        ///// <returns></returns>
        //public IoTResult Write(string address, int value)
        //{
        //    return Write(address, BitConverter.GetBytes(value).ToByteFormat(Format), false);
        //}

        ///// <summary>
        ///// 写入数据
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <param name="value">值</param>
        ///// <returns></returns>
        //public IoTResult Write(string address, uint value)
        //{
        //    return Write(address, BitConverter.GetBytes(value).ToByteFormat(Format), false);
        //}

        ///// <summary>
        ///// 写入数据
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <param name="value">值</param>
        ///// <returns></returns>
        //public IoTResult Write(string address, long value)
        //{
        //    return Write(address, BitConverter.GetBytes(value).ToByteFormat(Format), false);
        //}

        ///// <summary>
        ///// 写入数据
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <param name="value">值</param>
        ///// <returns></returns>
        //public IoTResult Write(string address, ulong value)
        //{
        //    return Write(address, BitConverter.GetBytes(value).ToByteFormat(Format), false);
        //}

        ///// <summary>
        ///// 写入数据
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <param name="value">值</param>
        ///// <returns></returns>
        //public IoTResult Write(string address, float value)
        //{
        //    return Write(address, BitConverter.GetBytes(value).ToByteFormat(Format), false);
        //}

        ///// <summary>
        ///// 写入数据
        ///// </summary>
        ///// <param name="address">地址</param>
        ///// <param name="value">值</param>
        ///// <returns></returns>
        //public IoTResult Write(string address, double value)
        //{
        //    return Write(address, BitConverter.GetBytes(value).ToByteFormat(Format), false);
        //}
        #endregion

        #region 获取指令
        /// <summary>
        /// 获取读指令
        /// </summary>      
        /// <returns></returns>
        protected byte[] GetReadCommand(SiemensAddress[] datas)
        {
            //byte type, int beginAddress, ushort dbAddress, ushort length, bool isBit
            byte[] command = new byte[19 + datas.Length * 12];
            command[0] = 0x03;
            command[1] = 0x00;//[0][1]固定报文头
            command[2] = (byte)(command.Length / 256);
            command[3] = (byte)(command.Length % 256);//[2][3]整个读取请求长度为0x1F= 31 
            command[4] = 0x02;
            command[5] = 0xF0;
            command[6] = 0x80;//COTP
            command[7] = 0x32;//协议ID
            command[8] = 0x01;//1  客户端发送命令 3 服务器回复命令
            command[9] = 0x00;
            command[10] = 0x00;//[4]-[10]固定6个字节
            command[11] = 0x00;
            command[12] = 0x01;//[11][12]两个字节，标识序列号，回复报文相同位置和这个完全一样；范围是0~65535
            command[13] = (byte)((command.Length - 17) / 256);
            command[14] = (byte)((command.Length - 17) % 256); //parameter length（减17是因为从[17]到最后属于parameter）
            command[15] = 0x00;
            command[16] = 0x00;//data length
            command[17] = 0x04;//04读 05写
            command[18] = (byte)datas.Length;//读取数据块个数
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i];
                command[19 + i * 12] = 0x12;//variable specification
                command[20 + i * 12] = 0x0A;//Length of following address specification
                command[21 + i * 12] = 0x10;//Syntax Id: S7ANY 
                command[22 + i * 12] = data.IsBit ? (byte)0x01 : (byte)0x02;//Transport size: BYTE 
                command[23 + i * 12] = (byte)(data.Length / 256);
                command[24 + i * 12] = (byte)(data.Length % 256);//[23][24]两个字节,访问数据的个数，以byte为单位；
                command[25 + i * 12] = (byte)(data.DbBlock / 256);
                command[26 + i * 12] = (byte)(data.DbBlock % 256);//[25][26]DB块的编号
                command[27 + i * 12] = data.TypeCode;//访问数据块的类型
                command[28 + i * 12] = (byte)(data.BeginAddress / 256 / 256 % 256);
                command[29 + i * 12] = (byte)(data.BeginAddress / 256 % 256);
                command[30 + i * 12] = (byte)(data.BeginAddress % 256);//[28][29][30]访问DB块的偏移量
            }
            return command;
        }

        /// <summary>
        /// 获取读指令
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected byte[] GetReadCommand(SiemensAddress data)
        {
            return GetReadCommand(new SiemensAddress[] { data });
        }

        /// <summary>
        /// 获取写指令
        /// </summary>
        /// <param name="writes"></param>
        /// <returns></returns>
        protected byte[] GetWriteCommand(SiemensWriteAddress[] writes)
        {
            //（如果不是最后一个 WriteData.Length == 1 ，则需要填充一个空数据）
            var writeDataLength = writes.Sum(t => t.WriteData.Length == 1 ? 2 : t.WriteData.Length);
            if (writes[writes.Length - 1].WriteData.Length == 1) writeDataLength--;

            //前19个固定的、16为Item长度、writes.Length为Imte的个数
            byte[] command = new byte[19 + writes.Length * 16 + writeDataLength];

            command[0] = 0x03;
            command[1] = 0x00;//[0][1]固定报文头
            command[2] = (byte)(command.Length / 256);
            command[3] = (byte)(command.Length % 256);//[2][3]整个读取请求长度
            command[4] = 0x02;
            command[5] = 0xF0;
            command[6] = 0x80;
            command[7] = 0x32;//protocol Id
            command[8] = 0x01;//1  客户端发送命令 3 服务器回复命令 Job
            command[9] = 0x00;
            command[10] = 0x00;//[9][10] redundancy identification (冗余的识别)
            command[11] = 0x00;
            command[12] = 0x01;//[11]-[12]protocol data unit reference
            command[13] = (byte)((12 * writes.Length + 2) / 256);
            command[14] = (byte)((12 * writes.Length + 2) % 256);//Parameter length
            command[15] = (byte)((writeDataLength + 4 * writes.Length) / 256);
            command[16] = (byte)((writeDataLength + 4 * writes.Length) % 256);//[15][16] Data length

            //Parameter
            command[17] = 0x05;//04读 05写 Function Write
            command[18] = (byte)writes.Length;//写入数据块个数 Item count
            //Item[]
            for (int i = 0; i < writes.Length; i++)
            {
                var write = writes[i];
                var typeCode = write.TypeCode;
                var beginAddress = write.BeginAddress;
                var dbBlock = write.DbBlock;
                var writeData = write.WriteData;

                command[19 + i * 12] = 0x12;
                command[20 + i * 12] = 0x0A;
                command[21 + i * 12] = 0x10;//[19]-[21]固定
                command[22 + i * 12] = write.IsBit ? (byte)0x01 : (byte)0x02;//写入方式，1是按位，2是按字
                command[23 + i * 12] = (byte)(writeData.Length / 256);
                command[24 + i * 12] = (byte)(writeData.Length % 256);//写入数据个数
                command[25 + i * 12] = (byte)(dbBlock / 256);
                command[26 + i * 12] = (byte)(dbBlock % 256);//DB块的编号
                command[27 + i * 12] = typeCode;
                command[28 + i * 12] = (byte)(beginAddress / 256 / 256 % 256);
                command[29 + i * 12] = (byte)(beginAddress / 256 % 256);
                command[30 + i * 12] = (byte)(beginAddress % 256);//[28][29][30]访问DB块的偏移量      

            }
            var index = 18 + writes.Length * 12;
            //Data
            for (int i = 0; i < writes.Length; i++)
            {
                var write = writes[i];
                var writeData = write.WriteData;
                var coefficient = write.IsBit ? 1 : 8;

                command[1 + index] = 0x00;
                command[2 + index] = write.IsBit ? (byte)0x03 : (byte)0x04;// 03bit（位）04 byte(字节)
                command[3 + index] = (byte)(writeData.Length * coefficient / 256);
                command[4 + index] = (byte)(writeData.Length * coefficient % 256);//按位计算出的长度

                if (write.WriteData.Length == 1)
                {
                    if (write.IsBit)
                        command[5 + index] = writeData[0] == 0x01 ? (byte)0x01 : (byte)0x00; //True or False 
                    else command[5 + index] = writeData[0];

                    if (i >= writes.Length - 1)
                        index += 4 + 1;
                    else index += 4 + 2; // fill byte  （如果不是最后一个bit，则需要填充一个空数据）
                }
                else
                {
                    writeData.CopyTo(command, 5 + index);
                    index += 4 + writeData.Length;
                }
            }
            return command;
        }

        /// <summary>
        /// 获取写指令
        /// </summary>
        /// <param name="write"></param>
        /// <returns></returns>
        protected byte[] GetWriteCommand(SiemensWriteAddress write)
        {
            return GetWriteCommand(new SiemensWriteAddress[] { write });
        }

        #endregion

        #region IIoTBase
        public IoTResult<T> Read<T>(string address)
        {
            var info = Read<T>(address, 1);
            if (info.IsSucceed)
                return info.ToVal(info.Value.FirstOrDefault());
            else
                return info.ToVal(default(T));
        }

        /// <summary>
        /// 针对与PLC中类型“WString”的读
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="length">无效</param>
        /// <param name="encoding">默认为 Encoding.BigEndianUnicode </param>
        /// <returns></returns>
        public IoTResult<string> ReadString(string address, int length = 512, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.BigEndianUnicode;
            var readResut = new IoTResult<byte[]>();
            try
            {
                //[总长度][数量][数据...]
                readResut = Read(address, (UInt16)length, false);
                if (readResut.IsSucceed)
                {
                    var sl = BitConverter.ToUInt16(new byte[] { readResut.Value[3], readResut.Value[2] }, 0) * 2;
                    var strData = readResut.Value.Skip(4).Take(sl).ToArray();
                    var nr = encoding.GetString(strData);

                    return readResut.ToVal<string>(nr);
                }
                else
                {
                    return readResut.ToVal<string>();
                }
            }
            catch (Exception ex)
            {
                readResut.AddError(ex);
            }
            return readResut.ToVal<string>().ToEnd();
        }

        public IoTResult<IEnumerable<T>> Read<T>(string address, int number)
        {
            try
            {
                bool isOneBool = false;//是否为bool类型且数量为1
                int address2 = 0;//DB10.2.3 中的3
                var tType = typeof(T);
                var ynum = WordHelp.OccupyBitNum<T>();//实际的读取长度
                if (tType == typeof(bool))
                {
                    if (number == 1)
                    {
                        isOneBool = true;
                        ynum = 1;
                    }
                    else
                    {
                        ynum = Convert.ToUInt16(number % 8 == 0 ? number / 8 : number / 8 + 1);
                        var info = SiemensAddress.ConvertArg(address);
                        address2 = info.Address2;
                        if (address2 > 0)//跨长度补充
                        {
                            var v1 = 8 - address2;//剩下的数量
                            var v2 = number < 8 ? number : number % 8;//读取数量
                            if (v2 > v1)
                            {
                                ynum += 1;
                            }
                        }
                    }
                }
                else if (tType == typeof(string))
                {
                    ynum = Convert.ToUInt16(number * 256);//string
                }
                else if (tType == typeof(DateTime))
                {
                    ynum = Convert.ToUInt16(number * 2);
                }
                else if (tType == typeof(TimeSpan))
                {
                    ynum = Convert.ToUInt16(number * 4);
                }
                else if (tType == typeof(Char))
                {
                    ynum = Convert.ToUInt16(number * 1);
                }
                else
                    ynum = Convert.ToUInt16(number * ynum);

                var readResut = Read(address, ynum, isOneBool);
                if (readResut.IsSucceed)
                {
                    T[] valJg = new T[0];
                    if (tType == typeof(string))
                    {
                        valJg = readResut.Value.SplitBlock(256, true).Select(o => (T)(object)Client.Encoding.GetString(o.Skip(2).Take(o[1]).ToArray())).ToArray();
                    }
                    else if (tType == typeof(DateTime))
                    {
                        valJg = readResut.Value.ByteToObj<UInt16>(Format, true).Select(o => (T)(object)new DateTime(1990, 1, 1).AddDays(o)).ToArray();
                    }
                    else if (tType == typeof(TimeSpan))
                    {
                        valJg = readResut.Value.ByteToObj<UInt32>(Format, true).Select(o => (T)(object)TimeSpan.FromMilliseconds(o)).ToArray();
                    }
                    else if (tType == typeof(Char))
                    {
                        valJg = Client.Encoding.GetString(readResut.Value).Select(o => (T)(object)o).ToArray();
                    }
                    //正常类型
                    else
                    {
                        valJg = readResut.Value.ByteToObj<T>(Format, true);
                        if (tType == typeof(bool))
                        {
                            valJg = valJg.Skip(address2).Take(number).ToArray();
                        }
                    }

                    return readResut.ToVal<IEnumerable<T>>(valJg);
                }
                else
                {
                    return readResut.ToVal<IEnumerable<T>>();
                }
            }
            catch (Exception ex)
            {
                return new IoTResult<IEnumerable<T>>().AddError(ex);
            }

        }

        public IoTResult Write<T>(string address, T value)
        {
            return Write<T>(address, new T[] { value });
        }

        /// <summary>
        /// 针对与PLC中类型“WString”的写
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="length">无效</param>
        /// <param name="encoding">默认为 Encoding.BigEndianUnicode </param>
        /// <returns></returns>
        public IoTResult WriteString(string address, string value, int length = -1, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.BigEndianUnicode;

            var valueBytes = encoding.GetBytes(value);
            if (valueBytes.Length > 508)
                return IoTResult.Create().AddError($"字符串长度不能超过{508 / 2}");

            var sl = BitConverter.GetBytes((UInt16)(valueBytes.Length / 2));
            var bytes = new byte[] { 0, 254, sl[1], sl[0] }.Concat(valueBytes).ToArray();
            return Write(address, bytes, false);
        }

        public IoTResult Write<T>(string address, params T[] value)
        {
            try
            {
                var tType = typeof(T);
                if (tType == typeof(bool))
                {
                    if (value.Length == 1)
                        return Write(address, new byte[1] { (bool)(object)value[0] ? (byte)1 : (byte)0 }.ToByteFormat(Format), true);
                    else
                        throw new NotImplementedException("暂不支持写多个bool类型");
                }
                else if (tType == typeof(string))
                {
                    var valueBytes = value.Select(o => Client.Encoding.GetBytes((string)(object)o));
                    if (valueBytes.Any(o => o.Length > 254))
                        return IoTResult.Create().AddError($"字符串长度不能超过{254}");

                    List<byte> bytes = new List<byte>(256 * value.Count());
                    if (value.Length == 1)
                    {
                        bytes.AddRange(new byte[] { 254, Convert.ToByte(valueBytes.FirstOrDefault().Count()) }.Concat(valueBytes.FirstOrDefault()));
                    }
                    else
                    {
                        foreach (var item in valueBytes)
                        {
                            var bc = Enumerable.Repeat<byte>(0, 254 - item.Length);
                            bytes.AddRange(new byte[] { 254, Convert.ToByte(item.Length) }.Concat(item).Concat(bc));
                        }
                    }
                    return Write(address, bytes.ToArray(), false);
                }
                else if (tType == typeof(DateTime))
                {
                    var data2 = value.Select(o => (DateTime)(object)o).Select(o => BitConverter.GetBytes(Convert.ToUInt16((o - new DateTime(1990, 1, 1)).TotalDays)).Reverse()).SelectMany(o=>o).ToArray();
                    return Write(address, data2, false);
                }
                else if (tType == typeof(TimeSpan))
                {
                    var data = value.Select(o => BitConverter.GetBytes(Convert.ToUInt32(((TimeSpan)(object)o).TotalMilliseconds)).Reverse()).SelectMany(o => o).ToArray();
                    return Write(address, data, false);
                }
                else if (tType == typeof(Char))
                {
                    var data = Client.Encoding.GetBytes(value.Select(o => (Char)(object)o).ToArray());
                    return Write(address, data, false);
                }
                else
                {
                    var obj = value.ObjToByte(Format);
                    return Write(address, obj, false);
                }
            }
            catch (Exception ex)
            {
                return IoTResult.Create().AddError(ex);
            }
        }
        #endregion
    }

    /// <summary>
    /// Siemens命令常量
    /// </summary>
    partial class SiemensConstant
    {
        /// <summary>
        /// Head头读取长度
        /// </summary>
        public static readonly ushort InitHeadLength = 4;

        /// <summary>
        /// 第一次初始化指令交互报文
        /// </summary>
        public static readonly byte[] Command1 = new byte[22]
        {
            0x03,0x00,0x00,0x16,0x11,0xE0,0x00,0x00,
            0x00,0x01,0x00,0xC0,0x01,0x0A,0xC1,0x02,
            0x01,0x02,0xC2,0x02,0x01,0x00
        };

        /// <summary>
        /// 第二次初始化指令交互报文
        /// </summary>
        public static readonly byte[] Command2 = new byte[25]
        {
            0x03,0x00,0x00,0x19,0x02,0xF0,0x80,0x32,
            0x01,0x00,0x00,0x04,0x00,0x00,0x08,0x00,
            0x00,0xF0,0x00,0x00,0x01,0x00,0x01,0x01,0xE0
        };

        /// <summary>
        /// 第一次初始化指令交互报文
        /// </summary>
        public static readonly byte[] Command1_200Smart = new byte[22]
        {
            0x03,0x00,0x00,0x16,0x11,0xE0,0x00,0x00,
            0x00,0x01,0x00,0xC1,0x02,0x10,0x00,0xC2,
            0x02,0x03,0x00,0xC0,0x01,0x0A
        };

        /// <summary>
        /// 第二次初始化指令交互报文
        /// </summary>
        public static readonly byte[] Command2_200Smart = new byte[25]
        {
            0x03,0x00,0x00,0x19,0x02,0xF0,0x80,0x32,
            0x01,0x00,0x00,0xCC,0xC1,0x00,0x08,0x00,
            0x00,0xF0,0x00,0x00,0x01,0x00,0x01,0x03,0xC0
        };

        /// <summary>
        /// 第一次初始化指令交互报文
        /// </summary>
        public static readonly byte[] Command1_200 = new byte[]
        {
            0x03,0x00,0x00,0x16,0x11,0xE0,0x00,0x00,
            0x00,0x01,0x00,0xC1,0x02,0x4D,0x57,0xC2,
            0x02,0x4D,0x57,0xC0,0x01,0x09
        };

        /// <summary>
        /// 第二次初始化指令交互报文
        /// </summary>
        public static readonly byte[] Command2_200 = new byte[]
        {
            0x03,0x00,0x00,0x19,0x02,0xF0,0x80,0x32,
            0x01,0x00,0x00,0x00,0x00,0x00,0x08,0x00,
            0x00,0xF0,0x00,0x00,0x01,0x00,0x01,0x03,0xC0
        };
    }
}
