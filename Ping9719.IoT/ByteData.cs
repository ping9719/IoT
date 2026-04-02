using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
    public class ByteData
    {
        /// <summary>
        /// 元数据字节数组。
        /// </summary>
        public IEnumerable<byte> Data { get; set; }
        /// <summary>
        /// 字节格式。
        /// </summary>
        public EndianFormat EndianFormat { get; set; }

        public ByteData() : this(new byte[0], EndianFormat.DCBA) { }
        public ByteData(EndianFormat endianFormat) : this(new byte[0], endianFormat) { }
        public ByteData(IEnumerable<byte> data, EndianFormat endianFormat = EndianFormat.DCBA)
        {
            Data = data;
            EndianFormat = endianFormat;
        }

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
        /// 自定义的byte转换器字典
        /// </summary>
        public Dictionary<Type, IByteConverter> ByteConverterDict = new Dictionary<Type, IByteConverter>();

        /// <summary>
        /// 得到值。支持基本类型、类、结构体、数组和集合，只支持属性不支持字段。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="offset">偏差</param>
        /// <returns></returns>
        public T GetValue<T>(int offset = 0)
        {
            var t = typeof(T);

            //数组，集合
            if (t.IsArray || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)))
            {
                Type elementType = t.IsArray ? t.GetElementType() : t.GetGenericArguments()[0];

                List<object> values = new List<object>();
                while (true)
                {
                    if (offset >= Data.Count())
                        break;

                    var val = GetValue(elementType, offset, out int ul);
                    offset += ul;
                    values.Add(val);

                    if (offset >= Data.Count() || offset + ul > Data.Count())
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
                return (T)GetValue(t, offset, out int ul);
            }
        }
        private object GetValue(Type t, int offset, out int useLength)
        {
            useLength = 0;
            if (ByteConverterDict.TryGetValue(t, out var v1))
            {
                useLength += v1.ByteLength;
                return v1.ToObject(Data.Skip(offset), EndianFormat);
            }
            else if (ByteDefaultConverters.TryGetValue(t, out var v2))
            {
                useLength += v2.ByteLength;
                return v2.ToObject(Data.Skip(offset), EndianFormat);
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
                    if (ByteConverterDict.TryGetValue(prop.PropertyType, out var v1n))
                        converter = v1n;
                    else if (ByteDefaultConverters.TryGetValue(prop.PropertyType, out var v2n))
                        converter = v2n;
                    else
                        throw new NotSupportedException($"属性 {prop.Name} 的类型 {prop.PropertyType.Name} 未找到对应的字节转换器");

                    var value = converter.ToObject(Data.Skip(offset), EndianFormat);
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
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="count">数量</param>
        /// <param name="offset">第一个偏差</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public IEnumerable<T> GetValues<T>(int count, int offset = 0)
        {
            var t = typeof(T);
            var results = new List<T>(count);
            if (ByteConverterDict.TryGetValue(t, out var v1))
            {
                for (int i = 0; i < count; i++)
                {
                    var obj = (T)v1.ToObject(Data.Skip(offset + i * v1.ByteLength), EndianFormat);
                    results.Add(obj);
                }
                return results;
            }
            else if (ByteDefaultConverters.TryGetValue(t, out var v2))
            {
                for (int i = 0; i < count; i++)
                {
                    var obj = (T)v2.ToObject(Data.Skip(offset + i * v2.ByteLength), EndianFormat);
                    results.Add(obj);
                }
                return results;
            }
            else if (t.IsClass || (t.IsValueType && !t.IsPrimitive && !t.IsEnum))
            {
                for (int i = 0; i < count; i++)
                {
                    if (offset >= Data.Count())
                        break;

                    var value = (T)GetValue(t, offset, out int ul);
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
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public byte[] ToBytes(object data)
        {
            if (data == null)
                return null;

            var t = data.GetType();
            if (data is byte[] ba)
                return ba.ToArray();

            if (data is IEnumerable arr && !(data is string))
            {
                var bytes = new List<byte>();
                foreach (var item in arr)
                {
                    bytes.AddRange(ToBytes(item));
                }
                return bytes.ToArray();
            }
            else if (ByteConverterDict.TryGetValue(t, out var v1))
            {
                return v1.ToBytes(data, EndianFormat);
            }
            else if (ByteDefaultConverters.TryGetValue(t, out var v2))
            {
                return v2.ToBytes(data, EndianFormat);
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
                    if (ByteConverterDict.TryGetValue(prop.PropertyType, out var v1n))
                        converter = v1n;
                    else if (ByteDefaultConverters.TryGetValue(prop.PropertyType, out var v2n))
                        converter = v2n;
                    else
                        throw new NotSupportedException($"属性 {prop.Name} 的类型 {prop.PropertyType.Name} 未找到对应的字节转换器");
                    var value = prop.GetValue(data);
                    bytes.AddRange(converter.ToBytes(value, EndianFormat));
                }
                return bytes.ToArray();
            }

            throw new NotSupportedException($"不支持的类型：{t.FullName}");

        }
    }
}
