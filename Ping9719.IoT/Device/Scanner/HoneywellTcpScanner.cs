using Ping9719.IoT;
using Ping9719.IoT.Communication.TCP;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Scanner
{
    /// <summary>
    /// 霍尼韦尔扫码器（支持HF800，等...）
    /// 必须去下载软件“DataMax”去设置‘工作模式’为‘外部触发’；解码设置：触发超时时间设置为5000。
    /// 官网：https://sps.honeywell.com/us/en/products/productivity/barcode-scanners/fixed-mount/hf800
    /// 文档：https://prod-edam.honeywell.com/content/dam/honeywell-edam/sps/ppr/en-us/public/products/barcode-scanners/fixed-mount/hf800/sps-ppr-hf800-en-qs.pdf?download=false
    /// 下载软件：https://hsmftp.honeywell.com/
    /// </summary>
    public class HoneywellTcpScanner : SocketBase, IScannerBase
    {
        public string stateCode = "TRIGGER";//开始扫描
        public string endCode = "UNTRIG";//取消扫描

        /// <summary>
        /// 使用Tcp的方式
        /// </summary>
        public HoneywellTcpScanner(string ip, int port = 55256, int timeout = 1500)
        {
            if (socket == null)
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SetIpEndPoint(ip, port);
            this.timeout = timeout;
        }

        /// <summary>
        /// 在外部触发模式下执行一次
        /// </summary>
        /// <param name="timeout">保持时长（毫秒），填写解码设置：触发超时时间设置的值</param>
        /// <returns></returns>
        public IoTResult<string> ReadOne(int timeout)
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
                var aaa = SendPackageSingle(Encoding.UTF8.GetBytes(stateCode));
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
