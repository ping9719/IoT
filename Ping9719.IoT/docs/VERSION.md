
## 版本记录：[version history]   
> *表示部分功能可能与前版本不兼容 [*For some code is incompatible with previous versions]

## v0.5.0（...） 
1.[优化]`TcpClient`客户端增加`OpenTimeOut`默认8秒提升网络较复杂下的成功率
## v0.4.0（25-10-29）
*1.[更改]将`params T[]`替换为 `IEnumerable<T>` 避免重载冲突   
2.[新增]tcp支持更多的初始化方式    
*3.[更改]TCP默认编码为UTF8   
*4.[更改]协议中去掉默认的`ConnectionMode`   
5.[优化]Open更改为先关闭在打开   
6.[新增]客户端支持自定义心跳   
7.[优化]部分退出后在`Receive2`中出现的while异常优化   
## v0.3.0（25-08-31）
*1.[更改]将`IIoT`更改为`IClientData`     
2.[新增]增加`ReadWriteBase`对字符串类型进行实现   
3.[新增]tcp和串口增加连接字符串构造函数   
4.[修复]`ModbusRtu`写失败   
## v0.2.0（25-08-01）
*1.[新增]更改内置处理器名称     
2.[修复]byte[]开头结尾null判断     
3.[新增]增加7种内置数据处理器   
4.[新增]新增 平均点位算法`AveragePoint`   
*5.[修复]客户端，可以打开失败也可以重连，优化线程池   
6.[修复]客户端指定指定调度器，避免ui冲突    
*7.[更改]接收模式ToString 更名 ToEnd   
*8.[更改]最大重连时间单位ms变为s    
9.[修复]客户端 IsOpen 优化   
10.[新增]CRC 是否小端    
11.[修复]客户端关闭异步等待优化    
12.[修复]客户端异步读取超时为-1，解决usb读取等问题    
13.[新增]`TcpServer`
## v0.1.0（25-07-18）
1.发布