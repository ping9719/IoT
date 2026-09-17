## 

## Ping9719.IoT   
跨平台工业通讯库。包含主流的通讯方式（TCP、UDP、MQTT、USB、蓝牙...）和主流的通讯协议（ModBus、S7、CIP、MC、FINS...）开箱即可简单、方便的使用。

## 语言选择
[简体中文](README.md) || [English](README_en-US.md)    

## 资料链接  <a id="DocMain"></a>
[进入文档](Ping9719.IoT/docs/README.md)    
[版本记录](Ping9719.IoT/docs/VERSION.md)    

[源代码（主库 Github）](https://github.com/ping9719/IoT)   
[源代码（备库 Gitee）](https://gitee.com/ping9719/IoT)    
[去提交问题、提交BUG](https://github.com/ping9719/IoT/issues)    

## 项目框架图
![](img/frame.png)

## 如何安装？
![](img/bao.png)

## 包名&介绍

| 包名（NuGet）         |  环境                            		|  介绍                      | 
|-----------------------|---------------------------------------|----------------------------|
| Ping9719.IoT          | net45 ; netstandard2.0 ; net8.0     	|跨平台工业通讯库。包含基础的通信方式、通信协议、常用算法、常用设备协议|
| Ping9719.IoT.Hid      | net45 ; netstandard2.0 ; net8.0       |跨平台工业通讯库Hid扩展库。包含不常用的通信方式（USB，蓝牙，串口） |
| Ping9719.IoT.WPF</br>(暂未发布)      | net45 ; net8.0-windows |控件库。对通信方式、通信协议、常用算法实现的控件|
| Ping9719.IoT.Avalonia</br>(暂未发布) | net8.0 ; netstandard2.0|控件库。对通信方式、通信协议、常用算法实现的控件| 

## 四大亮点

### 一 <b>常用协议</b>实现 `IClientData`或`IReadWrite`，可通过泛型方式进行读或写。  
```CSharp
client.Read<bool>("abc");//读1个
client.Read<bool>("abc", 5);//读5个
client.Write<bool>("abc", true);//写1个
client.Write<int>("abc", new int[] { 10, 20, 30 });//写多个
```

### 二 <b>所有客户端协议</b>可快速的切换为不同的方式，比如从`TCP`切换为`USB` 
> 这里以`ModbusRtu`举列，默认只支持串口。但是如果你想实现`ModbusRtuOverTcpClient`（使用TCP的方式走`ModbusRtu`协议）其他的都是同理。 

```CSharp
var client = new ModbusRtuClient(new SerialPortClient("COM1", 9600));//使用串口方式，默认 
client = new ModbusRtuClient(new TcpClient("127.0.0.1", 502));//也可直接使用Tcp方式，ModbusRtuOverTcpClient
client.Client.ConnectionMode = ConnectionMode.AutoReconnection;
client.Client.Open();//打开

//切换为USB方式。会采用旧的属性，如果旧的打开了会自动关闭旧的，打开新的
client.SetClient(new UsbHidClient(UsbHidClient.GetNames[0]));
```

### 三 客户端`ClientBase`包含丰富的功能，且代码一致性高。   
>以下代码所有通用，包含 `TcpClient`、`SerialPortClient`、`UsbHidClient` 等...
```CSharp
ClientBase client = new TcpClient("127.0.0.1", 502);//Tcp方式
client.Encoding = Encoding.UTF8;

//1：连接模式。断线重连使用得比较多
client.ConnectionMode = ConnectionMode.Manual;//手动。需要自己去打开和关闭，此方式比较灵活。
client.ConnectionMode = ConnectionMode.AutoOpen;//自动打开。没有执行Open()时每次发送和接收会自动打开和关闭，比较合适需要短链接的场景，如需要临时的长链接也可以调用Open()后在Close()。
client.ConnectionMode = ConnectionMode.AutoReconnection;//自动断线重连。在执行了Open()后，如果检测到断开后会自动打开，比较合适需要长链接的场景。调用Close()将不再重连。

//2：接收模式。以您以为的最好的方式来处理粘包问题
client.ReceiveMode = ReceiveMode.ParseByteAll();
client.ReceiveModeReceived = ReceiveMode.ParseByteAll();

//3：数据处理器。可在发送时加入换行，接收时去掉换行，也可自定义
client.SendDataProcessors.Add(new EndAddValueDataProcessor("\r\n", client1.Encoding));
client.ReceivedDataProcessors.Add(new EndClearValueDataProcessor("\r\n", client1.Encoding));

//4：事件驱动。
client.Opened += (a) => { Console.WriteLine("链接成功。"); };
client.Closed += (a, b) => { Console.WriteLine($"关闭成功。关闭代码：{b}"); };
client.Received += (a, b) => { Console.WriteLine($"收到消息：{a.Encoding.GetString(b)}"); };

client.Open();//打开，在打开前处理属性和事件

//5：简单的发送、接收和发送等待操作。 
client.Send("abc");//发送
client.Receive();//接收
client.Receive(3000);//接收，3秒超时
client.Receive(ReceiveMode.ParseToEnd("\n", 3000));//接收\n字符串结尾的，超时为3秒 
client.SendReceive("abc", 3000);//发送并等待接收数据，3秒超时
client.SendReceive("abc", ReceiveMode.ParseToEnd("\n", 3000));//发送并接收\n字符串结尾的，超时为3秒 
```

### 四 返回类型统一为 `IoTResult`，不需要在单独使用`Try`来处理异常信息。
> `IoTResult<T>`包含`Value`，`IoTResult`不包含 
```CSharp
var info = client.Read<bool>("abc");
if (info.IsSucceed)//应判断后在取值
   var val = info.Value;
else
   var err = info.ErrorText;
```