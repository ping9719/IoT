using Ping9719.IoT;
using Ping9719.IoT.Common;
using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Rfid
{
    /// <summary>
    /// 东集
    /// </summary>
    public class DongJiRfid
    {
        public ClientBase Client { get; private set; }
        public DongJiRfid(ClientBase client, int timeout = 1500)
        {
            Client = client;
            Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            Client.TimeOut = timeout;
            //Client.ConnectionMode = ConnectionMode.AutoOpen;
        }
        public DongJiRfid(string ip, int port = 10000) : this(new TcpClient(ip, port)) { }
        public DongJiRfid(string portName, int baudRate, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One) : this(new SerialPortClient(portName, baudRate, parity, dataBits, stopBits)) { }

        /// <summary>
        /// 开始寻卡
        /// </summary>
        /// <returns></returns>
        public IoTResult Go()
        {
            IoTResult result = new IoTResult();
            try
            {
                //开始寻卡
                var sendInfo = Client.Encoding.GetBytes(JsonUtil.SerializeObject(new DongJiRfidModel<DongJiRfidParamEpcFilter> { code = 1018, data = new DongJiRfidParamEpcFilter { antennaEnable = 1, inventoryMode = 1 } }) + "$");
                var retValue_Send = Client.Send(sendInfo);
                if (!retValue_Send.IsSucceed)
                {
                    result.IsSucceed = false;
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 结束寻卡
        /// </summary>
        /// <returns></returns>
        public IoTResult End()
        {
            IoTResult result = new IoTResult();
            try
            {
                //开始寻卡
                var sendInfo = Client.Encoding.GetBytes(JsonUtil.SerializeObject(new DongJiRfidModel { code = 1011 }) + "$");
                var retValue_Send = Client.Send(sendInfo);
                if (!retValue_Send.IsSucceed)
                {
                    result.IsSucceed = false;
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <returns></returns>
        public IoTResult<T> Read<T>()
        {
            IoTResult<T> result = new IoTResult<T>();
            try
            {
                //停止寻卡
                var sendInfo = Client.Encoding.GetBytes(JsonUtil.SerializeObject(new DongJiRfidModel { code = 1011 }) + "$");
                var retValue_Send = Client.SendReceive(sendInfo);
                //if (!retValue_Send.IsSucceed)
                //    return new Result<string>(retValue_Send).EndTime();

                //开始寻卡
                sendInfo = Client.Encoding.GetBytes(JsonUtil.SerializeObject(new DongJiRfidModel<DongJiRfidParamEpcFilter> { code = 1018, data = new DongJiRfidParamEpcFilter { antennaEnable = 1, inventoryMode = 0 } }) + "$");
                retValue_Send = Client.SendReceive(sendInfo);
                if (!retValue_Send.IsSucceed)
                {
                    result.IsSucceed = false;
                    return result;
                }

                var data = Client.Encoding.GetString(retValue_Send.Value);
                if (!data.EndsWith("$"))
                {
                    result.IsSucceed = false;
                    return result;
                }
                var newval = data.Split('$').Where(o => !string.IsNullOrEmpty(o)).LastOrDefault();
                var datajson = JsonUtil.DeserializeObject<DongJiRfidModel<List<DongJiRfidLogBaseEpcInfo>>>(newval);
                if (datajson.code != 0)
                {
                    return result.AddError(datajson.rtMsg);
                }
                if (datajson.data == null || !datajson.data.Any())
                {
                    return result.AddError("读取失败，未读取到RFID信息");
                }

                var byte1 = DataConvert.StringToByteArray(datajson.data.FirstOrDefault().epc, false);
                if (typeof(T) == typeof(byte[]))
                    result.Value = (T)(object)byte1;
                else if (typeof(T) == typeof(Int16))
                    result.Value = (T)(object)BitConverter.ToInt16(byte1,0);
                else if (typeof(T) == typeof(UInt16))
                    result.Value = (T)(object)BitConverter.ToUInt16(byte1, 0);
                else if (typeof(T) == typeof(Int32))
                    result.Value = (T)(object)BitConverter.ToInt32(byte1, 0);
                else if (typeof(T) == typeof(UInt32))
                    result.Value = (T)(object)BitConverter.ToUInt32(byte1, 0);
                else if (typeof(T) == typeof(string))
                    result.Value = (T)(object)Client.Encoding.GetString(byte1);
                else
                {
                    result.AddError("不支持的类型");
                }
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <returns></returns>
        public IoTResult<List<string>> Read2()
        {
            IoTResult<List<string>> result = new IoTResult<List<string>>();
            try
            {
                //停止寻卡
                var aaaa = JsonUtil.SerializeObject(new DongJiRfidModel { code = 1011 });
                var sendInfo = Client.Encoding.GetBytes(aaaa + "$");
                var retValue_Send = Client.SendReceive(sendInfo);
                //if (!retValue_Send.IsSucceed)
                //    return new Result<string>(retValue_Send).EndTime();

                //开始寻卡
                sendInfo = Client.Encoding.GetBytes(JsonUtil.SerializeObject(new DongJiRfidModel<DongJiRfidParamEpcFilter> { code = 1018, data = new DongJiRfidParamEpcFilter { antennaEnable = 1, inventoryMode = 0 } }) + "$");
                retValue_Send = Client.SendReceive(sendInfo);
                if (!retValue_Send.IsSucceed)
                {
                    result.IsSucceed = false;
                    return result;
                }

                var data = Client.Encoding.GetString(retValue_Send.Value);
                if (!data.EndsWith("$"))
                {
                    result.IsSucceed = false;
                    return result;
                }

                var newval = data.Split('$').Where(o => !string.IsNullOrEmpty(o)).LastOrDefault();
                var datajson = JsonUtil.DeserializeObject<DongJiRfidModel<List<DongJiRfidLogBaseEpcInfo>>>(newval);
                //var datajson = JsonUtil.DeserializeObject<DongJiRfidModel<List<DongJiRfidLogBaseEpcInfo>>>(data.Substring(0, data.Length - 1));

                if (datajson.code == 1000)
                {
                    return result.AddError(datajson.rtMsg);
                }
                if (datajson.data == null || !datajson.data.Any())
                {
                    return result.AddError("读取失败，未读取到RFID信息");
                }

                result.Value = datajson?.data?.Select(o => new string(o.epc.Reverse().Take(5).Reverse().ToArray()).PadLeft(5, '0')).ToList() ?? new List<string>();
                //result.Value = encoding.GetString(DataConvert.StringToByteArray(datajson.data[0].epc, false));
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

    }

    public class DongJiRfidModel<T>
    {
        public int code { get; set; }
        public T data { get; set; }
        public int rtCode { get; set; }
        public string rtMsg { get; set; }
    }

    public class DongJiRfidModel
    {
        public int code { get; set; }
        public int rtCode { get; set; }
        public string rtMsg { get; set; }
    }


    public class DongJiRfidParamEpcFilter
    {
        public int antennaEnable { get; set; }
        public int inventoryMode { get; set; }
    }


    public class DongJiRfidLogBaseEpcInfo
    {
        //public string additionDataBytes { get; set; }
        //public string additionDataHexStr { get; set; }
        //public int antId { get; set; }
        //public string bEpc { get; set; }
        public string epc { get; set; }
        //public string pc { get; set; }
        //public long readTime { get; set; }
        public int rssi { get; set; }
    }

}
