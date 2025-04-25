# Ping9719.IoT

### 工业互联网通讯库协议实现，包括主流PLC、ModBus、CIP、MC、FINS......等常用协议。
##### Iot device communication protocol implementation, including mainstream PLC, ModBus, CIP, MC, FINS...... Such common protocols.
#

### 全部文档：[doc]
[查看 "IoT" 文档 ( "IoT" document)](Ping9719.IoT/docs/README.md)   
[查看 "IoT" 版本文档 ( "IoT" Version document)](Ping9719.IoT/docs/VERSION.md)  

[查看 "IoT.WPF" 文档 ( "IoT.WPF" document)](Ping9719.IoT.WPF/docs/README.md)   
[查看 "IoT.Avalonia" 文档 ( "IoT.Avalonia" document)](Ping9719.IoT.Avalonia/docs/README.md)   
#

### 库：[library]
源代码：[Github (主库)](https://github.com/ping9719/IoT)  
源代码：[Gitee (备用库)](https://gitee.com/ping9719/IoT)   
#

# 前言、亮点（Merit）
1.常用设备实现接口“IIoT”可进行读写 
```CSharp
client.Read<bool>("abc");//读1个
client.Read<bool>("abc",5);//读5个
client.Write<bool>("abc",true);//写值
client.Write<int>("abc",10,20,30);//写多个
```
2.实现“ClientBase”可实现简单快速的从TCP、串口、UDP、USB等中切换 
```CSharp
var client1 = new TcpClient(ip, port);//Tcp方式
var client2 = new SerialPortClient(portName, baudRate);//串口方式
var client3 = new UdpClient(ip, port);//Udp方式

var client = new OmronCipClient(client1);//使用的方式
client.Client.Open();//打开
```
3.客户端“ClientBase”实现事件，ReceiveMode多种接受模式
```CSharp
ClientBase client1 = new TcpClient(ip, port);//Tcp方式
client1.ConnectionMode = ConnectionMode.AutoOpen;//自动打开
client1.ConnectionMode = ConnectionMode.AutoReconnection;//断线重连
client1.Opened = (a) =>{Log.AddLog("链接成功")};
client1.Closed = (a,b) =>{Log.AddLog("关闭成功")};
client1.Received = (a,b) =>{Log.AddLog("收到消息"+b)};
client1.Open();

client1.Send("abc");//发送
client1.Receive();//等待并接受
client1.Receive(ReceiveMode.ParseToString("\n", 5000));//接受字符串结尾为\n的，超时为5秒 
client1.SendReceive("abc", ReceiveMode.ParseToString("\n", 5000));//发送并接受 ，超时为5秒 
```

### 内容树：[Content tree]

# [Ping9719.IoT](Ping9719.IoT/docs/README.md)   
- Modbus
    - ModbusRtuClient
    - ModbusTcpClient
    - ModbusAsciiClient
- PLC
    - 罗克韦尔 (AllenBradleyCipClient) （未通过测试） 
    - 汇川 (InovanceModbusTcpClient)
    - 三菱 (MitsubishiMcClient)
    - 欧姆龙 (OmronFinsClient,OmronCipClient)
    - 西门子 (SiemensS7Client)
- 机器人 (Robot)
    - 爱普生 (EpsonRobot) （进行中） 
- 通讯 (Communication)
    - TcpClient
    - TcpServer （待开发） 
    - SerialPortClient
    - UdpClient （待开发） 
    - UdpServer （待开发） 
    - HttpServer （待开发） 
    - MqttClient （待开发） 
    - MqttServer （待开发） 
- 算法 (Algorithm)
    - CRC
    - 傅立叶算法(Fourier) （待开发） 
    - PID （待开发） 
    - RSA （待开发） 
- 设备和仪器 (Device)
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
        - 快克焊接机 (KuaiKeWeld)（不推荐） 
