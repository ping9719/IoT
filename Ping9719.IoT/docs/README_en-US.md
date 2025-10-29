# Language Selection:  
[Simplified Chinese](README.md) || [English](README_en-US.md)

# Table of Contents
- [Communication](#Communication)
    - [ClientBase](#ClientBase)
        - [1. ConnectionMode](#ConnectionMode)
        - [2. ReceiveMode](#ReceiveMode)
        - [3. IDataProcessor](#IDataProcessor)
        - [4. Heartbeat](#Heartbeat)
    - [TcpClient](#TcpClient)
    - [TcpServer](#TcpServer)
    - [SerialPortClient (Serial Port)](#SerialPortClient)
    - [UsbHidClient (USB)](#UsbHidClient)
    - [BleClient (Bluetooth)](#BleClient)
    - UdpClient (In Progress)
    - UdpServer (Planned)
    - HttpServer (Planned)
    - MqttClient (Planned)
    - MqttServer (Planned)
- [Modbus](#Modbus)
    - ModbusRtuClient
    - ModbusTcpClient
    - ModbusAsciiClient
- [PLC](#PLC)
    - [Type Mapping Table](#PlcType)
    - Rockwell (AllenBradleyCipClient) (Pending Testing)
    - [Inovance (InovanceModbusTcpClient)](#InovanceModbusTcpClient)
    - [Mitsubishi (MitsubishiMcClient)](#MitsubishiMcClient)
    - [Omron (OmronFinsClient, OmronCipClient)](#OmronFinsClient)
    - [Siemens (SiemensS7Client)](#SiemensS7Client)
- [Robot](#Robot)
    - Epson (EpsonRobot) (Pending Testing)
- [Algorithm](#Algorithm)
    - [AveragePoint](#AveragePoint)
    - [CRC](#CRC)
    - [LRC](#LRC)
    - Fourier Transform (Planned)
    - [GaleShapleyAlgorithm](#GaleShapleyAlgorithm)
    - PID (Planned)
    - RSA (Planned)
- [Device & Instrument](#Device)
    - [Airtight Testing](#Airtight)
        - Cosmo Airtight Tester (CosmoAirtight)
    - Fct
        - MengXun Electronics (MengXunFct)
    - [Laser Marking](#Mark)
        - Han's Laser Marking (DaZhuMark)
        - HuaPu Laser Marking (HuaPuMark)
    - [RFID](#Rfid)
        - Pepperl+Fuchs RFID (BeiJiaFuRfid)
        - TaiHeSen RFID (TaiHeSenRfid)
        - WanQuan RFID (WanQuanRfid)
    - [Barcode Scanner](#Scanner)
        - Honeywell Scanner (HoneywellScanner)
        - Mindeo Scanner (MindeoScanner)
    - [Screw Driver](#Screw)
        - Quick Screw Driver (KuaiKeDeskScrew, KuaiKeScrew, KuaiKeTcpScrew) (Not Recommended)
        - MiLe Screw Driver (MiLeScrew)
    - [Temperature Control](#TemperatureControl)
        - Quick Temperature Controller (KuaiKeTemperatureControl) (Not Recommended)
    - [Welding Machine](#Weld)
        - Quick Welder (KuaiKeWeld)
- [FAQ](#Issue)
    - 1. How to implement a custom protocol?

# Communication <a id="Communication"></a>
Interact using specified communication methods.

## ClientBase <a id="ClientBase"></a>
Both `TcpClient` and `SerialPortClient` inherit from `ClientBase` and share the same usage pattern.

### 1. Connection Mode <a id="ConnectionMode"></a>

Three connection modes are supported:

> 1. **Manual** (General-purpose): You must explicitly open and close the connection. This offers maximum flexibility.  
> 2. **AutoOpen** (Suitable for short-lived connections): If `Open()` hasn’t been called, each send/receive operation automatically opens and closes the connection. Ideal for short connections. For temporary long-lived connections, you can call `Open()` manually and later `Close()`.  
> 3. **AutoReconnection** (Suitable for persistent connections): After calling `Open()`, the client automatically attempts to reconnect if disconnected. Ideal for long-lived connections. Calling `Close()` disables reconnection.

Auto-reconnection rules:

> 1. After disconnection, the first reconnection attempt waits 1 second.  
> 2. If unsuccessful, the wait time increases by 1 second per attempt, up to the maximum reconnection time (`MaxReconnectionTime`).  
> 3. Reconnection continues until successful or until the user manually calls `Close()`.

Sample code:
```csharp
var client1 = new TcpClient("127.0.0.1", 8080);
client1.ConnectionMode = ConnectionMode.Manual;        // Manual (default)
client1.ConnectionMode = ConnectionMode.AutoOpen;      // Auto-open
client1.ConnectionMode = ConnectionMode.AutoReconnection; // Auto-reconnect
client1.MaxReconnectionTime = 10; // Max reconnection wait time in seconds (default: 10s)
```

### 2. Receive Mode <a id="ReceiveMode"></a>

Data can be received in two ways:
1. Via the `Received` event.
2. Via the `Receive()` or `SendReceive()` methods.

**Note**: Method-based reception takes precedence over event-based reception. If data is consumed by a method, the `Received` event will not fire for that data.

```csharp
// Default for method-based reception
client.ReceiveMode = ReceiveMode.ParseByteAll();
// Default for event-based reception
client.ReceiveModeReceived = ReceiveMode.ParseByteAll();
```

Incoming data is first split into frames according to the **Receive Mode**, then processed by the **Data Processor**.

> Example: If the remote side sends `"ab\r\n"` and `"cd\r\n"` with a 100ms interval between them,  
> `"ab\r\n"` is considered one frame, and `"a"` is one character.  
> Assume 100ms between frames and 1ms between characters within a frame.

| Code                                 | Result     | Description | Recommended Scenario |
| ------------------------------------ | ---------- | ----------- | -------------------- |
| `ReceiveMode.ParseByte(2)`           | `ab`       | Read a fixed number of bytes | When frame length is known or remaining length is known |
| `ReceiveMode.ParseByteAll()`         | `a` or `ab\r\n` | Read all immediately available bytes | High-performance scenarios with unknown framing; TCP usually receives full frames, serial usually receives one char at a time |
| `ReceiveMode.ParseChar(1)`           | `a`        | Read a fixed number of characters | Same as `ParseByte()` |
| `ReceiveMode.ParseTime(10)`          | `ab\r\n`   | Wait specified time (ms) after last byte before ending frame | When nothing is known but full frames are desired (sacrifices latency); default for serial |
| `ReceiveMode.ParseToEnd("\r\n")`     | `ab\r\n`   | Read until specified terminator | When frame terminator is known |

### 3. Data Processor (IDataProcessor) <a id="IDataProcessor"></a>

Features:
> 1. Process data uniformly before sending.  
> 2. Process received data before forwarding.  
> 3. Multiple processors can be chained; earlier-added processors run first (note: for symmetric send/receive processing, the order may need to be reversed).

Built-in Data Processors <a id="IDataProcessorIn"></a>

| Name | Description |
| ---- | ----------- |
| EndAddValueDataProcessor | Append fixed value (e.g., `\r\n`) to end |
| EndClearValueDataProcessor | Remove fixed value (e.g., `\r\n`) from end |
| PadLeftDataProcessor | Pad left (head) to reach specified length |
| PadRightDataProcessor | Pad right (tail) to reach specified length |
| StartAddValueDataProcessor | Prepend fixed value |
| StartClearValueDataProcessor | Remove fixed value from start |
| TrimDataProcessor | Trim specified values from both ends |
| TrimEndDataProcessor | Trim specified values from end |
| TrimStartDataProcessor | Trim specified values from start |

Custom Data Processor:

1. Implement `IDataProcessor`, e.g., `public class MyClass : IDataProcessor`.
2. Usage:
```csharp
client1.SendDataProcessors.Add(new MyClass());
client1.ReceivedDataProcessors.Add(new MyClass());
```

### 4. Heartbeat <a id="Heartbeat"></a>

Execute a custom method at regular intervals.  
**Note**: Heartbeat is **disabled** in `ConnectionMode.AutoOpen`.

```csharp
client1.HeartbeatTime = 5000; // Interval in ms (default: 5000ms). Set to 0 to disable.
client1.Heartbeat = (a) =>
{
    var result = a.Send("1");
    return result.IsSucceed;
};

client1.Open(); // Open after configuring properties and events
```

## TcpClient <a id="TcpClient"></a>
`TcpClient : ClientBase`

```csharp
ClientBase client1 = new TcpClient("127.0.0.1", 502);
client1.Encoding = Encoding.UTF8;

// 1. Connection Mode (AutoReconnection is commonly used)
client1.ConnectionMode = ConnectionMode.Manual;        // Manual (default)
client1.ConnectionMode = ConnectionMode.AutoOpen;      // Auto-open
client1.ConnectionMode = ConnectionMode.AutoReconnection; // Auto-reconnect

// 2. Receive Mode (handle packet sticking as needed)
client1.ReceiveMode = ReceiveMode.ParseByteAll();
client1.ReceiveModeReceived = ReceiveMode.ParseByteAll();

// 3. Data Processors (e.g., add/remove line breaks)
client1.SendDataProcessors.Add(new EndAddValueDataProcessor("\r\n", client1.Encoding));
client1.ReceivedDataProcessors.Add(new EndClearValueDataProcessor("\r\n", client1.Encoding));

// 4. Event handlers
client1.Opened = (a) => { Console.WriteLine("Connected."); };
client1.Closed = (a, b) => { Console.WriteLine($"Closed. {(b ? "Manually" : "Automatically")}"); };
client1.Received = (a, b) => { Console.WriteLine($"Received: {a.Encoding.GetString(b)}"); };

client1.Open(); // Open after configuration

// 5. Send/Receive operations
client1.Send("abc");
client1.Receive();
client1.Receive(3000); // 3s timeout
client1.Receive(ReceiveMode.ParseToEnd("\n", 3000)); // Read until \n, 3s timeout
client1.SendReceive("abc", 3000); // Send and wait for reply (3s timeout)
client1.SendReceive("abc", ReceiveMode.ParseToEnd("\n", 3000)); // Send and receive until \n
```

## TcpServer <a id="TcpServer"></a>
`TcpServer : ServiceBase`

```csharp
var service = new TcpService("127.0.0.1", 8005);
service.Encoding = Encoding.UTF8;
service.ReceiveMode = ReceiveMode.ParseByteAll();        // For Receive()
service.ReceiveModeReceived = ReceiveMode.ParseByteAll(); // For Received event

service.Opened = (a) =>
{
    Console.WriteLine($"Client [{(a as INetwork)?.Socket?.RemoteEndPoint}] connected");
};
service.Closed = (a) =>
{
    Console.WriteLine("Client disconnected");
};
service.Received = (a, b) =>
{
    Console.WriteLine($"Client [{(a as INetwork)?.Socket?.RemoteEndPoint}] received: {a.Encoding.GetString(b)}");
};

service.Open(); // Configure before opening

if (service.Clients.Any())
{
    // Send to first client (same API as TcpClient)
    service.Clients[0].Send("123");
}
```

## SerialPortClient <a id="SerialPortClient"></a>
`SerialPortClient : ClientBase`

> Serial communication is point-to-point, so only `SerialPortClient` exists (no `SerialPortService`). Use two `SerialPortClient` instances for bidirectional communication.

```csharp
var client1 = new SerialPortClient("COM1", 9600);

// Default properties (optional to set)
client1.ConnectionMode = ConnectionMode.Manual; // Auto-reconnect is rarely useful for serial
client1.Encoding = Encoding.ASCII;
client1.TimeOut = 3000;
client1.ReceiveMode = ReceiveMode.ParseTime();        // Default for Receive()
client1.ReceiveModeReceived = ReceiveMode.ParseTime(); // Default for Received event

// Events and I/O same as TcpClient (omitted for brevity)

client1.Open(); // Configure before opening
```

## UsbHidClient (USB) <a id="UsbHidClient"></a>
`UsbHidClient : ClientBase`

1. Requires NuGet package `Ping9719.IoT.Hid`.
2. Get report descriptor: `UsbHidClient.GetReportDescriptor(UsbHidClient.GetNames[0])`
> - Report Types: Input, Output, Feature  
> - Report ID: Usually in frame header (default `0x00`). Handle via data processors.  
> - Report Length: Fixed length (pad with `0x00` if shorter; defaults: Low-speed=8, Full-speed=64, High-speed=1024). Handle via data processors.

```csharp
var names = UsbHidClient.GetNames; // Enumerate USB devices
var client = new UsbHidClient(names[0]); // Use first device

// Use data processors to handle report formatting
{
    client.SendDataProcessors.Add(new StartAddValueDataProcessor(0));      // Add report ID
    client.SendDataProcessors.Add(new PadRightDataProcessor(64));         // Pad to report length
    client.ReceivedDataProcessors.Add(new StartClearValueDataProcessor(0)); // Remove report ID
    client.ReceivedDataProcessors.Add(new TrimEndDataProcessor(0));       // Trim padding
}
```

## BleClient (Bluetooth) <a id="BleClient"></a>
`BleClient : ClientBase`

1. Requires NuGet package `Ping9719.IoT.Hid`.

```csharp
var names = BleClient.GetNames; // Enumerate Bluetooth devices
var client = new BleClient(names[0]); // Use first device
```

# Modbus <a id="Modbus"></a>
`ModbusRtuClient : IClientData`  
`ModbusTcpClient : IClientData`  
`ModbusAsciiClient : IClientData`

Modbus RTU frame: `Station ID` + `Function Code` + `Address` + `Length` + `Checksum`  
Modbus TCP frame: `Transaction ID` + `0x0000` + `Length` + `Station ID` + `Function Code` + `Address` + `Length`

```csharp
var client = new ModbusRtuClient("COM1", 9600, format: EndianFormat.ABCD);
var client = new ModbusRtuClient(new TcpClient("127.0.0.1", 502), format: EndianFormat.ABCD); // RTU over TCP
var client = new ModbusTcpClient("127.0.0.1", 502, format: EndianFormat.ABCD);
var client = new ModbusTcpClient(new SerialPortClient("COM1", 9600), format: EndianFormat.ABCD); // TCP over serial

client.Client.ConnectionMode = ConnectionMode.AutoReconnection; // Recommended for TCP; less so for serial
client.Client.Open();

client.Read<Int16>("100");              // Read register
client.Read<Int16>("100.1");            // Read bit within register (single-bit only; use uint16/int16)
client.Read<Int16>("s=2;x=3;100");      // Read with custom station/function/address
client.Read<bool>("100");               // Read coil
client.Read<bool>("100", 10);           // Read multiple coils

client.Write<Int16>("100", 100);        // Write single register
client.Write<Int16>("100", 100, 110);   // Write multiple registers

client.ReadString("100", 5, Encoding.ASCII); // Read string
client.ReadString("100", 5, null);           // Read as hex string
client.WriteString("500", "abcd", 10, Encoding.ASCII); // Write string (padded with 0x00 if needed)
```

# PLC <a id="PLC"></a>

## Common PLC Type Mapping <a id="PlcType"></a>
> Types marked with * are commonly used. Unless specified otherwise, all types are supported.

| C#/.NET | Siemens S7 | Mitsubishi MC | Omron FINS | Omron CIP | Inovance |
| ------- | ---------- | ------------- | ---------- | --------- | -------- |
| Bool    | Bool       |               |            | BOOL      |          |
| Byte    | Byte       |               |            | BYTE      |          |
| Float * | Real       |               |            | REAL      |          |
| Double *| LReal      |               |            | LREAL     |          |
| Int16 * | Int        |               |            | INT       |          |
| Int32 * | DInt       |               |            | DINT      |          |
| Int64 * |            |               |            | LINT      |          |
| UInt16 *| Word       |               |            | UINT      |          |
| UInt32 *| DWord      |               |            | UDINT     |          |
| UInt64 *|            |               |            | ULINT     |          |
| string  | String     |               |            | STRING    |          |
| DateTime| Date       |               |            | DATE_AND_TIME |      |
| TimeSpan| Time       |               |            |           |          |
| Char    | Char       |               |            |           |          |

## Rockwell (AllenBradleyCipClient)
`AllenBradleyCipClient : IClientData`

```csharp
// Some devices may work with OmronCipClient as alternative
var client = new AllenBradleyCipClient("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;
client.Client.Open();

client.Read<bool>("abc");
client.Write<bool>("abc", true);
```

## Inovance (InovanceModbusTcpClient) <a id="InovanceModbusTcpClient"></a>
`InovanceModbusTcpClient : IClientData`

```csharp
var client = new InovanceModbusTcpClient("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;
client.Client.Open();

client.Read<bool>("M1");
client.Read<Int16>("D1");
client.Write<bool>("M1", true);
client.Write<Int16>("D1", 12);
```

## Mitsubishi (MitsubishiMcClient) <a id="MitsubishiMcClient"></a>
`MitsubishiMcClient : IClientData`

Test Coverage:

| Type   | Single Read/Write | Batch Read/Write |
| ------ | ----------------- | ---------------- |
| bool   | ✔️                | ✔️ (via loop)    |
| short  | ✔️                | ✔️               |
| int32  | ✔️                | ✔️               |
| float  | ✔️                | ✔️               |
| double | ✔️                | ✔️               |
| string | ✔️                | ✔️               |

> Note: Batch writing of bool arrays uses single-point writes (slower).  
> Also supports byte, sbyte, ushort, uint32, int64, uint64 (less common; test as needed).

```csharp
var client = new MitsubishiMcClient(MitsubishiVersion.Qna_3E, "127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;
client.Client.Open();

client.Read<Int16>("W0");
client.Write<Int16>("W0", 10);
```

## Omron (OmronFinsClient) <a id="OmronFinsClient"></a>
`OmronFinsClient : IClientData`

```csharp
var client = new OmronFinsClient("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;
client.Client.Open();

client.Read<Int16>("W0");
client.Write<Int16>("W0", 10);
```

## Omron (OmronCipClient)
`OmronCipClient : IClientData`

```csharp
var client = new OmronCipClient("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;
client.Client.Open();

client.Read<bool>("abc");
client.Write<bool>("abc", true);
```

## Siemens (SiemensS7Client) <a id="SiemensS7Client"></a>
`SiemensS7Client : IClientData`

```csharp
var client = new SiemensS7Client(SiemensVersion.S7_1200, "127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;
client.Client.Open();

// Supports basic types (int, float...) plus: string, DateTime, TimeSpan, Char
client.Read<Int16>("BD100");
client.Write<Int16>("BD100", 10);

// Strings
client.Read<string>("BD100"); // PLC type must be String (ASCII only)
client.ReadString("BD100");   // PLC type must be WString (supports UTF-16, e.g., Chinese)
```

# Robot <a id="Robot"></a>

## Epson (EpsonRobot)
`EpsonRobot : IClient`

```csharp
var client = new EpsonRobot("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;
client.Client.Open();

client.Start();
client.Pause();
```

# Algorithm <a id="Algorithm"></a>

## AveragePoint <a id="AveragePoint"></a>

> Simple linear interpolation. Given start=2, end=8, and 4 points with equal spacing:  
> 2--[4]--[6]--8 → intermediate points are 4 and 6.

```csharp
// Output:
// 0[2, 2.5]
// 1[4, 3]
// 2[6, 3.5]
// 3[8, 4]
var result1 = AveragePoint.Start("2,2.5", "8,4", 4);
var result2 = AveragePoint.Start(new double[] { 2, 2.5 }, new double[] { 8, 4 }, 4);

// Result: [2, 4, 6, 8]
var result3 = AveragePoint.Start(2, 8, 4);
```

## CRC <a id="CRC"></a>

```csharp
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

// CRC Validation
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

```csharp
LRC.GetLRC(bytes);
LRC.CheckLRC(bytes);
```

## GaleShapleyAlgorithm <a id="GaleShapleyAlgorithm"></a>

> Preference example (higher preference first):  
> M0 ❤ W1, W0  
> M1 ❤ W0, W1  
> M2 ❤ W0, W1, W2, W3, W4  
> M3 ❤ W3  
> M4 ❤ W3  

```csharp
// 1. Participants
var men = new string[] { "M0", "M1", "M2", "M3", "M4" };
var women = new string[] { "W0", "W1", "W2", "W3", "W4" };

// 2. Convert to items
var mItems = men.Select(m => new GaleShapleyItem<string>(m)).ToList();
var wItems = women.Select(w => new GaleShapleyItem<string>(w)).ToList();

// 3. Set preferences
mItems[0].Preferences = new List<GaleShapleyItem<string>> { wItems[1], wItems[0] };
mItems[1].Preferences = new List<GaleShapleyItem<string>> { wItems[0], wItems[1] };
mItems[2].Preferences = new List<GaleShapleyItem<string>> { wItems[0], wItems[1], wItems[2], wItems[3], wItems[4] };
mItems[3].Preferences = new List<GaleShapleyItem<string>> { wItems[3] };
mItems[4].Preferences = new List<GaleShapleyItem<string>> { wItems[3] };

// 4. Run algorithm
GaleShapleyAlgorithm.Run(mItems);

// 5. Output results
foreach (var man in mItems)
{
    Console.Write($"{man.Item} ❤ {(man.Match?.Item ?? "null")}   ");
}
// Expected: M0❤W1   M1❤W0   M2❤W2   M3❤W3   M4❤null
```

# Device & Instrument <a id="Device"></a>

**Important**: For devices requiring persistent connections, call `dev1.Client.Open()`. For auto-open, set `dev1.Client.ConnectionMode = ConnectionMode.AutoOpen;`. Client configuration is omitted in examples below unless critical or non-standard.

## Cosmo Airtight Tester (CosmoAirtight) <a id="CosmoAirtight"></a>

```csharp
var dev1 = new CosmoAirtight("COM1");
```

## Fct

```csharp
var dev1 = new MengXunFct("127.0.0.1"); // MengXun Electronics
```

## Laser Marking (Mark) <a id="Mark"></a>

```csharp
var dev1 = new DaZhuMark("127.0.0.1"); // Han's Laser
var dev2 = new HuaPuMark("127.0.0.1"); // HuaPu Laser
```

## RFID <a id="Rfid"></a>

```csharp
var rfid1 = new BeiJiaFuRfid("127.0.0.1"); // Pepperl+Fuchs
var rfid2 = new DongJiRfid("127.0.0.1");   // DongJi
var rfid3 = new TaiHeSenRfid("127.0.0.1"); // TaiHeSen
var rfid4 = new WanQuanRfid("127.0.0.1");  // WanQuan

// WanQuan usage example
rfid4.ReadString(RfidAddress.GetRfidAddressStr(RfidArea.ISO15693, null, 1), 2, EncodingEnum.ASCII.GetEncoding());
rfid4.WriteString(RfidAddress.GetRfidAddressStr(RfidArea.ISO15693, null, 1), "A001", 2, EncodingEnum.ASCII.GetEncoding());
```

## Barcode Scanner (Scanner) <a id="Scanner"></a>

```csharp
var dev1 = new HoneywellScanner("127.0.0.1"); // Honeywell
var dev2 = new MindeoScanner("127.0.0.1");    // Mindeo
```

## Screw Driver (Screw) <a id="Screw"></a>

```csharp
var dev1 = new MiLeScrew("127.0.0.1"); // MiLe
```

## Temperature Control (TemperatureControl) <a id="TemperatureControl"></a>

```csharp
// KuaiKe temperature controller (not recommended)
```

## Welding Machine (Weld) <a id="Weld"></a>

```csharp
var dev1 = new KuaiKeWeld("COM1");
```

# FAQ <a id="Issue"></a>

## 1. How to implement a custom protocol?

```csharp
// XXX Protocol Implementation
public class XXX
{
    public ClientBase Client { get; private set; } // Communication channel

    public XXX(ClientBase client)
    {
        Client = client;
        // Client.ReceiveMode = ReceiveMode.ParseTime();
        Client.Encoding = Encoding.ASCII;
        // Client.ConnectionMode = ConnectionMode.AutoOpen;
    }

    // Default: TcpClient
    public XXX(string ip, int port = 1500) : this(new TcpClient(ip, port)) { }
    // Default: SerialPortClient
    // public XXX(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, Handshake handshake = Handshake.None) : this(new SerialPortClient(portName, baudRate, parity, dataBits, stopBits, handshake)) { }

    // Example: Send "info1\r\n" and wait for response
    public IoTResult ReadXXX()
    {
        string command = $"info1\r\n";
        try
        {
            return Client.SendReceive(command);
        }
        catch (Exception ex)
        {
            return IoTResult.Create().AddError(ex);
        }
    }
}

// Usage
var client = new XXX("127.0.0.1");
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;
client.Client.Open();

var info = client.ReadXXX();
```