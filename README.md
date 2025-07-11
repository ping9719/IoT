# Ping9719.IoT

### 工业互联网通讯库协议实现，包括主流PLC、ModBus、CIP、MC、FINS......等常用协议可通过不同的管道（TCP、UDP、MQTT、USB、蓝牙...）简单的交互数据。
##### The industrial internet communication library protocol has been implemented, including mainstream PLC, ModBus, CIP, MC, FINS... and other common protocols. Through different channels (TCP, UDP, MQTT, USB, Bluetooth...), data can be simply exchanged.
#

### 语言选择：
[简体中文](README.md) </br>
[English](README_en-US.md) </br>

### 源代码：
[Github (主库)](https://github.com/ping9719/IoT)  
[Gitee (备用库)](https://gitee.com/ping9719/IoT)   
#

### 目录：  
| 目录     |  框架                      | 详细文档                                      | 版本文档                                     |依赖                  |包（NuGet）</br>(稳定后发布) | 简介|
|----------|----------------------------|-----------------------------------------------|----------------------------------------------|----------------------|-----------------------------|-------|
| IoT      | net45;</br>netstandard2.0  | [文档](Ping9719.IoT/docs/README.md)           |[文档](Ping9719.IoT/docs/VERSION.md)          | System.IO.Ports      |Ping9719.IoT                 | 跨平台的库。主要包含通信（TCP，UDP，USB...）协议（ModBus，MC，FINS...）算法（CRC，LRC...）设备（RFID，扫码枪...） |
| Hid      | net45;</br>netstandard2.0  | [文档](Ping9719.IoT.Hid/docs/README.md)       |[文档](Ping9719.IoT.Hid/docs/VERSION.md)      | IoT;</br>HidSharp    |Ping9719.IoT.Hid             | 跨平台管道链接库。对IoT进行的扩充，支持在windows、安卓、苹果的手机、平板、电脑上进行USB和蓝牙发送和接受数据，使PLC和设备通信可使用USB或蓝牙  |
| WPF      | net45;</br>net8.0-windows  | [文档](Ping9719.IoT.WPF/docs/README.md)       |[文档](Ping9719.IoT.WPF/docs/VERSION.md)      | IoT;                 |Ping9719.IoT.WPF             | 界面UI库。只支持在windows平台上快速的调试IoT中的协议和设备   |
| Avalonia | net8.0;</br>netstandard2.0 | [文档](Ping9719.IoT.Avalonia/docs/README.md)  |[文档](Ping9719.IoT.Avalonia/docs/VERSION.md) | IoT;</br>Avalonia    |Ping9719.IoT.Avalonia        | 跨平台的界面UI库。支持在windows、安卓、苹果的手机、平板、电脑上快速的调试IoT中的协议和设备 |

#



# 必读、前言、亮点
1.常用设备实现接口“IIoT”可通过泛型方式进行读写  
```CSharp
client.Read<bool>("abc");//读1个
client.Read<bool>("abc", 5);//读5个
client.Write<bool>("abc", true);//写值
client.Write<int>("abc", 10, 20, 30);//写多个
client.Write<int>("abc", new int[] { 10, 20, 30 });//写多个
```
2.通信管道实现“ClientBase”可实现简单快速的从TCP、串口、UDP、USB等中切换 
```CSharp
var type1 = new TcpClient(ip, port);//Tcp方式
var type2 = new SerialPortClient(portName, baudRate);//串口方式
var type3 = new UdpClient(ip, port);//Udp方式
var type4 = new UsbHidClient(ip, port);//USB方式 

var client1 = new ModbusTcpClient(type1);//使用Tcp方式
var client2 = new ModbusTcpClient(type2);//使用串口方式
client1.Client.Open();//打开
```
3.客户端“ClientBase”实现事件，ReceiveMode多种接受模式
> 注意：所有的客户端都是一样的，包含TcpClient，SerialPortClient，UsbHidClient...
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
client1.Receive();//等待接受
client1.Receive(3000);//等待接受，3秒超时
client1.Receive(ReceiveMode.ParseToString("\n", 5000));//接受字符串结尾为\n的，超时为5秒 
client1.SendReceive("abc", ReceiveMode.ParseToString("\n", 5000));//发送并接受 ，超时为5秒 
client1.SendReceive("abc",3000);//发送并等待接受，3秒超时
```
4.返回为“IoTResult”，内置了异常处理等信息
> `IoTResult<T>`包含`Value`，`IoTResult`不包含 
```CSharp
var info = client.Read<bool>("abc");
if (info.IsSucceed)//应该判断后在取值
{ var val = info.Value; }
else
{ var err = info.ErrorText; }
```

# Ping9719.IoT
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