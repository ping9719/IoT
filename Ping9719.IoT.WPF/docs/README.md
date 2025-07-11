# Ping9719.IoT.WPF
支持在windows平台上快速的调试IoT中的协议和设备。

# 语言选择：
[简体中文](README.md) </br>
[English](README_en-US.md) </br>

### 列子:[ensample code:]
```CSharp
//名称空间
xmlns:piIoT="https://github.com/ping9719/IoT"
//假如是Rfid，则为：RfidView
<piIoT:RfidView DeviceData="{Binding Dev1}" Area="ISO15693" IsReadPara="True" Encoding="ASCII" ReadCount="2" WriteVal="A001"/>
```
`RfidView`
![](img/RfidView.png)