using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Common
{
    public class JsonUtil
    {
        public static T DeserializeObject<T>(string json)
        {
            return JsonMini.JsonFrom<T>(json);
            //DataContractJsonSerializer jsonFormator = new DataContractJsonSerializer(typeof(T));
            //using (Stream readStream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            //{
            //    return (T)jsonFormator.ReadObject(readStream);
            //}
        }

        public static string SerializeObject(object obj)
        {
            return JsonMini.JsonTo(obj);
            //DataContractJsonSerializer jsonFormator = new DataContractJsonSerializer(obj.GetType());
            //using (MemoryStream stream = new MemoryStream())
            //{
            //    jsonFormator.WriteObject(stream, obj);
            //    return Encoding.UTF8.GetString(stream.ToArray());
            //}
        }
    }

}
