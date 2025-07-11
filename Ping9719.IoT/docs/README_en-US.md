# Ping9719.IoT

### Industrial Internet of Things communication library protocol implementation, including mainstream PLC, ModBus, CIP, MC, FINS and other common protocols. Data exchange can be simply achieved through different channels (TCP, UDP, MQTT, USB, Bluetooth...).

### Language Selection:
[简体中文](README.md) --
[English](README_en-US.md) --

### Source Code:
[Github (Main Repo)](https://github.com/ping9719/IoT)  
[Gitee (Backup Repo)](https://gitee.com/ping9719/IoT)   

### Contents:  
| Directory     |  Framework                      | Documentation                                      | Version Docs                                     |Dependencies          |Package (NuGet)</br>(Stable Release) | Description|
|----------|----------------------------|---------------------------------------------------|--------------------------------------------------|----------------------|-------------------------------------|-------|
| IoT      | net45;</br>netstandard2.0  | [Docs](Ping9719.IoT/docs/README.md)              |[Version](Ping9719.IoT/docs/VERSION.md)          | System.IO.Ports      |Ping9719.IoT                       | Cross-platform library. Contains communication protocols (TCP, UDP, USB...), industrial protocols (ModBus, MC, FINS...), algorithms (CRC, LRC...), and device drivers (RFID, barcode scanners...) |
| Hid      | net45;</br>netstandard2.0  | [Docs](Ping9719.IoT.Hid/docs/README.md)          |[Version](Ping9719.IoT.Hid/docs/VERSION.md)       | IoT;</br>HidSharp    |Ping9719.IoT.Hid                   | Cross-platform communication library. Extends IoT with USB/Bluetooth support for Android/iOS/Windows devices, enabling PLC-device communication via USB/Bluetooth |
| WPF      | net45;</br>net8.0-windows  | [Docs](Ping9719.IoT.WPF/docs/README.md)          |[Version](Ping9719.IoT.WPF/docs/VERSION.md)       | IoT;                 |Ping9719.IoT.WPF                   | UI library for Windows. Enables quick debugging of IoT protocols and devices on Windows platforms |
| Avalonia | net8.0;</br>netstandard2.0 | [Docs](Ping9719.IoT.Avalonia/docs/README.md)     |[Version](Ping9719.IoT.Avalonia/docs/VERSION.md)  | IoT;</br>Avalonia    |Ping9719.IoT.Avalonia              | Cross-platform UI library. Supports protocol/device debugging on Windows, Android, and iOS devices |

# Essential Reading, Preface, Features
1. Common device interface "IIoT" supports generic type read/write operations  
```CSharp
client.Read<bool>("abc");//Read 1 item
client.Read<bool>("abc", 5);//Read 5 items
client.Write<bool>("abc", true);//Write single value
client.Write<int>("abc", 10, 20, 30);//Write multiple values
client.Write<int>("abc", new int[] { 10, 20, 30 });//Write array
Communication channel implementation "ClientBase" enables seamless switching between TCP, serial port, UDP, USB etc.
```Csharp

var type1 = new TcpClient(ip, port);//TCP mode
var type2 = new SerialPortClient(portName, baudRate);//Serial mode
var type3 = new UdpClient(ip, port);//UDP mode
var type4 = new UsbHidClient(ip, port);//USB mode 

var client1 = new ModbusTcpClient(type1);//Use TCP
var client2 = new ModbusTcpClient(type2);//Use serial
client1.Client.Open();//Open connection
Client "ClientBase" implements events with multiple receive modes
Note: All clients are consistent, including TcpClient, SerialPortClient, UsbHidClient...

```Csharp

ClientBase client1 = new TcpClient(ip, port);//TCP mode
//Important!! Connection modes have 3 options
client1.ConnectionMode = ConnectionMode.None;//Manual: Requires explicit Open()/Close() for maximum flexibility
client1.ConnectionMode = ConnectionMode.AutoOpen;//Auto-open: Automatically opens/closes connection for each send/receive (suitable for short-term connections)
client1.ConnectionMode = ConnectionMode.AutoReconnection;//Auto-reconnect: Automatically reconnects after disconnection (suitable for persistent connections)
client1.Opened = (a) =>{Log.AddLog("Connected successfully")};
client1.Closed = (a,b) =>{Log.AddLog("Connection closed")};
client1.Received = (a,b) =>{Log.AddLog("Received message: "+b)};
client1.Open();

client1.Send("abc");//Send data
client1.Receive();//Wait for response
client1.Receive(3000);//Wait up to 3 seconds
client1.Receive(ReceiveMode.ParseToString("\n", 5000));//Receive until \n delimiter with 5s timeout
client1.SendReceive("abc", ReceiveMode.ParseToString("\n", 5000));//Send and receive with 5s timeout
client1.SendReceive("abc",3000);//Send and wait up to 3 seconds
Returns "IoTResult" containing built-in error handling
IoTResult<T> contains Value, IoTResult does not

```Csharp

var info = client.Read<bool>("abc");
if (info.IsSucceed)//Check success before accessing value
{ var val = info.Value; }
else
{ var err = info.ErrorText; }
Ping9719.IoT
Communication
TcpClient
TcpServer （under testing）
SerialPortClient
UdpClient （in progress）
UdpServer （planned）
HttpServer （planned）
MqttClient （planned）
MqttServer （planned）
Modbus
ModbusRtuClient
ModbusTcpClient
ModbusAsciiClient
PLC
Rockwell (AllenBradleyCipClient) （under testing）
Inovance (InovanceModbusTcpClient)
Mitsubishi (MitsubishiMcClient)
Omron (OmronFinsClient,OmronCipClient)
Siemens (SiemensS7Client)
Robot
Epson (EpsonRobot) （under testing）
Algorithm
CRC
LRC
Fourier Algorithm (Fourier) （planned）
Stable Marriage Algorithm (GaleShapleyAlgorithm)
PID （planned）
RSA （planned）
Devices & Instruments
Airtight Testing (Airtight)
Cosmo Airtight (CosmoAirtight)
Fct
MengXun FCT (MengXunFct)
Laser Marking (Mark)
Han's Laser (DaZhuMark)
HuaPu Laser (HuaPuMark)
RFID
Pepperl+Fuchs RFID (BeiJiaFuRfid)
TaiHeSen RFID (TaiHeSenRfid)
WanQuan RFID (WanQuanRfid)
Barcode Scanners (Scanner)
Honeywell Scanner (HoneywellScanner)
Mindeo Scanner (MindeoScanner)
Screwdrivers (Screw)
Quick Screwdriver (KuaiKeDeskScrew,KuaiKeScrew,KuaiKeTcpScrew)（not recommended）
Miller Screwdriver (MiLeScrew)
Temperature Control (TemperatureControl)
Quick Temp Control (KuaiKeTemperatureControl)（not recommended）
Welding Machines (Weld)
Quick Welding Machine (KuaiKeWeld)
Extensions
1. How to Use Custom Protocols