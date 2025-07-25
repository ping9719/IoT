﻿# Ping9719.IoT
主要包含通信（TCP，UDP，USB...）协议（ModBus，MC，FINS...）算法（CRC，LRC...）设备（RFID，扫码枪...）

# 语言选择：
[简体中文](README.md) || [English](README_en-US.md) 

# 目录 
- [通讯 (Communication)](#Communication)
    - [客户端基础(ClientBase) 必读！！](#ClientBase)
        - 1.介绍 
        - [2.链接模式（ConnectionMode）](#ConnectionMode)
        - [3.数据处理器（IDataProcessor）](#IDataProcessor)
            - 3.1.介绍  
            - 3.2.自定义数据处理器
            - [3.3.内置的数据处理器](#IDataProcessorIn)
        - [4.接受模式（ReceiveMode）](#ReceiveMode)
    - [TcpClient](#TcpClient)
    - TcpServer （待测试） 
    - [SerialPortClient](#SerialPortClient)
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
    - [常用plc类型对照表，重要，必读 ！！！](#PlcType)
    - 罗克韦尔 (AllenBradleyCipClient) （待测试）   
    - [汇川 (InovanceModbusTcpClient)](#InovanceModbusTcpClient)
    - [三菱 (MitsubishiMcClient)](#MitsubishiMcClient)
    - [欧姆龙 (OmronFinsClient,OmronCipClient)](#OmronFinsClient)
    - [西门子 (SiemensS7Client)](#SiemensS7Client)
- [机器人 (Robot)](#Robot)
    - 爱普生 (EpsonRobot) （待测试） 
- [算法 (Algorithm)](#Algorithm)
    - [平均点位算法（AveragePoint）](#AveragePoint)
    - [CRC](#CRC)
    - LRC
    - 傅立叶算法（Fourier）（待开发） 
    - [稳定婚姻配对算法(GaleShapleyAlgorithm)](#GaleShapleyAlgorithm)
    - PID （待开发） 
    - RSA （待开发） 
- [设备和仪器 (Device)](#Device)
    - [气密检测 (Airtight)](#Airtight)
        - 科斯莫气密检测 (CosmoAirtight)
    - Fct
        - 盟讯电子 (MengXunFct)
    - [激光刻印 (Mark)](#Mark)
        - 大族激光刻印 (DaZhuMark)
        - 华普激光刻印 (HuaPuMark)
    - [无线射频 (Rfid)](#Rfid)
        - 倍加福Rfid (BeiJiaFuRfid)
        - 泰和森Rfid (TaiHeSenRfid)
        - 万全Rfid (WanQuanRfid)
    - [扫码枪 (Scanner)](#Scanner)
        - 霍尼韦尔扫码器 (HoneywellScanner)
        - 民德扫码器 (MindeoScanner)
    - [螺丝机 (Screw)](#Screw)
        - 快克螺丝机 (KuaiKeDeskScrew,KuaiKeScrew,KuaiKeTcpScrew)（不推荐） 
        - 米勒螺丝机 (MiLeScrew)
    - [温控 (TemperatureControl)](#TemperatureControl)
        - 快克温控 (KuaiKeTemperatureControl)（不推荐） 
    - [焊接机 (Weld)](#Weld)
        - 快克焊接机 (KuaiKeWeld)
- [扩展](#Ext)
    - 1.如何使用自定义协议

# 通讯 (Communication) <a id="Communication"></a>
## 客户端基础(ClientBase)  <a id="ClientBase"></a>
#### 1.介绍  
> 1. `ClientBase` 是所有客户端的基类 

#### 2.链接模式 <a id="ConnectionMode"></a>    

2.1 三种链接模式：   
> 1.手动（通用场景）。需要自己去打开和关闭，此方式比较灵活。     
> 2.自动打开（适用短链接）。没有执行Open()时每次发送和接收会自动打开和关闭，比较合适需要短链接的场景，如需要临时的长链接也可以调用Open()后在Close()。    
> 3.自动断线重连（适用长链接）。在执行了Open()后，如果检测到断开后会自动尝试断线重连，比较合适需要长链接的场景。调用Close()将不再重连。   

2.2 自动断线重连规则：   
> 1.当断开链接后进行尝试重连，第一次需等待1秒。   
> 2.如没有成功就继续增加一秒等待时间，直到达到最大重连时间（`MaxReconnectionTime`）。   
> 3.直到重连成功，或用户手动调用关闭(`Close()`)。   

2.3 简要代码：  
```CSharp
var client1 = new TcpClient("127.0.0.1", 8080);
client1.ConnectionMode = ConnectionMode.Manual;//手动。
client1.ConnectionMode = ConnectionMode.AutoOpen;//自动打开。
client1.ConnectionMode = ConnectionMode.AutoReconnection;//自动断线重连。
client1.MaxReconnectionTime = 10;//最大重连时间，单位秒。默认10秒。
```

## 3.数据处理器(IDataProcessor) <a id="IDataProcessor"></a>
#### 3.1.介绍  
> 1.在发送数据时可以对数据进行统一的处理后在发送 </br>
> 2.在接受数据后可以对数据进行处理后在转发出去  </br>
> 3.数据处理器可以多个叠加，先添加的先处理（所以某些情况下接受的处理器应该发送的处理器的是倒序）。

#### 3.2.自定义数据处理器   
1. 只需要你的类实现接口`IDataProcessor`就行了，比如：`public class MyCalss : IDataProcessor`。   

2. 开始使用自定义数据处理器
```CSharp
client1.SendDataProcessors.Add(new MyCalss());
client1.ReceivedDataProcessors.Add(new MyCalss());
```

#### 3.3.内置的数据处理器  <a id="IDataProcessorIn"></a>

| 名称| 说明 |
| ----------- | -------------- |
| EndAddValueDataProcessor   | 向结尾添加固定的值。比如结尾添加回车换行 |
| EndClearValueDataProcessor | 向结尾移除固定的值。比如结尾移除回车换行 |
| PadLeftDataProcessor   | 向左侧（头部）添加固定的值达到指定的长度。 |
| PadRightDataProcessor | 向右侧（尾部）添加固定的值达到指定的长度。 |
| StartAddValueDataProcessor   | 向开头添加固定的值 |
| StartClearValueDataProcessor | 向开头移除固定的值 |
| TrimDataProcessor   | 移除前后指定的匹配项。 |
| TrimEndDataProcessor   | 移除结尾指定的匹配项。 |
| TrimStartDataProcessor | 移除开头指定的匹配项。 |

## 4.接受模式（ReceiveMode）  <a id="ReceiveMode"></a>
#### 4.1.数据接受介绍 
在客户端中有2处可以接受到数据，1是事件`Received`，2是方法`Receive()`或`SendReceive()`。其中方法的优先级大于事件，方法如果接受到数据了，事件将不会再接受到。
```CSharp
//在方法中的默认方式
client.ReceiveMode = ReceiveMode.ParseByteAll();
//在事件中的默认方式
client.ReceiveModeReceived = ReceiveMode.ParseByteAll();
```
接受的数据先通过‘接受模式’进行分开每一帧，在经过‘数据处理器’处理数据   
> 假如对方给你发送字符串“ab\r\n”和“cd\r\n”他们之间间隔了100毫秒。   
> 这里“ab\r\n”为一帧，“a”为一位的意思。    
> 假如每一帧之间相距100ms，每一位之间相距1ms。    

| 代码                                 | 结果      | 说明 | 推荐场景 | 
| ------------------------------------ | --------- |------------ | ------------ | 
| `ReceiveMode.ParseByte(2)`           | ab        | 读取指定的字节数量 | 在通信协议里面已经指定了一帧是固定长度的情况下或已知道剩余接收的长度的情况下 | 
| `ReceiveMode.ParseByteAll()`         | a或ab\r\n | 读取所有立即可用的字节 | 需要高效率又什么都不知道的情况下。tcp等协议一般一帧是全部信息，串口一般一帧是一位信息 | 
| `ReceiveMode.ParseChar(1)`         | a         | 读取指定的字符数量 | 同 `ParseByte()` | 
| `ReceiveMode.ParseTime(10)`          | ab\r\n    | 读取达到指定的时间间隔后没有新消息后结束 | 在什么都不知道的情况下又想获取完整信息的妥协方案，代价是牺牲指定的时间，一般在串口中默认 | 
| `ReceiveMode.ParseToEnd("\r\n") ` | ab\r\n    | 读取到指定的信息后结束 | 知道每一帧的结尾的情况下 | 

## TcpClient <a id="TcpClient"></a>
`TcpClient : ClientBase`
```CSharp
var client1 = new TcpClient("127.0.0.1", 8080);
//重要！！！连接模式是非常重要的功能，有3种模式 
client1.ConnectionMode = ConnectionMode.Manual;//手动。需要自己去打开和关闭，此方式比较灵活。
client1.ConnectionMode = ConnectionMode.AutoOpen;//自动打开。没有执行Open()时每次发送和接收会自动打开和关闭，比较合适需要短链接的场景，如需要临时的长链接也可以调用Open()后在Close()。
client1.ConnectionMode = ConnectionMode.AutoReconnection;//自动断线重连。在执行了Open()后，如果检测到断开后会自动打开，比较合适需要长链接的场景。调用Close()将不再重连。
client1.Encoding = Encoding.UTF8;
//数据处理器，发送加入换行，接受去掉换行
client1.SendDataProcessors.Add(new EndAddValueDataProcessor("\r\n", client1.Encoding));
client1.ReceivedDataProcessors.Add(new EndClearValueDataProcessor("\r\n", client1.Encoding));
//接受模式
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

//打开链接，设置所有属性必须在打开前
client1.Open();

//发送或接收数据 
client1.Send("abc");//发送
client1.Receive();//接收
client1.Receive(3000);//接收，3秒超时
client1.Receive(ReceiveMode.ParseToString("\n", 5000));//接收字符串结尾为\n的，超时为5秒 
client1.SendReceive("abc", 3000);//发送并接收，3秒超时
client1.SendReceive("abc", ReceiveMode.ParseToString("\n", 5000));//发送并接收 ，超时为5秒 
```

## TcpServer
`TcpServer : ServiceBase`
```CSharp
var service = new TcpService("127.0.0.1",8005);
```


## SerialPortClient <a id="SerialPortClient"></a>
`SerialPortClient : ClientBase`
```CSharp
var client1 = new SerialPortClient("COM1", 9600);

//以下是初始化默认属性，可以不设置
client1.ConnectionMode = ConnectionMode.Manual;//手动打开，串口使用断线重连意义不大
client1.Encoding = Encoding.ASCII;//如何解析字符串
client1.TimeOut = 3000;//超时时间
client1.ReceiveMode = ReceiveMode.ParseTime();//方法“Receive()”的默认方式，串口根据时间来接收数据更好
client1.ReceiveModeReceived = ReceiveMode.ParseTime();//时间“Received”的默认方式

//所有事件和TcpClient一样，这里不在重复

//打开链接，设置所有属性必须在打开前
client1.Open();

//所有发送和接收和TcpClient一样，这里不在重复
```

# Modbus <a id="Modbus"></a>
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

# PLC <a id="PLC"></a>
## 常用plc类型对照表 <a id="PlcType"></a>
> 带 * 号的为常用类型，一般情况下没有特殊说明就是全部支持的。

| C#</br>.Net | 西门子S7</br>SiemensS7 | 三菱MC</br>MitsubishiMc | 欧姆龙Fins</br>OmronFins |欧姆龙Cip</br>OmronCip |汇川</br>Inovance |
| ----------- | ---------------------- | ----------------------- | ------------------------ | --------------------- | ---------------- |
| Bool        |Bool|||BOOL||
| Byte        |Byte|||BYTE||
| Float *     |Real|||REAL||
| Double *    |LReal|||LREAL||
| Int16 *     |Int|||INT||
| Int32 *     |DInt|||DINT||
| Int64 *     ||||LINT||
| UInt16 *    |Word|||UINT||
| UInt32 *    |DWord|||UDINT||
| UInt64 *    ||||ULINT||
| string      |String|||STRING||
| DateTime    |Date|||DATE_AND_TIME||
| TimeSpan    |Time|||||
| Char        |Char|||||

## 罗克韦尔 (AllenBradleyCipClient)
```CSharp
//部分机器可使用OmronCipClient替代 
AllenBradleyCipClient client = new AllenBradleyCipClient("127.0.0.1");
client.Read<bool>("abc");//读
client.Write<bool>("abc",true);//写
```

## 汇川 (InovanceModbusTcpClient) <a id="InovanceModbusTcpClient"></a>
```CSharp
InovanceModbusTcpClient client = new InovanceModbusTcpClient("127.0.0.1");
client.Read<bool>("M1");//读
client.Read<Int16>("D1");//读
client.Write<bool>("M1",true);//写
client.Write<Int16>("D1",12);//写
```

## 三菱 (MitsubishiMcClient) <a id="MitsubishiMcClient"></a>
### 测试覆盖表

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

## 欧姆龙 (OmronFinsClient) <a id="OmronFinsClient"></a>
```CSharp
OmronFinsClient client = new OmronFinsClient("127.0.0.1");
client.Read<Int16>("W0");//读
client.Write<Int16>("W0",10);//写
```

## 欧姆龙 (OmronCipClient)
```CSharp
OmronCipClient client = new OmronCipClient("127.0.0.1");
client.Read<bool>("abc");//读
client.Write<bool>("abc",true);//写
```

## 西门子 (SiemensS7Client) <a id="SiemensS7Client"></a>
```CSharp
SiemensS7Client client = new SiemensS7Client("127.0.0.1");
//读写支持：基础(int,float...),在加额外的：string、DateTime、TimeSpan、Char
client.Read<Int16>("BD100");//读
client.Write<Int16>("BD100",10);//写

//字符串
client.Read<string>("BD100");//plc的类型必须为string，只支持字母数字等ASCII编码
client.ReadString("BD100");//plc的类型必须为WString，支持中文等UTF16编码，
```

# 机器人 (Robot) <a id="Robot"></a>
## 爱普生 (EpsonRobot)
```CSharp
EpsonRobot client = new EpsonRobot("127.0.0.1");
client.Client.Open();
client.Start();
client.Pause();
```

# 算法 (Algorithm) <a id="Algorithm"></a>

## 平均点位算法（AveragePoint） <a id="AveragePoint"></a>
> 简单的平均算法，假如开头为2，结尾为8，一共4个点，每个点距离相同。如图所示：   
> 2--[4]--[6]--8   
> 就可知道中间点位为4和6

```CSharp
//输出的结果：
//0[2, 2.5]
//1[4, 3]
//2[6, 3.5]
//3[8, 4]
var aaa = AveragePoint.Start("2,2.5", " 8,4", 4);
var aaa = AveragePoint.Start(new double[] { 2, 2.5 }, new double[] { 8, 4 }, 4);

//结果：[2, 4, 6, 8]
var aaa = AveragePoint.Start(2, 8, 4);
```

## CRC <a id="CRC"></a>
```CSharp
byte[] bytes = new byte[] { 1, 2 };
//CRC 算法
var c1 = CRC.Crc8(bytes);
var c2 = CRC.Crc8Itu(bytes);
var c3 = CRC.Crc8Rohc(bytes);
var c4 = CRC.Crc16(bytes);
var c5 = CRC.Crc16Usb(bytes);
var c6 = CRC.Crc16Modbus(bytes);
var c7 = CRC.Crc32(bytes);
var c8 = CRC.Crc32Q(bytes);
var c9 = CRC.Crc32Sata(bytes);
//CRC 验证
CRC.CheckCrc8(c1);
CRC.CheckCrc8Itu(c2);
CRC.CheckCrc8Rohc(c3);
CRC.CheckCrc16(c4);
CRC.CheckCrc16Usb(c5);
CRC.CheckCrc16Modbus(c6);
CRC.CheckCrc32(c7);
CRC.CheckCrc32Q(c8);
CRC.CheckCrc32Sata(c9);
```
## LRC
```CSharp
LRC.GetLRC(bytes);
LRC.CheckLRC(bytes);
```
## 稳定婚姻配对算法(GaleShapleyAlgorithm) <a id="GaleShapleyAlgorithm"></a>
>假如偏好关系如下（越喜欢的越靠前）：<br/>
M0❤W1,W0<br/>M1❤W0,W1<br/>M2❤W0,W1,W2,W3,W4<br/>M3❤W3<br/>M4❤W3

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

# 设备和仪器 (Device) <a id="Device"></a>

各种仪器需要长链接必须打开 `dev1.Client.Open();` 需要自动打开请设置 `dev1.Client.ConnectionMode = ConnectionMode.AutoOpen;` 以下列子中不在重复对客户端相关的描述或设置，非常重要或不一致除外。
   


## 科斯莫气密检测 (CosmoAirtight) <a id="CosmoAirtight"></a>
```CSharp
CosmoAirtight dev1 = new CosmoAirtight("COM1");//科斯莫
```

## Fct
```CSharp
MengXunFct dev1 = new MengXunFct("127.0.0.1");//盟讯电子
```
## 激光刻印 (Mark) <a id="Mark"></a>
```CSharp
DaZhuMark dev1 = new DaZhuMark("127.0.0.1");//大族
HuaPuMark dev2 = new HuaPuMark("127.0.0.1");//华普
```
## 无线射频 (Rfid) <a id="Rfid"></a>
```CSharp
BeiJiaFuRfid rfid1 = new BeiJiaFuRfid("127.0.0.1");//倍加福
DongJiRfid rfid2 = new DongJiRfid("127.0.0.1");//东集
TaiHeSenRfid rfid3 = new TaiHeSenRfid("127.0.0.1");//泰和森
WanQuanRfid rfid4 = new WanQuanRfid("127.0.0.1");//万全

//万全使用方式 
rfid4.ReadString(RfidAddress.GetRfidAddressStr(RfidArea.ISO15693, null, 1), 2, EncodingEnum.ASCII.GetEncoding());
rfid4.WriteString(RfidAddress.GetRfidAddressStr(RfidArea.ISO15693, null, 1), "A001", 2, EncodingEnum.ASCII.GetEncoding());
```
## 扫码枪 (Scanner) <a id="Scanner"></a>
```CSharp
HoneywellScanner dev1 = new HoneywellScanner("127.0.0.1");//霍尼韦尔
MindeoScanner dev1 = new MindeoScanner("127.0.0.1");//民德
```
## 螺丝机 (Screw) <a id="Screw"></a>
```CSharp
MiLeScrew dev1 = new MiLeScrew("127.0.0.1");//米勒
```
## 温控 (TemperatureControl) <a id="TemperatureControl"></a>
```CSharp
//快克温控不推荐
```
## 焊接机 (Weld) <a id="Weld"></a>
```CSharp
KuaiKeWeld dev1 = new KuaiKeWeld("COM1");
```

# 扩展 <a id="Ext"></a>
## 1.如何使用自定义协议
```CSharp
//XXX协议实现
public class XXX
{
    public ClientBase Client { get; private set; }//通讯管道

    public XXX(ClientBase client)
    {
        Client = client;
        //Client.ReceiveMode = ReceiveMode.ParseTime();
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
