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
#if NETSTANDARD2_0_OR_GREATER || NET8_0_OR_GREATER

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
        public T Get<T>(string uri, object query = null, object body = null) => Send<T>(System.Net.Http.HttpMethod.Get, uri, query, body);

        /// <summary>
        /// GET请求
        /// </summary>
        /// <typeparam name="T">类型。可以为string和对象</typeparam>
        /// <param name="uri">多个地址。如["https://0.0.0.1","/User/Login"]</param>
        /// <param name="query">请求头。URI中的查询信息</param>
        /// <param name="body">请求体。转为json字符串形式</param>
        /// <returns>成功返回对象，成功但StatusCode不在200-299返回默认值</returns>
        public T Get<T>(IEnumerable<object> uri, object query = null, object body = null) => Send<T>(System.Net.Http.HttpMethod.Get, uri, query, body);

        /// <summary>
        /// POST请求
        /// </summary>
        /// <typeparam name="T">类型。可以为string和对象</typeparam>
        /// <param name="uri">地址</param>
        /// <param name="query">请求头。URI中的查询信息</param>
        /// <param name="body">请求体。转为json字符串形式</param>
        /// <returns>成功返回对象，成功但StatusCode不在200-299返回默认值</returns>
        public T Post<T>(string uri, object body = null, object query = null) => Send<T>(System.Net.Http.HttpMethod.Post, uri, query, body);

        /// <summary>
        /// POST请求
        /// </summary>
        /// <typeparam name="T">类型。可以为string和对象</typeparam>
        /// <param name="uri">多个地址。如["https://0.0.0.1","/User/Login"]</param>
        /// <param name="query">请求头。URI中的查询信息</param>
        /// <param name="body">请求体。转为json字符串形式</param>
        /// <returns>成功返回对象，成功但StatusCode不在200-299返回默认值</returns>
        public T Post<T>(IEnumerable<object> uri, object body = null, object query = null) => Send<T>(System.Net.Http.HttpMethod.Post, uri, query, body);

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <typeparam name="T">类型。可以为string和对象</typeparam>
        /// <param name="uri">地址</param>
        /// <param name="query">请求头。URI中的查询信息</param>
        /// <param name="body">请求体。转为json字符串形式</param>
        /// <returns>成功返回对象，成功但StatusCode不在200-299返回默认值</returns>
        public T Send<T>(System.Net.Http.HttpMethod method, string uri, object query = null, object body = null) => Send<T>(method, new string[] { uri }, query, body);

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <typeparam name="T">类型。可以为string和对象</typeparam>
        /// <param name="uri">多个地址。如["https://0.0.0.1","/User/Login"]</param>
        /// <param name="query">请求头。URI中的查询信息</param>
        /// <param name="body">请求体。转为json字符串形式</param>
        /// <returns>成功返回对象，成功但StatusCode不在200-299返回默认值</returns>
        public T Send<T>(System.Net.Http.HttpMethod method, IEnumerable<object> uri, object query = null, object body = null)
        {
            System.Net.Http.HttpRequestMessage httpRequestMessage = new System.Net.Http.HttpRequestMessage(method, "".AppendPathSegments(uri).SetQueryParams(query));

            string jsonContent = string.Empty;
            if (body != null)
            {
                jsonContent = JsonParse.SerializeObject(body);
                var content = new System.Net.Http.StringContent(jsonContent, Encoding.UTF8, "application/json");
                httpRequestMessage.Content = content;
            }

            var re = httpClient.SendAsync(httpRequestMessage).Result;
            if (!re.IsSuccessStatusCode)
                return default(T)!;

            var con = re.Content.ReadAsStringAsync().Result;

            ReceivedHttp?.Invoke(new ApiHelpRequestMessage()
            {
                Method = httpRequestMessage.Method?.Method ?? "",
                Uri = httpRequestMessage.RequestUri?.ToString() ?? "",
                Body = jsonContent ?? "",
                Content = con,
            });

            var tType = typeof(T);
            if (tType == typeof(string))
                return (T)(object)con;

            return JsonParse.DeserializeObject<T>(con)!;
        }

#endif
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
        public string Content { get; set; }

        public override string ToString() => $"[{Method}][{Uri}]{Environment.NewLine}Body[{Body}]{Environment.NewLine}Content[{Content}]{Environment.NewLine}";
    }
}
