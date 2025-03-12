//------------------------------------------------------------------------------
//  此代码版权归作者majorworld所有
//  当前版本为1 .0.12最后更新时间2023-11-16
//  Gitee源代码仓库：https://gitee.com/majorworld
//  交流QQ群：1157159110
//  感谢您的下载和使用，请保留此说明
//  2023-11-16进行优化
//  2023-09-19按群友的反馈，添加了对字符串二次转义后的解析支持
//  2023-09-16添加了在实体类继承父类时，同样按父类的FastIgnore特性去过滤
//  2023-09-06添加了对转义字符的支持，按照用户MLing592建议增加了类似JsonConvert.DeserializeObject(string value, Type t)序列化
//------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ping9719.IoT.Common
{
    /// <summary>
    /// 过滤不需要序列化的字段
    /// </summary>
    public class JsonIgnoreAttribute : Attribute { }

    /// <summary>
    /// Json序列化
    /// </summary>
    public static class JsonMini
    {
        #region 公共方法
        /// <summary>
        /// 全局时间序列化样式，默认为yyyy-MM-dd HH:mm:ss
        /// </summary>
        public static string TimeFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// 将Json字符串转为指定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <returns></returns>
        public static T JsonFrom<T>(this string s)
        {
            return (T)s.JsonFrom(typeof(T));
        }

        public static object JsonFrom(this string s, Type t)
        {
            if (string.IsNullOrEmpty(s))
                throw new NullReferenceException("不能为空");
            s = s.Trim();
            StringBuilder sb = new StringBuilder(s.Length);
            Dictionary<string, object> dict = new Dictionary<string, object>();
            List<object> list = new List<object>();
            int index = 0;

            if (s[0] == '\"' && s[s.Length - 1] == '\"')
            {
                s = s.Trim('\"').Replace("\\\"", "\"");//去除转义
            }
            if (s.Length < 2 || (s[0] == '{' && s[s.Length - 1] != '}') || (s[0] == '[' && s[s.Length - 1] != ']'))
            {
                throw new NullReferenceException("不是JSON字符串");
            }
            switch (s[0])
            {
                case '{':
                    return s.GetObject(ref index, sb, list).ToObject(t);
                case '[':
                    var data = s.GetArray(ref index, sb, dict);
                    if (t == typeof(DataTable))
                        return data.ToDataTable();//处理表类型
                    if (t.GetGenericArguments().Length < 1)
                        return ChangeData(t, data);//处理各种类型
                    if (t.IsList())
                        return ChangeList(t.GetGenericArguments()[0], data);//处理集合类型
                    throw new NullReferenceException("类型应该为集合");
                default:
                    throw new NullReferenceException("缺失{或[");
            }
        }

        /// <summary>
        /// 将Json字符串转为对象<br/>
        /// 重载，当前数据类型速度最快
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Dictionary<string, object> JsonFrom(this string s)
        {
            return JsonFrom<Dictionary<string, object>>(s);
        }

        /// <summary>
        /// 将对象转为Json字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="timeFormat">时间序列化样式，默认为yyyy-MM-dd HH:mm:ss</param>
        /// <returns></returns>
        public static string JsonTo<T>(this T t, string timeFormat = null)
        {
            StringBuilder sb = new StringBuilder();
            t.CodeObject(sb, timeFormat ?? TimeFormat);
            return sb.ToString();
        }

        /// <summary>
        /// 将dynamic转为Json字符串
        /// </summary>
        /// <param name="t"></param>
        /// <param name="timeFormat"></param>
        /// <returns></returns>
        public static string JsonTo(dynamic t, string timeFormat = null)
        {
            StringBuilder sb = new StringBuilder();
            t.CodeObject(sb, timeFormat ?? TimeFormat);
            return sb.ToString();
        }

        #endregion

        #region 动态获取
        /// <summary>
        /// 获取数组类型元素的数量，方便PickData方法循环提取数组里所有元素
        /// </summary>
        /// <param name="json"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int ArrayCount(string json, string array = "")
        {
            if (string.IsNullOrEmpty(array.Trim(' ')))
            {
                return json.JsonFrom<List<object>>().Count();
            }
            var result = PickData(json, array);
            if (result is IList arr)
            {
                return arr.Count;
            }
            return 0;
        }
        /// <summary>
        /// 获取字段，array参数用空格分隔，数字为索引，非数字为字段
        /// </summary>
        /// <param name="json"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        public static object PickData(string json, string array)
        {
            var arr = array.Trim(' ').Split(' ');
            if (arr.Length > 0)
            {
                if (int.TryParse(arr[0], out int _))
                {
                    return PickData(json.JsonFrom<List<object>>(), array.Trim(' ').Split(' '), 0);
                }
            }
            return PickData(json.JsonFrom(), array.Trim(' ').Split(' '), 0);
        }
        static object PickData(object json, string[] array, int index = 0)
        {
            if (index >= array.Length)
            {
                return json;
            }
            var key = array[index];
            if (int.TryParse(key, out int pos))
            {
                if (json is List<object> list)
                {
                    index++;
                    return PickData(list[pos], array, index);
                }
            }
            else
            {
                if (json is IDictionary<string, object> dict)
                {
                    if (dict.TryGetValue(key, out object value))
                    {
                        index++;
                        return PickData(value, array, index);
                    }
                }
            }
            return null;
        }
        #endregion

        #region 转换为实体类对应的类型

        static bool IsDictionary(this Type type) => (typeof(IDictionary).IsAssignableFrom(type));

        static bool IsList(this Type type) => (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>));

        /// <summary>
        /// 处理数组类型
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        static object ChangeArray(Type type, IList data)
        {
            var array = Array.CreateInstance(type, data.Count);
            for (int i = 0; i < data.Count; i++)
            {
                array.SetValue(type.ChangeData(data[i]), i);
            }
            return array;
        }

        /// <summary>
        /// 转换ArrayList类型
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static ArrayList ChangeArrayList(object value)
        {
            ArrayList array = new ArrayList();
            if (value is IList list)
            {
                foreach (var item in list)
                    array.Add(item);
            }
            return array;
        }

        /// <summary>
        /// 转换Color类型
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static Color ChangeColor(string s)
        {
            if (s[0] == '#')
                return Color.FromArgb(Convert.ToInt32(s.Substring(1), 16));
            var c = s.Split(',');
            if (c.Length == 3)
                return Color.FromArgb(int.Parse(c[0]), int.Parse(c[1]), int.Parse(c[2]));
            else if (c.Length == 4)
                return Color.FromArgb(int.Parse(c[0]), int.Parse(c[1]), int.Parse(c[2]), int.Parse(c[3]));
            return Color.FromName(s);
        }

        /// <summary>
        /// 转换为各种数据
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="p"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        static object ChangeData(this Type p, object value)
        {
            if (value is null)
                return null;
            if (value.GetType() == p)
                return value;
            else if (value is Dictionary<string, object> dictionary)
                return ToObject(dictionary, p);//解析子级实体类的数据
            //else if (p == typeof(Bitmap) || p == typeof(Image))
            //    return Base64ToImage(value as string);
            else if (p == typeof(string))
                return value.JsonTo();
            else if (p.IsPrimitive && p != typeof(char))
                return Convert.ChangeType(value, p);
            else if (p == typeof(byte[]))
                return Convert.FromBase64String(value as string);
            else if (p == typeof(Color))
                return ChangeColor(value as string);
            else if (p == typeof(Point))
                return ChangePoint(value as string);
            else if (p == typeof(Guid))
                return Guid.Parse(value as string);
            else if (p == typeof(ArrayList))
                return ChangeArrayList(value);//处理动态数组类型
            else if (p.IsEnum)
                return Enum.ToObject(p, value);//处理枚举
            else if (p == typeof(DataTable))
            {
                if (value is List<object> list)
                {
                    return list.ToDataTable();
                }
            }
            else if (value is IList list)
            {
                if (p.GetGenericArguments().Length < 1)
                {
                    if (p.IsArray)
                        return ChangeArray(p.GetElementType(), value as IList);//处理数组类型
                    List<dynamic> d = new List<dynamic>();
                    foreach (var kv in list as dynamic)
                    {
                        if (kv is Dictionary<string, object> dict)
                            d.Add(dict.ToObject(typeof(object)));
                    }
                    return d;//解析dynamic
                }
                return ChangeList(p.GetGenericArguments()[0], value as List<object>);//处理List类型
            }
            Type t = Nullable.GetUnderlyingType(p);
            if (t is null)
            {
                if (value is string text && string.IsNullOrEmpty(text))
                {
                    return null;
                }
                return Convert.ChangeType(value, p);
            }

            return ChangeData(t, value);//处理可空类型
        }

        /// <summary>
        /// 处理字典类型
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        static object ChangeDictionary(Type type, Dictionary<string, object> dict)
        {
            //反射创建泛型字典
            var t = typeof(Dictionary<,>).MakeGenericType(new[] { type.GetGenericArguments()[0], type.GetGenericArguments()[1] }); ;
            var d = Activator.CreateInstance(t) as IDictionary;
            foreach (var item in dict)
                d.Add(type.GetGenericArguments()[0].ChangeData(item.Key), type.GetGenericArguments()[1].ChangeData(item.Value));
            return d;
        }

        /// <summary>
        /// 处理集合类型
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        static object ChangeList(Type type, List<object> data)
        {
            IList list = Activator.CreateInstance(typeof(List<>).MakeGenericType(type)) as IList;
            foreach (var item in data)
                list.Add(type.ChangeData(item));
            return list;
        }

        /// <summary>
        /// 转换Point类型
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static Point ChangePoint(string s)
        {
            var c = s.Split(',');
            return new Point(int.Parse(c[0]), int.Parse(c[1]));
        }

        /// <summary>
        /// List转DataTable
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        static DataTable ToDataTable<T>(this List<T> list)
        {
            DataTable dt = new DataTable();
            for (int i = 0; i < list.Count; i++)
            {
                Dictionary<string, object> dict = list[i] as Dictionary<string, object>;
                if (i == 0)
                {
                    foreach (var item in dict)
                    {
                        if (item.Value is null)
                            dt.Columns.Add(item.Key, typeof(object));
                        else
                            dt.Columns.Add(item.Key, item.Value.GetType());
                    }
                }
                dt.Rows.Add(dict.Values.ToArray());
            }
            return dt;
        }


        /// <summary>
        /// 获取动态类型
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        static dynamic GetDynamic(this Dictionary<string, object> dict)
        {
            dynamic d = new ExpandoObject();
            foreach (var kv in dict)
            {
                if (kv.Value is IList list)
                {
                    List<dynamic> array = new List<dynamic>();
                    foreach (var item in list)
                    {
                        if (item is Dictionary<string, object> dictChild)
                        {
                            array.Add(GetDynamic(dictChild));
                        }
                    }
                    (d as ICollection<KeyValuePair<string, object>>).Add(new KeyValuePair<string, object>(kv.Key, array));
                }
                else if (kv.Value is Dictionary<string, object> dictionary)
                    (d as ICollection<KeyValuePair<string, object>>).Add(new KeyValuePair<string, object>(kv.Key, ToObject(dictionary, typeof(object))));
                else
                    (d as ICollection<KeyValuePair<string, object>>).Add(kv);
            }
            return d;
        }
        /// <summary>
        /// 转换为对象
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        static object ToObject(this Dictionary<string, object> dict, Type type)
        {
            //(1/4)返回原始解析数据
            if (type == typeof(Dictionary<string, object>))
            {
                return dict;
            }
            //(2/4)返回dynamic类型数据
            if (type.UnderlyingSystemType.Name == "Object")
            {
                return GetDynamic(dict);
            }
            //(3/4)返回DataSet类型数据
            if (type == typeof(DataSet))
            {
                DataSet ds = new DataSet();
                foreach (var item in dict)
                {
                    if (item.Value is List<object> list)
                    {
                        var dt = list.ToDataTable();
                        dt.TableName = item.Key;
                        ds.Tables.Add(dt);
                    }
                }
                return ds;
            }

            //(4/4)返回所绑定的实体类数据
            object obj = null;
            if (type == typeof(string))
            {
                obj = dict.JsonTo();
            }
            else
            {
                obj = Activator.CreateInstance(type);
            }
            var props = type.GetCacheInfo();
            foreach (var kv in dict)
            {
                var prop = props.Where(x => string.Equals(x.Name, kv.Key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (prop is null)
                {
                    if (type.IsDictionary())
                        return ChangeDictionary(type, dict);//解析值是字典的数据（非缓存字段的字典）
                    continue;
                }
                if (prop.CanWrite)
                    prop.SetValue(obj, prop.PropertyType.ChangeData(kv.Value), null);//递归调用当前方法，解析子级
            }
            return obj;
        }

        #endregion 转换

        #region 解析字符串为对象

        /// <summary>
        /// 解析集合
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="sb"></param>
        /// <param name="dict"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        static List<object> GetArray(this string s, ref int index, StringBuilder sb, Dictionary<string, object> dict)
        {
            index++;
            List<object> list = new List<object>();
            while (index < s.Length)
            {
                switch (s[index])
                {
                    case ',':
                        index++;
                        break;
                    case '"':
                        list.Add(s.GetString(ref index, sb));
                        break;
                    case ']':
                        ++index;
                        return list;
                    case ' ':
                    case '\r':
                    case '\n':
                    case '\t':
                    case '\f':
                    case '\b':
                        ++index;
                        break;
                    default:
                        list.Add(s.GetData(s[index], ref index, sb, dict, list));
                        break;
                }
            }
            return list;
        }

        /// <summary>
        /// 解析对象
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="sb"></param>
        /// <param name="list"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        static Dictionary<string, object> GetObject(this string s, ref int index, StringBuilder sb, List<object> list)
        {
            index++;
            Dictionary<string, object> dict = new Dictionary<string, object>();
            string key = string.Empty;
            bool iskey = true;
            while (index < s.Length)
            {
                switch (s[index])
                {
                    case ',':
                        iskey = true;
                        key = string.Empty;
                        index++;
                        break;
                    case ':':
                        iskey = false;
                        index++;
                        break;
                    case '}':
                        ++index;
                        return dict;
                    case '"':
                        if (iskey)
                            key = s.GetString(ref index, sb);
                        else
                            dict.Add(key, s.GetString(ref index, sb));
                        break;
                    case ' ':
                    case '\r':
                    case '\n':
                    case '\t':
                    case '\f':
                    case '\b':
                        index++;
                        break;
                    default:
                        dict.Add(key, s.GetData(s[index], ref index, sb, dict, list));
                        break;
                }
            }
            throw new FormatException("解析错误，不完整的Json");
        }

        /// <summary>
        /// 获取布尔数据
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="state"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        static bool GetBool(this string s, ref int index, bool state)
        {
            if (state)
            {
                if (s[index + 1] == 'r' && s[index + 2] == 'u' && s[index + 3] == 'e')
                {
                    index += 4;
                    return true;
                }
            }
            else
            {
                if (s[index + 1] == 'a' && s[index + 2] == 'l' && s[index + 3] == 's' && s[index + 4] == 'e')
                {
                    index += 5;
                    return false;
                }
            }
            throw new FormatException($"\"{string.Concat(s[index], s[index + 1], s[index + 2], s[index + 3])}\"处Json格式无法解析");
        }

        /// <summary>
        /// 自动获取数据
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="s"></param>
        /// <param name="c"></param>
        /// <param name="index"></param>
        /// <param name="sb"></param>
        /// <param name="dict"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        static object GetData(this string s, char c, ref int index, StringBuilder sb, Dictionary<string, object> dict, List<object> list)
        {
            switch (c)
            {
                case 't':
                    return s.GetBool(ref index, true);
                case 'f':
                    return s.GetBool(ref index, false);
                case 'n':
                    return s.GetNull(ref index);
                case '{':
                    return s.GetObject(ref index, sb, list);
                case '[':
                    return s.GetArray(ref index, sb, dict);
                default:
                    return s.GetNumber(ref index, sb);
            }
        }

        /// <summary>
        /// 获取空数据
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        static object GetNull(this string s, ref int index)
        {
            if (s[index + 1] == 'u' && s[index + 2] == 'l' && s[index + 3] == 'l')
            {
                index += 4;
                return null;
            }
            throw new FormatException($"\"{string.Concat(s[index], s[index + 1], s[index + 2], s[index + 3])}\"处Json格式无法解析");
        }

        /// <summary>
        /// 获取数字数据
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="s"></param>
        /// <param name="index"></param>
        /// <param name="sb"></param>
        /// <returns></returns>
        static object GetNumber(this string s, ref int index, StringBuilder sb)
        {
            sb.Clear();
            for (; index < s.Length; ++index)
            {
                if (s[index] == ',' || s[index] == '}' || s[index] == ']' || s[index] == ' ' || s[index] == '\n' || s[index] == '\r')
                    break;
                else
                    sb.Append(s[index]);
            }
            string code = sb.ToString();
            if (int.TryParse(code, out int x))
                return x;
            if (long.TryParse(code, out long y))
                return y;
            if (double.TryParse(code, out double z))
                return z;
            throw new FormatException($"\"{code}\"处Json格式无法解析");
        }

        /// <summary>
        /// 获取字符串数据
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="sb"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        static string GetString(this string s, ref int index, StringBuilder sb)
        {
            sb.Clear();
            index++;

            for (; index < s.Length; ++index)
            {
                switch (s[index])
                {
                    case '"':
                        index++;
                        return sb.ToString();
                    case '\\':
                        switch (s[index + 1])
                        {
                            case 'u'://添加对unicode字符串的支持
                                if (int.TryParse(s.Substring(index + 2, 4), System.Globalization.NumberStyles.AllowHexSpecifier, null, out int c))
                                {
                                    index = index + 5;
                                    sb.Append((char)c);
                                    break;
                                }
                                break;
                            #region 2023年9月6日添加对转义字符的支持
                            case 'r':
                                index++;
                                sb.Append('\r');
                                break;
                            case 'n':
                                index++;
                                sb.Append('\n');
                                break;
                            case 't':
                                index++;
                                sb.Append('\t');
                                break;
                            case 'f':
                                index++;
                                sb.Append('\f');
                                break;
                            case 'b':
                                index++;
                                sb.Append('\b');
                                break;
                            case '\'':
                                index++;
                                sb.Append('\'');
                                break;
                            #endregion
                            default:
                                if (s[index + 1] == '"' || s[index + 1] == '\\')
                                    index++;
                                sb.Append(s[index]);
                                break;
                        }
                        break;
                    default:
                        sb.Append(s[index]);
                        break;
                }
            }
            throw new FormatException($"\"{sb}\"处Json格式无法解析");
        }

        #endregion 解析

        #region 编码对象为字符串
        /// <summary>
        /// 缓存数据加速序列化速度，主要是减少不必要的GetCustomAttributes获取特性并过滤字段
        /// </summary>
        static ConcurrentDictionary<Type, PropertyInfo[]> InfoCache = new ConcurrentDictionary<Type, PropertyInfo[]>();

        ///// <summary>
        ///// base64 转 Image
        ///// </summary>
        ///// <param name="base64"></param>
        //static Bitmap Base64ToImage(this string base64)
        //{
        //    if (string.IsNullOrEmpty(base64))
        //    {
        //        return null;
        //    }
        //    int index = base64.IndexOf(",");
        //    if (index > -1)
        //    {
        //        base64 = base64.Substring(index, base64.Length - index);
        //    }
        //    byte[] bytes = Convert.FromBase64String(base64);
        //    MemoryStream memStream = new MemoryStream(bytes);
        //    Image mImage = Image.FromStream(memStream);
        //    return new Bitmap(mImage);
        //}

        ///// <summary>
        ///// Image 转成 base64
        ///// </summary>
        ///// <param name="fileFullName"></param>
        //static string ImageToBase64(this Bitmap bmp, System.Drawing.Imaging.ImageFormat format = null)
        //{
        //    if (bmp is null)
        //    {
        //        return string.Empty;
        //    }
        //    MemoryStream ms = new MemoryStream();
        //    bmp.Save(ms, format ?? System.Drawing.Imaging.ImageFormat.Png);
        //    byte[] arr = new byte[ms.Length]; ms.Position = 0;
        //    ms.Read(arr, 0, (int)ms.Length); ms.Close();
        //    return Convert.ToBase64String(arr);
        //}

        /// <summary>
        /// 序列化
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
        /// <param name="timeFormat"></param>
        static void CodeObject(this object obj, StringBuilder sb, string timeFormat)
        {
            switch (obj)
            {
                case null:
                    sb.Append("null");
                    break;
#if NET45_OR_GREATER

#endif
#if  NETFRAMEWORK

#endif
                //case Bitmap bmp:
                //    sb.Append($"\"{ImageToBase64(bmp)}\"");
                //    break;
                //case Image img:
                //    sb.Append($"\"{ImageToBase64(img as Bitmap)}\"");
                //    break;
                case Enum _:
                    sb.Append($"{Convert.ToInt32(obj)}");
                    break;
                case byte[] bytes:
                    sb.Append($"\"{Convert.ToBase64String(bytes)}\"");
                    break;
                case Array array:
                    sb.Append('[');
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (i != 0)
                            sb.Append(",");
                        var aaaa = array.GetValue(i);
                        array.GetValue(i).CodeObject(sb, timeFormat);
                    }
                    sb.Append(']');
                    break;
                case string text:
                    #region 2023年9月6日添加对转义字符的支持
                    sb.ToLiteral(text);
                    #endregion
                    break;
                case char c:
                    //sb.Append($"\"{obj}\"");
                    sb.Append("\"");
                    sb.ToCharLiteral(c);
                    sb.Append("\"");
                    break;
                case bool _:
                    sb.Append($"{obj.ToString().ToLower()}");
                    break;
                case DataTable dt:
                    dt.CodeDataTable(sb, timeFormat);
                    break;
                case DataSet ds:
                    sb.Append('{');
                    for (int i = 0; i < ds.Tables.Count; i++)
                    {
                        if (i != 0)
                            sb.Append(",");
                        sb.Append($"\"{ds.Tables[i].TableName}\":");
                        ds.Tables[i].CodeDataTable(sb, timeFormat);
                    }
                    sb.Append('}');
                    break;
                case DateTime time:
                    sb.AppendFormat($"\"{time.ToString(timeFormat)}\"");
                    break;
                case Guid id:
                    sb.AppendFormat($"\"{id}\"");
                    break;
                case Color color:
                    if (color.A == 255)
                        sb.Append($"\"{color.R},{color.G},{color.B}\"");
                    else
                        sb.Append($"\"{color.A},{color.R},{color.G},{color.B}\"");
                    break;
                case Point point:
                    sb.Append($"\"{point.X},{point.Y}\"");
                    break;
                case ArrayList list:
                    sb.Append('[');
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (i != 0)
                            sb.Append(",");
                        list[i].CodeObject(sb, timeFormat);
                    }
                    sb.Append(']');
                    break;
                default:
                    CodeOther(obj, sb, timeFormat);
                    break;
            }
        }
        /// <summary>
        /// 将字符可见
        /// </summary>
        /// <param name="sb">https://gitee.com/majorworld</param>
        /// <param name="input"></param>
        static void ToLiteral(this StringBuilder sb, string input)
        {
            sb.Append("\"");
            foreach (var c in input)
            {
                sb.ToCharLiteral(c);
            }
            sb.Append("\"");
        }

        static void ToCharLiteral(this StringBuilder sb, char c)
        {
            switch (c)
            {
                case '\"': sb.Append("\\\""); break;
                case '\\': sb.Append(@"\\"); break;
                case '\0': sb.Append(@"\u0000"); break;
                case '\a': sb.Append(@"\u0007"); break;
                case '\b': sb.Append(@"\b"); break;
                case '\f': sb.Append(@"\f"); break;
                case '\n': sb.Append(@"\n"); break;
                case '\r': sb.Append(@"\r"); break;
                case '\t': sb.Append(@"\t"); break;
                case '\v': sb.Append(@"\u000b"); break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        /// <summary>
        /// 序列化其他
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
        /// <param name="timeFormat"></param>
        static void CodeOther(object obj, StringBuilder sb, string timeFormat)
        {
            Type type = obj.GetType();
            //数字
            if (type.IsPrimitive && type != typeof(char))
            {
                sb.Append($"{obj}");
                return;
            }
            //字典
            else if (type.IsDictionary())
            {
                sb.Append('{');
                var collection = obj as IDictionary;
                var enumerator = collection.GetEnumerator();
                int index = 0;
                while (enumerator.MoveNext())
                {
                    if (index != 0)
                        sb.Append(",");
                    sb.Append($"\"{enumerator.Key}\":");
                    enumerator.Value.CodeObject(sb, timeFormat);
                    index++;
                }
                sb.Append('}');
                return;
            }
            //集合
            else if (type.IsList())
            {
                sb.Append('[');
                if (obj is IList list)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (i != 0)
                            sb.Append(",");
                        list[i].CodeObject(sb, timeFormat);
                    }
                }
                sb.Append(']');
                return;
            }
            else if (type.UnderlyingSystemType.Name == "ExpandoObject")
            {
                sb.Append('{');
                bool first = true;
                foreach (dynamic item in obj as dynamic)
                {
                    if (!first)
                        sb.Append(',');
                    first = false;
                    object value = item.Value;
                    sb.Append($"\"{item.Key}\":");
                    value.CodeObject(sb, timeFormat);
                }
                sb.Append('}');
                return;
            }

            //对象
            var prop = type.GetCacheInfo();
            if (prop is null)
            {
                sb.Append("null");
                return;
            }
            sb.Append('{');
            for (int i = 0; i < prop.Length; i++)
            {
                PropertyInfo p = prop[i];
                if (i != 0)
                    sb.Append(",");
                var data = p.GetValue(obj, null);
                sb.Append($"\"{p.Name}\":");
                data.CodeObject(sb, timeFormat);
            }
            sb.Append("}");
        }

        /// <summary>
        /// 尝试获取缓存中的类型，排除忽略的字段
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static PropertyInfo[] GetCacheInfo(this Type type)
        {
            if (InfoCache.TryGetValue(type, out PropertyInfo[] props)) { }
            else
            {
                props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                List<PropertyInfo> cache = new List<PropertyInfo>();

                #region 2023-09-09如果继承的字段有忽略特性
                var faces = type.GetInterfaces();
                List<string> list = new List<string>();
                foreach (Type cell in faces)
                {
                    var collection = cell.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    foreach (var item in collection)
                    {
                        if (Attribute.GetCustomAttributes(item, typeof(JsonIgnoreAttribute))?.Length > 0)
                            list.Add(item.Name);
                    }
                }
                #endregion

                foreach (var item in props)
                {
                    if (Attribute.GetCustomAttributes(item, typeof(JsonIgnoreAttribute))?.Length == 0 && !list.Contains(item.Name))
                        cache.Add(item);
                }
                InfoCache[type] = cache.ToArray();
                props = cache.ToArray();
            }
            return props;
        }

        /// <summary>
        /// 序列化DataTable
        /// <para>https://gitee.com/majorworld</para>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="sb"></param>
        /// <param name="timeFormat">时间格式化样式，默认为yyyy-MM-dd HH:mm:ss</param>
        static void CodeDataTable(this DataTable dt, StringBuilder sb, string timeFormat)
        {
            sb.Append('[');
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i != 0)
                    sb.Append(",");
                var item = dt.Rows[i];
                sb.Append('{');
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    if (j != 0)
                        sb.Append(",");
                    var cell = dt.Columns[j];
                    sb.Append($"\"{cell}\":");
                    item[j].CodeObject(sb, timeFormat);
                }
                sb.Append('}');
            }
            sb.Append(']');
        }
        #endregion
    }
}
