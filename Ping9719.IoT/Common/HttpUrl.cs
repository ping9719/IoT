using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Common
{
    /// <summary>
    /// 对Url的解析
    /// </summary>
    public static class HttpUrl
    {
        /// <summary>
        /// 追加路径片段
        /// </summary>
        /// <param name="url">地址，如 https://www.baidu.com</param>
        /// <param name="segments">路径片段，如 abc/123</param>
        /// <returns>地址</returns>
        public static string AppendPathSegments(this string url, IEnumerable<object> segments)
        {
            string urlStr = url.Trim();
            foreach (var segment in segments)
            {
                var val = segment?.ToString()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(val))
                    continue;

                if (urlStr.EndsWith("/"))
                    urlStr = urlStr.Substring(0, urlStr.Length - 1);

                if (val.Length > 0)
                    urlStr += string.IsNullOrEmpty(urlStr) || val.StartsWith("/") ? val : $"/{val}";
            }
            return urlStr;
        }

        /// <summary>
        /// 设置Query参数
        /// </summary>
        /// <param name="url">地址，如 https://www.baidu.com/s</param>
        /// <param name="values">参数，支持字典和对象</param>
        /// <returns>地址</returns>
        public static string SetQueryParams(this string url, object values)
        {
            if (values == null)
                return url;

            List<string> kv = new List<string>();
            if (values is IEnumerable jh)
            {
                if (jh is IDictionary dict)
                {
                    foreach (DictionaryEntry item in dict)
                        kv.Add($"{item.Key?.ToString()}={item.Value?.ToString()}");
                }
            }
            else
            {
                foreach (var item in values.GetType().GetProperties())
                {
                    if (item.CanRead)
                        kv.Add($"{item.Name}={item.GetValue(values)?.ToString()}");
                }
            }

            string urlStr = url;
            if (kv.Any())
            {
                if (!urlStr.Contains("?"))
                    urlStr += "?";
                else
                {
                    if (!urlStr.EndsWith("&"))
                        urlStr += "&";
                }

                urlStr += string.Join("&", kv);
            }

            return urlStr;
        }
    }
}
