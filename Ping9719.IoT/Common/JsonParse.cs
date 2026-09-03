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
        /// <summary>
        /// 将字符串解析为json对象的函数
        /// </summary>
        public static Func<string, object> DeserializeFunc = null;
        /// <summary>
        /// 将对象解析为json字符串的函数
        /// </summary>
        public static Func<object, string> SerializeFunc = null;

#if NET8_0_OR_GREATER
        public static readonly System.Text.Json.JsonSerializerOptions JsonSerializerOptions = new System.Text.Json.JsonSerializerOptions()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,//解决中文转义问题
                                                                                            //WriteIndented = false,//格式化输出
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,//不忽略null值
            PropertyNamingPolicy = null,//属性命名策略，保持原样
            PropertyNameCaseInsensitive = true,//忽略属性名称大小写

            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,//允许将字符串 "123" 反序列化为 int 类型
            ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,//跳过注释
            AllowTrailingCommas = true,//允许结尾多余的逗号
        };
#else
        private static class NewtonsoftJsonHelper
        {
            private static readonly System.Reflection.MethodInfo _deserializeMethod;
            private static readonly System.Reflection.MethodInfo _serializeMethod;

            static NewtonsoftJsonHelper()
            {
                Type jsonConvertType = Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json", false);
                if (jsonConvertType == null)
                    return;

                _deserializeMethod = jsonConvertType.GetMethod("DeserializeObject", new[] { typeof(string), typeof(Type) });
                _serializeMethod = jsonConvertType.GetMethod("SerializeObject", new[] { typeof(object) });

                IsAvailable = _deserializeMethod != null && _serializeMethod != null;
            }

            public static bool IsAvailable;
            public static object Deserialize(string json, Type type) => _deserializeMethod.Invoke(null, new object[] { json, type });
            public static string Serialize(object obj) => (string)_serializeMethod.Invoke(null, new object[] { obj });
        }
#endif

        protected internal static T DeserializeObject<T>(string json)
        {
            if (DeserializeFunc != null)
                return (T)DeserializeFunc.Invoke(json);
            else if (UseJsonParse != null)
                return UseJsonParse.DeserializeObject<T>(json);
            else
            {
#if NET8_0_OR_GREATER
                return System.Text.Json.JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
#else
                if (NewtonsoftJsonHelper.IsAvailable)
                    return (T)NewtonsoftJsonHelper.Deserialize(json, typeof(T));

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
            if (SerializeFunc != null)
                return SerializeFunc.Invoke(obj);
            else if (UseJsonParse != null)
                return UseJsonParse.SerializeObject(obj);
            else
            {
#if NET8_0_OR_GREATER
                return System.Text.Json.JsonSerializer.Serialize(obj, JsonSerializerOptions);
#else
                if (NewtonsoftJsonHelper.IsAvailable)
                    return NewtonsoftJsonHelper.Serialize(obj);

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