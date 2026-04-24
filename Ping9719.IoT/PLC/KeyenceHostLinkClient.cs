using Ping9719.IoT.Common;
using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TcpClient = Ping9719.IoT.Communication.TcpClient;

namespace Ping9719.IoT.PLC;

/// <summary>
/// 基恩士客户端（KV HostLink 上位链路协议）
/// 支持Float/Int/Bool/Short/UShort/UInt/ULong/Double/String的读写
/// </summary>
public class KeyenceHostLinkClient : ReadWriteBase, IClientData
{
    public ClientBase Client { get; private set; }
    /// <summary>
    /// 站号（默认01）
    /// </summary>
    public byte Station { get; set; } = 1;

    #region 协议常量
    // 协议前缀/后缀
    private const string ProtocolPrefix = "%";       // 指令起始符
    private const string ProtocolTerminator = "\r"; // 指令结束符（CR）
    private const string ResponseOk = "OK";         // 成功响应标识
    private const string ResponseError = "ER";      // 错误响应标识

    // 通讯命令码
    private const string CmdReadSingle = "RD";    // 读单个地址
    private const string CmdReadMultiple = "RDS"; // 读多个地址
    private const string CmdWriteSingle = "WR";   // 写单个地址
    private const string CmdWriteMultiple = "WRS";// 写多个地址

    // 数据类型后缀
    private const string TypeUnsigned16 = ".U";   // 16位无符号整数
    private const string TypeSigned16 = ".S";     // 16位有符号整数
    private const string TypeHex16 = ".H";        // 16位十六进制

    // 数据类型后缀映射
    private static readonly Dictionary<Type, string> _typeSuffixMap = new()
    {
        { typeof(bool), TypeUnsigned16 },
        { typeof(char), TypeUnsigned16 },
        { typeof(short), TypeSigned16 },
        { typeof(ushort), TypeUnsigned16 },
        { typeof(int), TypeSigned16 },
        { typeof(uint), TypeUnsigned16 },
        { typeof(float), TypeUnsigned16 },
        { typeof(ulong), TypeUnsigned16 },
        { typeof(double), TypeUnsigned16 }
    };
    #endregion

    #region 构造函数
    /// <summary>
    /// 初始化基恩士HostLink客户端
    /// </summary>
    /// <param name="client">通讯客户端</param>
    public KeyenceHostLinkClient(ClientBase client)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        Client.Encoding = Encoding.ASCII;
        Client.TimeOut = 3000;
        Client.IsAutoDiscard = true;
    }

    /// <summary>
    /// 初始化基恩士HostLink客户端
    /// </summary>
    /// <param name="ipAddress">PLC IP地址</param>
    /// <param name="port">通讯端口（基恩士HostLink默认8501）</param>
    public KeyenceHostLinkClient(string ipAddress, int port = 8501) : this(new TcpClient(ipAddress, port)) { }
    #endregion

    #region 协议解析方法

    /// <summary>
    /// 获取类型对应的数据类型后缀
    /// </summary>
    private string GetTypeSuffix<T>()
    {
        if (_typeSuffixMap.TryGetValue(typeof(T), out string suffix))
            return suffix;
        
        return TypeUnsigned16;
    }

    /// <summary>
    /// 构建基恩士HostLink标准指令
    /// </summary>
    private string BuildHostLinkCommand(string command, string address, string typeSuffix, int count = 1)
    {
        // CmdReadMultiple + Sp + startAddress + TypeUnsigned16 + Sp + wordCount + Cr
        return $"{command} {address}{typeSuffix} {count}{ProtocolTerminator}";
    }
    #endregion

    #region 数据转换方法
    /// <summary>
    /// 将PLC返回的字符串转换为指定类型
    /// </summary>
    private T ConvertResponseToType<T>(string[] responseParts)
    {
        if (responseParts == null || responseParts.Length == 0)
            return default;

        try
        {
            Type targetType = typeof(T);
            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlyingType == typeof(bool))
            {
                var rawValue = responseParts[0].Trim().ToUpper();
                return (T)(object)(rawValue == "1" || rawValue == "TRUE" || rawValue == "ON");
            }

            if (underlyingType == typeof(short))
                return (T)(object)short.Parse(responseParts[0]);
            if (underlyingType == typeof(ushort))
                return (T)(object)ushort.Parse(responseParts[0]);
            if (underlyingType == typeof(int))
            {
                if (responseParts.Length < 2)
                    throw new FormatException($"Int解析失败：需要2个寄存器值，实际有{responseParts.Length}个");
                return (T)(object)ConvertToInt(responseParts[0], responseParts[1]);
            }

            if (underlyingType == typeof(uint))
            {
                if (responseParts.Length < 2)
                    throw new FormatException($"UInt解析失败：需要2个寄存器值，实际有{responseParts.Length}个");
                return (T)(object)ConvertToUInt(responseParts[0], responseParts[1]);
            }

            if (underlyingType == typeof(float))
            {
                if (responseParts.Length < 2)
                    throw new FormatException($"Float解析失败：需要2个寄存器值，实际有{responseParts.Length}个");
                return (T)(object)ConvertToFloat(responseParts[0], responseParts[1]);
            }

            if (underlyingType == typeof(double))
            {
                if (responseParts.Length < 4)
                    throw new FormatException($"Double解析失败：需要4个寄存器值，实际有{responseParts.Length}个");
                return (T)(object)ConvertToDouble(responseParts[0], responseParts[1], responseParts[2], responseParts[3]);
            }

            if (underlyingType == typeof(ulong))
            {
                if (responseParts.Length < 4)
                    throw new FormatException($"ULong解析失败：需要4个寄存器值，实际有{responseParts.Length}个");
                return (T)(object)ConvertToULong(responseParts[0], responseParts[1], responseParts[2], responseParts[3]);
            }

            if (underlyingType == typeof(string))
                return (T)(object)string.Join(" ", responseParts).Trim();
            if (underlyingType == typeof(char))
                return (T)(object)char.Parse(responseParts[0].Trim());

            throw new NotSupportedException($"不支持的数据类型：{targetType.Name}");
        }
        catch (Exception ex)
        {
            throw new Exception($"数据转换失败（原始数据：[{string.Join(",", responseParts)}]）：{ex.Message}", ex);
        }
    }

    /// <summary>
    /// 两个16位无符号整数转Float
    /// </summary>
    private float ConvertToFloat(string lowWordStr, string highWordStr)
    {
        if (!ushort.TryParse(lowWordStr, out ushort lowWord) || !ushort.TryParse(highWordStr, out ushort highWord))
            throw new FormatException($"Float解析失败：低字={lowWordStr}，高字={highWordStr}");

        byte[] lowBytes = BitConverter.GetBytes(lowWord);
        byte[] highBytes = BitConverter.GetBytes(highWord);
        byte[] floatBytes = [lowBytes[0], lowBytes[1], highBytes[0], highBytes[1]];

        return BitConverter.ToSingle(floatBytes, 0);
    }

    /// <summary>
    /// 两个16位有符号整数转Int
    /// </summary>
    private int ConvertToInt(string lowWordStr, string highWordStr)
    {
        if (!short.TryParse(lowWordStr, out short lowWord) || !short.TryParse(highWordStr, out short highWord))
            throw new FormatException($"Int解析失败：低字={lowWordStr}，高字={highWordStr}");

        byte[] lowBytes = BitConverter.GetBytes(lowWord);
        byte[] highBytes = BitConverter.GetBytes(highWord);
        byte[] intBytes = [lowBytes[0], lowBytes[1], highBytes[0], highBytes[1]];

        return BitConverter.ToInt32(intBytes, 0);
    }

    /// <summary>
    /// 两个16位无符号整数转UInt
    /// </summary>
    private uint ConvertToUInt(string lowWordStr, string highWordStr)
    {
        if (!ushort.TryParse(lowWordStr, out ushort lowWord) || !ushort.TryParse(highWordStr, out ushort highWord))
            throw new FormatException($"UInt解析失败：低字={lowWordStr}，高字={highWordStr}");

        byte[] lowBytes = BitConverter.GetBytes(lowWord);
        byte[] highBytes = BitConverter.GetBytes(highWord);
        byte[] uintBytes = [lowBytes[0], lowBytes[1], highBytes[0], highBytes[1]];

        return BitConverter.ToUInt32(uintBytes, 0);
    }

    /// <summary>
    /// 四个16位无符号整数转ULong
    /// </summary>
    private ulong ConvertToULong(string w1Str, string w2Str, string w3Str, string w4Str)
    {
        if (!ushort.TryParse(w1Str, out ushort w1) || !ushort.TryParse(w2Str, out ushort w2) ||
            !ushort.TryParse(w3Str, out ushort w3) || !ushort.TryParse(w4Str, out ushort w4))
            throw new FormatException($"ULong解析失败：{w1Str},{w2Str},{w3Str},{w4Str}");

        byte[] bytes =
        [
            BitConverter.GetBytes(w1)[0], BitConverter.GetBytes(w1)[1],
            BitConverter.GetBytes(w2)[0], BitConverter.GetBytes(w2)[1],
            BitConverter.GetBytes(w3)[0], BitConverter.GetBytes(w3)[1],
            BitConverter.GetBytes(w4)[0], BitConverter.GetBytes(w4)[1]
        ];

        return BitConverter.ToUInt64(bytes, 0);
    }

    /// <summary>
    /// 四个16位无符号整数转Double
    /// </summary>
    private double ConvertToDouble(string w1Str, string w2Str, string w3Str, string w4Str)
    {
        if (!ushort.TryParse(w1Str, out ushort w1) || !ushort.TryParse(w2Str, out ushort w2) ||
            !ushort.TryParse(w3Str, out ushort w3) || !ushort.TryParse(w4Str, out ushort w4))
            throw new FormatException($"Double解析失败：{w1Str},{w2Str},{w3Str},{w4Str}");

        byte[] bytes =
        [
            BitConverter.GetBytes(w1)[0], BitConverter.GetBytes(w1)[1],
            BitConverter.GetBytes(w2)[0], BitConverter.GetBytes(w2)[1],
            BitConverter.GetBytes(w3)[0], BitConverter.GetBytes(w3)[1],
            BitConverter.GetBytes(w4)[0], BitConverter.GetBytes(w4)[1]
        ];

        return BitConverter.ToDouble(bytes, 0);
    }
    #endregion

    #region 通讯方法
    /// <summary>
    /// 解析PLC响应为字符串数组
    /// </summary>
    private string[] ParseResponse(string response, int expectedCount)
    {
        if (string.IsNullOrEmpty(response))
            throw new Exception("PLC返回空响应，通讯异常");

        // 拆分响应数据
        string[] parts = response.Split([' ', '\t', '\n'], StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < expectedCount)
            throw new Exception($"PLC响应数据不足：预期{expectedCount}个值，实际{parts.Length}个，响应内容：{response}");

        return parts;
    }
    #endregion

    #region 读
    /// <summary>
    /// 通用读
    /// </summary>
    /// <param name="address"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public override IoTResult<T> Read<T>(string address)
    {
        try
        {
            int registerCount = DataHelp.GetWordCount<T>();
            string typeSuffix = GetTypeSuffix<T>();

            string command = registerCount == 1 ? CmdReadSingle : CmdReadMultiple;
            string fullCommand = BuildHostLinkCommand(command, address, typeSuffix, registerCount);

           var receiveResult = Client.SendReceive(fullCommand);
            if (!receiveResult.IsSucceed)
                return receiveResult.ToVal<T>();

            string[] responseParts = ParseResponse(receiveResult.Value.Trim(), registerCount);
            var result = receiveResult.ToVal<T>(ConvertResponseToType<T>(responseParts));
            return result.ToEnd();
        }
        catch (Exception ex)
        {
            return IoTResult.Create<T>().AddError(ex).ToEnd();
        }
    }

    /// <summary>
    /// 通用批量读取
    /// </summary>
    /// <param name="address"></param>
    /// <param name="number"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public override IoTResult<IEnumerable<T>> Read<T>(string address, int number)
    {
        var result = IoTResult.Create<IEnumerable<T>>();
        try
        {
            if (number <= 0)
                throw new ArgumentOutOfRangeException(nameof(number), "读取数量必须大于0");

            // 获取每个数据需要的寄存器数量和类型后缀
            int registersPerItem = DataHelp.GetWordCount<T>();
            string typeSuffix = GetTypeSuffix<T>();
            int totalRegisters = number * registersPerItem;

            // 构建批量读取指令
            string fullCommand = BuildHostLinkCommand(CmdReadMultiple, address, typeSuffix, totalRegisters);

            // 发送指令并获取响应
            var receiveResult = Client.SendReceive(fullCommand);
            if (!receiveResult.IsSucceed)
                return receiveResult.ToVal<IEnumerable<T>>();

            // 解析响应
            string[] responseParts = ParseResponse(receiveResult.Value.Trim(), totalRegisters);

            // 将寄存器值分组并转换为数据类型
            var dataList = new List<T>();
            for (int i = 0; i < number; i++)
            {
                string[] itemParts = new string[registersPerItem];
                Array.Copy(responseParts, i * registersPerItem, itemParts, 0, registersPerItem);
                dataList.Add(ConvertResponseToType<T>(itemParts));
            }

            result.Value = dataList;
            result.IsSucceed = true;
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }
        return result.ToEnd();
    }

    /// <summary>
    /// 读取字符串
    /// </summary>
    /// <param name="address"></param>
    /// <param name="length"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public override IoTResult<string> ReadString(string address, int length, Encoding encoding)
    {
        var result = IoTResult.Create<string>();
        try
        {
            encoding ??= Client.Encoding;

            // 每个16位寄存器存2个ASCII字符
            int wordCount = (length + 1) / 2;

            // 构建读取指令
            string fullCommand = BuildHostLinkCommand(CmdReadMultiple, address, TypeHex16, wordCount);

            // 发送指令并获取响应
            var receiveResult = Client.SendReceive(fullCommand);
            if (!receiveResult.IsSucceed)
               return receiveResult.ToVal<string>();

            // 解析响应
            string[] responseParts = ParseResponse(receiveResult.Value.Trim(), wordCount);

            // 转换为字符串
            var sb = new StringBuilder();
            foreach (var part in responseParts)
            {
                var hexStr = part.PadLeft(4, '0'); // 补前导0确保4位十六进制
                // 高字节转字符
                byte highByte = Convert.ToByte(hexStr.Substring(0, 2), 16);
                // 低字节转字符
                byte lowByte = Convert.ToByte(hexStr.Substring(2, 2), 16);

                if (sb.Length < length) sb.Append(encoding.GetString([highByte]));
                if (sb.Length < length) sb.Append(encoding.GetString([lowByte]));
            }

            result.Value = sb.ToString().Substring(0, Math.Min(sb.Length, length));
            result.IsSucceed = true;
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }
        return result.ToEnd();
    }
    #endregion

    #region 写
    /// <summary>
    /// 通用写入
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public override IoTResult Write<T>(string address, T value)
    {
        var result = IoTResult.Create();
        try
        {
            // 获取寄存器数量和类型后缀
            int registerCount = DataHelp.GetWordCount<T>();
            string typeSuffix = GetTypeSuffix<T>();

            // 根据值的类型构建不同的写入命令
            Type type = typeof(T);

            if (type == typeof(bool))
            {
                // 布尔值写入处理
                WriteSingleBool(address, Convert.ToBoolean(value), out bool success);
                result.IsSucceed = success;
            }
            else if (registerCount == 1)
            {
                // 单寄存器写入
                string command = $"{CmdWriteSingle} {address}{typeSuffix} {value}{ProtocolTerminator}";
                var receiveResult = Client.SendReceive(command);
                if (!receiveResult.IsSucceed)
                    return receiveResult;

                var cleanResponse = receiveResult.Value.Trim().ToUpper();
                result.IsSucceed = string.IsNullOrEmpty(cleanResponse) || cleanResponse == "0" || cleanResponse == "OK";
            }
            else
            {
                // 多寄存器写入额外处理
                result.IsSucceed = WriteMultiRegisterValue(address, value, typeSuffix, registerCount);
            }
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }
        return result.ToEnd();
    }

    /// <summary>
    /// 通用写入（值集合）
    /// </summary>
    /// <param name="address"></param>
    /// <param name="values"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    public override IoTResult Write<T>(string address, IEnumerable<T> values)
    {
        var result = IoTResult.Create();
        try
        {
            var valueList = values?.ToList() ?? throw new ArgumentNullException(nameof(values));
            if (valueList.Count == 0)
                throw new ArgumentException("写入值集合不能为空", nameof(values));

            int registerCount = DataHelp.GetWordCount<T>();
            string typeSuffix = GetTypeSuffix<T>();

            if (registerCount != 1)
                throw new NotSupportedException($"批量写入暂不支持{typeof(T).Name}类型");

            // 构建批量写入指令
            string valuesStr = string.Join(" ", valueList.Select(v => v.ToString()));
            string command = $"{CmdWriteMultiple} {address}{typeSuffix} {valueList.Count} {valuesStr}{ProtocolTerminator}";

            var receiveResult = Client.SendReceive(command);
            if (!receiveResult.IsSucceed)
                return receiveResult;
            var cleanResponse = receiveResult.Value.Trim().ToUpper();
            result.IsSucceed = string.IsNullOrEmpty(cleanResponse) || cleanResponse == "0" || cleanResponse == "OK";
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }
        return result.ToEnd();
    }

    /// <summary>
    /// 写入字符串
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <param name="length"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public override IoTResult WriteString(string address, string value, int length, Encoding encoding)
    {
        var result = IoTResult.Create();
        try
        {
            value ??= string.Empty;

            // 算出需要的寄存器数量（每个寄存器存2个字符）
            var charCount = Math.Min(value.Length, length);
            var wordCount = (charCount + 1) / 2;

            // 将字符串转换为16进制字数组（每个字4位16进制）
            var hexWords = new StringBuilder();
            for (int i = 0; i < wordCount; i++)
            {
                // 取当前寄存器对应的两个字符
                char c1 = i * 2 < charCount ? value[i * 2] : '\0';
                char c2 = i * 2 + 1 < charCount ? value[i * 2 + 1] : '\0';

                // 转换为16进制（高字节+低字节）
                string hexHigh = ((byte)c1).ToString("X2");
                string hexLow = ((byte)c2).ToString("X2");
                hexWords.Append($"{hexHigh}{hexLow} ");
            }

            // 构建写入指令
            var command = $"{CmdWriteMultiple} {address}{TypeHex16} {wordCount} {hexWords.ToString().Trim()}{ProtocolTerminator}";
            var receiveResult = Client.SendReceive(command);
            if (!receiveResult.IsSucceed)
                return receiveResult;

            var cleanResponse = receiveResult.Value.Trim().ToUpper();
            result.IsSucceed = string.IsNullOrEmpty(cleanResponse) || cleanResponse == "0" || cleanResponse == "OK";
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }
        return result.ToEnd();
    }

    /// <summary>
    /// 写入单个布尔值
    /// </summary>
    private void WriteSingleBool(string bitAddress, bool value, out bool success)
    {
        try
        {
            // 带/不带R前缀的地址
            var parts = bitAddress.Split('.');
            if (parts.Length != 2)
                throw new ArgumentException("位地址格式错误，必须为「寄存器地址.位号」（如R100.1）", nameof(bitAddress));

            var registerAddress = parts[0].Trim();
            var bitNumber = parts[1].Trim();
            string actualAddress = registerAddress.StartsWith("R") ? $"{registerAddress}.{bitNumber}" : $"R{registerAddress}.{bitNumber}";

            // 构造写位指令：WR 位地址 0/1
            var writeValue = value ? "1" : "0";
            var command = $"{CmdWriteSingle} {actualAddress} {writeValue}{ProtocolTerminator}";
            var receiveResult = Client.SendReceive(command);
            if (!receiveResult.IsSucceed)
                throw new Exception(receiveResult.ErrorText);

            var cleanResponse = receiveResult.Value.Trim().ToUpper();
            success = string.IsNullOrEmpty(cleanResponse) || cleanResponse == "0" || cleanResponse == "OK";
        }
        catch
        {
            success = false;
        }
    }

    /// <summary>
    /// 写入多寄存器值
    /// </summary>
    private bool WriteMultiRegisterValue<T>(string address, T value, string typeSuffix, int registerCount)
    {
        try
        {
            Type type = typeof(T);
            string command;

            if (type == typeof(float))
            {
                float floatValue = Convert.ToSingle(value);
                var lh = ConvertFloatToUInt16Pair(floatValue);
                command = $"{CmdWriteMultiple} {address}{typeSuffix} {registerCount} {lh[0]} {lh[1]}{ProtocolTerminator}";
            }
            else if (type == typeof(int))
            {
                int intValue = Convert.ToInt32(value);
                var lh = ConvertIntToInt16Pair(intValue);
                command = $"{CmdWriteMultiple} {address}{typeSuffix} {registerCount} {lh[0]} {lh[1]}{ProtocolTerminator}";
            }
            else if (type == typeof(uint))
            {
                uint uintValue = Convert.ToUInt32(value);
                var lh = ConvertUIntToUInt16Pair(uintValue);
                command = $"{CmdWriteMultiple} {address}{typeSuffix} {registerCount} {lh[0]} {lh[1]}{ProtocolTerminator}";
            }
            else if (type == typeof(double))
            {
                double doubleValue = Convert.ToDouble(value);
                var w4 = ConvertDoubleToUInt16Quad(doubleValue);
                command = $"{CmdWriteMultiple} {address}{typeSuffix} {registerCount} {w4[0]} {w4[1]} {w4[2]} {w4[3]}{ProtocolTerminator}";
            }
            else if (type == typeof(ulong))
            {
                ulong ulongValue = Convert.ToUInt64(value);
                var w4 = ConvertULongToUInt16Quad(ulongValue);
                command = $"{CmdWriteMultiple} {address}{typeSuffix} {registerCount} {w4[0]} {w4[1]} {w4[2]} {w4[3]}{ProtocolTerminator}";
            }
            else
            {
                throw new NotSupportedException($"不支持的数据类型：{type.Name}");
            }

            var receiveResult = Client.SendReceive(command);
            if (!receiveResult.IsSucceed)
                throw new Exception(receiveResult.ErrorText);

            var cleanResponse = receiveResult.Value.Trim().ToUpper();
            return string.IsNullOrEmpty(cleanResponse) || cleanResponse == "0" || cleanResponse == "OK";
        }
        catch
        {
            return false;
        }
    }
    #endregion

    #region 数据类型转换
    /// <summary>
    /// Float转两个16位无符号整数（低字+高字）
    /// </summary>
    private ushort[] ConvertFloatToUInt16Pair(float value)
    {
        byte[] floatBytes = BitConverter.GetBytes(value);
        return new ushort[] { BitConverter.ToUInt16(floatBytes, 0), BitConverter.ToUInt16(floatBytes, 2) };
    }

    /// <summary>
    /// Int转两个16位有符号整数（低字+高字）
    /// </summary>
    private short[] ConvertIntToInt16Pair(int value)
    {
        byte[] intBytes = BitConverter.GetBytes(value);
        return new short[] { BitConverter.ToInt16(intBytes, 0), BitConverter.ToInt16(intBytes, 2) };
    }

    /// <summary>
    /// UInt转两个16位无符号整数（低字+高字）
    /// </summary>
    private ushort[] ConvertUIntToUInt16Pair(uint value)
    {
        byte[] uintBytes = BitConverter.GetBytes(value);
        return new ushort[] { BitConverter.ToUInt16(uintBytes, 0), BitConverter.ToUInt16(uintBytes, 2) };
    }

    /// <summary>
    /// ULong转四个16位无符号整数
    /// </summary>
    private ushort[] ConvertULongToUInt16Quad(ulong value)
    {
        byte[] ulongBytes = BitConverter.GetBytes(value);
        return new ushort[]
        {
            BitConverter.ToUInt16(ulongBytes, 0), BitConverter.ToUInt16(ulongBytes, 2),
            BitConverter.ToUInt16(ulongBytes, 4), BitConverter.ToUInt16(ulongBytes, 6),
        };
    }

    /// <summary>
    /// Double转四个16位无符号整数
    /// </summary>
    private ushort[] ConvertDoubleToUInt16Quad(double value)
    {
        byte[] doubleBytes = BitConverter.GetBytes(value);
        return new ushort[]
        {
            BitConverter.ToUInt16(doubleBytes, 0), BitConverter.ToUInt16(doubleBytes, 2),
            BitConverter.ToUInt16(doubleBytes, 4), BitConverter.ToUInt16(doubleBytes, 6),
        };
    }
    #endregion
}