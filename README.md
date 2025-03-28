# Ping9719.IoT

### 工业互联网通讯库协议实现，包括主流PLC、ModBus、CIP、MC、FINS......等常用协议。
##### Iot device communication protocol implementation, including mainstream PLC, ModBus, CIP, MC, FINS...... Such common protocols.
#

### 全部文档：[doc]
[查看IoT文档 (Iot document)](Ping9719.IoT/docs/README.md)   
[查看IoT版本文档 (IoT Version document)](Ping9719.IoT/docs/VERSION.md)  

[查看IoT.WPF文档 (Iot.WPF document)](Ping9719.IoT.WPF/README.md)   
#

### 亮点（Merit）
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
client1.ConnectionMode = ConnectionMode.AutoOpen;//自动打开
//client1.ConnectionMode = ConnectionMode.AutoReconnection;//断线重连
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
    - ModbusRtu (ModbusRtuClient,ModbusRtuOverTcpClient)
    - ModbusTcp (ModbusTcpClient)
    - ModbusAscii (ModbusAsciiClient)
- PLC
    - 罗克韦尔 (AllenBradleyCipClient) （未通过测试） 
    - 汇川 (InovanceModbusTcpClient)
    - 三菱 (MitsubishiMcClient)
    - 欧姆龙 (OmronFinsClient,OmronCipClient)
    - 西门子 (SiemensS7Client)
- 机器人 (Robot)
    - 爱普生 (EpsonRobot) （进行中） 
- 通讯 (Communication)
    - TcpClient （进行中） 
    - TcpServer （待开发） 
    - UdpClient （待开发） 
    - UdpServer （待开发） 
    - HttpServer （待开发） 
    - MqttClient （待开发） 
    - MqttServer （待开发） 
    - SerialPortClient （待开发） 
- 算法 (Algorithm)
    - CRC （待开发） 
    - 傅立叶算法(Fourier) （待开发） 
    - PID （待开发） 
    - RSA （待开发） 
- 设备和仪器 (Device)
    - Fct
        - 盟讯电子 (MengXunFct)
    - 激光刻印 (Mark)
        - 大族激光刻印 (DaZhuMark)
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


# [Ping9719.IoT.WPF](Ping9719.IoT.WPF/README.md)   
- Modbus
    - ModbusRtu (...)（待开发） 
    - ModbusTcp (...)（待开发） 
    - ModbusAscii (...)（待开发） 
- PLC
    - 罗克韦尔 (...) （待开发） 
    - 汇川 (...)（待开发） 
    - 三菱 (...)（待开发） 
    - 欧姆龙 (...)（待开发） 
    - 西门子 (...)（待开发） 
- 机器人 (Robot)
    - 爱普生 (...) （待开发） 
- 通讯 (Communication)
    - TcpClient （待开发） 
    - TcpServer （待开发） 
    - UdpClient （待开发） 
    - UdpServer （待开发） 
    - HttpServer （待开发） 
    - MqttClient （待开发） 
    - MqttServer （待开发） 
    - SerialPortClient （待开发） 
- 算法 (Algorithm)
    - CRC（...） 
    - 傅立叶算法(...) （待开发） 
    - PID （...） （待开发） 
    - RSA （...） （待开发） 
- 设备和仪器 (Device)
    - Fct
        - 盟讯电子 (...)（待开发） 
    - 激光刻印 (Mark)
        - 大族激光刻印 (...)
    - 无线射频 (Rfid)
        - 倍加福Rfid (BeiJiaFuRfidView)
        - 泰和森Rfid (TaiHeSenRfidView)
        - 万全Rfid (WanQuanRfidView)
    - 扫码枪 (Scanner)
        - 霍尼韦尔扫码器 (...)（待开发） 
        - 民德扫码器 (...)（待开发） 
    - 螺丝机 (Screw)
        - 米勒螺丝机 (...)（待开发） 