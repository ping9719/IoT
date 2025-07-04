# Ping9719.IoT

### Industrial IoT communication protocol implementation, including mainstream PLC, ModBus, CIP, MC, FINS... and other common protocols.
#

### Language Choice:
[简体中文](README.md) --
[English](README2.md) --

### Full Documentation:
[View "IoT" document](Ping9719.IoT/docs/README.md)   
[View "IoT" Version document](Ping9719.IoT/docs/VERSION.md)  

[View "IoT.WPF" document](Ping9719.IoT.WPF/docs/README.md)   
[View "IoT.Avalonia" document](Ping9719.IoT.Avalonia/docs/README.md)   
#

### Library:
Source Code: [Github (Main Repo)](https://github.com/ping9719/IoT)  
Source Code: [Gitee (Mirror Repo)](https://gitee.com/ping9719/IoT)   
#

# Preface & Features
1. Common device interface "IIoT" enables read/write operations  
```CSharp
client.Read<bool>("abc"); // Read 1 value
client.Read<bool>("abc", 5); // Read 5 values
client.Write<bool>("abc", true); // Write value
client.Write<int>("abc", 10, 20, 30); // Write multiple values
```
2.Communication pipeline "ClientBase" allows quick switching between TCP, Serial Port, UDP, USB, etc.
```CSharp
var type1 = new TcpClient(ip, port); // TCP mode
var type2 = new SerialPortClient(portName, baudRate); // Serial mode
var type3 = new UdpClient(ip, port); // UDP mode

var client1 = new ModbusTcpClient(type1); // Using TCP
var client2 = new ModbusTcpClient(type2); // Using Serial Port
client1.Client.Open(); // Open connection
```
3."ClientBase" implements events with multiple ReceiveMode options
```CSharp
ClientBase client1 = new TcpClient(ip, port); // TCP mode
// Important!!! ConnectionMode is critical with 3 modes: 
client1.ConnectionMode = ConnectionMode.None; // Manual. Requires manual Open/Close. Flexible.
client1.ConnectionMode = ConnectionMode.AutoOpen; // Auto-Open. Automatically opens/closes per operation if not Open(). Ideal for short connections. Call Open()/Close() for long sessions.
client1.ConnectionMode = ConnectionMode.AutoReconnection; // Auto-reconnect. After Open(), auto-reconnects if disconnected. Ideal for long connections. Close() stops reconnection.
client1.Opened = (a) => { Log.AddLog("Connected successfully"); };
client1.Closed = (a, b) => { Log.AddLog("Closed successfully"); };
client1.Received = (a, b) => { Log.AddLog("Received: " + b); };
client1.Open();

client1.Send("abc"); // Send
client1.Receive(); // Wait and receive
client1.Receive(ReceiveMode.ParseToString("\n", 5000)); // Receive until "\n", timeout=5s 
client1.SendReceive("abc", ReceiveMode.ParseToString("\n", 5000)); // Send+Receive, timeout=5s ```
```
4.Returns "IoTResult" with built-in exception handling
```CSharp
var info = client.Read<bool>("abc");
if (info.IsSucceed) // Check before getting value
{ var val = info.Value; }
else
{ var err = info.ErrorText; }
```

# Ping9719.IoT
- [通讯 (Communication)]
    - TcpClient
    - TcpServer （待开发） 
    - SerialPortClient
    - UdpClient （进行中） 
    - UdpServer （待开发） 
    - HttpServer （待开发） 
    - MqttClient （待开发） 
    - MqttServer （待开发） 
- [Modbus]
    - ModbusRtuClient
    - ModbusTcpClient
    - ModbusAsciiClient
- [PLC]
    - 罗克韦尔 (AllenBradleyCipClient) （进行中）   
    - 汇川 (InovanceModbusTcpClient)
    - 三菱 (MitsubishiMcClient)
    - 欧姆龙 (OmronFinsClient,OmronCipClient)
    - 西门子 (SiemensS7Client)
- [机器人 (Robot)]
    - 爱普生 (EpsonRobot) （进行中） 
- [算法 (Algorithm)]
    - CRC
    - LRC
    - 傅立叶算法(Fourier) （待开发） 
    - 配对算法(GaleShapleyAlgorithm)
    - PID （待开发） 
    - RSA （待开发） 
- [设备和仪器 (Device)]
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
        - 快克焊接机 (KuaiKeWeld)（不推荐） 
    - 扩展 (Rests)
        - 1.如何使用自定义协议 (Use a custom protocol)
