# Ping9719.IoT

### 工业互联网通讯库协议实现，包括主流PLC、ModBus、CIP、MC、FINS......等常用协议可通过不同的管道（TCP、UDP、MQTT、USB、蓝牙...）简单的交互数据。
##### The industrial internet communication library protocol has been implemented, including mainstream PLC, ModBus, CIP, MC, FINS... and other common protocols. Through different channels (TCP, UDP, MQTT, USB, Bluetooth...), data can be simply exchanged.
#

### 语言选择：
[简体中文](README.md) || [English](README_en-US.md) 

### 源代码：
[Github (主库)](https://github.com/ping9719/IoT)  
[Gitee (备用库)](https://gitee.com/ping9719/IoT)   
#

### 项目：  
| 项目     |  框架                      | 详细文档                                      | 版本文档                                     |依赖                  |包（NuGet）                  | 简介|
|----------|----------------------------|-----------------------------------------------|----------------------------------------------|----------------------|-----------------------------|-------|
| IoT      | net45;</br>netstandard2.0  | [文档](Ping9719.IoT/docs/README.md)           |[文档](Ping9719.IoT/docs/VERSION.md)          | System.IO.Ports      |Ping9719.IoT                 | 跨平台的库。主要包含通信（TCP，UDP，USB...）协议（ModBus，MC，FINS...）算法（CRC，LRC...）设备（RFID，扫码枪...） |
| Hid      | net45;</br>netstandard2.0  | [文档](Ping9719.IoT.Hid/docs/README.md)       |[文档](Ping9719.IoT.Hid/docs/VERSION.md)      | IoT;</br>HidSharp    |Ping9719.IoT.Hid             | 跨平台管道链接库。对IoT进行的扩充，支持在windows、安卓、苹果的手机、平板、电脑上进行USB和蓝牙发送和接收数据，使PLC和设备通信可使用USB或蓝牙  |
| WPF      | net45;</br>net8.0-windows  | [文档](Ping9719.IoT.WPF/docs/README.md)       |[文档](Ping9719.IoT.WPF/docs/VERSION.md)      | IoT;                 |Ping9719.IoT.WPF </br>(暂未发布)   | 界面UI库。只支持在windows平台上快速的调试IoT中的协议和设备   |
| Avalonia | net8.0;</br>netstandard2.0 | [文档](Ping9719.IoT.Avalonia/docs/README.md)  |[文档](Ping9719.IoT.Avalonia/docs/VERSION.md) | IoT;</br>Avalonia    |Ping9719.IoT.Avalonia </br>(暂未发布) | 跨平台的界面UI库。支持在windows、安卓、苹果的手机、平板、电脑上快速的调试IoT中的协议和设备 |

#

# 必读、前言、亮点
1.<b>标准</b>协议实现接口“IIoT”，可通过泛型方式进行读写  
```CSharp
client.Read<bool>("abc");//读1个
client.Read<bool>("abc", 5);//读5个
client.Write<bool>("abc", true);//写值
client.Write<int>("abc", 10, 20, 30);//写多个
client.Write<int>("abc", new int[] { 10, 20, 30 });//写多个
```

2.<b>所有</b>协议可通过`ClientBase Client`简单快速的从TCP、串口、UDP、USB...等中切换 
> 这里以`ModbusRtu`举列，默认只支持串口。但是如果你想实现`ModbusRtuOverTcpClient`（使用TCP的方式走`ModbusRtu`协议）其他的都是同理。 

```CSharp
var client0 = new ModbusRtuClient("COM1");//使用串口方式，这是构造函数已包含的默认方式 
var client1 = new ModbusRtuClient(new TcpClient("127.0.0.1", 502));//使用Tcp方式，ModbusRtuOverTcpClient
var client2 = new ModbusRtuClient(new UsbHidClient("xxxxx001"));//使用Usb方式，ModbusRtuOverUsbClient

client1.Client.Open();//打开
```

3.客户端`ClientBase`包含常用事件；接收数据可传入`ReceiveMode`，包含五种内置的接收模式
> 注意：所有的客户端都是一样的，包含TcpClient，SerialPortClient，UsbHidClient...
```CSharp
ClientBase client1 = new TcpClient("127.0.0.1", 502);//Tcp方式
            
//重要！！！连接模式是非常重要的功能，有3种模式 
client1.ConnectionMode = ConnectionMode.Manual;//手动。需要自己去打开和关闭，此方式比较灵活。
client1.ConnectionMode = ConnectionMode.AutoOpen;//自动打开。没有执行Open()时每次发送和接收会自动打开和关闭，比较合适需要短链接的场景，如需要临时的长链接也可以调用Open()后在Close()。
client1.ConnectionMode = ConnectionMode.AutoReconnection;//自动断线重连。在执行了Open()后，如果检测到断开后会自动打开，比较合适需要长链接的场景。调用Close()将不再重连。
client1.Encoding = Encoding.UTF8;
//数据处理器，发送加入换行，接受去掉换行
client1.SendDataProcessors.Add(new DataEndAddProcessor("\r\n", client1.Encoding));
client1.ReceivedDataProcessors.Add(new DataEndClearProcessor("\r\n", client1.Encoding));
//常用的3种事件
client1.Opened = (a) => { Console.WriteLine("链接成功。"); };
client1.Closed = (a, b) => { Console.WriteLine($"关闭成功。{(b ? "手动断开" : "自动断开")}"); };
client1.Received = (a, b) => { Console.WriteLine($"收到消息：{a.Encoding.GetString(b)}"); };
//打开，在打开前处理属性和事件
client1.Open();

//发送或接收数据 
client1.Send("abc");//发送
client1.Receive();//接收
client1.Receive(3000);//接收，3秒超时
client1.Receive(ReceiveMode.ParseToString("\n", 5000));//接收字符串结尾为\n的，超时为5秒 
client1.SendReceive("abc", 3000);//发送并接收，3秒超时
client1.SendReceive("abc", ReceiveMode.ParseToString("\n", 5000));//发送并接收 ，超时为5秒 
```

4.客户端和服务端支持消息处理器
> 消息处理器：在发送和接收数据时预先处理数据（比如发送时末尾加换行，接收时末尾去换行），
也可以自定义消息处理器。

```CSharp
ClientBase client1 = new TcpClient("127.0.0.1", 502);
//数据处理器，发送加入换行，接受去掉换行
client1.SendDataProcessors.Add(new DataEndAddProcessor("\r\n", client1.Encoding));
client1.ReceivedDataProcessors.Add(new DataEndClearProcessor("\r\n", client1.Encoding));
```

5.返回为“IoTResult”，内置了异常处理等信息
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