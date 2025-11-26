using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// Http服务端。某些情况下需要管理员权限运行！！
    /// </summary>
    public class HttpService : ServiceBase
    {
        /// <summary>
        /// Http监听
        /// </summary>
        public HttpListener HttpListener;

        /// <summary>
        /// 接收到信息
        /// 1.请求的数据
        /// 2.响应的数据
        /// 3.响应的文本（当ContentType可能是文本类型的时候尝试做解析）
        /// 4.返回的数据，只能是 string,byte[]
        /// </summary>
        public Func<HttpListenerRequest, HttpListenerResponse, string, object> Received;
        /// <summary>
        /// 接收过程中出现错误
        /// 1.请求的数据
        /// 2.响应的数据
        /// 3.错误
        /// 4.返回的数据，只能是 string,byte[]
        /// </summary>
        public Func<HttpListenerRequest, HttpListenerResponse, Exception, object> ReceivedException;

        /// <summary>
        /// 是否已启动
        /// </summary>
        public override bool IsOpen => HttpListener?.IsListening ?? false;

        /// <summary>
        /// HttpService 为短链接不存在 客户端
        /// </summary>
        public override ClientBase[] Clients => new ClientBase[] { };
        /// <summary>
        /// 监听所有ip的指定端口
        /// </summary>
        /// <param name="port"></param>
        public HttpService(int port)
        {
            HttpListener = new HttpListener();
            HttpListener.Prefixes.Add($"http://*:{port}/");
        }
        /// <summary>
        /// 监听指定ip的指定端口
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public HttpService(string ip, int port)
        {
            HttpListener = new HttpListener();
            HttpListener.Prefixes.Add($"http://{ip}:{port}/");
        }
        /// <summary>
        /// 监听统一资源标识符 ( URI ) 前缀
        /// </summary>
        /// <param name="uriPrefix"></param>
        public HttpService(string[] uriPrefix)
        {
            HttpListener = new HttpListener();
            if (uriPrefix != null)
            {
                foreach (var item in uriPrefix)
                {
                    HttpListener.Prefixes.Add(item);
                }
            }
        }

        Task task = null;
        /// <summary>
        /// 开始监听。某些情况下需要管理员权限运行！！
        /// </summary>
        /// <returns></returns>
        public override IoTResult Open()
        {
            try
            {
                //如果打开了，就先关闭
                if (task != null && !task.IsCompleted)
                {
                    var aClose = Close();
                    if (!aClose.IsSucceed)
                        return aClose;
                }

                HttpListener.Start();

                task = Task.Factory.StartNew(async (a) =>
                {
                    var cc = (HttpService)a;
                    while (true)
                    {
                        try
                        {
                            if (HttpListener == null)
                                break;

                            var context = await cc.HttpListener.GetContextAsync();

                            //响应默认值
                            context.Response.ContentEncoding = Encoding.UTF8;
                            context.Response.StatusCode = (int)HttpStatusCode.OK;

                            object data = null;
                            try
                            {
                                //可能是文本类型
                                string text = null;
                                if (context.Request.ContentType != null && (context.Request.ContentType.Contains("text") ||
                                    context.Request.ContentType.Contains("json") || context.Request.ContentType.Contains("xml") ||
                                    context.Request.ContentType.Contains("javascript") || context.Request.ContentType.Contains("x-www-form-urlencoded") ||
                                    context.Request.ContentType.Contains("form-data") || context.Request.ContentType.Contains("xhtml+xml")))
                                {
                                    try
                                    {
                                        using (var reader = new StreamReader(context.Request.InputStream, (context.Request.ContentEncoding ?? Encoding.UTF8)))
                                        {
                                            text = reader.ReadToEnd();
                                        }
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }

                                //执行自定义的
                                data = Received?.Invoke(context.Request, context.Response, text);
                            }
                            catch (Exception ex)
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                data = ReceivedException?.Invoke(context.Request, context.Response, ex);
                            }
                            finally
                            {
                                //处理返回的数据
                                if (data != null)
                                {
                                    if (data is string dStr)
                                    {
                                        var enco = context.Response.ContentEncoding ?? Encoding.UTF8;
                                        var Dbyte = enco.GetBytes(dStr);
                                        context.Response.ContentLength64 = Dbyte.Length;
                                        context.Response.OutputStream.Write(Dbyte, 0, Dbyte.Length);
                                    }
                                    else if (data is byte[] bBy)
                                    {
                                        context.Response.ContentLength64 = bBy.Length;
                                        context.Response.OutputStream.Write(bBy, 0, bBy.Length);
                                    }
                                }

                            }

                        }
                        catch (Exception ex)
                        {
                           
                        }
                    }
                }, this, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();

            }
            catch (Exception ex)
            {
                return new IoTResult().AddError(ex);
            }

            return new IoTResult();
        }

        /// <summary>
        /// 结束监听
        /// </summary>
        /// <returns></returns>
        public override IoTResult Close()
        {
            try
            {
                HttpListener?.Abort();
                HttpListener?.Close();
            }
            catch (Exception)
            {

            }
            finally
            {
                HttpListener = null;
                task?.Wait();
            }
            return new IoTResult();
        }

    }
}
