using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Common
{
    /// <summary>
    /// json解析接口
    /// </summary>
    public interface IJsonParse
    {
        /// <summary>
        /// 将字符串解析为json对象
        /// </summary>
        T DeserializeObject<T>(string json);
        /// <summary>
        /// 将对象解析为json字符串
        /// </summary>
        string SerializeObject(object obj);
    }

    /// <summary>
    /// json解析
    /// </summary>
    public class JsonParse
    {
        /// <summary>
        /// json解析接口
        /// </summary>
        public static IJsonParse UseJsonParse = null;

        protected internal static T DeserializeObject<T>(string json)
        {
            if (UseJsonParse != null)
                return UseJsonParse.DeserializeObject<T>(json);
            else
            {
#if NET8_0_OR_GREATER
                return System.Text.Json.JsonSerializer.Deserialize<T>(json);
#else
                System.Runtime.Serialization.Json.DataContractJsonSerializer jsonFormator = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
                using (Stream readStream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    return (T)jsonFormator.ReadObject(readStream);
                }
#endif
            }
        }

        protected internal static string SerializeObject(object obj)
        {
            if (UseJsonParse != null)
                return UseJsonParse.SerializeObject(obj);
            else
            {
#if NET8_0_OR_GREATER
                return System.Text.Json.JsonSerializer.Serialize(obj);
#else
            System.Runtime.Serialization.Json.DataContractJsonSerializer jsonFormator = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                jsonFormator.WriteObject(stream, obj);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
#endif
            }
        }
    }

}
