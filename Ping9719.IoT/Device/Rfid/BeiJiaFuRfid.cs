using Ping9719.IoT;
using Ping9719.IoT.Communication.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Rfid
{
    /// <summary>
    /// 倍加福Rfid。支持B17等
    /// </summary>
    public class BeiJiaFuRfid : SocketBase
    {

        /// <summary>
        /// 使用Tcp的方式
        /// </summary>
        public BeiJiaFuRfid(string ip, int port = 10000, int timeout = 1500)
        {
            if (socket == null)
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SetIpEndPoint(ip, port);
            this.timeout = timeout;
        }

        /// <summary>
        /// 读取，并尝试将读取的结果转为数字
        /// </summary>
        /// <param name="index">通道</param>
        /// <returns></returns>
        public IoTResult<int> ReadInt(int index = 0)
        {
            var info = Read(index);
            if (!info.IsSucceed)
                return new IoTResult<int>(info);

            if (!int.TryParse(info.Value, out int val))
            {
                return new IoTResult<int>().AddError("rfid不是有效的数字");
            }

            return new IoTResult<int>()
            {
                Value = val
            };
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="index">通道索引</param>
        /// <returns></returns>
        public IoTResult<string> Read(int index = 0)
        {
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return new IoTResult<string>(conn).ToEnd();
            }

            byte[] ReadLabelMessage1 = new byte[6] { 0x00, 0x06, 0x10, 0x22, 0x00, 0x00 };//读取1通道标签信息
            ReadLabelMessage1[3] = Convert.ToByte(ReadLabelMessage1[3] + index * 2);

            IoTResult<string> result = new IoTResult<string>();
            try
            {
                var aaa = SendPackageSingle(ReadLabelMessage1);
                if (!aaa.IsSucceed)
                    return new IoTResult<string>(aaa).ToEnd();

                var bbb = SocketRead();
                if (!bbb.IsSucceed)
                    return new IoTResult<string>(bbb).ToEnd();

                if (bbb.Value.Length <= 5 || bbb.Value[4] != 0x00)
                {
                    return new IoTResult<string>(bbb).AddError("未读取到RFID信息").ToEnd();
                }

                byte[] ThisByte = new byte[8];
                Buffer.BlockCopy(bbb.Value, 6, ThisByte, 0, 8);
                var data1 = Encoding.Default.GetString(ThisByte);
                if (data1 == "\0\0\0\0\0\0\0\0")
                    data1 = "000000";

                result.Value = data1;
            }
            catch (Exception ex)
            {
                result.IsSucceed = false;
                result.AddError(ex);
            }
            finally
            {
                if (isAutoOpen)
                    Dispose();
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 写入
        /// </summary>
        public IoTResult Write(string value, int index = 0)
        {
            IoTResult result = new IoTResult();
            if (value.Length != 8)
            {
                result.IsSucceed = false;
                result.AddError("写入字符串长度必须为8");
                return result;
            }
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return new IoTResult<string>(conn).ToEnd();
            }

            byte[] WriteLabelMessage = new byte[6] { 0x00, 0x0E, 0x40, 0x22, 0x00, 0x00 };//写标签信息
            WriteLabelMessage[3] = Convert.ToByte(WriteLabelMessage[3] + index * 2);
            byte[] senddata1 = Encoding.Default.GetBytes(value);
            byte[] getall = new byte[WriteLabelMessage.Length + senddata1.Length];
            Array.Copy(WriteLabelMessage, 0, getall, 0, WriteLabelMessage.Length);
            Array.Copy(senddata1, 0, getall, WriteLabelMessage.Length, senddata1.Length);


            try
            {
                var aaa = SendPackageSingle(getall);
                if (!aaa.IsSucceed)
                    return aaa.ToEnd();

                var bbb = SocketRead();
                if (!bbb.IsSucceed)
                    return bbb.ToEnd();

                if (bbb.Value.Length <= 5 || bbb.Value[4] != 0x00)
                {
                    return aaa.AddError("写入RFID失败").ToEnd();
                }
            }
            catch (Exception ex)
            {
                result.IsSucceed = false;
                result.AddError(ex);
            }
            finally
            {
                if (isAutoOpen)
                    Dispose();
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 设置模式
        /// </summary>
        public IoTResult SetMode()

        {
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return new IoTResult<string>(conn).ToEnd();
            }

            byte[] SetLabelType = new byte[6] { 0x00, 0x06, 0x04, 0x02, 0x33, 0x33 };
            IoTResult result = new IoTResult();
            try
            {
                var aaa = SendPackageSingle(SetLabelType);
                if (!aaa.IsSucceed)
                    return aaa.ToEnd();

                var bbb = SocketRead();
                if (!bbb.IsSucceed)
                    return bbb.ToEnd();

                if (bbb.Value.Length <= 5 || bbb.Value[4] != 0x00)
                {
                    return aaa.AddError("未读取到RFID信息").ToEnd();
                }
            }
            catch (Exception ex)
            {
                result.IsSucceed = false;
                result.AddError(ex);
            }
            finally
            {
                if (isAutoOpen)
                    Dispose();
            }
            return result.ToEnd();
        }
    }
}
