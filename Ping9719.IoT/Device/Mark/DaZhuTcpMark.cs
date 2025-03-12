﻿using Ping9719.IoT;
using Ping9719.IoT.Communication.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Mark
{
    /// <summary>
    /// 大族激光刻印
    /// </summary>
    public class DaZhuTcpMark : SocketBase
    {
        /// <summary>
        /// 使用Tcp的方式
        /// </summary>
        public DaZhuTcpMark(string ip, int port = 9001, int timeout = 60000)
        {
            if (socket == null)
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SetIpEndPoint(ip, port);
            this.timeout = timeout;
        }

        /// <summary>
        /// 加载指定模板
        /// </summary>
        /// <param name="name">模板名称</param>
        /// <param name="id">卡号</param>
        /// <param name="isClose">是否关闭在重新打开</param>
        /// <returns>可替换的文本数</returns>
        public IoTResult<int> Initialize(string name, string id, bool isClose = false)
        {
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return new IoTResult<int>(conn).ToEnd();
            }

            string comm = $"<Initialize,{id},{(isClose ? 1 : 0)},{name}>";

            var result = new IoTResult<int>();
            try
            {
                var aaa = SendPackageSingle(Encoding.UTF8.GetBytes(comm));
                if (!aaa.IsSucceed)
                    return new IoTResult<int>(aaa).ToEnd();

                var bbb = Analysis(Encoding.UTF8.GetString(aaa.Value));
                if (!bbb.IsSucceed)
                    return new IoTResult<int>(bbb).ToEnd();

                result.Value = Convert.ToInt32(bbb.Value.FirstOrDefault());
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
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return new IoTResult<string[]>(conn).ToEnd();
            }

            string comm = $"<Offset,{id},{type},{x},{y},{a}>";
            var result = new IoTResult<string[]>();
            try
            {
                var aaa = SendPackageSingle(Encoding.UTF8.GetBytes(comm));
                if (!aaa.IsSucceed)
                    return new IoTResult<string[]>(aaa).ToEnd();

                var bbb = Analysis(Encoding.UTF8.GetString(aaa.Value));
                if (!bbb.IsSucceed)
                    return new IoTResult<string[]>(bbb).ToEnd();

                result.Value = bbb.Value;
            }
            catch (Exception ex)
            {
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
        /// 卸载所有模板
        /// </summary>
        /// <returns></returns>
        public IoTResult Uninstall()
        {
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return conn.ToEnd();
            }

            string comm = $"<Uninstall>";

            var result = new IoTResult();
            try
            {
                var aaa = SendPackageSingle(Encoding.UTF8.GetBytes(comm));
                if (!aaa.IsSucceed)
                    return aaa.ToEnd();

                var bbb = Analysis(Encoding.UTF8.GetString(aaa.Value));
                if (!bbb.IsSucceed)
                    return aaa.ToEnd();
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
        /// 得到所有卡号
        /// </summary>
        /// <returns>卡号</returns>
        public IoTResult<string[]> GetCard()
        {
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return new IoTResult<string[]>(conn).ToEnd();
            }

            string comm = $"<GetCard>";

            var result = new IoTResult<string[]>();
            try
            {
                var aaa = SendPackageSingle(Encoding.UTF8.GetBytes(comm));
                if (!aaa.IsSucceed)
                    return new IoTResult<string[]>(aaa).ToEnd();

                var bbb = Analysis(Encoding.UTF8.GetString(aaa.Value));
                if (!bbb.IsSucceed)
                    return new IoTResult<string[]>(bbb).ToEnd();

                result.Value = bbb.Value;
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
        /// 替换打印模板
        /// </summary>
        /// <param name="id"></param>
        /// <param name="key">名称</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public IoTResult Data(string key, string value, string id)
        {
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return new IoTResult<string[]>(conn).ToEnd();
            }

            string comm = $"<Data,{id},{key},{value}>";
            var result = new IoTResult<string[]>();
            try
            {
                var aaa = SendPackageSingle(Encoding.UTF8.GetBytes(comm));
                if (!aaa.IsSucceed)
                    return new IoTResult<string[]>(aaa).ToEnd();

                var bbb = Analysis(Encoding.UTF8.GetString(aaa.Value));
                if (!bbb.IsSucceed)
                    return new IoTResult<string[]>(bbb).ToEnd();

                result.Value = bbb.Value;
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
        /// 开始打印
        /// </summary>
        /// <returns>打印时间（秒）</returns>
        public IoTResult<double> MarkStart(params string[] id)
        {
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return new IoTResult<double>(conn).ToEnd();
            }

            string comm = $"<MarkStart,{string.Join(",", id)}>";
            var result = new IoTResult<double>();
            try
            {
                var aaa = SendPackageSingle(Encoding.UTF8.GetBytes(comm));
                if (!aaa.IsSucceed)
                    return new IoTResult<double>(aaa).ToEnd();

                var bbb = Analysis(Encoding.UTF8.GetString(aaa.Value));
                if (!bbb.IsSucceed)
                    return new IoTResult<double>(bbb).ToEnd();

                result.Value = Convert.ToDouble(bbb.Value.FirstOrDefault());
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
        /// 红光预览
        /// </summary>
        /// <returns>预览时间（秒）</returns>
        public IoTResult<double> RedStart(params string[] id)
        {
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return new IoTResult<double>(conn).ToEnd();
            }

            string comm = $"<RedStart,{string.Join(",", id)}>";
            var result = new IoTResult<double>();
            try
            {
                var aaa = SendPackageSingle(Encoding.UTF8.GetBytes(comm));
                if (!aaa.IsSucceed)
                    return new IoTResult<double>(aaa).ToEnd();

                var bbb = Analysis(Encoding.UTF8.GetString(aaa.Value));
                if (!bbb.IsSucceed)
                    return new IoTResult<double>(bbb).ToEnd();

                result.Value = Convert.ToDouble(bbb.Value.FirstOrDefault());
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
        /// 停止指定设备刻印
        /// </summary>
        public IoTResult Stop(params string[] id)
        {
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return new IoTResult<string[]>(conn).ToEnd();
            }

            string comm = $"<Stop,{string.Join(",", id)}>";
            var result = new IoTResult();
            try
            {
                var aaa = SendPackageSingle(Encoding.UTF8.GetBytes(comm));
                if (!aaa.IsSucceed)
                    return aaa.ToEnd();

                var bbb = Analysis(Encoding.UTF8.GetString(aaa.Value));
                if (!bbb.IsSucceed)
                    return bbb.ToEnd();
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
        /// 停止所有设备刻印
        /// </summary>
        public IoTResult StopAll()
        {
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return conn.ToEnd();
            }

            string comm = $"<Stop>";
            var result = new IoTResult();
            try
            {
                var aaa = SendPackageSingle(Encoding.UTF8.GetBytes(comm));
                if (!aaa.IsSucceed)
                    return aaa.ToEnd();

                var bbb = Analysis(Encoding.UTF8.GetString(aaa.Value));
                if (!bbb.IsSucceed)
                    return bbb.ToEnd();
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
        /// 查询指定设备状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns>值：Run/Ready</returns>
        public IoTResult<string> State(string id)
        {
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return new IoTResult<string>(conn).ToEnd();
            }

            string comm = $"<State,{id}>";
            var result = new IoTResult<string>();
            try
            {
                var aaa = SendPackageSingle(Encoding.UTF8.GetBytes(comm));
                if (!aaa.IsSucceed)
                    return new IoTResult<string>(aaa).ToEnd();

                var bbb = Analysis(Encoding.UTF8.GetString(aaa.Value));
                if (!bbb.IsSucceed)
                    return new IoTResult<string>(bbb).ToEnd();

                result.Value = bbb.Value.FirstOrDefault();
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
        /// 查询所有设备状态（只有一个在打标就是Run）
        /// </summary>
        /// <returns>值：Run/Ready</returns>
        public IoTResult<string> State()
        {
            if (isAutoOpen)
            {
                var conn = Connect();
                if (!conn.IsSucceed)
                    return new IoTResult<string>(conn).ToEnd();
            }

            string comm = $"<State>";
            var result = new IoTResult<string>();
            try
            {
                var aaa = SendPackageSingle(Encoding.UTF8.GetBytes(comm));
                if (!aaa.IsSucceed)
                    return new IoTResult<string>(aaa).ToEnd();

                var bbb = Analysis(Encoding.UTF8.GetString(aaa.Value));
                if (!bbb.IsSucceed)
                    return new IoTResult<string>(bbb).ToEnd();

                result.Value = bbb.Value.FirstOrDefault();
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
        /// 解析返回结果
        /// </summary>
        /// <param name="str">返回的结果</param>
        /// <returns></returns>
        private IoTResult<string[]> Analysis(string str)
        {
            IoTResult<string[]> result = new IoTResult<string[]>();
            if (str.StartsWith("<") && str.EndsWith(">"))
            {
                var con = str.Substring(1, str.Length - 2).Split(',');
                if (con.Length > 0 && con[0] == "OK")
                {
                    result.Value = con.Skip(1).ToArray();
                }
                else if (con.Length > 0 && con[0] == "NG")
                {
                    result.IsSucceed = false;
                    result.AddError( string.Join(",", con.Skip(1)));
                }
                else
                {
                    result.IsSucceed = false;
                    result.AddError($"不是有效的格式【{con}】");
                }
            }
            else
            {
                result.IsSucceed = false;
            }
            return result;
        }

    }
}
