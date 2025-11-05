
# 语言选择：   
[简体中文](README.md) || [English](README_en-US.md) 

# 目录 
- [通讯 (Communication)](#Communication)
    - [客户端（ClientBase）](#ClientBase)
        - [1.连接模式（ConnectionMode）](#ConnectionMode)
        - [2.接收模式（ReceiveMode）](#ReceiveMode)
        - [3.数据处理器（IDataProcessor）](#IDataProcessor)
        - [4.心跳（Heartbeat）](#Heartbeat)
    - [TcpClient](#TcpClient)
    - [TcpServer](#TcpServer)
    - [SerialPortClient (串口)](#SerialPortClient)
    - [UsbHidClient (USB)](#UsbHidClient)
    - [BleClient (蓝牙)](#BleClient)
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
    - [类型对照表](#PlcType)
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
    - [LRC](#LRC)
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
- [常见问题](#Issue)
    - 1.如何使用自定义协议？

# 通讯 (Communication) <a id="Communication"></a>
使用指定的方式进行交互信息。
## 客户端(ClientBase)  <a id="ClientBase"></a>
`TcpClient`或`SerialPortClient`都是实现于`ClientBase`，他们的使用方式都是一样的。

### 1.链接模式 <a id="ConnectionMode"></a>    

三种链接模式：   
> 1.手动（通用场景）。需要自己去打开和关闭，此方式比较灵活。     
2.自动打开（适用短链接）。没有执行Open()时每次发送和接收会自动打开和关闭，比较合适需要短链接的场景，如需要临时的长链接也可以调用Open()后在Close()。    
3.自动断线重连（适用长链接）。在执行了Open()后，如果检测到断开后会自动尝试断线重连，比较合适需要长链接的场景。调用Close()将不再重连。   

自动断线重连规则：   
> 1.当断开链接后进行尝试重连，第一次需等待1秒。   
> 2.如没有成功就继续增加一秒等待时间，直到达到最大重连时间（`MaxReconnectionTime`）。   
> 3.直到重连成功，或用户手动调用关闭(`Close()`)。   

简要代码：  
```CSharp
var client1 = new TcpClient("127.0.0.1", 8080);
client1.ConnectionMode = ConnectionMode.Manual;//手动，系统默认。
client1.ConnectionMode = ConnectionMode.AutoOpen;//自动打开。
client1.ConnectionMode = ConnectionMode.AutoReconnection;//自动断线重连。
client1.MaxReconnectionTime = 10;//最大重连时间，单位秒。默认10秒。
```

### 2.接收模式（ReceiveMode）  <a id="ReceiveMode"></a>
数据接收介绍 
在客户端中有2处可以接收到数据，1是事件`Received`，2是方法`Receive()`或`SendReceive()`。其中方法的优先级大于事件，方法如果接收到数据了，事件将不会再接收到。
```CSharp
//在方法中的默认方式
client.ReceiveMode = ReceiveMode.ParseByteAll();
//在事件中的默认方式
client.ReceiveModeReceived = ReceiveMode.ParseByteAll();
```
接收的数据先通过‘接收模式’进行分开每一帧，在经过‘数据处理器’处理数据   
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

### 3.数据处理器(IDataProcessor) <a id="IDataProcessor"></a>
介绍  
> 1.在发送数据时可以对数据进行统一的处理后在发送 </br>
> 2.在接收数据后可以对数据进行处理后在转发出去  </br>
> 3.数据处理器可以多个叠加，先添加的先处理（所以某些情况下接收的处理器应该发送的处理器的是倒序）。

内置的数据处理器  <a id="IDataProcessorIn"></a>

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

自定义数据处理器   
1. 只需要你的类实现接口`IDataProcessor`就行了，比如：`public class MyCalss : IDataProcessor`。   

2. 开始使用自定义数据处理器
```CSharp
client1.SendDataProcessors.Add(new MyCalss());
client1.ReceivedDataProcessors.Add(new MyCalss());
```
### 4.心跳（Heartbeat） <a id="Heartbeat"></a>
自定义每隔多少的间隔操作指定的方法。   
注意：在`ConnectionMode.AutoOpen`模式下不生效心跳。
```CSharp
//心跳间隔。默认为5秒。设置为0可以暂停发送心跳
client1.HeartbeatTime = 5000;
//每次发送“1”并告知心跳结果。
client1.Heartbeat = (a) =>
{
    var aa = a.Send("1");
    return aa.IsSucceed;
};

client1.Open();//打开，在打开前处理属性和事件
```

## TcpClient <a id="TcpClient"></a>
`TcpClient : ClientBase`
```CSharp
ClientBase client1 = new TcpClient("127.0.0.1", 502);
client1.Encoding = Encoding.UTF8;

//1：连接模式。断线重连使用得比较多
client1.ConnectionMode = ConnectionMode.Manual;//手动，系统默认。需要自己去打开和关闭，此方式比较灵活。
client1.ConnectionMode = ConnectionMode.AutoOpen;//自动打开。没有执行Open()时每次发送和接收会自动打开和关闭，比较合适需要短链接的场景，如需要临时的长链接也可以调用Open()后在Close()。
client1.ConnectionMode = ConnectionMode.AutoReconnection;//自动断线重连。在执行了Open()后，如果检测到断开后会自动打开，比较合适需要长链接的场景。调用Close()将不再重连。

//2：接收模式。以您以为的最好的方式来处理粘包问题
client1.ReceiveMode = ReceiveMode.ParseByteAll();
client1.ReceiveModeReceived = ReceiveMode.ParseByteAll();

//3：数据处理器。可在发送时加入换行，接收时去掉换行，也可自定义
client1.SendDataProcessors.Add(new EndAddValueDataProcessor("\r\n", client1.Encoding));
client1.ReceivedDataProcessors.Add(new EndClearValueDataProcessor("\r\n", client1.Encoding));

//4：事件驱动。
client1.Opened = (a) => { Console.WriteLine("链接成功。"); };
client1.Closed = (a, b) => { Console.WriteLine($"关闭成功。{(b ? "手动断开" : "自动断开")}"); };
client1.Received = (a, b) => { Console.WriteLine($"收到消息：{a.Encoding.GetString(b)}"); };

client1.Open();//打开，在打开前处理属性和事件

//5：简单的发送、接收和发送等待操作。 
client1.Send("abc");//发送
client1.Receive();//接收
client1.Receive(3000);//接收，3秒超时
client1.Receive(ReceiveMode.ParseToEnd("\n", 3000));//接收\n字符串结尾的，超时为3秒 
client1.SendReceive("abc", 3000);//发送并等待接收数据，3秒超时
client1.SendReceive("abc", ReceiveMode.ParseToEnd("\n", 3000));//发送并接收\n字符串结尾的，超时为3秒 
```

## TcpServer   <a id="TcpServer"></a>   
`TcpServer : ServiceBase`
```CSharp
var service = new TcpService("127.0.0.1", 8005);
service.Encoding = Encoding.UTF8;
//接收模式
service.ReceiveMode = ReceiveMode.ParseByteAll();//方法“Receive()”的默认方式
service.ReceiveModeReceived = ReceiveMode.ParseByteAll();//事件“Received”的默认方式
service.Opened = (a) =>
{
    Console.WriteLine($"客户端[{(a as INetwork)?.Socket?.RemoteEndPoint}]连接成功");
};
service.Closed = (a) =>
{
    Console.WriteLine($"客户端关闭成功");
};
service.Received = (a, b) =>
{
    Console.WriteLine($"客户端[{(a as INetwork)?.Socket?.RemoteEndPoint}]收到消息：" + a.Encoding.GetString(b));
};

//打开链接，设置所有属性必须在打开前
service.Open();

if (service.Clients.Any())
{
    //给第一个客户端发送信息，这里和'TcpClient'使用方式一样，不做多余的说明
    service.Clients[0].Send("123");
}
```


## SerialPortClient <a id="SerialPortClient"></a>
`SerialPortClient : ClientBase`   
> 串口是点到点传输，所以只有 `SerialPortClient` 没有 `SerialPortService` 。使用2个`SerialPortClient` 即可。
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
## UsbHidClient (USB) <a id="UsbHidClient"></a>
`UsbHidClient : ClientBase`   

1.需要安装扩展包 `Ping9719.IoT.Hid` 才能使用。   
2.获取报告信息 `UsbHidClient.GetReportDescriptor(UsbHidClient.GetNames[0])`    
> 1.报告类型：Input, Output, Feature   
> 2.报告ID：一般在帧头，默认值为 `0x00`。可以使用“消息处理器”来处理。    
> 3.报告长度：要求的固定长度，不足一般末尾添加`0x00`补齐（低速默认8，全速64，高速1024）。可以使用“消息处理器”来处理。    

```CSharp
var names = UsbHidClient.GetNames;//获取所有Usb设备
var client = new UsbHidClient(names[0]);//访问第一个设备

//使用消息处理器来处理报告
{
    //加入报告ID（实际需要看文档）
    client.SendDataProcessors.Add(new StartAddValueDataProcessor(0));
    //加入报告长度补齐（实际需要看文档）
    client.SendDataProcessors.Add(new PadRightDataProcessor(64));
    //清除报告ID（实际需要看文档）
    client.ReceivedDataProcessors.Add(new StartClearValueDataProcessor(0));
    //清除报告长度补齐（实际需要看文档）
    client.ReceivedDataProcessors.Add(new TrimEndDataProcessor(0));
}
```

## BleClient (蓝牙) <a id="BleClient"></a>
`UsbHidClient : ClientBase`  

1.需要安装扩展包 `Ping9719.IoT.Hid` 才能使用。 
```CSharp
var names = BleClient.GetNames;//获取所有蓝牙设备
var client = new BleClient(names[0]);//访问第一个设备
```

# Modbus <a id="Modbus"></a>
`ModbusRtuClient : IClientData`   
`ModbusTcpClient : IClientData`   
`ModbusAsciiClient : IClientData`   

Modbus Rtu : `站号` + `功能码` + `地址` + `长度` + `校验码`   
Modbus Tcp : `消息号` + `0x0000` + `后续字节长度` + `站号` + `功能码` + `地址` + `长度`   

```CSharp
var client = new ModbusRtuClient("COM1", 9600, format: EndianFormat.ABCD);
var client = new ModbusRtuClient(new TcpClient("127.0.0.1", 502), format: EndianFormat.ABCD);//ModbusRtu协议走TCP
var client = new ModbusTcpClient("127.0.0.1", 502, format: EndianFormat.ABCD);
var client = new ModbusTcpClient(new SerialPortClient("COM1", 9600), format: EndianFormat.ABCD);//ModbusTcp协议走串口
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;//断线重连。tcp推荐断线重连，串口推荐另外两种
client.Client.Open();//打开

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
`AllenBradleyCipClient : IClientData`  
```CSharp
//部分机器可使用OmronCipClient替代 
AllenBradleyCipClient client = new AllenBradleyCipClient("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;//断线重连
client.Client.Open();//打开

client.Read<bool>("abc");//读
client.Write<bool>("abc",true);//写
```

## 汇川 (InovanceModbusTcpClient) <a id="InovanceModbusTcpClient"></a>
`InovanceModbusTcpClient : IClientData`  
```CSharp
var client = new InovanceModbusTcpClient("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;//断线重连
client.Client.Open();//打开

client.Read<bool>("M1");//读
client.Read<Int16>("D1",5);//读取5个
client.Write<bool>("M1",true);//写
client.Write<Int16>("D1",new Int16[]{1,2});//写多个
```

## 三菱 (MitsubishiMcClient) <a id="MitsubishiMcClient"></a>
`MitsubishiMcClient : IClientData`  
测试覆盖表

| 类型         | 单点读写         | 批量读写      |
|--------------|------------------|---------------|
| bool         | ✔️               | ✔️（内部循环） |
| short        | ✔️               | ✔️            |
| int32        | ✔️               | ✔️            |
| float        | ✔️               | ✔️            |
| double       | ✔️               | ✔️            |
| string       | ✔️               | ✔️            |

> 注：bool数组批量写入采用循环单点写入方式，速度相对较慢。
>
> ​        还支持byte、sbyte、ushort、uint32、int64、uint64类型。由于用到的情况较少，请自行测试。


```CSharp
var client = new MitsubishiMcClient(MitsubishiVersion.Qna_3E, "127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;//断线重连
client.Client.Open();//打开

client.Read<Int16>("W0");//读
client.Read<Int16>("W0",5);//读取5个
client.Write<Int16>("W0",10);//写
client.Write<Int16>("W0",new Int16[]{1,2});//写多个
```

## 欧姆龙 (OmronFinsClient) <a id="OmronFinsClient"></a>
`OmronFinsClient : IClientData`  
```CSharp
OmronFinsClient client = new OmronFinsClient("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;//断线重连
client.Client.Open();//打开

client.Read<Int16>("W0");//读
client.Read<Int16>("W0",5);//读取5个
client.Write<Int16>("W0",10);//写
client.Write<Int16>("W0",new Int16[]{1,2});//写多个
```

## 欧姆龙 (OmronCipClient)
`OmronCipClient : IClientData` 
```CSharp
OmronCipClient client = new OmronCipClient("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;//断线重连
client.Client.Open();//打开

//读写
client.Read<bool>("abc");//读
client.Write<bool>("abc",true);//写

//读写多个
client.Read<bool[]>("abc");//读
client.Read<bool[]>("abc",5);//读多个，并截取前5个
client.Write<bool[]>("abc",new bool[]{true,false});//写
```

## 西门子 (SiemensS7Client) <a id="SiemensS7Client"></a>
`SiemensS7Client : IClientData` 
```CSharp
var client = new SiemensS7Client(SiemensVersion.S7_1200, "127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;//断线重连
client.Client.Open();//打开

//支持常用类型(int,float...)
client.Read<Int16>("BD100.0.0");//读
client.Write<Int16>("BD100.0.0",10);//写

//支持特殊类型(string、DateTime、TimeSpan、Char)
client.Read<DateTime>("BD100.0.0");//读
client.Write<DateTime>("BD100.0.0",DateTime.Now);//写

//支持超长的读和写
client.Read<Int16>("BD100.0.0",9999);//连续读9999个数据，大概只需百毫秒
client.Write<Int16>("BD100.0.0",new Int16[]{1,2,3});//连续写9999个数据，大概只需百毫秒

//字符串说明
client.Read<string>("BD100.0.0");//plc的类型必须为string，只支持字母数字等ASCII编码
client.ReadString("BD100.0.0");//plc的类型必须为WString，支持中文等UTF16编码，
```

# 机器人 (Robot) <a id="Robot"></a>
## 爱普生 (EpsonRobot)
`EpsonRobot : IClient` 
```CSharp
EpsonRobot client = new EpsonRobot("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;//断线重连
client.Client.Open();//打开

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
## LRC <a id="LRC"></a>
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

# 常见问题 <a id="Issue"></a>
## 1.如何使用自定义协议？
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
        //Client.ConnectionMode = ConnectionMode.AutoOpen;
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
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;//断线重连
client.Client.Open();

var info = client.ReadXXX();
```

