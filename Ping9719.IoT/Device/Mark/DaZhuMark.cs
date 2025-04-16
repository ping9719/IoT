using Ping9719.IoT;
using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Ping9719.IoT.Device.Mark
{
    /// <summary>
    /// 大族激光刻印
    /// </summary>
    public class DaZhuMark
    {
        public ClientBase Client { get; private set; }
        public DaZhuMark(ClientBase client, int timeout = 60000)
        {
            Client = client;
            Client.ReceiveMode = ReceiveMode.ParseTime();
            Client.Encoding = Encoding.UTF8;
            Client.TimeOut = timeout;
            Client.ConnectionMode = ConnectionMode.AutoOpen;
        }

        public DaZhuMark(string ip, int port = 9001) : this(new TcpClient(ip, port)) { }

        /// <summary>
        /// 加载指定模板
        /// </summary>
        /// <param name="name">模板名称</param>
        /// <param name="id">卡号</param>
        /// <param name="isClose">是否关闭在重新打开</param>
        /// <returns>可替换的文本数</returns>
        public IoTResult<int> Initialize(string name, string id, bool isClose = false)
        {
            string comm = $"<Initialize,{id},{(isClose ? 1 : 0)},{name}>";

            var result = new IoTResult<int>();
            try
            {
                var aaa = Client.SendReceive(comm);
                if (!aaa.IsSucceed)
                    return aaa.ToVal<int>().ToEnd();

                var bbb = Analysis(aaa.Value);
                if (!bbb.IsSucceed)
                    return bbb.ToVal<int>().ToEnd();

                result.Value = Convert.ToInt32(bbb.Value.FirstOrDefault());
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 设置偏移（在加载指定模板后使用）
        /// </summary>
        /// <param name="id">卡号</param>
        /// <param name="type">1相对位置 2绝对位置</param>
        /// <param name="x">x偏移</param>
        /// <param name="y">y偏移</param>
        /// <param name="a">旋转偏移</param>
        /// <returns></returns>
        public IoTResult Offset(string id, int type, float x, float y, float a)
        {
            string comm = $"<Offset,{id},{type},{x},{y},{a}>";
            var result = new IoTResult<string[]>();
            try
            {
                var aaa = Client.SendReceive(comm);
                if (!aaa.IsSucceed)
                    return aaa.ToEnd();

                var bbb = Analysis(aaa.Value);
                if (!bbb.IsSucceed)
                    return bbb.ToEnd();

                result.Value = bbb.Value;
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 卸载所有模板
        /// </summary>
        /// <returns></returns>
        public IoTResult Uninstall()
        {
            string comm = $"<Uninstall>";

            var result = new IoTResult();
            try
            {
                var aaa = Client.SendReceive(comm);
                if (!aaa.IsSucceed)
                    return aaa.ToEnd();

                var bbb = Analysis(aaa.Value);
                if (!bbb.IsSucceed)
                    return bbb.ToEnd();
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 得到所有卡号
        /// </summary>
        /// <returns>卡号</returns>
        public IoTResult<string[]> GetCard()
        {
            string comm = $"<GetCard>";

            var result = new IoTResult<string[]>();
            try
            {
                var aaa = Client.SendReceive(comm);
                if (!aaa.IsSucceed)
                    return aaa.ToVal<string[]>().ToEnd();

                var bbb = Analysis(aaa.Value);
                if (!bbb.IsSucceed)
                    return bbb.ToVal<string[]>().ToEnd();

                result.Value = bbb.Value;
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 替换打印模板
        /// </summary>
        /// <param name="id"></param>
        /// <param name="key">名称</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public IoTResult Data(string key, string value, string id)
        {
            string comm = $"<Data,{id},{key},{value}>";
            var result = new IoTResult<string[]>();
            try
            {
                var aaa = Client.SendReceive(comm);
                if (!aaa.IsSucceed)
                    return aaa.ToEnd();

                var bbb = Analysis(aaa.Value);
                if (!bbb.IsSucceed)
                    return bbb.ToEnd();

                result.Value = bbb.Value;
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 开始打印
        /// </summary>
        /// <returns>打印时间（秒）</returns>
        public IoTResult<double> MarkStart(params string[] id)
        {
            string comm = $"<MarkStart,{string.Join(",", id)}>";
            var result = new IoTResult<double>();
            try
            {
                var aaa = Client.SendReceive(comm);
                if (!aaa.IsSucceed)
                    return aaa.ToVal<double>().ToEnd();

                var bbb = Analysis(aaa.Value);
                if (!bbb.IsSucceed)
                    return bbb.ToVal<double>().ToEnd();

                result.Value = Convert.ToDouble(bbb.Value.FirstOrDefault());
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 红光预览
        /// </summary>
        /// <returns>预览时间（秒）</returns>
        public IoTResult<double> RedStart(params string[] id)
        {
            string comm = $"<RedStart,{string.Join(",", id)}>";
            var result = new IoTResult<double>();
            try
            {
                var aaa = Client.SendReceive(comm);
                if (!aaa.IsSucceed)
                    return aaa.ToVal<double>().ToEnd();

                var bbb = Analysis(aaa.Value);
                if (!bbb.IsSucceed)
                    return bbb.ToVal<double>().ToEnd();

                result.Value = Convert.ToDouble(bbb.Value.FirstOrDefault());
            }
            catch (Exception ex)
            {

                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 停止指定设备刻印
        /// </summary>
        public IoTResult Stop(params string[] id)
        {
            string comm = $"<Stop,{string.Join(",", id)}>";
            var result = new IoTResult();
            try
            {
                var aaa = Client.SendReceive(comm);
                if (!aaa.IsSucceed)
                    return aaa.ToEnd();

                var bbb = Analysis(aaa.Value);
                if (!bbb.IsSucceed)
                    return bbb.ToEnd();
            }
            catch (Exception ex)
            {

                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 停止所有设备刻印
        /// </summary>
        public IoTResult StopAll()
        {
            string comm = $"<Stop>";
            var result = new IoTResult();
            try
            {
                var aaa = Client.SendReceive(comm);
                if (!aaa.IsSucceed)
                    return aaa.ToEnd();

                var bbb = Analysis(aaa.Value);
                if (!bbb.IsSucceed)
                    return bbb.ToEnd();
            }
            catch (Exception ex)
            {

                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 查询指定设备状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns>值：Run/Ready</returns>
        public IoTResult<string> State(string id)
        {
            string comm = $"<State,{id}>";
            var result = new IoTResult<string>();
            try
            {
                var aaa = Client.SendReceive(comm);
                if (!aaa.IsSucceed)
                    return aaa.ToVal<string>().ToEnd();

                var bbb = Analysis(aaa.Value);
                if (!bbb.IsSucceed)
                    return bbb.ToVal<string>().ToEnd();

                result.Value = bbb.Value.FirstOrDefault();
            }
            catch (Exception ex)
            {

                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 查询所有设备状态（只有一个在打标就是Run）
        /// </summary>
        /// <returns>值：Run/Ready</returns>
        public IoTResult<string> State()
        {
            string comm = $"<State>";
            var result = new IoTResult<string>();
            try
            {
                var aaa = Client.SendReceive(comm);
                if (!aaa.IsSucceed)
                    return aaa.ToVal<string>().ToEnd();

                var bbb = Analysis(aaa.Value);
                if (!bbb.IsSucceed)
                    return bbb.ToVal<string>().ToEnd();

                result.Value = bbb.Value.FirstOrDefault();
            }
            catch (Exception ex)
            {

                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 解析返回结果
        /// </summary>
        /// <param name="str">返回的结果</param>
        /// <returns></returns>
        private static IoTResult<string[]> Analysis(string str)
        {
            IoTResult<string[]> result = new IoTResult<string[]>();
            if (str.StartsWith("<") && str.EndsWith(">"))
            {
                var con = str.Substring(1, str.Length - 2).Split(',');
                if (con.Length > 0 && con[0] == "OK")
                    result.Value = con.Skip(1).ToArray();
                else if (con.Length > 0 && con[0] == "NG")
                    result.AddError(string.Join(",", con.Skip(1)));
                else
                    result.AddError($"不是有效的格式【{con}】");
            }
            else
            {
                result.IsSucceed = false;
            }
            return result;
        }

    }
}
