using Ping9719.IoT.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication
{
    /// <summary>
    /// Http 客户端
    /// </summary>
    public class HttpClient : ClientBase
    {
        static HttpClient Default_ = null;
        /// <summary>
        /// 获取默认的HttpClient实例
        /// </summary>
        public static HttpClient Default => Default_ == null ? Default_ = new HttpClient() : Default_;

        public System.Net.Http.HttpClient httpClient;

        public Action<ApiHelpRequestMessage> ReceivedHttp;
        public HttpClient()
        {
            TimeOut = 5000;

            httpClient = new System.Net.Http.HttpClient();
            httpClient.Timeout = TimeSpan.FromMilliseconds(TimeOut);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Chrome/132.0.0.0 Safari/537.36");
            //ServicePointManager.Expect100Continue = false;

            ConnectionMode = ConnectionMode.Manual;
        }

        /// <summary>
        /// GET请求
        /// </summary>
        /// <typeparam name="T">类型。可以为string和对象</typeparam>
        /// <param name="uri">地址</param>
        /// <param name="query">请求头。URI中的查询信息</param>
        /// <param name="body">请求体。转为json字符串形式</param>
        /// <returns>成功返回对象，成功但StatusCode不在200-299返回默认值</returns>
        public IoTResult<T> Get<T>(string uri, object query = null, object body = null) => Send<T>(System.Net.Http.HttpMethod.Get, uri, query, body);

        /// <summary>
        /// GET请求
        /// </summary>
        /// <typeparam name="T">类型。可以为string和对象</typeparam>
        /// <param name="uri">多个地址。如["https://0.0.0.1","/User/Login"]</param>
        /// <param name="query">请求头。URI中的查询信息</param>
        /// <param name="body">请求体。转为json字符串形式</param>
        /// <returns>成功返回对象，成功但StatusCode不在200-299返回默认值</returns>
        public IoTResult<T> Get<T>(IEnumerable<object> uri, object query = null, object body = null) => Send<T>(System.Net.Http.HttpMethod.Get, uri, query, body);

        /// <summary>
        /// POST请求
        /// </summary>
        /// <typeparam name="T">类型。可以为string和对象</typeparam>
        /// <param name="uri">地址</param>
        /// <param name="query">请求头。URI中的查询信息</param>
        /// <param name="body">请求体。转为json字符串形式</param>
        /// <returns>成功返回对象，成功但StatusCode不在200-299返回默认值</returns>
        public IoTResult<T> Post<T>(string uri, object body = null, object query = null) => Send<T>(System.Net.Http.HttpMethod.Post, uri, query, body);

        /// <summary>
        /// POST请求
        /// </summary>
        /// <typeparam name="T">类型。可以为string和对象</typeparam>
        /// <param name="uri">多个地址。如["https://0.0.0.1","/User/Login"]</param>
        /// <param name="query">请求头。URI中的查询信息</param>
        /// <param name="body">请求体。转为json字符串形式</param>
        /// <returns>成功返回对象，成功但StatusCode不在200-299返回默认值</returns>
        public IoTResult<T> Post<T>(IEnumerable<object> uri, object body = null, object query = null) => Send<T>(System.Net.Http.HttpMethod.Post, uri, query, body);

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <typeparam name="T">类型。可以为string和对象</typeparam>
        /// <param name="uri">地址</param>
        /// <param name="query">请求头。URI中的查询信息</param>
        /// <param name="body">请求体。转为json字符串形式</param>
        /// <returns>成功返回对象，成功但StatusCode不在200-299返回默认值</returns>
        public IoTResult<T> Send<T>(System.Net.Http.HttpMethod method, string uri, object query = null, object body = null) => Send<T>(method, new string[] { uri }, query, body);

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <typeparam name="T">类型。可以为string和对象</typeparam>
        /// <param name="method">请求方式。</param>
        /// <param name="uri">多个地址。如["https://0.0.0.1","/User/Login"]</param>
        /// <param name="query">请求头。URI中的查询信息</param>
        /// <param name="body">请求体。转为json字符串形式</param>
        /// <param name="mediaType">媒体类型。默认为‘application/json’会调用自带的转换器，如果是自定义只接受body为字符串的类型</param>
        /// <returns>成功返回对象，成功但StatusCode不在200-299返回默认值</returns>
        public IoTResult<T> Send<T>(System.Net.Http.HttpMethod method, IEnumerable<object> uri, object query = null, object body = null, string mediaType = "application/json")
        {
            var result = IoTResult.Create<T>();
            try
            {
                System.Net.Http.HttpRequestMessage httpRequestMessage = new System.Net.Http.HttpRequestMessage(method, "".AppendPathSegments(uri).SetQueryParams(query));

                string myContent = string.Empty;
                if (body != null)
                {
                    if (mediaType == "application/json")
                    {
                        myContent = JsonParse.SerializeObject(body);
                        httpRequestMessage.Content = new System.Net.Http.StringContent(myContent, Encoding.UTF8, mediaType);
                    }
                    else
                    {
                        httpRequestMessage.Content = new System.Net.Http.StringContent(body.ToString(), Encoding.UTF8, mediaType);
                    }
                }

                var re = httpClient.SendAsync(httpRequestMessage).Result;
                if (!re.IsSuccessStatusCode)
                {
                    ReceivedHttp?.Invoke(new ApiHelpRequestMessage()
                    {
                        Method = httpRequestMessage.Method?.Method ?? "",
                        Uri = httpRequestMessage.RequestUri?.ToString() ?? "",
                        Body = myContent ?? "",
                        StatusCode = (int)re.StatusCode,
                        Content = "",
                    });
                    return result.AddError(re.StatusCode.ToString()).ToEnd();
                }

                var con = re.Content.ReadAsStringAsync().Result;

                ReceivedHttp?.Invoke(new ApiHelpRequestMessage()
                {
                    Method = httpRequestMessage.Method?.Method ?? "",
                    Uri = httpRequestMessage.RequestUri?.ToString() ?? "",
                    Body = myContent ?? "",
                    StatusCode = (int)re.StatusCode,
                    Content = con,
                });

                var tType = typeof(T);
                if (tType == typeof(string))
                    result.Value = (T)(object)con;
                else
                    result.Value = JsonParse.DeserializeObject<T>(con);
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }
        protected override OpenClientData Open2()
        {
            throw new NotImplementedException();
        }
    }

    public class ApiHelpRequestMessage
    {
        public string Method { get; set; }
        public string Uri { get; set; }
        public string Body { get; set; }
        public int StatusCode { get; set; }
        public string Content { get; set; }

        public override string ToString() => $"[{Method}][{Uri}]{Environment.NewLine}Body[{Body}]{Environment.NewLine}StatusCode[{StatusCode}]{Environment.NewLine}Content[{Content}]{Environment.NewLine}";
    }
}
