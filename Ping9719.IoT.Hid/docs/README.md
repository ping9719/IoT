
# Ping9719.IoT.Hid    
对IoT进行的扩充，支持在windows、安卓、苹果的手机、平板、电脑上进行USB和蓝牙发送和接收数据，使PLC和设备通信可使用USB或蓝牙   

# 语言选择：
[简体中文](README.md) || [English](README_en-US.md) 

## UsbHidClient
获取报告信息 `UsbHidClient.GetReportDescriptor(UsbHidClient.GetNames[0])`    
报告介绍：
> 1.报告类型：Input, Output, Feature   
> 2.报告ID：一般在帧头，默认值为 `0x00`。可以使用“消息处理器”来处理。    
> 3.报告长度：要求的固定长度，不足一般末尾添加`0x00`补齐（低速默认8，全速64，高速1024）。可以使用“消息处理器”来处理。    

`UsbHidClient : ClientBase`
```CSharp
var names = UsbHidClient.GetNames;
var client = new UsbHidClient(names[0]);

//使用消息处理器来处理报告
{
    //加入报告ID（实际需要看文档）
    client.SendDataProcessors.Add(new StartAddValueDataProcessor(0));
    //加入报告长度补齐（实际需要看文档）
    client.SendDataProcessors.Add(new PadRightDataProcessor(64));
    //清除报告ID（实际需要看文档）
    client.ReceivedDataProcessors.Add(new StartClearValueDataProcessor(0));
    //清除报告长度补齐（实际需要看文档）
    client.ReceivedDataProcessors.Add(new TrimEndDataProcessor(0));
}
```

## BleClient
`BleClient : ClientBase`
```CSharp
var names = BleClient.GetNames;
var client = new BleClient(names[0]);
```