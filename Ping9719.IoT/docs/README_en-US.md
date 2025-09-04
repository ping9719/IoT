# Language:
[简体中文](README.md) || [English](README_en-US.md) 

# Table of Contents
- [Communication](#Communication)
    - [ClientBase](#ClientBase)
        - [1. Connection Mode (ConnectionMode)](#ConnectionMode)
        - [2. Receive Mode (ReceiveMode)](#ReceiveMode)
        - [3. Data Processor (IDataProcessor)](#IDataProcessor)
    - [TcpClient](#TcpClient)
    - [TcpServer](#TcpServer)
    - [SerialPortClient](#SerialPortClient)
    - UdpClient (In Progress)
    - UdpServer (To Be Developed)
    - HttpServer (To Be Developed)
    - MqttClient (To Be Developed)
    - MqttServer (To Be Developed)
- [Modbus](#Modbus)
    - ModbusRtuClient
    - ModbusTcpClient
    - ModbusAsciiClient
- [PLC](#PLC)
    - [Type Mapping Table](#PlcType)
    - Rockwell (AllenBradleyCipClient) (To Be Tested)
    - [Inovance (InovanceModbusTcpClient)](#InovanceModbusTcpClient)
    - [Mitsubishi (MitsubishiMcClient)](#MitsubishiMcClient)
    - [Omron (OmronFinsClient, OmronCipClient)](#OmronFinsClient)
    - [Siemens (SiemensS7Client)](#SiemensS7Client)
- [Robot](#Robot)
    - Epson Robot (EpsonRobot) (To Be Tested)
- [Algorithm](#Algorithm)
    - [Average Point Algorithm (AveragePoint)](#AveragePoint)
    - [CRC](#CRC)
    - [LRC](#LRC)
    - Fourier Algorithm (Fourier) (To Be Developed)
    - [Stable Marriage Matching Algorithm (GaleShapleyAlgorithm)](#GaleShapleyAlgorithm)
    - PID (To Be Developed)
    - RSA (To Be Developed)
- [Device](#Device)
    - [Airtight Testing (Airtight)](#Airtight)
        - Cosmo Airtight Tester (CosmoAirtight)
    - Fct
        - MengXun Fct (MengXunFct)
    - [Laser Marking (Mark)](#Mark)
        - Han's Laser Marking (DaZhuMark)
        - HuaPu Laser Marking (HuaPuMark)
    - [RFID (Rfid)](#Rfid)
        - Pepperl+Fuchs RFID (BeiJiaFuRfid)
        - TaiHeSen RFID (TaiHeSenRfid)
        - WanQuan RFID (WanQuanRfid)
    - [Barcode Scanner (Scanner)](#Scanner)
        - Honeywell Scanner (HoneywellScanner)
        - Mindeo Scanner (MindeoScanner)
    - [Screw Driver (Screw)](#Screw)
        - Quick Screw Driver (KuaiKeDeskScrew, KuaiKeScrew, KuaiKeTcpScrew) (Not Recommended)
        - MiLe Screw Driver (MiLeScrew)
    - [Temperature Control (TemperatureControl)](#TemperatureControl)
        - Quick Temperature Controller (KuaiKeTemperatureControl) (Not Recommended)
    - [Welding Machine (Weld)](#Weld)
        - Quick Welding Machine (KuaiKeWeld)
- [Extensions](#Ext)
    - 1. How to Use Custom Protocols

# Communication <a id="Communication"></a>
Interact using specified methods.

## ClientBase <a id="ClientBase"></a>
`TcpClient` and `SerialPortClient` both inherit from `ClientBase`, so their usage is identical.

### 1. Connection Mode <a id="ConnectionMode"></a>

Three connection modes:
> 1. Manual (General Use). You need to manually open and close the connection, offering greater flexibility.
> 2. Auto Open (Suitable for Short Connections). If `Open()` hasn't been called, it automatically opens and closes on every send/receive operation. Suitable for short-connection scenarios. For temporary long connections, you can call `Open()` and later `Close()`.
> 3. Auto Reconnection (Suitable for Long Connections). After calling `Open()`, it will automatically attempt to reconnect if a disconnection is detected. Suitable for long-connection scenarios. Calling `Close()` will stop reconnection attempts.

Auto Reconnection Rules:
> 1. After disconnection, reconnection attempts start with a 1-second wait.
> 2. If unsuccessful, the wait time increases by 1 second each time, up to the maximum reconnection time (`MaxReconnectionTime`).
> 3. This continues until reconnection succeeds or the user manually calls `Close()`.

Sample Code:
```CSharp
var client1 = new TcpClient("127.0.0.1", 8080);
client1.ConnectionMode = ConnectionMode.Manual; // Manual
client1.ConnectionMode = ConnectionMode.AutoOpen; // Auto Open
client1.ConnectionMode = ConnectionMode.AutoReconnection; // Auto Reconnection
client1.MaxReconnectionTime = 10; // Maximum reconnection time in seconds. Default is 10 seconds.
```

### 2. Receive Mode (ReceiveMode) <a id="ReceiveMode"></a>
Data Reception Introduction
There are two ways to receive data in a client: 1) the `Received` event, and 2) the `Receive()` or `SendReceive()` methods. Method-based reception takes precedence over events; if data is received via a method, the event will not trigger.

```CSharp
// Default mode for methods
client.ReceiveMode = ReceiveMode.ParseByteAll();
// Default mode for events
client.ReceiveModeReceived = ReceiveMode.ParseByteAll();
```

Received data is first split into frames by the 'Receive Mode', then processed by 'Data Processors'.
> Suppose the other side sends the strings "ab\r\n" and "cd\r\n" with a 100ms interval between them.
> Here, "ab\r\n" is one frame, where "a" represents one byte.
> Assume each frame is 100ms apart, and each byte is 1ms apart.

| Code | Result | Description | Recommended Scenario |
| ------------------------------------ | --------- |------------ | ------------ |
| `ReceiveMode.ParseByte(2)` | ab | Read a specified number of bytes | When the communication protocol specifies a fixed frame length or when the remaining length to receive is known |
| `ReceiveMode.ParseByteAll()` | a or ab\r\n | Read all immediately available bytes | When high efficiency is needed and no other information is available. TCP protocols usually treat a full message as one frame, while serial ports usually treat one byte as one frame |
| `ReceiveMode.ParseChar(1)` | a | Read a specified number of characters | Same as `ParseByte()` |
| `ReceiveMode.ParseTime(10)` | ab\r\n | End after a specified time interval with no new messages | A compromise when no information is known but complete data is desired, at the cost of the specified time delay. Often the default for serial ports |
| `ReceiveMode.ParseToEnd("\r\n")` | ab\r\n | End after reading the specified ending sequence | When the end of each frame is known |

### 3. Data Processor (IDataProcessor) <a id="IDataProcessor"></a>
Introduction
> 1. Data can be uniformly processed before sending.
> 2. Data can be processed after receiving but before forwarding.
> 3. Multiple data processors can be stacked; the order of processing follows the order of addition (so, in some cases, the receive processors should be the reverse order of the send processors).

Built-in Data Processors <a id="IDataProcessorIn"></a>

| Name | Description |
| ----------- | -------------- |
| EndAddValueDataProcessor | Add a fixed value to the end. For example, add a line break. |
| EndClearValueDataProcessor | Remove a fixed value from the end. For example, remove a line break. |
| PadLeftDataProcessor | Add a fixed value to the left (head) to reach a specified length. |
| PadRightDataProcessor | Add a fixed value to the right (tail) to reach a specified length. |
| StartAddValueDataProcessor | Add a fixed value to the beginning. |
| StartClearValueDataProcessor | Remove a fixed value from the beginning. |
| TrimDataProcessor | Remove specified matching items from both ends. |
| TrimEndDataProcessor | Remove specified matching items from the end. |
| TrimStartDataProcessor | Remove specified matching items from the beginning. |

Custom Data Processor
1. Simply implement the `IDataProcessor` interface in your class, e.g.: `public class MyCalss : IDataProcessor`.

2. Start using the custom data processor
```CSharp
client1.SendDataProcessors.Add(new MyCalss());
client1.ReceivedDataProcessors.Add(new MyCalss());
```

## TcpClient <a id="TcpClient"></a>
`TcpClient : ClientBase`
```CSharp
var client1 = new TcpClient("127.0.0.1", 8080);
// IMPORTANT!!! Connection mode is a crucial feature, with 3 available modes
client1.ConnectionMode = ConnectionMode.Manual; // Manual. You need to open and close it yourself, offering more flexibility.
client1.ConnectionMode = ConnectionMode.AutoOpen; // Auto Open. If Open() hasn't been called, it will automatically open and close on every send/receive. Suitable for short-connection scenarios. For temporary long connections, you can call Open() then Close().
client1.ConnectionMode = ConnectionMode.AutoReconnection; // Auto Reconnection. After calling Open(), if a disconnection is detected, it will automatically reconnect. Suitable for long-connection scenarios. Calling Close() will stop reconnection attempts.
client1.Encoding = Encoding.UTF8;
// Data processors: add line break when sending, remove line break when receiving
client1.SendDataProcessors.Add(new EndAddValueDataProcessor("\r\n", client1.Encoding));
client1.ReceivedDataProcessors.Add(new EndClearValueDataProcessor("\r\n", client1.Encoding));
// Receive mode
client1.ReceiveMode = ReceiveMode.ParseByteAll(); // Default mode for the "Receive()" method
client1.ReceiveModeReceived = ReceiveMode.ParseByteAll(); // Default mode for the "Received" event
client1.Opening = (a) =>
{
    Console.WriteLine("Connecting");
    return true;
};
client1.Opened = (a) =>
{
    Console.WriteLine("Connected successfully");
};
client1.Closing = (a) =>
{
    Console.WriteLine("Closing");
    return true;
};
client1.Closed = (a, b) =>
{
    Console.WriteLine("Closed successfully" + b);
};
client1.Received = (a, b) =>
{
    Console.WriteLine("Received message:" + a.Encoding.GetString(b));
};

// Open the connection; all properties must be set before opening
client1.Open();

// Send or receive data
client1.Send("abc"); // Send
client1.Receive(); // Receive
client1.Receive(3000); // Receive, 3-second timeout
client1.Receive(ReceiveMode.ParseToEnd("\n", 5000)); // Receive string ending with \n, timeout 5 seconds
client1.SendReceive("abc", 3000); // Send and receive, 3-second timeout
client1.SendReceive("abc", ReceiveMode.ParseToEnd("\n", 5000)); // Send and receive, timeout 5 seconds
```

## TcpServer <a id="TcpServer"></a>
`TcpServer : ServiceBase`
> `TcpServer` has only undergone basic testing. Please test the required functionalities yourself before use.
```CSharp
var service = new TcpService("127.0.0.1", 8005);
service.Encoding = Encoding.UTF8;
// Receive mode
service.ReceiveMode = ReceiveMode.ParseByteAll(); // Default mode for the "Receive()" method
service.ReceiveModeReceived = ReceiveMode.ParseByteAll(); // Default mode for the "Received" event
service.Opened = (a) =>
{
    Console.WriteLine($"Client[{(a as INetwork)?.Socket?.RemoteEndPoint}] connected successfully");
};
service.Closed = (a) =>
{
    Console.WriteLine($"Client closed successfully");
};
service.Received = (a, b) =>
{
    Console.WriteLine($"Client[{(a as INetwork)?.Socket?.RemoteEndPoint}] received message: " + a.Encoding.GetString(b));
};

// Open the connection; all properties must be set before opening
service.Open();

if (service.Clients.Any())
{
    // Send a message to the first client; usage is the same as 'TcpClient', no further explanation needed
    service.Clients[0].Send("123");
}
```

## SerialPortClient <a id="SerialPortClient"></a>
`SerialPortClient : ClientBase`
> Serial ports transmit point-to-point, so there is only `SerialPortClient`, no `SerialPortService`. Use two `SerialPortClient` instances instead.
```CSharp
var client1 = new SerialPortClient("COM1", 9600);

// The following are default initialization properties, which can be left unset
client1.ConnectionMode = ConnectionMode.Manual; // Manual open; auto-reconnection has limited use for serial ports
client1.Encoding = Encoding.ASCII; // How to parse strings
client1.TimeOut = 3000; // Timeout duration
client1.ReceiveMode = ReceiveMode.ParseTime(); // Default mode for the "Receive()" method; better to receive serial data based on time
client1.ReceiveModeReceived = ReceiveMode.ParseTime(); // Default mode for the "Received" event

// All events are the same as TcpClient, not repeated here

// Open the connection; all properties must be set before opening
client1.Open();

// All sending and receiving operations are the same as TcpClient, not repeated here
```

# Modbus <a id="Modbus"></a>
`ModbusRtuClient : IIoT`
`ModbusTcpClient : IIoT`
`ModbusAsciiClient : IIoT`
```CSharp
var client = new ModbusRtuClient("COM1", 9600, format: EndianFormat.ABCD);
var client = new ModbusRtuClient(new TcpClient("127.0.0.1", 502), format: EndianFormat.ABCD); // ModbusRtu protocol over TCP
var client = new ModbusTcpClient("127.0.0.1", 502, format: EndianFormat.ABCD);
var client = new ModbusTcpClient(new SerialPortClient("COM1", 9600), format: EndianFormat.ABCD); // ModbusTcp protocol over serial port
client.Client.Open();

client.Read<Int16>("100"); // Read register
client.Read<Int16>("100.1"); // Read bit in register; bit reading only supports single reads, preferably uint16, int16
client.Read<Int16>("s=2;x=3;100"); // Read register, specifying station number, function code, address
client.Read<bool>("100"); // Read coil
client.Read<bool>("100", 10); // Read multiple coils

client.Write<Int16>("100", 100); // Write register
client.Write<Int16>("100", 100, 110); // Write multiple registers

client.ReadString("100", 5, Encoding.ASCII); // Read string
client.ReadString("100", 5, null); // Read string in hexadecimal format
client.WriteString("500", "abcd", 10, Encoding.ASCII); // Write string; if count > 0 and insufficient, automatically pad with 0X00 at the end
```

# PLC <a id="PLC"></a>
## Common PLC Type Mapping Table <a id="PlcType"></a>
> Types marked with * are commonly used. Unless otherwise specified, all types are generally supported.

| C#</br>.Net | Siemens S7</br>SiemensS7 | Mitsubishi MC</br>MitsubishiMc | Omron Fins</br>OmronFins | Omron Cip</br>OmronCip | Inovance</br>Inovance |
| ----------- | ---------------------- | ----------------------- | ------------------------ | --------------------- | ---------------- |
| Bool        | Bool ||| BOOL ||
| Byte        | Byte ||| BYTE ||
| Float *     | Real ||| REAL ||
| Double *    | LReal ||| LREAL ||
| Int16 *     | Int ||| INT ||
| Int32 *     | DInt ||| DINT ||
| Int64 *     |||| LINT ||
| UInt16 *    | Word ||| UINT ||
| UInt32 *    | DWord ||| UDINT ||
| UInt64 *    |||| ULINT ||
| string      | String ||| STRING ||
| DateTime    | Date ||| DATE_AND_TIME ||
| TimeSpan    | Time |||||
| Char        | Char |||||

## Rockwell (AllenBradleyCipClient)
```CSharp
// Some machines can use OmronCipClient as an alternative
AllenBradleyCipClient client = new AllenBradleyCipClient("127.0.0.1");
client.Read<bool>("abc"); // Read
client.Write<bool>("abc", true); // Write
```

## Inovance (InovanceModbusTcpClient) <a id="InovanceModbusTcpClient"></a>
```CSharp
InovanceModbusTcpClient client = new InovanceModbusTcpClient("127.0.0.1");
client.Read<bool>("M1"); // Read
client.Read<Int16>("D1"); // Read
client.Write<bool>("M1", true); // Write
client.Write<Int16>("D1", 12); // Write
```

## Mitsubishi (MitsubishiMcClient) <a id="MitsubishiMcClient"></a>

Test Coverage Table

| Type | Single Read/Write | Batch Read/Write (Array) |
|--------------|------------------|-------------------------|
| bool | ✔️ | ✔️ (Loop single write, slower) |
| short | ✔️ | ✔️ |
| int32 | ✔️ | ✔️ |
| float | ✔️ | ✔️ |
| double | ✔️ | ✔️ |
| string | ✔️ | ✔️ |

> Note: Batch writing of bool arrays uses a loop of single writes, which is relatively slow.
>
> Also supports byte, sbyte, ushort, uint32, int64, uint64 types. These are less commonly used; please test them yourself.

```CSharp
MitsubishiMcClient client = new MitsubishiMcClient("127.0.0.1");
client.Read<Int16>("W0"); // Read
client.Write<Int16>("W0", 10); // Write
```

## Omron (OmronFinsClient) <a id="OmronFinsClient"></a>
```CSharp
OmronFinsClient client = new OmronFinsClient("127.0.0.1");
client.Read<Int16>("W0"); // Read
client.Write<Int16>("W0", 10); // Write
```

## Omron (OmronCipClient)
```CSharp
OmronCipClient client = new OmronCipClient("127.0.0.1");
client.Read<bool>("abc"); // Read
client.Write<bool>("abc", true); // Write
```

## Siemens (SiemensS7Client) <a id="SiemensS7Client"></a>
```CSharp
SiemensS7Client client = new SiemensS7Client("127.0.0.1");
// Read/Write support: basics (int, float...), plus extras: string, DateTime, TimeSpan, Char
client.Read<Int16>("BD100"); // Read
client.Write<Int16>("BD100", 10); // Write

// Strings
client.Read<string>("BD100"); // PLC type must be string, supports only ASCII-encoded characters like letters and numbers
client.ReadString("BD100"); // PLC type must be WString, supports UTF16-encoded characters like Chinese
```

# Robot <a id="Robot"></a>
## Epson (EpsonRobot)
```CSharp
EpsonRobot client = new EpsonRobot("127.0.0.1");
client.Client.Open();
client.Start();
client.Pause();
```

# Algorithm <a id="Algorithm"></a>

## Average Point Algorithm (AveragePoint) <a id="AveragePoint"></a>
> A simple averaging algorithm. For example, if the start is 2, the end is 8, and there are 4 points with equal spacing between them. As shown:
> 2--[4]--[6]--8
> The intermediate points are known to be 4 and 6.

```CSharp
// Output:
// 0[2, 2.5]
// 1[4, 3]
// 2[6, 3.5]
// 3[8, 4]
var aaa = AveragePoint.Start("2,2.5", " 8,4", 4);
var aaa = AveragePoint.Start(new double[] { 2, 2.5 }, new double[] { 8, 4 }, 4);

// Result: [2, 4, 6, 8]
var aaa = AveragePoint.Start(2, 8, 4);
```

## CRC <a id="CRC"></a>
```CSharp
byte[] bytes = new byte[] { 1, 2 };
// CRC Algorithms
var c1 = CRC.Crc8(bytes);
var c2 = CRC.Crc8Itu(bytes);
var c3 = CRC.Crc8Rohc(bytes);
var c4 = CRC.Crc16(bytes);
var c5 = CRC.Crc16Usb(bytes);
var c6 = CRC.Crc16Modbus(bytes);
var c7 = CRC.Crc32(bytes);
var c8 = CRC.Crc32Q(bytes);
var c9 = CRC.Crc32Sata(bytes);
// CRC Verification
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

## Stable Marriage Matching Algorithm (GaleShapleyAlgorithm) <a id="GaleShapleyAlgorithm"></a>
> Suppose the preference order is as follows (higher preference first):
> M0❤W1,W0   M1❤W0,W1   M2❤W0,W1,W2,W3,W4   M3❤W3   M4❤W3

```CSharp
// 1. All participants
var ms = new string[] { "M0", "M1", "M2", "M3", "M4", };
var ws = new string[] { "W0", "W1", "W2", "W3", "W4", };
// 2. Convert to items
var msi = ms.Select(o => new GaleShapleyItem<string>(o)).ToList();
var wsi = ws.Select(o => new GaleShapleyItem<string>(o)).ToList();
// 3. Set preference lists. Suppose the preferences are as follows (from highest to lowest preference):
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
    // M0❤W1   M1❤W0   M2❤W0   M3❤W3   M4❤W3
    Console.Write($"{item.Item}❤{(item.Match?.Item) ?? "null"}   ");
}
```

# Device <a id="Device"></a>

For devices requiring a persistent connection, you must call `dev1.Client.Open();`. To enable auto-open, set `dev1.Client.ConnectionMode = ConnectionMode.AutoOpen;`. The following examples will not repeat client-related descriptions or settings unless they are critical or different.

## Cosmo Airtight Tester (CosmoAirtight) <a id="CosmoAirtight"></a>
```CSharp
CosmoAirtight dev1 = new CosmoAirtight("COM1"); // Cosmo
```

## Fct
```CSharp
MengXunFct dev1 = new MengXunFct("127.0.0.1"); // MengXun Electronics
```

## Laser Marking (Mark) <a id="Mark"></a>
```CSharp
DaZhuMark dev1 = new DaZhuMark("127.0.0.1"); // Han's
HuaPuMark dev2 = new HuaPuMark("127.0.0.1"); // HuaPu
```

## RFID (Rfid) <a id="Rfid"></a>
```CSharp
BeiJiaFuRfid rfid1 = new BeiJiaFuRfid("127.0.0.1"); // Pepperl+Fuchs
DongJiRfid rfid2 = new DongJiRfid("127.0.0.1"); // DongJi
TaiHeSenRfid rfid3 = new TaiHeSenRfid("127.0.0.1"); // TaiHeSen
WanQuanRfid rfid4 = new WanQuanRfid("127.0.0.1"); // WanQuan

// WanQuan Usage
rfid4.ReadString(RfidAddress.GetRfidAddressStr(RfidArea.ISO15693, null, 1), 2, EncodingEnum.ASCII.GetEncoding());
rfid4.WriteString(RfidAddress.GetRfidAddressStr(RfidArea.ISO15693, null, 1), "A001", 2, EncodingEnum.ASCII.GetEncoding());
```

## Barcode Scanner (Scanner) <a id="Scanner"></a>
```CSharp
HoneywellScanner dev1 = new HoneywellScanner("127.0.0.1"); // Honeywell
MindeoScanner dev1 = new MindeoScanner("127.0.0.1"); // Mindeo
```

## Screw Driver (Screw) <a id="Screw"></a>
```CSharp
MiLeScrew dev1 = new MiLeScrew("127.0.0.1"); // MiLe
```

## Temperature Control (TemperatureControl) <a id="TemperatureControl"></a>
```CSharp
// Quick Temperature Controller (Not Recommended)
```

## Welding Machine (Weld) <a id="Weld"></a>
```CSharp
KuaiKeWeld dev1 = new KuaiKeWeld("COM1");
```

# Extensions <a id="Ext"></a>
## 1. How to Use Custom Protocols
```CSharp
// XXX Protocol Implementation
public class XXX
{
    public ClientBase Client { get; private set; } // Communication channel

    public XXX(ClientBase client)
    {
        Client = client;
        // Client.ReceiveMode = ReceiveMode.ParseTime();
        Client.Encoding = Encoding.ASCII;
        Client.ConnectionMode = ConnectionMode.AutoOpen;
    }

    // Default using TcpClient
    public XXX(string ip, int port = 1500) : this(new TcpClient(ip, port)) { }
    // Default using SerialPortClient
    // public XXX(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, Handshake handshake = Handshake.None) : this(new SerialPortClient(portName, baudRate, parity, dataBits, stopBits, handshake)) { }

    // Example: sends "info1\r\n" and waits for a string response
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

// Usage
var client = new XXX("127.0.0.1");
client.Client.Open();
var info = client.ReadXXX();
```