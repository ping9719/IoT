using Ping9719.IoT;
using Ping9719.IoT.Common;
using Ping9719.IoT.Communication.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Scanner
{
    /// <summary>
    /// 民德扫码枪 使用 透传工具232-网口 后通过网口发送指令 
    /// 恢复出厂设置 16 4D 0D 25 25 25 44 45 46 2E
    /// 设置为主机模式 16 4D 0D 30 34 30 31 44 30 35 2E
    /// </summary>
    public class MindeoTcpScanner : SocketBase, IScannerBase
    {
        /// <summary>
        /// 使用Tcp的方式 透传工具用作TCPServer 上位机软件作TCPClient
        /// </summary>
        public MindeoTcpScanner(string ip, int port = 8899, int timeout = 1500)
        {
            if (socket == null)
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SetIpEndPoint(ip, port);
            this.timeout = timeout;
        }

        /// <summary>
        /// 在主机模式下执行一次
        /// </summary>
        /// <param name="timeout">保持时长（毫秒），填写解码设置：触发超时时间设置的值</param>
        /// <returns></returns>
        public IoTResult<string> ReadOne(int timeout = 1200)
        {
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return new IoTResult<string>(conn).ToEnd();
            }

            socket.ReceiveTimeout = timeout;
            socket.SendTimeout = timeout;
            var result = new IoTResult<string>();
            try
            {
                //相当与发送164D0D16540D2E指令一次
                var aaa = SendPackageSingle(new byte[] { 0x16, 0x4D, 0x0D, 0x16, 0x54, 0x0D, 0x2E });
                if (!aaa.IsSucceed)
                    return new IoTResult<string>(aaa).ToEnd();

                result.Value = Encoding.UTF8.GetString(aaa.Value);
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
