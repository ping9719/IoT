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
    /// 泰和森Rfid
    /// </summary>
    public class TaiHeSenRfid : SocketBase
    {
        public Encoding encoding = Encoding.ASCII;

        /// <summary>
        /// 使用Tcp的方式
        /// </summary>
        public TaiHeSenRfid(string ip, int port = 4000, int timeout = 1500)
        {
            if (socket == null)
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SetIpEndPoint(ip, port);
            this.timeout = timeout;
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <returns></returns>
        public IoTResult<string> Read()
        {
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return new IoTResult<string>(conn).ToEnd();
            }

            byte[] ReadLabelMessage = new byte[7] { 0xAA, 0xFF, 0x22, 0x00, 0x00, 0x21, 0xBB };//发送读值指令，固定编码格式
            IoTResult<string> result = new IoTResult<string>();
            try
            {
                var retValue_Send = SendPackageSingle(ReadLabelMessage);
                if (!retValue_Send.IsSucceed)
                    return new IoTResult<string>(retValue_Send).ToEnd();

                if (retValue_Send.Value.Length <= 8 || retValue_Send.Value[2] == 0xFF)  //根据ths协议，读取失败有固定的编码格式
                {
                    return new IoTResult<string>(retValue_Send).AddError("读取失败，未读取到RFID信息").ToEnd();
                }

                int datalength = retValue_Send.Value[4] - 5;//实际数据长度
                byte[] ThisByte = retValue_Send.Value.Skip(8).Take(datalength).ToArray();
                var aaa = encoding.GetString(ThisByte, 0, ThisByte.Length);
                result.Value = aaa;
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
        public IoTResult Write(string value)
        {
            IoTResult result = new IoTResult();
            if (value.Length > 12 || value.Length < 2)
            {
                result.IsSucceed = false;
                result.AddError("写入字符串长度必须为2-12字");
                return result;
            }
            if (value.Length % 2 != 0)
            {
                result.IsSucceed = false;
                result.AddError("写入字符串长度必须为偶数");
                return result;
            }

            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return new IoTResult<string>(conn).ToEnd();
            }
            var sendByte = encoding.GetBytes(value);

            //组合成发送指令码
            List<byte> WriteLabelMessage = new List<byte>
            {
                0xAA, 0xFF, 0x51, //命令
                0x00, (byte)(11+sendByte.Length), //长度
                0x00, 0x00, 0x00, 0x00,//密码
                0x01, //EPC
                0x00, 0x01, 0x00, (byte)(sendByte.Length/2+1),//偏移地址+数据长度
                (byte)(sendByte.Length*4), 0x00, //位数
            };  //组合标签信息

            WriteLabelMessage.AddRange(sendByte);
            var aaa = (byte)WriteLabelMessage.Skip(1).Sum(o => o);//检验吗
            WriteLabelMessage.Add(aaa);
            WriteLabelMessage.Add(0xBB);

            try
            {
                var retValue = SendPackageSingle(WriteLabelMessage.ToArray());
                if (!retValue.IsSucceed)
                    return retValue.ToEnd();

                if (retValue.Value.Length <= 8 || retValue.Value[2] == 0xFF)  //根据ths协议，写失败有固定的编码格式
                {
                    return new IoTResult<string>(retValue).AddError("写入失败").ToEnd();
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
