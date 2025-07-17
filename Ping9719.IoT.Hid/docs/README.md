# Ping9719.IoT.Hid
对IoT进行的扩充，支持在windows、安卓、苹果的手机、平板、电脑上进行USB和蓝牙发送和接收数据，使PLC和设备通信可使用USB或蓝牙

# 语言选择：
[简体中文](README.md) || [English](README_en-US.md) 

## UsbHidClient
> 发现UBS的头帧和尾帧有固定`0x00`，需要内部处理，暂未处理，可手动处理。    

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