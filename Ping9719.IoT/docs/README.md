
# 安装包 [NuGet]
```CSharp
//等待稳定后发布，现在请自行拉取代码 
Install-Package Ping9719.IoT
```
# Ping9719.IoT
- [前言](#前言、亮点（Merit）)
- [Modbus](#Modbus)
    - ModbusRtu (ModbusRtuClient,ModbusRtuOverTcpClient)
    - ModbusTcp (ModbusTcpClient)
    - ModbusAscii (ModbusAsciiClient)
- [PLC](#PLC)
    - 罗克韦尔 (AllenBradleyCipClient) （未测试）  
    - 汇川 (InovanceModbusTcpClient)
    - 三菱 (MitsubishiMcClient)
    - 欧姆龙 (OmronFinsClient,OmronCipClient)
    - 西门子 (SiemensS7Client)
- [机器人 (Robot)](#机器人 (Robot))
    - 爱普生 (EpsonRobot) （进行中） 
- [通讯 (Communication)](#通讯 (Communication))
    - TcpClient （进行中） 
    - TcpServer （待开发） 
    - UdpClient （待开发） 
    - UdpServer （待开发） 
    - HttpServer （待开发） 
    - MqttClient （待开发） 
    - MqttServer （待开发） 
    - SerialPortClient （待开发） 
- [算法 (Algorithm)](#算法 (Algorithm))
    - CRC
    - 傅立叶算法(Fourier) （待开发） 
    - PID （待开发） 
    - RSA （待开发） 
- [设备和仪器 (Device)](#设备和仪器 (Device))
    - Fct
        - 盟讯电子 (MengXunFct)
    - 激光刻印 (Mark)
        - 大族激光刻印 (DaZhuTcpMark)
    - 无线射频 (Rfid)
        - 倍加福Rfid (BeiJiaFuRfid)
        - 泰和森Rfid (TaiHeSenRfid)
        - 万全Rfid (WanQuanRfid)
    - 扫码枪 (Scanner)
        - 霍尼韦尔扫码器 (HoneywellScanner)
        - 民德扫码器 (MindeoScanner)
    - 螺丝机 (Screw)
        - 快克螺丝机 (KuaiKeDeskScrew,KuaiKeScrew,KuaiKeTcpScrew)
        - 米勒螺丝机 (MiLeScrew)
    - 温控 (TemperatureControl)
        - 快克温控 (KuaiKeTemperatureControl)
    - 焊接机 (Weld)
        - 快克焊接机 (KuaiKeWeld)


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
var client1 = new TcpClient(ip, port);//Tcp方式
var client2 = new SerialPortClient(portName, baudRate);//串口方式
var client3 = new UdpClient(ip, port);//Udp方式

var plc = new OmronCipClient(client1);//使用的方式
plc.Client.Open();//打开通道
plc.Read<bool>("abc");//读
```
3.客户端“ClientBase”实现事件，ReceiveMode多种接受模式
```CSharp
ClientBase client1 = new TcpClient(ip, port);//Tcp方式
client1.IsAutoOpen=fasle;//自动打开
client1.IsReconnection=true;//断线重连
client1.Open();
client1.Opened = (a) =>{Log.AddLog("链接成功")};
client1.Closed = (a,b) =>{Log.AddLog("关闭成功")};
client1.Received = (a,b) =>{Log.AddLog("收到消息"+b)};

client1.Send("abc");//发送
client1.Receive();//等待并接受
client1.Receive(ReceiveMode.ParseToString("\n", 5000));//接受字符串结尾为\n的，超时为5秒 
client1.SendReceive("abc", ReceiveMode.ParseToString("\n", 5000));//发送并接受 ，超时为5秒 
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
client.Start();
client.Pause();
```

# 通讯 (Communication)
## TcpClient
```CSharp
TcpClient tcpClient = new TcpClient("127.0.0.1", 8080);
tcpClient.IsReconnection = true;
tcpClient.Open();
tcpClient.Opening = (a) =>
{
    TextBoxLog.AddLog("连接中");
    return true;
};
tcpClient.Opened = (a) =>
{
    TextBoxLog.AddLog("连接成功");
};
tcpClient.Closing = (a) =>
{
    TextBoxLog.AddLog("关闭中");
    return true;
};
tcpClient.Closed = (a,b) =>
{
    TextBoxLog.AddLog("关闭成功"+ b);
};
tcpClient.Received = (a,b) =>
{
    Log.AddLog("收到消息:"+ DataConvert.ByteArrayToString(b));
};
tcpClient.Warning = (a,b) =>
{
    TextBoxLog.AddLog("错误"+ b.ToString());
};

tcpClient.Send("abc");//发送
tcpClient.Receive();//等待并接受
tcpClient.Receive(ReceiveMode.ParseToString("\n", 5000));//接受字符串结尾为\n的，超时为5秒 
tcpClient.SendReceive("abc", ReceiveMode.ParseToString("\n", 5000));//发送并接受 
```

# 算法 (Algorithm)
## CRC
```CSharp
byte[] bytes = new byte[0];
//CRC
CRC.Crc8(bytes)
CRC.Crc8Itu(bytes)
CRC.Crc8Rohc(bytes)
CRC.Crc16(bytes)
CRC.Crc16Usb(bytes)
CRC.Crc16Modbus(bytes)
CRC.Crc32(bytes)
CRC.Crc32Q(bytes)
CRC.Crc32Sata(bytes)
//CRC 验证
CRC.CheckCrc8(bytes)
CRC.CheckCrc8Itu(bytes)
CRC.CheckCrc8Rohc(bytes)
CRC.CheckCrc16(bytes)
CRC.CheckCrc16Usb(bytes)
CRC.CheckCrc16Modbus(bytes)
CRC.CheckCrc32(bytes)
CRC.CheckCrc32Q(bytes)
CRC.CheckCrc32Sata(bytes)
```
# 设备和仪器 (Device)
## Fct
```CSharp
//编写中...（Writing...）
```
## 激光刻印 (Mark)
```CSharp
//编写中...（Writing...）
```
## 无线射频 (Rfid)
```CSharp
//编写中...（Writing...）
```
## 扫码枪 (Scanner)
```CSharp
//编写中...（Writing...）
```
## 螺丝机 (Screw)
```CSharp
//编写中...（Writing...）
```
## 温控 (TemperatureControl)
```CSharp
//编写中...（Writing...）
```
## 焊接机 (Weld)
```CSharp
//编写中...（Writing...）
```
