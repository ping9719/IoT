# Ping9719.IoT
主要包含通信（TCP，UDP，USB...）协议（ModBus，MC，FINS...）算法（CRC，LRC...）设备（RFID，扫码枪...）

# 语言选择：
[简体中文](README.md) </br>
[English](README_en-US.md) </br>

# 目录 
- [通讯 (Communication)](#通讯 (Communication))
    - TcpClient
    - TcpServer （待测试） 
    - SerialPortClient
    - UdpClient （进行中） 
    - UdpServer （待开发） 
    - HttpServer （待开发） 
    - MqttClient （待开发） 
    - MqttServer （待开发） 
- [Modbus](#Modbus)
    - ModbusRtuClient
    - ModbusTcpClient
    - ModbusAsciiClient
- [PLC](#PLC)
    - 罗克韦尔 (AllenBradleyCipClient) （待测试）   
    - 汇川 (InovanceModbusTcpClient)
    - 三菱 (MitsubishiMcClient)
    - 欧姆龙 (OmronFinsClient,OmronCipClient)
    - 西门子 (SiemensS7Client)
- [机器人 (Robot)](#机器人 (Robot))
    - 爱普生 (EpsonRobot) （待测试） 
- [算法 (Algorithm)](#算法 (Algorithm))
    - CRC
    - LRC
    - 傅立叶算法(Fourier) （待开发） 
    - 稳定婚姻配对算法(GaleShapleyAlgorithm)
    - PID （待开发） 
    - RSA （待开发） 
- [设备和仪器 (Device)](#设备和仪器 (Device))
    - 气密检测 (Airtight)
        - 科斯莫气密检测 (CosmoAirtight)
    - Fct
        - 盟讯电子 (MengXunFct)
    - 激光刻印 (Mark)
        - 大族激光刻印 (DaZhuMark)
        - 华普激光刻印 (HuaPuMark)
    - 无线射频 (Rfid)
        - 倍加福Rfid (BeiJiaFuRfid)
        - 泰和森Rfid (TaiHeSenRfid)
        - 万全Rfid (WanQuanRfid)
    - 扫码枪 (Scanner)
        - 霍尼韦尔扫码器 (HoneywellScanner)
        - 民德扫码器 (MindeoScanner)
    - 螺丝机 (Screw)
        - 快克螺丝机 (KuaiKeDeskScrew,KuaiKeScrew,KuaiKeTcpScrew)（不推荐） 
        - 米勒螺丝机 (MiLeScrew)
    - 温控 (TemperatureControl)
        - 快克温控 (KuaiKeTemperatureControl)（不推荐） 
    - 焊接机 (Weld)
        - 快克焊接机 (KuaiKeWeld)
- [扩展](#扩展)
    - [1.如何使用自定义协议](#1.如何使用自定义协议)

# 通讯 (Communication)
## TcpClient
`TcpClient : ClientBase`
```CSharp
var client1 = new TcpClient("127.0.0.1", 8080);
client1.ConnectionMode = ConnectionMode.AutoReconnection;//断线重连，默认手动
client1.Encoding = Encoding.UTF8;//如何解析字符串
client1.TimeOut = 3000;//超时时间
client1.ReceiveMode = ReceiveMode.ParseByteAll();//方法“Receive()”的默认方式
client1.ReceiveModeReceived = ReceiveMode.ParseByteAll();//时间“Received”的默认方式
client1.Opening = (a) =>
{
    Console.WriteLine("连接中");
    return true;
};
client1.Opened = (a) =>
{
    Console.WriteLine("连接成功");
};
client1.Closing = (a) =>
{
    Console.WriteLine("关闭中");
    return true;
};
client1.Closed = (a, b) =>
{
    Console.WriteLine("关闭成功" + b);
};
client1.Received = (a, b) =>
{
    Console.WriteLine("收到消息:" + a.Encoding.GetString(b));
};
client1.Warning = (a, b) =>
{
    Console.WriteLine("错误" + b.ToString());
};
//打开链接，设置所有属性必须在打开前
client1.Open();

client1.Send("abc");//发送
client1.Receive();//等待并接受
client1.Receive(ReceiveMode.ParseByteAll(6000));//读取所有，超时为6秒 
client1.Receive(ReceiveMode.ParseByte(10, 6000));//读取10个字节，超时为6秒 
client1.Receive(ReceiveMode.ParseToString("\n", 6000));//读取字符串结尾为\n的，超时为6秒 
client1.SendReceive("abc", ReceiveMode.ParseToString("\n", 6000));//发送并读取字符串结尾为\n的，超时为6秒 
```

## TcpServer
`TcpServer : ServiceBase`
```CSharp
var service = new TcpService("127.0.0.1",8005);
```


## SerialPortClient
`SerialPortClient : ClientBase`
```CSharp
var client1 = new SerialPortClient("COM1", 9600);

//以下是初始化默认属性，可以不设置
client1.ConnectionMode = ConnectionMode.Manual;//手动打开，串口使用断线重连意义不大
client1.Encoding = Encoding.ASCII;//如何解析字符串
client1.TimeOut = 3000;//超时时间
client1.ReceiveMode = ReceiveMode.ParseTime();//方法“Receive()”的默认方式，串口根据时间来接受数据更好
client1.ReceiveModeReceived = ReceiveMode.ParseTime();//时间“Received”的默认方式

//所有事件和TcpClient一样，这里不在重复

//打开链接，设置所有属性必须在打开前
client1.Open();

//所有发送和接受和TcpClient一样，这里不在重复
```

# Modbus
`ModbusRtuClient : IIoT`   
`ModbusTcpClient : IIoT`   
`ModbusAsciiClient : IIoT`
```CSharp
var client = new ModbusRtuClient("COM1", 9600, format: EndianFormat.ABCD);
var client = new ModbusRtuClient(new TcpClient("127.0.0.1", 502), format: EndianFormat.ABCD);//ModbusRtu协议走TCP
var client = new ModbusTcpClient("127.0.0.1", 502, format: EndianFormat.ABCD);
var client = new ModbusTcpClient(new SerialPortClient("COM1", 9600), format: EndianFormat.ABCD);//ModbusTcp协议走串口
client.Client.Open();

client.Read<Int16>("100");//读寄存器
client.Read<Int16>("100.1");//读寄存器中的位，读位只支持单个读，最好是uint16,int16
client.Read<Int16>("s=2;x=3;100");//读寄存器，对应站号，功能码，地址
client.Read<bool>("100");//读线圈
client.Read<bool>("100", 10);//读多个线圈

client.Write<Int16>("100", 100);//写寄存器
client.Write<Int16>("100", 100, 110);//写多个寄存器

client.ReadString("100", 5, Encoding.ASCII);//读字符串
client.ReadString("100", 5, null);//读字符串，以16进制的方式
client.WriteString("500", "abcd", 10, Encoding.ASCII);//写字符串，数量>0时且不足会自动在结尾补充0X00在结尾
```

# PLC
## 常用plc类型对照表
| C#</br>.Net | 西门子S7</br>SiemensS7 | 三菱MC</br>MitsubishiMc | 欧姆龙Fins</br>OmronFins |欧姆龙Cip</br>OmronCip |汇川</br>Inovance |
| -- | -------- | ------ | ---------- | -------- | --- |
| Bool |Bool|||BOOL||
| Byte |Byte|||BYTE||
| Float |Real|||REAL||
| Double |LReal|||LREAL||
| Int16 |Int|||INT||
| Int32 |DInt|||DINT||
| Int64 ||||LINT||
| UInt16 |Word|||UINT||
| UInt32 |DWord|||UDINT||
| UInt64 ||||ULINT||
| string ||||STRING||
| DateTime ||||DATE_AND_TIME||

## 罗克韦尔 (AllenBradleyCipClient)
```CSharp
//部分机器可使用OmronCipClient替代 
AllenBradleyCipClient client = new AllenBradleyCipClient("127.0.0.1");
client.Read<bool>("abc");//读
client.Write<bool>("abc",true);//写
```

## 汇川 (InovanceModbusTcpClient)
```CSharp
InovanceModbusTcpClient client = new InovanceModbusTcpClient("127.0.0.1");
client.Read<bool>("M1");//读
client.Read<Int16>("D1");//读
client.Write<bool>("M1",true);//写
client.Write<Int16>("D1",12);//写
```

## 三菱 (MitsubishiMcClient)
### 三菱MC客户端功能测试覆盖表

| 类型         | 单点读写         | 批量读写（数组）         |
|--------------|------------------|-------------------------|
| bool         | ✔️               | ✔️（循环单点写入，较慢） |
| short        | ✔️               | ✔️                      |
| int32        | ✔️               | ✔️                      |
| float        | ✔️               | ✔️                      |
| double       | ✔️               | ✔️                      |
| string       | ✔️               | ✔️                      |

> 注：bool数组批量写入采用循环单点写入方式，速度相对较慢。
>
> ​        还支持byte、sbyte、ushort、uint32、int64、uint64类型。由于用到的情况较少，请自行测试。


```CSharp
MitsubishiMcClient client = new MitsubishiMcClient("127.0.0.1");
client.Read<Int16>("W0");//读
client.Write<Int16>("W0",10);//写
```

## 欧姆龙 (OmronCipClient)
```CSharp
OmronCipClient client = new OmronCipClient("127.0.0.1");
client.Read<bool>("abc");//读
client.Write<bool>("abc",true);//写
```

## 欧姆龙 (OmronFinsClient)
```CSharp
OmronFinsClient client = new OmronFinsClient("127.0.0.1");
client.Read<Int16>("W0");//读
client.Write<Int16>("W0",10);//写
```
## 西门子 (SiemensS7Client)
```CSharp
SiemensS7Client client = new SiemensS7Client("127.0.0.1");
client.Read<Int16>("BD100");//读
client.Write<Int16>("BD100",10);//写
```

# 机器人 (Robot)
## 爱普生 (EpsonRobot)
```CSharp
EpsonRobot client = new EpsonRobot("127.0.0.1");
client.Client.Open();
client.Start();
client.Pause();
```

# 算法 (Algorithm)
## CRC
```CSharp
byte[] bytes = new byte[0];
//CRC
CRC.Crc8(bytes);
CRC.Crc8Itu(bytes);
CRC.Crc8Rohc(bytes);
CRC.Crc16(bytes);
CRC.Crc16Usb(bytes);
CRC.Crc16Modbus(bytes);
CRC.Crc32(bytes);
CRC.Crc32Q(bytes);
CRC.Crc32Sata(bytes);
//CRC 验证
CRC.CheckCrc8(bytes);
CRC.CheckCrc8Itu(bytes);
CRC.CheckCrc8Rohc(bytes);
CRC.CheckCrc16(bytes);
CRC.CheckCrc16Usb(bytes);
CRC.CheckCrc16Modbus(bytes);
CRC.CheckCrc32(bytes);
CRC.CheckCrc32Q(bytes);
CRC.CheckCrc32Sata(bytes);
```
## LRC
```CSharp
LRC.GetLRC(bytes);
LRC.CheckLRC(bytes);
```
## 稳定婚姻配对算法(GaleShapleyAlgorithm)
```CSharp
//1.参与配对的所有人
var ms = new string[] { "M0", "M1", "M2", "M3", "M4", };
var ws = new string[] { "W0", "W1", "W2", "W3", "W4", };
//2.转为项
var msi = ms.Select(o => new GaleShapleyItem<string>(o)).ToList();
var wsi = ms.Select(o => new GaleShapleyItem<string>(o)).ToList();
//3.配置偏好列表。假如喜欢关系如下：（喜欢程度从高到低）
//M0❤W1,W0   M1❤W0,W1   M2❤W0,W1,W2,W3,W4   M3❤W3   M4❤W3
msi[0].Preferences = new List<GaleShapleyItem<string>>() { wsi[1], wsi[0] };
msi[1].Preferences = new List<GaleShapleyItem<string>>() { wsi[0], wsi[1] };
msi[2].Preferences = new List<GaleShapleyItem<string>>() { wsi[0], wsi[1], wsi[2], wsi[3], wsi[4] };
msi[3].Preferences = new List<GaleShapleyItem<string>>() { wsi[3] };
msi[4].Preferences = new List<GaleShapleyItem<string>>() { wsi[3] };
//4.开始计算
GaleShapleyAlgorithm.Run(msi);
//5.打印结果
foreach (var item in msi)
{
    //M0❤M1   M1❤M0   M2❤M2   M3❤M3   M4❤null
    Console.Write($"{item.Item}❤{(item.Match?.Item) ?? "null"}   ");
}
```

# 设备和仪器 (Device)

各种仪器需要长链接必须打开 `dev1.Client.Open();` 需要自动打开请设置 `dev1.Client.ConnectionMode = ConnectionMode.AutoOpen;` 以下列子中不在重复对客户端相关的描述或设置，非常重要或不一致除外。
   


## 科斯莫气密检测 (CosmoAirtight)
```CSharp
CosmoAirtight dev1 = new CosmoAirtight("COM1");//科斯莫
```

## Fct
```CSharp
MengXunFct dev1 = new MengXunFct("127.0.0.1");//盟讯电子
```
## 激光刻印 (Mark)
```CSharp
DaZhuMark dev1 = new DaZhuMark("127.0.0.1");//大族
HuaPuMark dev2 = new HuaPuMark("127.0.0.1");//华普
```
## 无线射频 (Rfid)
```CSharp
BeiJiaFuRfid rfid1 = new BeiJiaFuRfid("127.0.0.1");//倍加福
DongJiRfid rfid2 = new DongJiRfid("127.0.0.1");//东集
TaiHeSenRfid rfid3 = new TaiHeSenRfid("127.0.0.1");//泰和森
WanQuanRfid rfid4 = new WanQuanRfid("127.0.0.1");//万全

//万全使用方式 
rfid4.ReadString(RfidAddress.GetRfidAddressStr(RfidArea.ISO15693, null, 1), 2, EncodingEnum.ASCII.GetEncoding());
rfid4.WriteString(RfidAddress.GetRfidAddressStr(RfidArea.ISO15693, null, 1), "A001", 2, EncodingEnum.ASCII.GetEncoding());
```
## 扫码枪 (Scanner)
```CSharp
HoneywellScanner dev1 = new HoneywellScanner("127.0.0.1");//霍尼韦尔
MindeoScanner dev1 = new MindeoScanner("127.0.0.1");//民德
```
## 螺丝机 (Screw)
```CSharp
MiLeScrew dev1 = new MiLeScrew("127.0.0.1");//米勒
```
## 温控 (TemperatureControl)
```CSharp
//快克温控不推荐
```
## 焊接机 (Weld)
```CSharp
KuaiKeWeld dev1 = new KuaiKeWeld("COM1");
```

# 扩展
## 1.如何使用自定义协议
```CSharp
//XXX协议实现
public class XXX
{
    public ClientBase Client { get; private set; }//通讯管道

    public XXX(ClientBase client)
    {
        Client = client;
        Client.ReceiveMode = ReceiveMode.ParseTime();
        Client.Encoding = Encoding.ASCII;
        Client.ConnectionMode = ConnectionMode.AutoOpen;
    }

    //默认使用TcpClient
    public XXX(string ip, int port = 1500) : this(new TcpClient(ip, port)) { }
    //默认使用SerialPortClient
    //public XXX(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, Handshake handshake = Handshake.None) : this(new SerialPortClient(portName, baudRate, parity, dataBits, stopBits, handshake)) { }

    //这是一个示例，他发送“info1\r\n” 并等待返回字符串的结果
    public IoTResult ReadXXX()
    {
        string comm = $"info1\r\n";
        try
        {
            return Client.SendReceive(comm);
        }
        catch (Exception ex)
        {
            return IoTResult.Create().AddError(ex);
        }
    }

}

//使用
var client = new XXX("127.0.0.1");
client.Client.Open();
var info = client.ReadXXX();
```
