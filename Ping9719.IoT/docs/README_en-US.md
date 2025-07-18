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
