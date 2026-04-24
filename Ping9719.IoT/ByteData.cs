using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ping9719.IoT
{
    public class IoTAttribute : Attribute
    {
        /// <summary>
        /// 偏移量。-1自动计算，0表示从第一个字节开始
        /// </summary>
        public int Offset { get; set; } = -1;
        /// <summary>
        /// 是否忽略，默认false
        /// </summary>
        public bool IsIgnore { get; set; } = false;
    }

    /// <summary>
    /// byte数据
    /// </summary>
    public static class ByteData
    {
        private static readonly Dictionary<Type, IByteConverter> ByteDefaultConverters = new Dictionary<Type, IByteConverter>
        {
            //1
            { typeof(Byte), new ByteByteConverter() },
            { typeof(SByte), new SByteByteConverter() },
            //2
            { typeof(Int16), new Int16ByteConverter() },
            { typeof(UInt16), new UInt16ByteConverter() },
            //4
            { typeof(Int32), new Int32ByteConverter() },
            { typeof(UInt32), new UInt32ByteConverter() },
            { typeof(Single), new SingleByteConverter() },
            //8
            { typeof(Int64), new Int64ByteConverter() },
            { typeof(UInt64), new UInt64ByteConverter() },
            { typeof(Double), new DoubleByteConverter() },
        };

        /// <summary>
        /// 得到值。支持基本类型、类、结构体、数组和集合，只支持属性不支持字段。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">数据</param>
        /// <param name="endianFormat">字节序</param>
        /// <param name="converterDict">自定义字节转换器字典</param>
        /// <param name="isUseDefaultConverter">是否使用默认字节转换器</param>
        /// <param name="offset">偏移量</param>
        /// <returns></returns>
        public static T GetValue<T>(IEnumerable<byte> data, EndianFormat endianFormat, Dictionary<Type, IByteConverter> converterDict = null, bool isUseDefaultConverter = true, int offset = 0)
        {
            var t = typeof(T);
            converterDict ??= new Dictionary<Type, IByteConverter>();

            //数组，集合
            if (t.IsArray || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)))
            {
                Type elementType = t.IsArray ? t.GetElementType() : t.GetGenericArguments()[0];

                List<object> values = new List<object>();
                while (true)
                {
                    if (offset >= data.Count())
                        break;

                    var val = GetValue(elementType, data, endianFormat, offset, converterDict, isUseDefaultConverter, out int ul);
                    offset += ul;
                    values.Add(val);

                    if (offset >= data.Count() || offset + ul > data.Count())
                        break;
                }

                if (t.IsArray)
                {
                    Array array = Array.CreateInstance(elementType, values.Count);
                    for (int i = 0; i < values.Count; i++)
                        array.SetValue(values[i], i);

                    return (T)(object)array;
                }
                else
                {
                    Type listType = typeof(List<>).MakeGenericType(elementType);
                    IList list = (IList)Activator.CreateInstance(listType);
                    foreach (var item in values)
                        list.Add(item);

                    return (T)(object)list;
                }
            }
            else
            {
                return (T)GetValue(t, data, endianFormat, offset, converterDict, isUseDefaultConverter, out int ul);
            }
        }
        private static object GetValue(Type t, IEnumerable<byte> data, EndianFormat endianFormat, int offset, Dictionary<Type, IByteConverter> converterDict, bool isUseDefaultConverter, out int useLength)
        {
            useLength = 0;
            converterDict ??= new Dictionary<Type, IByteConverter>();
            if (converterDict.TryGetValue(t, out var v1))
            {
                useLength += v1.ByteLength;
                return v1.ToObject(data.Skip(offset), endianFormat);
            }
            else if (isUseDefaultConverter && ByteDefaultConverters.TryGetValue(t, out var v2))
            {
                useLength += v2.ByteLength;
                return v2.ToObject(data.Skip(offset), endianFormat);
            }
            //类，结构体
            else if (t.IsClass || (t.IsValueType && !t.IsPrimitive && !t.IsEnum))
            {
                var instance = Activator.CreateInstance(t);
                var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in properties)
                {
                    if (!prop.CanWrite)
                        continue;

                    var attr = prop.GetCustomAttribute<IoTAttribute>() ?? new IoTAttribute();
                    if (attr.IsIgnore)
                        continue;

                    if (attr.Offset >= 0)
                        offset = attr.Offset;

                    IByteConverter converter = null;
                    if (converterDict.TryGetValue(prop.PropertyType, out var v1n))
                        converter = v1n;
                    else if (isUseDefaultConverter && ByteDefaultConverters.TryGetValue(prop.PropertyType, out var v2n))
                        converter = v2n;
                    else
                        throw new NotSupportedException($"属性 {prop.Name} 的类型 {prop.PropertyType.Name} 未找到对应的字节转换器");

                    var value = converter.ToObject(data.Skip(offset), endianFormat);
                    prop.SetValue(instance, value);

                    useLength += converter.ByteLength;
                    offset += converter.ByteLength;
                }

                return instance;
            }

            throw new NotSupportedException($"不支持的类型：{t.FullName}");
        }

        /// <summary>
        /// 得到多个值。支持基本类型、类、结构体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">数据</param>
        /// <param name="count">解析的数量</param>
        /// <param name="endianFormat">字节序</param>
        /// <param name="converterDict">自定义字节转换器字典</param>
        /// <param name="isUseDefaultConverter">是否使用默认字节转换器</param>
        /// <param name="offset">第一个偏差</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static IEnumerable<T> GetValues<T>(IEnumerable<byte> data, int count, EndianFormat endianFormat,  Dictionary<Type, IByteConverter> converterDict = null, bool isUseDefaultConverter = true, int offset = 0)
        {
            var t = typeof(T);
            converterDict ??= new Dictionary<Type, IByteConverter>();
            var results = new List<T>(count);
            if (converterDict.TryGetValue(t, out var v1))
            {
                for (int i = 0; i < count; i++)
                {
                    if (offset >= data.Count())
                        break;

                    var obj = v1.ToObject(data.Skip(offset + i * v1.ByteLength), endianFormat);
                    if (obj is T t1)
                        results.Add(t1);
                    else if (obj is IEnumerable<T> t2)
                        results.AddRange(t2);
                }
                return results;
            }
            else if (isUseDefaultConverter && ByteDefaultConverters.TryGetValue(t, out var v2))
            {
                for (int i = 0; i < count; i++)
                {
                    var obj = v2.ToObject(data.Skip(offset + i * v2.ByteLength), endianFormat);
                    if (obj is T t1)
                        results.Add(t1);
                    else if (obj is IEnumerable<T> t2)
                        results.AddRange(t2);
                }
                return results;
            }
            else if (t.IsClass || (t.IsValueType && !t.IsPrimitive && !t.IsEnum))
            {
                for (int i = 0; i < count; i++)
                {
                    if (offset >= data.Count())
                        break;

                    var value = (T)GetValue(t, data, endianFormat, offset, converterDict, isUseDefaultConverter, out int ul);
                    results.Add(value);

                    // 前进到下一项的起始偏移
                    offset += ul;
                }
                return results;
            }
            else
            {
                throw new NotSupportedException($"不支持的类型：{t.FullName}");
            }
        }

        /// <summary>
        /// 将对象转换为字节数组。支持基本类型、类、结构体、数组和集合，只支持属性不支持字段。
        /// </summary>
        /// <param name="data">对象</param>
        /// <param name="endianFormat">字节序</param>
        /// <param name="converterDict">自定义字节转换器字典</param>
        /// <param name="isUseDefaultConverter">是否使用默认字节转换器</param>
        /// <returns>字节数组</returns>
        /// <exception cref="NotSupportedException"></exception>
        public static byte[] ToBytes(object data, EndianFormat endianFormat, Dictionary<Type, IByteConverter> converterDict = null, bool isUseDefaultConverter = true)
        {
            if (data == null)
                return null;

            converterDict ??= new Dictionary<Type, IByteConverter>();

            var t = data.GetType();
            if (data is byte[] ba)
                return ba.ToArray();

            if (data is IEnumerable arr && !(data is string))
            {
                var bytes = new List<byte>();
                foreach (var item in arr)
                {
                    bytes.AddRange(ToBytes(item, endianFormat, converterDict, isUseDefaultConverter));
                }
                return bytes.ToArray();
            }
            else if (converterDict.TryGetValue(t, out var v1))
            {
                return v1.ToBytes(data, endianFormat);
            }
            else if (ByteDefaultConverters.TryGetValue(t, out var v2))
            {
                return v2.ToBytes(data, endianFormat);
            }
            else if (t.IsClass || (t.IsValueType && !t.IsPrimitive && !t.IsEnum))
            {
                var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                List<byte> bytes = new List<byte>();
                foreach (var prop in properties)
                {
                    if (!prop.CanRead)
                        continue;
                    var attr = prop.GetCustomAttribute<IoTAttribute>() ?? new IoTAttribute();
                    if (attr.IsIgnore)
                        continue;
                    IByteConverter converter = null;
                    if (converterDict.TryGetValue(prop.PropertyType, out var v1n))
                        converter = v1n;
                    else if (ByteDefaultConverters.TryGetValue(prop.PropertyType, out var v2n))
                        converter = v2n;
                    else
                        throw new NotSupportedException($"属性 {prop.Name} 的类型 {prop.PropertyType.Name} 未找到对应的字节转换器");
                    var value = prop.GetValue(data);
                    bytes.AddRange(converter.ToBytes(value, endianFormat));
                }
                return bytes.ToArray();
            }

            throw new NotSupportedException($"不支持的类型：{t.FullName}");

        }
    }
}
