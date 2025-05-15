
# 安装包 [NuGet]
```CSharp
//等待稳定后发布，现在请自行拉取代码 
//After it stabilizes, it will be released. Now, please pull the code by yourself
Install-Package Ping9719.IoT
```
# Ping9719.IoT
- [前言](#前言、亮点（Merit）)
- [通讯 (Communication)](#通讯 (Communication))
    - TcpClient
    - TcpServer （待开发） 
    - SerialPortClient
    - UdpClient （待开发） 
    - UdpServer （待开发） 
    - HttpServer （待开发） 
    - MqttClient （待开发） 
    - MqttServer （待开发） 
- [Modbus](#Modbus)
    - ModbusRtuClient
    - ModbusTcpClient
    - ModbusAsciiClient
- [PLC](#PLC)
    - 罗克韦尔 (AllenBradleyCipClient) （进行中）   
    - 汇川 (InovanceModbusTcpClient)
    - 三菱 (MitsubishiMcClient)
    - 欧姆龙 (OmronFinsClient,OmronCipClient)
    - 西门子 (SiemensS7Client)
- [机器人 (Robot)](#机器人 (Robot))
    - 爱普生 (EpsonRobot) （进行中） 
- [算法 (Algorithm)](#算法 (Algorithm))
    - CRC
    - LRC
    - 傅立叶算法(Fourier) （待开发） 
    - 稳定婚姻配对算法(GaleShapleyAlgorithm) （完善中） 
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
- [扩展 (Rests)](#扩展 (Rests))
    - [1.如何使用自定义协议 (Use a custom protocol)](#1.如何使用自定义协议 (Use a custom protocol))


# 前言、亮点（Merit）
1.常用设备实现接口“IIoT”可进行读写 
```CSharp
client.Read<bool>("abc");//读1个
client.Read<bool>("abc",5);//读5个
client.Write<bool>("abc",true);//写值
client.Write<int>("abc",10,20,30);//写多个
```
2.通信管道实现“ClientBase”可实现简单快速的从TCP、串口、UDP、USB等中切换 
```CSharp
var type1 = new TcpClient(ip, port);//Tcp方式
var type2 = new SerialPortClient(portName, baudRate);//串口方式
var type3 = new UdpClient(ip, port);//Udp方式

var client1 = new ModbusTcpClient(type1);//使用Tcp方式
var client2 = new ModbusTcpClient(type2);//使用串口方式
client1.Client.Open();//打开
```
3.客户端“ClientBase”实现事件，ReceiveMode多种接受模式
```CSharp
ClientBase client1 = new TcpClient(ip, port);//Tcp方式
//重要！！！连接模式是非常重要的功能，有3种模式 
client1.ConnectionMode = ConnectionMode.None;//手动。需要自己去打开和关闭，此方式比较灵活。
client1.ConnectionMode = ConnectionMode.AutoOpen;//自动打开。没有执行Open()时每次发送和接受会自动打开和关闭，比较合适需要短链接的场景，如需要临时的长链接也可以调用Open()后在Close()。
client1.ConnectionMode = ConnectionMode.AutoReconnection;//自动断线重连。在执行了Open()后，如果检测到断开后会自动打开，比较合适需要长链接的场景。调用Close()将不再重连。
client1.Opened = (a) =>{Log.AddLog("链接成功")};
client1.Closed = (a,b) =>{Log.AddLog("关闭成功")};
client1.Received = (a,b) =>{Log.AddLog("收到消息"+b)};
client1.Open();

client1.Send("abc");//发送
client1.Receive();//等待并接受
client1.Receive(ReceiveMode.ParseToString("\n", 5000));//接受字符串结尾为\n的，超时为5秒 
client1.SendReceive("abc", ReceiveMode.ParseToString("\n", 5000));//发送并接受 ，超时为5秒 
```
4.返回为“IoTResult”，内置了异常处理等信息
```CSharp
var info = client.Read<bool>("abc");
if (info.IsSucceed)//应该判断后在取值
{ var val = info.Value; }
else
{ var err = info.ErrorText; }
```

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
## Modbus
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
## 常用plc类型对照表（plc type comparison table）
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

# 扩展 (Rests)
## 1.如何使用自定义协议 (Use a custom protocol)
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
