
#
### 版本记录：[version history]
###### *表示部分代码可能与前版本不兼容 [*For some code is incompatible with previous versions]
## v0.3.0
*1.[更改]将IIoT更改为IClientData     
2.[新增]增加ReadWriteBase对字符串类型进行实现
## v0.2.0
*1.[新增]更改内置处理器名称     
2.[修复]byte[]开头结尾null判断     
3.[新增]增加7种内置数据处理器   
4.[新增]新增 平均点位算法（AveragePoint）   
*5.[修复]客户端，可以打开失败也可以重连，优化线程池   
6.[修复]客户端指定指定调度器，避免ui冲突    
*7.[更改]接受模式ToString 更名 ToEnd   
*8.[更改]最大重连时间单位ms变为s    
9.[修复]客户端 IsOpen 优化   
10.[新增]CRC 是否小端    
11.[修复]客户端关闭异步等待优化    
12.[修复]客户端异步读取超时为-1，解决usb读取等问题    
13.[新增]TcpServer
## v0.1.0
1.发布