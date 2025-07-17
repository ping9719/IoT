using Ping9719.IoT;
using Ping9719.IoT.Algorithm;
using Ping9719.IoT.Common;
using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Mark
{
    /// <summary>
    /// 华普激光刻印
    /// </summary>
    public class HuaPuMark
    {
        private static readonly Dictionary<byte, string> errCode = new Dictionary<byte, string>()
        {
            {1,"发现EZCAD在运行" },
            {2,"找不到EZCAD.CFG" },
            {3,"打开LMC1失败" },
            {4,"没有有效的lmc1设备" },
            {5,"lmc1版本错误" },
            {6,"找不到设备配置文件" },
            {7,"报警信号" },
            {8,"用户停止" },
            {9,"不明错误" },
            {10,"超时" },
            {11,"未初始化" },
            {12,"读文件错误" },
            {13,"窗口为空" },
            {14,"找不到指定名称的字体" },
            {15,"错误的笔号" },
            {16,"指定名称的对象不是文本对象" },
            {17,"保存文件失败" },
            {18,"找不到指定对象" },
            {19,"当前状态下不能执行此操作" },
            {31,"重码" },
            {32,"接收错误的消息" },
        };
        public ClientBase Client { get; private set; }
        public HuaPuMark(ClientBase client)
        {
            Client = client;
            //Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.ASCII;
            //Client.TimeOut = timeout;
            Client.ConnectionMode = ConnectionMode.AutoOpen;
        }

        public HuaPuMark(string ip, int port = 2000) : this(new TcpClient(ip, port)) { }

        /// <summary>
        /// 加载指定模板文件
        /// </summary>
        /// <param name="filePath">绝对路径</param>
        /// <returns></returns>
        public IoTResult Initialize(string filePath)
        {
            string comm = $"LOADFILE {filePath} \r\n";
            try
            {
                var aaa = Client.SendReceive(comm);
                if (!aaa.IsSucceed)
                    return aaa;

                return Analysis(aaa);
            }
            catch (Exception ex)
            {
                return IoTResult.Create().AddError(ex);
            }
        }

        /// <summary>
        /// 替换打印模板
        /// </summary>
        /// <param name="key">名称</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public IoTResult Data(string key, string value)
        {
            string comm = $"SETVAR {key} {value} \r\n";
            try
            {
                var aaa = Client.SendReceive(comm);
                if (!aaa.IsSucceed)
                    return aaa;

                return Analysis(aaa);
            }
            catch (Exception ex)
            {
                return IoTResult.Create().AddError(ex);
            }
        }

        /// <summary>
        /// 开始打印
        /// </summary>
        /// <param name="isAgain">是否强制打标，是不验证，否变量 0-9 是不会进行重码检验的，变量 10、11、12、13、14 会进行重码检验, 重码不打标</param>
        /// <returns></returns>
        public IoTResult MarkStart(bool isAgain = true, int timeout = 60000)
        {
            string comm = isAgain ? "RUN AGAIN \r\n" : $"RUN \r\n";
            try
            {
                var aaa = Client.SendReceive(comm, timeout);
                if (!aaa.IsSucceed)
                    return aaa;

                return Analysis(aaa);
            }
            catch (Exception ex)
            {
                return IoTResult.Create().AddError(ex);
            }
        }

        /// <summary>
        /// 停止指定设备刻印
        /// </summary>
        public IoTResult Stop()
        {
            string comm = $"STOP \r\n";
            try
            {
                var aaa = Client.SendReceive(comm);
                if (!aaa.IsSucceed)
                    return aaa;

                return Analysis(aaa);
            }
            catch (Exception ex)
            {
                return IoTResult.Create().AddError(ex);
            }
        }

        /// <summary>
        /// 解析返回结果
        /// </summary>
        /// <param name="str">返回的结果</param>
        /// <returns></returns>
        private static IoTResult<string> Analysis(IoTResult<string> str)
        {
            //if (!str.EndsWith("\r\n"))
            //    return IoTResult.Create().AddError("返回结果格式错误，结尾不是换行");

            if (byte.TryParse(str.Value.Trim(), out byte b11))
            {
                if (b11 == 0)
                    return str;
                else if (errCode.ContainsKey(b11))
                    return str.AddError(errCode[b11]);
                else
                    return str.AddError($"返回结果格式错误，未知的错误代码[{b11}]");
            }
            else
                return str.AddError($"返回结果格式错误，不是有效的数字[{str.Value.Trim()}]");
        }
    }
}
