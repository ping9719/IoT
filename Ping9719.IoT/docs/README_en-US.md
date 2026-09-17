# Language Selection
[简体中文](README.md) || [English](README_en-US.md)

# Table of Contents
<!-- TOC-->
- [ByteData `beta` <a id="ByteData"></a>](#bytedata-beta)
- [Byte Data (ByteData) `beta`](#byte-data-bytedata-beta)
  - [Batch Parse PLC Data](#batch-parse-plc-data)
- [Built-in Converters <a id="IByteConverter"></a>](#built-in-converters)
  - [Byte Converter](#byte-converter)
- [Custom Converters <a id="IByteConverter0"></a>](#custom-converters)
- [Communication <a id="Communication"></a>](#communication-1)
  - [ClientBase <a id="ClientBase"></a>](#clientbase)
- [Communication](#communication)
  - [Client Base (ClientBase) (Recommended Reading!!!)](#client-base-clientbase-recommended-reading)
    - [1. ConnectionMode <a id="ConnectionMode"></a>](#1-connectionmode)
    - [Three Connection Modes (ConnectionMode)](#three-connection-modes-connectionmode)
    - [2. ReceiveMode <a id="ReceiveMode"></a>](#2-receivemode)
    - [Receiving Data and Receive Mode (ReceiveMode)](#receiving-data-and-receive-mode-receivemode)
    - [3. IDataProcessor <a id="IDataProcessor"></a>](#3-idataprocessor)
    - [Send/Receive Data and Data Processor (IDataProcessor)](#sendreceive-data-and-data-processor-idataprocessor)
    - [4. Heartbeat <a id="Heartbeat"></a>](#4-heartbeat)
    - [Active Heartbeat and Passive Heartbeat (Heartbeat)](#active-heartbeat-and-passive-heartbeat-heartbeat)
  - [TcpClient <a id="TcpClient"></a>](#tcpclient-1)
  - [Service Base (ServiceBase) `beta`](#service-base-servicebase-beta)
  - [TcpClient](#tcpclient)
  - [TcpServer   <a id="TcpServer"></a>](#tcpserver-1)
  - [TcpServer](#tcpserver)
  - [UdpClient   <a id="UdpClient"></a>](#udpclient-1)
  - [UdpClient](#udpclient)
  - [SerialPortClient ; SerialClient <a id="SerialPortClient"></a>](#serialportclient--serialclient)
  - [SerialPortClient ; SerialClient (Serial Port)](#serialportclient--serialclient-serial-port)
  - [HttpClient <a id="HttpClient"></a>](#httpclient-1)
  - [HttpClient](#httpclient)
  - [HttpServer <a id="HttpServer"></a>](#httpserver-1)
  - [HttpServer](#httpserver)
  - [UsbHidClient (USB) <a id="UsbHidClient"></a>](#usbhidclient-usb-1)
  - [UsbHidClient (USB)](#usbhidclient-usb)
  - [BleClient (Bluetooth) <a id="BleClient"></a>](#bleclient-bluetooth-1)
  - [BleClient (Bluetooth)](#bleclient-bluetooth)
- [Modbus <a id="Modbus"></a>](#modbus-1)
- [Protocol](#protocol)
  - [Modbus](#modbus)
- [PLC <a id="PLC"></a>](#plc)
  - [Common PLC Type Comparison Table <a id="PlcType"></a>](#common-plc-type-comparison-table)
  - [PLC Type Comparison Table](#plc-type-comparison-table)
  - [Rockwell (AllenBradleyCipClient) <a id="AllenBradleyCipClient"></a>](#rockwell-allenbradleycipclient-1)
  - [Rockwell (AllenBradleyCipClient)](#rockwell-allenbradleycipclient)
  - [Keyence (KeyenceHostLinkClient) <a id="KeyenceHostLinkClient"></a>](#keyence-keyencehostlinkclient-1)
  - [Keyence (KeyenceHostLinkClient)](#keyence-keyencehostlinkclient)
  - [Inovance (InovanceModbusTcpClient) <a id="InovanceModbusTcpClient"></a>](#inovance-inovancemodbustcpclient-1)
  - [Inovance (InovanceModbusTcpClient)](#inovance-inovancemodbustcpclient)
  - [Mitsubishi (MitsubishiMcClient) <a id="MitsubishiMcClient"></a>](#mitsubishi-mitsubishimcclient-1)
  - [Mitsubishi (MitsubishiMcClient)](#mitsubishi-mitsubishimcclient)
  - [Omron (OmronFinsClient) <a id="OmronFinsClient"></a>](#omron-omronfinsclient-1)
  - [Omron (OmronFinsClient)](#omron-omronfinsclient)
  - [Omron (OmronCipClient)](#omron-omroncipclient)
  - [Siemens (SiemensS7Client) <a id="SiemensS7Client"></a>](#siemens-siemenss7client-1)
  - [Siemens (SiemensS7Client)](#siemens-siemenss7client)
- [Robot <a id="Robot"></a>](#robot)
  - [Epson (EpsonRobot)](#epson-epsonrobot)
- [Algorithm <a id="Algorithm"></a>](#algorithm-1)
  - [AffineTransform <a id="AffineTransform"></a>](#affinetransform)
- [Algorithm](#algorithm)
  - [Affine Transform (AffineTransform)](#affine-transform-affinetransform)
  - [AveragePoint <a id="AveragePoint"></a>](#averagepoint)
  - [Average Point (AveragePoint)](#average-point-averagepoint)
  - [CRC <a id="CRC"></a>](#crc-1)
  - [CRC](#crc)
  - [LRC <a id="LRC"></a>](#lrc-1)
  - [LRC](#lrc)
  - [Fourier Filter (FFTFilter) <a id="FFTFilter"></a>](#fourier-filter-fftfilter-1)
  - [Fourier Filter (FFTFilter)](#fourier-filter-fftfilter)
  - [Stable Marriage Matching (GaleShapleyAlgorithm) <a id="GaleShapleyAlgorithm"></a>](#stable-marriage-matching-galeshapleyalgorithm-1)
  - [Stable Marriage Matching (GaleShapleyAlgorithm)](#stable-marriage-matching-galeshapleyalgorithm)
  - [Linear Regression (LinearRegression) <a id="LinearRegression"></a>](#linear-regression-linearregression-1)
  - [Linear Regression (LinearRegression)](#linear-regression-linearregression)
- [Devices and Instruments (Device) <a id="Device"></a>](#devices-and-instruments-device-1)
- [Devices and Instruments (Device)](#devices-and-instruments-device)
  - [Cosmo Airtight Test (CosmoAirtight) <a id="CosmoAirtight"></a>](#cosmo-airtight-test-cosmoairtight)
  - [Airtight Test (Airtight)](#airtight-test-airtight)
  - [Laser Marking (Mark) <a id="Mark"></a>](#laser-marking-mark-1)
  - [Laser Marking (Mark)](#laser-marking-mark)
  - [RFID (Rfid) <a id="Rfid"></a>](#rfid-rfid-1)
  - [RFID (Rfid)](#rfid-rfid)
  - [Barcode Scanner (Scanner) <a id="Scanner"></a>](#barcode-scanner-scanner)
  - [Robot (Robot)](#robot-robot)
  - [Scanner (Scanner)](#scanner-scanner)
  - [Screw Machine (Screw) <a id="Screw"></a>](#screw-machine-screw-1)
  - [Screw Machine (Screw)](#screw-machine-screw)
  - [Welding Machine (Weld) <a id="Weld"></a>](#welding-machine-weld-1)
  - [Welding Machine (Weld)](#welding-machine-weld)
  - [Other List <a id="EstsList"></a>](#other-list)
- [FAQ <a id="Issue"></a>](#faq-1)
  - [1. How to use a custom protocol? <a id="UserProtocol"></a>](#1-how-to-use-a-custom-protocol-1)
- [FAQ](#faq)
  - [1. How to use a custom protocol?](#1-how-to-use-a-custom-protocol)
  - [2. How to customize Json parsing? <a id="UserJson"></a>](#2-how-to-customize-json-parsing-1)
  - [2. How to customize Json parsing?](#2-how-to-customize-json-parsing)
  - [3. How to switch communication methods at runtime?](#3-how-to-switch-communication-methods-at-runtime)
<!-- TOC -->

# Byte Data (ByteData) `beta`
> Please note: The byte data feature is in `beta` and may undergo significant changes.

Byte data is a tool for converting between byte arrays (`byte[]`) and various data types (numbers, classes, arrays, etc.).

Common examples:
```CSharp
public class test
{
    public Int16 aa { get; set; }
    public Int16 bb { get; set; }

    [IoT(IsIgnore = true)]
    public Int16 cc { get; set; }
}

var testArr = new byte[] { 0, 1, 0, 2, 0, 3, 0, 4 };

// Add a custom converter (if needed)
Dictionary<Type, IByteConverter> converterDict = new Dictionary<Type, IByteConverter>()
{
    { typeof(bool), new BoolBitByteConverter()}
};

// Parse a single value
var int16 = ByteData.GetValue<Int16>(testArr, EndianFormat.ABCD, converterDict);// 1
var obj = ByteData.GetValue<test>(testArr, EndianFormat.ABCD, converterDict);// {"aa":1,"bb":2,"cc":0}
// Parse all
var int16s = ByteData.GetValue<Int16[]>(testArr, EndianFormat.ABCD, converterDict);// [1,2,3,4]
var objs = ByteData.GetValue<test[]>(testArr, EndianFormat.ABCD, converterDict);// [{"aa":1,"bb":2,"cc":0},{"aa":3,"bb":4,"cc":0}]
// Parse a specified count
var int16ss = ByteData.GetValues<Int16>(testArr, 2, EndianFormat.ABCD, converterDict);// [1,2]
var objss = ByteData.GetValues<test>(testArr, 2, EndianFormat.ABCD, converterDict);// [{"aa":1,"bb":2,"cc":0},{"aa":3,"bb":4,"cc":0}]
// Reverse parsing
var bs = ByteData.ToBytes(obj, EndianFormat.ABCD);// [0,1,0,2]

// Special case (1 byte = 8 bools)
var bools = ByteData.GetValues<bool>(testArr, 1, EndianFormat.ABCD, converterDict);// [F,F,F,F,F,F,F,F]
```

## Batch Parse PLC Data
```CSharp
var client = new SiemensS7Client(SiemensVersion.S7_1200, "127.0.0.1");
var plcdata = client.Read<byte>("BD100.0.0", 100);// Read 100 raw data items

ByteData byteD = new ByteData(plcdata.Value, client.Format);
var v1 = byteD.GetValue<Int16>(0);// Read the 1st data item
var v2 = byteD.GetValue<Int16>(2);// Read the 2nd data item
var v3 = byteD.GetValue<Int16>(4);// Read the 3rd data item
```

## Byte Converter
Built-in converters are divided into "basic" and "special".  
Basic ones are included when `ByteData` is initialized; special ones are not.

**Basic:**

| Name | Description |
| ----------------  | --------- |
| ByteByteConverter |  Byte |
| SByteByteConverter | SByte  |
| Int16ByteConverter | Int16  |
| UInt16ByteConverter | UInt16   |
| Int32ByteConverter | Int32  |
| UInt32ByteConverter | UInt32  |
| Int64ByteConverter | Int64  |
| UInt64ByteConverter | UInt64   |
| SingleByteConverter | Single  |
| DoubleByteConverter | Double  |

**Special:**
> They need to be added via `byteData.ByteConverterDict.Add(typeof(Int16), new Int16ByteConverter())`.

| Name | Description |
| ----------------------- | --------- |
| BoolByteConverter | 1 byte = 1 bool |
| BoolBitByteConverter | 1 byte = 8 bools |
| StringByteConverter | String converter. If Encoding is null, a hexadecimal string is used. |

**Custom Converter:**

```CSharp
// The class implements the `IByteConverter` interface
// Example: converter for Int16
public class Int16ByteConverter : IByteConverter
{
    public int ByteLength => 2;
    public object ToObject(IEnumerable<byte> bytes, EndianFormat format) => BitConverter.ToInt16(DataConvert.EndianToNet(bytes, format, 0, ByteLength), 0);
    public byte[] ToBytes(object data, EndianFormat format) => DataConvert.EndianToNet(BitConverter.GetBytes((short)data), format);
}
```
~Usage:~
```CSharp
var testArr = new byte[] { 0, 1, 0, 2, 0, 3, 0, 4 };
ByteData byteData = new ByteData(testArr, EndianFormat.CDAB);
// Add a custom converter (if needed)
byteData.ByteConverterDict.Add(typeof(Int16), new Int16ByteConverter());
```

# Communication
## Client Base (ClientBase) (Recommended Reading!!!)
Most communication implementations are based on `ClientBase`, such as `TcpClient`, `SerialPortClient`, etc. The following content is common to all of them.

### Three Connection Modes (ConnectionMode)

1. ==Manual== (general scenario). You need to open and close it yourself; this mode is more flexible.     
2. ==Auto Open== (suitable for short connections). When `Open()` is not executed, each send and receive automatically opens and closes. This is suitable for scenarios requiring short connections. If a temporary long connection is needed, you can also call `Open()` and then `Close()`.    
3. ==Auto Reconnection== (suitable for long connections). After `Open()` is executed, if a disconnection is detected, it automatically attempts to reconnect. This is suitable for scenarios requiring long connections. Calling `Close()` will stop reconnection.   

**Auto Reconnection Introduction**
> 1. When the connection is disconnected, it attempts to reconnect. The first wait is 1 second.   
> 2. If unsuccessful, the wait time increases by one second each time until the maximum reconnection time (`MaxReconnectionTime`) is reached.   
> 3. This continues until reconnection succeeds, or the user manually calls `Close()`.   

**Basic Example**
```CSharp
var client1 = new TcpClient("127.0.0.1", 8080);
client1.ConnectionMode = ConnectionMode.Manual;// Manual, system default.
client1.ConnectionMode = ConnectionMode.AutoOpen;// Auto open.
client1.ConnectionMode = ConnectionMode.AutoReconnection;// Auto reconnection.
client1.MaxReconnectionTime = 10;// Maximum reconnection time, in seconds. Default is 10 seconds.
```

**Advanced Example (Using `IsAutoClose` to send or receive messages)**
> `IsAutoClose` defaults to true. It only takes effect when the mode is `AutoOpen`.   
> Use case: when you need to send or receive messages continuously in a short connection or when the connection status is unknown.
```CSharp
client1.ConnectionMode = ConnectionMode.AutoOpen;

// Without IsAutoClose
client.SendReceive("1/3"); // Open -> send -> receive -> close
client.SendReceive("2/3"); // Open -> send -> receive -> close
client.SendReceive("3/3"); // Open -> send -> receive -> close

// With IsAutoClose
client.IsAutoClose = false;
client.SendReceive("1/3"); // Open -> send -> receive
client.SendReceive("2/3"); // Send -> receive
client.IsAutoClose = true;
client.SendReceive("3/3"); // Send -> receive -> close
```

### Receiving Data and Receive Mode (ReceiveMode)
**Receiving Data:**   
In the client, there are 2 places where data can be received: the `Received` event and the `Receive()` / `SendReceive()` methods. The methods have higher priority than the event. ==If the method receives data, the event will not receive it.==
```CSharp
// Global receive mode setting
// Default mode in methods
client.ReceiveMode = ReceiveMode.ParseByteAll();
// Default mode in events
client.ReceiveModeReceived = ReceiveMode.ParseByteAll();
```

**Receive Mode:**   
Received data is first split into frames by the "Receive Mode", and then processed by the "Data Processor".
> Suppose the other party sends you the strings "ab\r\n" and "cd\r\n", with a 100 ms interval between them.   
> Here "ab\r\n" is one frame, and "a" means one unit.    
> Suppose each frame is 100 ms apart, and each unit is 1 ms apart.    

```CSharp
// This receive does not use the default mode; it uses the specified mode
client.SendReceive("state", ReceiveMode.ParseToEnd("\r\n"));
client.Receive(ReceiveMode.ParseToEnd("\r\n"));
```

| Code                                 | Result      | Description | Recommended Scenario |
| ------------------------------------ | --------- |------------ | ------------ |
| `ReceiveMode.ParseByte(2)`           | ab        | Read the specified number of bytes | When the communication protocol has specified that a frame has a fixed length, or when the remaining length to receive is known |
| `ReceiveMode.ParseByteAll()`         | a or ab\r\n | Read all immediately available bytes | When high efficiency is needed and nothing is known. Protocols such as TCP generally treat all information as one frame; serial ports generally treat one unit as one frame |
| `ReceiveMode.ParseChar(1)`         | a         | Read the specified number of characters | Same as `ParseByte()` |
| `ReceiveMode.ParseTime(10)`          | ab\r\n    | Read until no new message arrives after the specified time interval | A compromise when you know nothing but want to obtain complete information. The cost is sacrificing the specified time. Generally used as the default for serial ports |
| `ReceiveMode.ParseToEnd("\r\n") ` | ab\r\n    | Read until the specified information is encountered | When the end of each frame is known |

### Send/Receive Data and Data Processor (IDataProcessor)
**Send/Receive Data**  
1. When sending data, the data can be uniformly processed before being sent. </br>
2. After receiving data, the data can be processed before being forwarded.  </br>
3. Multiple data processors can be stacked. The ones added first are processed first (so in some cases, the receive processors should be in reverse order relative to the send processors).

**Data Processor**
```CSharp
// Add a data processor for sending
client.SendDataProcessors.Add(new EndAddValueDataProcessor("\r\n", client.Encoding));
// Add a data processor for receiving
client.ReceivedDataProcessors.Add(new EndClearValueDataProcessor("\r\n", client.Encoding));
```
| Name | Description |
| ----------- | -------------- |
| EndAddValueDataProcessor   | Add a fixed value to the end. For example, add CRLF at the end |
| EndClearValueDataProcessor | Remove a fixed value from the end. For example, remove CRLF from the end |
| PadLeftDataProcessor   | Add a fixed value to the left (head) to reach the specified length. |
| PadRightDataProcessor | Add a fixed value to the right (tail) to reach the specified length. |
| StartAddValueDataProcessor   | Add a fixed value to the beginning |
| StartClearValueDataProcessor | Remove a fixed value from the beginning |
| TrimDataProcessor   | Remove the specified matching item from both ends. |
| TrimEndDataProcessor   | Remove the specified matching item from the end. |
| TrimStartDataProcessor | Remove the specified matching item from the beginning. |

**Custom Data Processor**   

```CSharp
// Implement the `IDataProcessor` interface
// A data processor that does no processing
public class NullDataProcessor : IDataProcessor
{
    public byte[] DataProcess(byte[] data) => data;
}
```

Usage:
```CSharp
client1.SendDataProcessors.Add(new NullDataProcessor());
client1.ReceivedDataProcessors.Add(new NullDataProcessor());
```
### Active Heartbeat and Passive Heartbeat (Heartbeat)
> Note: Heartbeat does not take effect in `ConnectionMode.AutoOpen` mode.

**Active Heartbeat**  
> Actively send (loop) -> receive =》 ok   
```CSharp
client1.HeartbeatTime = 5000;// Interval. Set to 0 to pause sending heartbeats
// Send "1" each time and report the heartbeat result.
client1.Heartbeat = (a) =>
{
    var aa = a.Send("1");
    return aa.IsSucceed;
};

client1.Open();// Open; process properties and events before opening
```
**Passive Heartbeat**   
> Passively receive =》 ok  
```CSharp
client1.HeartbeatReceiveTime = 5000;// Detection interval.
client1.Open();// Open; process properties and events before opening
```

## Service Base (ServiceBase) `beta`

**Properties**

| Name   | Description  |
| ------ | --------- |
| Encoding | String encoding, default UTF8 |
| TimeOut | Timeout (send, receive, connect) (milliseconds). -1 means permanent; default 3000 |
| ReceiveMode | Method of receiving data |
| ReceiveModeReceived | Method of receiving data, under the `Received` event. |


**Methods**

| Name   | Description  |
| ------ | --------- |
| Open | Open |
| Close | Close |


## TcpClient
`TcpClient : ClientBase`
```CSharp
ClientBase client1 = new TcpClient("127.0.0.1", 502);
client1.Encoding = Encoding.UTF8;

// 1: Connection mode. Auto reconnection is commonly used
client1.ConnectionMode = ConnectionMode.Manual;// Manual, system default. You need to open and close it yourself; this mode is more flexible.
client1.ConnectionMode = ConnectionMode.AutoOpen;// Auto open. When Open() is not executed, each send and receive automatically opens and closes. This is suitable for scenarios requiring short connections. If a temporary long connection is needed, you can also call Open() and then Close().
client1.ConnectionMode = ConnectionMode.AutoReconnection;// Auto reconnection. After Open() is executed, if a disconnection is detected, it automatically opens again. This is suitable for scenarios requiring long connections. Calling Close() will stop reconnection.

// 2: Receive mode. Handle sticky packet issues in the best way you think
client1.ReceiveMode = ReceiveMode.ParseByteAll();
client1.ReceiveModeReceived = ReceiveMode.ParseByteAll();

// 3: Data processors. You can add a newline when sending, remove a newline when receiving, and customize them
client1.SendDataProcessors.Add(new EndAddValueDataProcessor("\r\n", client1.Encoding));
client1.ReceivedDataProcessors.Add(new EndClearValueDataProcessor("\r\n", client1.Encoding));

// 4: Event-driven.
client1.Opened += (a) => { Console.WriteLine("Connected successfully."); };
client1.Closed += (a, b) => { Console.WriteLine($"Closed successfully. Close code: {b}"); };
client1.Received += (a, b) => { Console.WriteLine($"Message received: {a.Encoding.GetString(b)}"); };

client1.Open();// Open; process properties and events before opening

// 5: Simple send, receive, and send-and-wait operations. 
client1.Send("abc");// Send
client1.Receive();// Receive
client1.Receive(3000);// Receive with 3-second timeout
client1.Receive(ReceiveMode.ParseToEnd("\n", 3000));// Receive data ending with \n, timeout 3 seconds 
client1.SendReceive("abc", 3000);// Send and wait to receive data, 3-second timeout
client1.SendReceive("abc", ReceiveMode.ParseToEnd("\n", 3000));// Send and receive data ending with \n, timeout 3 seconds 
```

## TcpServer
`TcpServer : ServiceBase`
```CSharp
var service = new TcpService("127.0.0.1", 8005);
service.Encoding = Encoding.UTF8;
// Receive mode
service.ReceiveMode = ReceiveMode.ParseByteAll();// Default mode for the "Receive()" method
service.ReceiveModeReceived = ReceiveMode.ParseByteAll();// Default mode for the "Received" event
service.Opened += (a) =>
{
    Console.WriteLine($"Client[{(a as INetwork)?.Socket?.RemoteEndPoint}] connected successfully");
};
service.Closed += (a, b) =>
{
    Console.WriteLine($"Client[{(a as INetwork)?.Socket?.RemoteEndPoint}] closed successfully, close code: {b}");
};
service.Received += (a, b) =>
{
    Console.WriteLine($"Client[{(a as INetwork)?.Socket?.RemoteEndPoint}] received message: " + a.Encoding.GetString(b));
};

// Open the connection. All properties must be set before opening.
service.Open();

if (service.Clients.Any())
{
    // Send a message to the first client. The usage here is the same as 'TcpClient'; refer to the 'TcpClient' documentation.
    service.Clients[0].Send("abc");// Send
    service.Clients[0].Receive();// Receive. (For server-side receiving, handling in events is recommended.)
    service.Clients[0].SendReceive("abc", 3000);// Send and wait to receive data, 3-second timeout. (For server-side receiving, handling in events is recommended.)
}
```

## UdpClient
`UdpClient : ClientBase`
```CSharp
// Remote send: 10.10.1.69:8001
// Local listen: 10.10.1.69:8002
var client = new UdpClient("10.10.1.69", 8001, 8002);
client.Encoding = Encoding.UTF8;
client.ConnectionMode = ConnectionMode.Manual;// Do not use AutoReconnection mode for UDP

client.Opened += (a) => { Console.WriteLine("Connected successfully."); };
client.Closed += (a, b) => { Console.WriteLine($"Closed successfully. Error code: {b}"); };
client.Received += (a, b) =>
{
    Console.WriteLine($"Received message from [{(a as INetwork)?.Socket?.RemoteEndPoint}]: {a.Encoding.GetString(b)}");
};

client.Open();// Open; process properties and events before opening

client.Send("abc");
var info1 = client.Receive(3000);// Receive with 3-second timeout
var info2 = client.SendReceive("abc");// Send and wait to receive

client.Close();
```

## SerialPortClient ; SerialClient (Serial Port)
`SerialPortClient : ClientBase`   
`SerialClient : ClientBase`  
> Serial ports are point-to-point transmission, so there is only `SerialPortClient` and no `SerialPortService`. Use two `SerialPortClient` instances instead.

Difference comparison table   

| Name | Package | Dependency | Pros and Cons |
|--|--|--|--|
|SerialPortClient|Ping9719.IoT|System.IO.Ports|Officially maintained and supported by .NET|
|SerialClient|Ping9719.IoT.Hid|HidSharp|Can solve the issue where some serial ports cannot be opened on Linux|

On Linux, you need to add the user to groups:
```
sudo usermod -a -G dialout $USER   
sudo usermod -a -G uucp $USER
```

The usage is the same. Here `SerialPortClient` is used as an example:
```CSharp
var client1 = new SerialPortClient("COM1", 9600);

// The following are default initialization properties and can be omitted
client1.ConnectionMode = ConnectionMode.Manual;// Manual open. Auto reconnection is not very meaningful for serial ports.
client1.Encoding = Encoding.ASCII;// How to parse strings
client1.TimeOut = 3000;// Timeout
client1.ReceiveMode = ReceiveMode.ParseTime();// Default mode for the "Receive()" method. For serial ports, receiving data based on time is better.
client1.ReceiveModeReceived = ReceiveMode.ParseTime();// Default mode for the "Received" event

// All events are the same as TcpClient and are not repeated here.

// Open the connection. All properties must be set before opening.
client1.Open();

// All sending and receiving are the same as TcpClient and are not repeated here.
``` 

## HttpClient
`HttpClient : ClientBase`   

> Want to customize JSON parsing? Please refer to [How to customize Json parsing?](#UserJson).

```CSharp

// 1. Common usage

HttpClient.Default.Get<string>("http://www.baidu.com");// http://www.baidu.com
HttpClient.Default.Get<string>(new string[] { "http://www.baidu.com", "s" });// http://www.baidu.com/s
HttpClient.Default.Get<string>("http://www.baidu.com", new { a = 1, b = "ab" });// http://www.baidu.com?a=1&b=ab
HttpClient.Default.Post<User>("http://www.baidu.com", new { id = 1 }, new { a = 1, b = "ab" });// http://www.baidu.com?a=1&b=ab  body:{id:1}

// 2. The body can automatically set Content-Type according to its type

// Content-Type: text/plain; charset=utf-8
HttpClient.Default.Post<string>("http://127.0.0.1:8080/a/b", "abc");
// Content-Type: application/octet-stream
HttpClient.Default.Post<string>("http://127.0.0.1:8080/a/b", new byte[] { 1, 2 });
// Content-Type: application/octet-stream
HttpClient.Default.Post<string>("http://127.0.0.1:8080/a/b", new MemoryStream(new byte[] { 1, 2 }));
// Content-Type: application/json; charset=utf-8
HttpClient.Default.Post<string>("http://127.0.0.1:8080/a/b", new byte[] { 1, 2 }.ToList());
// Content-Type: application/json; charset=utf-8
HttpClient.Default.Post<string>("http://127.0.0.1:8080/a/b", new { a = 1, b = 2 });

// 3. Use form (multipart/form-data) to upload files

var formData = new MultipartFormDataContent();
var filePath = @"D:\123.png";
formData.Add(new StreamContent(File.OpenRead(filePath)), "file", Path.GetFileName(filePath));// Upload a file named file
formData.Add(new StringContent("18"), "age");// Add age string
// Submit
HttpClient.Default.Post<string>("http://127.0.0.1:8080/a/b", content: formData);
```

## HttpServer
`HttpServer : ServiceBase`   

1. Implemented using `System.Net.HttpListener`. In some cases, administrator privileges are required to run.   

```CSharp
HttpService service = new HttpService(8090);
// Error handling (errors in Received will appear here)
service.ReceivedException = (request, response, err) =>
{
    return "{\"info\":\"" + err.Message + "\"}";
};
// Handle client data
service.Received = (request, response, data) =>
{
    // Request
    var method = request.HttpMethod;
    var urlAbs = request.Url?.AbsolutePath;
    var urlQue = request.Url?.Query;
    // Response
    response.ContentType = "application/json";

    if (method == HttpMethod.Get.Method)
    {
        // Home page
        if (urlAbs == "/")
        {
            return "{\"info\":\"Welcome\"}";
        }
        else if (urlAbs == "/a/b/c")
        {
            return "{\"info\":\"ok\"}";
        }
    }
    else if (method == HttpMethod.Post.Method)
    {

    }

    response.StatusCode = (int)HttpStatusCode.NotFound;
    return "{\"info\":\"Not Found\"}";
};
service.Open();
```

## UsbHidClient (USB)
`UsbHidClient : ClientBase`   

> Requires package `Ping9719.IoT.Hid`    
> Get report information: `UsbHidClient.GetReportDescriptor(UsbHidClient.GetNames[0])`    
>> 1. Report type: Input, Output, Feature   
>> 2. Report ID: generally in the frame header, default value is `0x00`. Can be handled using "message processors".    
>> 3. Report length: required fixed length. If insufficient, generally append `0x00` at the end to pad (low speed 8, full speed 64, high speed 1024). Can be handled using "message processors".    

```CSharp
var names = UsbHidClient.GetNames;// Get all USB devices
var client = new UsbHidClient(names[0]);// Access the first device

// Use message processors to handle reports
{
    // Add report ID (actual behavior depends on documentation)
    client.SendDataProcessors.Add(new StartAddValueDataProcessor(0));
    // Add report length padding (actual behavior depends on documentation)
    client.SendDataProcessors.Add(new PadRightDataProcessor(64));
    // Clear report ID (actual behavior depends on documentation)
    client.ReceivedDataProcessors.Add(new StartClearValueDataProcessor(0));
    // Clear report length padding (actual behavior depends on documentation)
    client.ReceivedDataProcessors.Add(new TrimEndDataProcessor(0));
}
```

## BleClient (Bluetooth)
`UsbHidClient : ClientBase`  

> Requires package `Ping9719.IoT.Hid` 
```CSharp
var names = BleClient.GetNames;// Get all Bluetooth devices
var client = new BleClient(names[0]);// Access the first device
```

# Protocol
## Modbus
`ModbusRtuClient : IClientData`   
`ModbusTcpClient : IClientData`   
`ModbusAsciiClient : IClientData`   

Modbus Rtu : `Station Number` + `Function Code` + `Address` + `Length` + `Checksum`   
Modbus Tcp : `Message Number` + `0x0000` + `Subsequent Byte Length` + `Station Number` + `Function Code` + `Address` + `Length`   

```CSharp
var client = new ModbusRtuClient("COM1", 9600, format: EndianFormat.ABCD);
var client = new ModbusRtuClient(new TcpClient("127.0.0.1", 502), format: EndianFormat.ABCD);// ModbusRtu protocol over TCP
var client = new ModbusTcpClient("127.0.0.1", 502, format: EndianFormat.ABCD);
var client = new ModbusTcpClient(new SerialPortClient("COM1", 9600), format: EndianFormat.ABCD);// ModbusTcp protocol over serial port
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;// Auto reconnection. TCP recommends auto reconnection; serial ports recommend the other two modes.
client.Client.Open();// Open

client.Read<Int16>("100");// Read register
client.Read<bool>("100.1");// Read bit in register
client.Read<Int16>("s=2;x=3;100");// Read register, corresponding to station number, function code, address
client.Read<bool>("100");// Read coil
client.Read<bool>("100", 10);// Read multiple coils
client.Read<bool>("100.1", 10);// Read bits in register

client.Write<Int16>("100", 100);// Write register
client.Write<Int16>("100", 100, 110);// Write multiple registers

client.ReadString("100", 5, Encoding.ASCII);// Read string
client.ReadString("100", 5, null);// Read string in hexadecimal form
client.WriteString("500", "abcd", 10, Encoding.ASCII);// Write string. When count > 0 and insufficient, it automatically pads with 0x00 at the end
```

## PLC Type Comparison Table
> Types marked with * are commonly used. Generally, unless otherwise specified, all are supported.

| C#</br>.Net | Siemens S7</br>SiemensS7 | Mitsubishi MC</br>MitsubishiMc | Omron Fins</br>OmronFins | Omron Cip</br>OmronCip | Inovance</br>Inovance |
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

## Rockwell (AllenBradleyCipClient)
`AllenBradleyCipClient : IClientData`  

This protocol is currently less tested. Please test it before using it in a production environment.    
It has been found that some models can also be replaced with `OmronCipClient`.
```CSharp
AllenBradleyCipClient client = new AllenBradleyCipClient("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;// Auto reconnection
client.Client.Open();// Open

client.Read<bool>("abc");// Read
client.Write<bool>("abc",true);// Write
```

## Keyence (KeyenceHostLinkClient)
`KeyenceHostLinkClient : IClientData`  

```CSharp
KeyenceHostLinkClient client = new KeyenceHostLinkClient("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;// Auto reconnection
client.Client.Open();// Open

client.Read<bool>("B0");// Read
client.Write<bool>("B0",true);// Write
```

## Inovance (InovanceModbusTcpClient)
`InovanceModbusTcpClient : IClientData`  
```CSharp
var client = new InovanceModbusTcpClient("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;// Auto reconnection
client.Client.Open();// Open

client.Read<bool>("M1");// Read
client.Read<Int16>("D1",5);// Read 5 items
client.Write<bool>("M1",true);// Write
client.Write<Int16>("D1",new Int16[]{1,2});// Write multiple
```

## Mitsubishi (MitsubishiMcClient)
`MitsubishiMcClient : IClientData`  
Test coverage table

| Type         | Single-point Read/Write | Batch Read/Write |
|--------------|------------------|---------------|
| bool         | ✔️               | ✔️ (internal loop) |
| short        | ✔️               | ✔️            |
| int32        | ✔️               | ✔️            |
| float        | ✔️               | ✔️            |
| double       | ✔️               | ✔️            |
| string       | ✔️               | ✔️            |

> Note: Batch writing of bool arrays uses looped single-point writing, which is relatively slow.
>
> It also supports byte, sbyte, ushort, uint32, int64, and uint64 types. Since they are used less often, please test them yourself.
>
> If read/write fails or times out, it may be because the IP, port, or communication data code is not set.   
> Using GX Works2 as an example:   
> [Parameter]-[PLC Parameter]-[Built-in Ethernet Port Setting]-[IP Address Setting] Check the IP here.   
> [Parameter]-[PLC Parameter]-[Built-in Ethernet Port Setting]-[Open Setting] Check the port here.   
> [Parameter]-[PLC Parameter]-[Built-in Ethernet Port Setting]-[Communication Data Code Setting] Binary code communication must be checked.   
> [Parameter]-[PLC Parameter]-[Built-in Ethernet Port Setting]-[Allow Writing During RUN] Must be checked.   
> Using GX Works3 as an example:   
> [Parameter]-[Module Parameter]-[Ethernet Port] Check the IP and port here.

```CSharp
//
var client = new MitsubishiMcClient(MitsubishiVersion.Qna_3E, "127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;// Auto reconnection
client.Client.Open();// Open

client.Read<Int16>("W0");// Read
client.Read<Int16>("W0",5);// Read 5 items
client.Write<Int16>("W0",10);// Write
client.Write<Int16>("W0",new Int16[]{1,2});// Write multiple
```

## Omron (OmronFinsClient)
`OmronFinsClient : IClientData`  
```CSharp
OmronFinsClient client = new OmronFinsClient("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;// Auto reconnection
client.Client.Open();// Open

client.Read<Int16>("W0");// Read
client.Read<bool>("D100",10);// Read 10 bools
client.Write<Int16>("W0",10);// Write
client.Write<Int16>("W0",new Int16[]{1,2});// Write multiple
```

## Omron (OmronCipClient)
`OmronCipClient : IClientData` 
```CSharp
OmronCipClient client = new OmronCipClient("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;// Auto reconnection
client.Client.Open();// Open

// Read and write
client.Read<bool>("abc");// Read
client.Write<bool>("abc",true);// Write

// Read and write arrays.
// Note: The PLC type 'ARRAY[0..4] OF INT' means an int16 array with length 5.
// Note: Reading necessarily returns 5 items; writing must also write 5 items.
client.Read<Int16[]>("abc");
client.Read<bool[]>("abc",5);// Read array and take the first 5 items
client.Write<Int16>("abc", new Int16[] { 1, 2, 0, 5,2 });// Type + length must match the PLC
```

## Siemens (SiemensS7Client)
`SiemensS7Client : IClientData` 
```CSharp
var client = new SiemensS7Client(SiemensVersion.S7_1200, "127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;// Auto reconnection
client.Client.Open();// Open

// Supports common types (int, float...)
client.Read<Int16>("BD100.0.0");// Read
client.Write<Int16>("BD100.0.0",10);// Write

// Supports special types (string, DateTime, TimeSpan, Char)
client.Read<DateTime>("BD100.0.0");// Read
client.Write<DateTime>("BD100.0.0",DateTime.Now);// Write

// Supports very long reads and writes
client.Read<Int16>("BD100.0.0",9999);// Continuously read 9999 data items; takes only about hundreds of milliseconds
client.Write<Int16>("BD100.0.0",new Int16[]{1,2,3});// Continuously write 9999 data items; takes only about hundreds of milliseconds
client.Write<bool>("BD100.0.0", new bool[] { true, false, true, true, false, false, false, false });// bool type only supports multiples of 8

// String description
client.Read<string>("BD100.0.0");// The PLC type must be string. Only ASCII encodings such as letters and numbers are supported.
client.ReadString("BD100.0.0");// The PLC type must be WString. Supports UTF16 encodings such as Chinese.
// Special PLC type: String[3]
client.ReadString("BD100.0.0", 3, Encoding.ASCII);
client.WriteString("BD100.0.0", "abc", 3, Encoding.ASCII);
```

# Algorithm
## Affine Transform (AffineTransform)
> Generally used for bidirectional conversion between camera coordinates and robot coordinates.
```CSharp
var converter = new AffineTransform();
// Add calibration coordinate pairs (camera coordinate xy + robot coordinate xy)
converter.AddCalibration(100, 150, 50, 75);
converter.AddCalibration(200, 250, 100, 125);
converter.AddCalibration(300, 350, 150, 175);
converter.AddCalibration(400, 450, 200, 225);
var isok = converter.Calibrate();
// Transform (camera coordinates => robot coordinates)
var p1 = converter.Transform(250, 300);
// Inverse transform (robot coordinates => camera coordinates)
var p2 = converter.TransformInverse(125, 150);                  
```

## Average Point (AveragePoint)
Use cases:   
1. Scenarios where a robot places/picks uniformly   
2. Scenarios where a servo moves uniformly

> Suppose: the known start is 2; the end is 8; there are 4 points in total.    
> 2--[?]--[?]--8    
> 2--[4]--[6]--8   
> By visual estimation, the second is 4 and the third is 6.

```CSharp
// Single group
// 2,4,6,8
var aaa1 = AveragePoint.Start(2, 8, 4);

// Multiple groups
// 1,2,3,4
// 2,4,6,8
var aaa2 = AveragePoint.Start("1,2", "4,8", 4);
```

## CRC
```CSharp
byte[] bytes = new byte[] { 1, 2 };
// CRC algorithms
var c1 = CRC.Crc8(bytes);
var c2 = CRC.Crc8Itu(bytes);
var c3 = CRC.Crc8Rohc(bytes);
var c4 = CRC.Crc16(bytes);
var c5 = CRC.Crc16Usb(bytes);
var c6 = CRC.Crc16Modbus(bytes);
var c7 = CRC.Crc32(bytes);
var c8 = CRC.Crc32Q(bytes);
var c9 = CRC.Crc32Sata(bytes);
// CRC verification
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

## Fourier Filter (FFTFilter)
Fourier filter (automatic parallelization). 1 million points takes 430 ms.
```CSharp
double[] result1 = FFTFilterOptimized.FilterFFT(data, 0.005);    // Use default parallel count 6
double[] result2 = FFTFilterOptimized.FilterFFT(data, 0.005, 8); // Use 8 parallel tasks
double[] result3 = FFTFilterOptimized.FilterFFT(data, 0.005, 1); // Disable parallel computation
```


## Stable Marriage Matching (GaleShapleyAlgorithm)
Use cases:   
1. Scenarios where similar items are paired

> Suppose the preference relationships are as follows (the more preferred, the earlier):<br/>
M0❤W1,W0<br/>M1❤W0,W1<br/>M2❤W0,W1,W2,W3,W4<br/>M3❤W3<br/>M4❤W3

```CSharp
// 1. All people participating in matching
var ms = new string[] { "M0", "M1", "M2", "M3", "M4", };
var ws = new string[] { "W0", "W1", "W2", "W3", "W4", };
// 2. Convert to items
var msi = ms.Select(o => new GaleShapleyItem<string>(o)).ToList();
var wsi = ms.Select(o => new GaleShapleyItem<string>(o)).ToList();
// 3. Configure preference lists. Suppose the liking relationships are as follows: (from high to low)
// M0❤W1,W0   M1❤W0,W1   M2❤W0,W1,W2,W3,W4   M3❤W3   M4❤W3
msi[0].Preferences = new List<GaleShapleyItem<string>>() { wsi[1], wsi[0] };
msi[1].Preferences = new List<GaleShapleyItem<string>>() { wsi[0], wsi[1] };
msi[2].Preferences = new List<GaleShapleyItem<string>>() { wsi[0], wsi[1], wsi[2], wsi[3], wsi[4] };
msi[3].Preferences = new List<GaleShapleyItem<string>>() { wsi[3] };
msi[4].Preferences = new List<GaleShapleyItem<string>>() { wsi[3] };
// 4. Start calculation
GaleShapleyAlgorithm.Run(msi);
// 5. Print results
foreach (var item in msi)
{
    // M0❤M1   M1❤M0   M2❤M2   M3❤M3   M4❤null
    Console.Write($"{item.Item}❤{(item.Match?.Item) ?? "null"}   ");
}
```


## Linear Regression (LinearRegression)
Use cases:   
1. Conversion between sensor output values (temperature, pressure, flow) and real values   
> Temperature sensor: voltage value (mV) → temperature (°C)   
> Measured data: 10 mV → 25°C, 30 mV → 75°C, 50 mV → 125°C  
> Inferred data: 20 mV → 50°C    
 
2. Predict future sales based on historical data     
3. Impact of product price changes on sales   
4. Relationship between health indicators and disease risk  
etc.

```CSharp
var regression = LinearRegression.Fit("10,30,50", "25,75,125");
double result = regression.Project(20);// 50
```


# Devices and Instruments (Device)
1. Please first read the contents in the `Client Base (ClientBase)` section.   
2. Please open before use, or set the auto-open mode.   
3. A special protocol-free device `RawDevice` is provided, which can be used to load different pipelines for sending and receiving data.


```CSharp
// Simple usage of the protocol-free device `RawDevice`
RawDevice rd = new RawDevice(new TcpClient("127.0.0.1", 502));
rd.Client.Encoding = Encoding.UTF8;
rd.Client.ConnectionMode = ConnectionMode.AutoReconnection;
rd.Client.Received += (a, b) =>
{
    // Switching before or after does not affect receiving data
    Console.WriteLine(a.Encoding.GetString(b));
};
rd.Client.Open();
rd.Client.Send("123");

// Switch to serial port at runtime before sending data
rd.SetClient(new SerialPortClient("COM2", 9600));
rd.Client.Send("123");
```

## Airtight Test (Airtight)
```CSharp
// Cosmo airtight test
CosmoAirtight dev1 = new CosmoAirtight("COM1");
```

## Laser Marking (Mark)
```CSharp
DaZhuMark dev1 = new DaZhuMark("127.0.0.1");// DaZhu
HuaPuMark dev2 = new HuaPuMark("127.0.0.1");// HuaPu
```
## RFID (Rfid)
```CSharp
BeiJiaFuRfid rfid1 = new BeiJiaFuRfid("127.0.0.1");// Pepperl+Fuchs
DongJiRfid rfid2 = new DongJiRfid("127.0.0.1");// DongJi
TaiHeSenRfid rfid3 = new TaiHeSenRfid("127.0.0.1");// TaiHeSen
WanQuanRfid rfid4 = new WanQuanRfid("127.0.0.1");// WanQuan

// WanQuan usage
rfid4.ReadString(RfidAddress.GetRfidAddressStr(RfidArea.ISO15693, null, 1), 2, EncodingEnum.ASCII.GetEncoding());
rfid4.WriteString(RfidAddress.GetRfidAddressStr(RfidArea.ISO15693, null, 1), "A001", 2, EncodingEnum.ASCII.GetEncoding());
```

<details><summary style="padding:10px;border:1px solid silver;border-radius:4px;">🖥️ WPF</summary>
<div style="padding:5px;margin-top:8px;border:1px solid silver;border-radius:4px;">

```CSharp
// Namespace
xmlns:piIoT="https://github.com/ping9719/IoT"
// If it is Rfid, then it is: RfidView
<piIoT:RfidView DeviceData="{Binding Dev1}" Area="ISO15693" IsReadPara="True" Encoding="ASCII" ReadCount="2" WriteVal="A001"/>
```
![](img/RfidView.png)

</div></details>

## Robot (Robot)
`EpsonRobot : IClient` 
```CSharp
// Epson robot
EpsonRobot client = new EpsonRobot("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;// Auto reconnection
client.Client.Open();// Open

client.Start();
client.Pause();
```

## Scanner (Scanner)
```CSharp
HoneywellScanner dev1 = new HoneywellScanner("127.0.0.1");// Honeywell
MindeoScanner dev1 = new MindeoScanner("127.0.0.1");// Mindeo
```
## Screw Machine (Screw)
```CSharp
MiLeScrew dev1 = new MiLeScrew("127.0.0.1");// MiLe
```

## Welding Machine (Weld)
```CSharp
// JBC welding machine
JBCWeld jBCWeld = new JBCWeld("COM5");
jBCWeld.Client.Open();
while (true)
{
    var assa = jBCWeld.ReadInfo();
    Console.WriteLine(assa.IsSucceed ? assa.Value : assa.ErrorText);
    Thread.Sleep(200);
}
```

# FAQ
## 1. How to use a custom protocol?
Taking the Hikvision general-purpose barcode scanner as an example:
```CSharp
/// <summary>
/// Hikvision general-purpose barcode scanner
/// </summary>
public class ScanCode : ClientHostBase
{
    public ScanCode(ClientBase client)
    {
        Client = client;
        Client.Encoding = Encoding.UTF8;
    }

    public ScanCode(string ip, int port = 1500) : this(new TcpClient(ip, port)) { }

    public IoTResult<string> ReadCode()
    {
        // Read code: send start
        // Success: "code content"; failure: "NoRead"
        var code = Client.SendReceive("start");
        return code.IsSucceed && code.Value == "NoRead" ? code.AddError(code.Value) : code;
    }
}
```
Usage:
```CSharp
ScanCode scanCode = new ScanCode("192.168.0.1", 5200);
scanCode.Client.ConnectionMode = ConnectionMode.AutoOpen;// Barcode scanners do not need auto reconnection
// scanCode.Client.Open();// Manual opening is not required in auto-open mode

// Read code content
scanCode.ReadCode();
```

## 2. How to customize Json parsing?
1. Json parsing prioritizes user-defined parsing.   
2. In `NET8`, `System.Text.Json` is used.   
3. In non-`NET8`, `Newtonsoft.Json` is preferred; if it fails, `System.Runtime.Serialization.Json` is used.  

Therefore, in non-`NET8`, custom Json parsing is recommended:
```CSharp
// Set at the program entry point; only needs to be set once.
JsonParse.SerializeFunc = (obj) => Newtonsoft.Json.JsonConvert.SerializeObject(obj);
JsonParse.DeserializeFunc = (json) => Newtonsoft.Json.JsonConvert.DeserializeObject(json);
```

## 3. How to switch communication methods at runtime?
> As long as it inherits from `ClientProviderBase`, it can be switched.
```CSharp
// Use serial port for ModbusRtu protocol communication
var client = new ModbusRtuClient(new SerialPortClient("COM1", 9600));
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;
client.Client.Open();

// Switch to TCP for ModbusRtu protocol communication.
// It will use the old properties. If the old one is open, it will automatically close the old one and open the new one.
client.SetClient(new TcpClient("127.0.0.1", 502));
```