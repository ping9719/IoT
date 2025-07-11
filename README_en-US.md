# Ping9719.IoT

### Industrial Internet Communication Library Protocol Implementation
Includes mainstream PLC, ModBus, CIP, MC, FINS... and other common protocols. Data can be easily exchanged through different channels (TCP, UDP, MQTT, USB, Bluetooth...).

### Language Options:
[简体中文](README.md) </br>
[English](README_en-US.md) </br>

### Source Code:
[Github (Primary Repository)](https://github.com/ping9719/IoT)  
[Gitee (Backup Repository)](https://gitee.com/ping9719/IoT)   

### Table of Contents:
| Directory  | Framework                 | Documentation                                 | Version History                            | Dependencies          | NuGet Package<br>(Released when stable) | Description |
|------------|---------------------------|-----------------------------------------------|--------------------------------------------|-----------------------|-----------------------------------------|-------------|
| IoT        | net45;<br>netstandard2.0  | [Docs](Ping9719.IoT/docs/README.md)           |[Docs](Ping9719.IoT/docs/VERSION.md)        | System.IO.Ports       | Ping9719.IoT                            | Cross-platform library. Includes communication (TCP, UDP, USB...), protocols (ModBus, MC, FINS...), algorithms (CRC, LRC...), and devices (RFID, scanners...) |
| Hid        | net45;<br>netstandard2.0  | [Docs](Ping9719.IoT.Hid/docs/README.md)       |[Docs](Ping9719.IoT.Hid/docs/VERSION.md)    | IoT;<br>HidSharp     | Ping9719.IoT.Hid                        | Cross-platform channel library. Extends IoT to support USB/Bluetooth data transmission on Windows, Android, iOS devices |
| WPF        | net45;<br>net8.0-windows | [Docs](Ping9719.IoT.WPF/docs/README.md)       |[Docs](Ping9719.IoT.WPF/docs/VERSION.md)    | IoT;                 | Ping9719.IoT.WPF                        | Windows-only UI library for rapid debugging of IoT protocols/devices |
| Avalonia   | net8.0;<br>netstandard2.0| [Docs](Ping9719.IoT.Avalonia/docs/README.md)  |[Docs](Ping9719.IoT.Avalonia/docs/VERSION.md)| IoT;<br>Avalonia     | Ping9719.IoT.Avalonia                   | Cross-platform UI library for debugging protocols/devices on Windows, Android, iOS devices |

---

# Must-Read: Preface & Highlights
1. **Unified Device Interface "IIoT"**  
Generic read/write operations for common devices:  
```csharp
client.Read<bool>("abc");          // Read single value
client.Read<bool>("abc", 5);       // Read 5 values
client.Write<bool>("abc", true);   // Write value
client.Write<int>("abc", 10, 20, 30);              // Write multiple values
client.Write<int>("abc", new int[] { 10, 20, 30 }); // Write array
```