# Ping9719.IoT.Hid
An extension for IoT that supports sending and receiving data via USB and Bluetooth on Windows, Android, iOS devices, tablets, and computers, enabling PLC and device communication through USB or Bluetooth

# Language Selection:
[简体中文](README.md) || [English](README_en-US.md) 

## UsbHidClient
> It has been observed that USB header and trailer frames have fixed `0x00` values, which can be handled using a "IDataProcessor"

`UsbHidClient : ClientBase`
```CSharp
var names = UsbHidClient.GetNames();
var client = new UsbHidClient(names[0]);
```

## BleClient
`BleClient : ClientBase`
```CSharp
var names = BleClient.GetNames();
var client = new BleClient(names[0]);
```