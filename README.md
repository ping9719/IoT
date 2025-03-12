# Ping9719.IoT

### 工业互联网通讯库协议实现，包括主流PLC、ModBus、CIP、MC、FINS......等常用协议。
##### Iot device communication protocol implementation, including mainstream PLC, ModBus, CIP, MC, FINS...... Such common protocols.
#

### 全部文档：[doc]
[查看入门文档 (Entry document)](Ping9719.IoT/docs/README.md)   
[查看版本文档 (Versioning)](Ping9719.IoT/docs/VERSION.md)   
#

### 内容树：[Content tree]

# [Ping9719.IoT](Ping9719.IoT/docs/README.md)   
- Modbus
    - ModbusRtu (ModbusRtuClient,ModbusRtuOverTcpClient)
    - ModbusTcp (ModbusTcpClient)
    - ModbusAscii (ModbusAsciiClient)
- PLC
    - 罗克韦尔 (AllenBradleyCipClient) （未测试）  
    - 汇川 (InovanceModbusTcpClient)
    - 三菱 (MitsubishiMcClient)
    - 欧姆龙 (OmronFinsClient,OmronCipClient)
    - 西门子 (SiemensS7Client)
- 机器人 (Robot)
    - 爱普生 (EpsonRobot) （进行中） 
- 通讯 (Communication)
    - TcpClient （进行中） 
    - TcpServer （待开发） 
    - UdpClient （待开发） 
    - UdpServer （待开发） 
    - HttpServer （待开发） 
    - MqttClient （待开发） 
    - MqttServer （待开发） 
    - SerialPortClient （待开发） 
- 算法 (Algorithm)
    - CRC
    - 傅立叶算法(Fourier) （待开发） 
    - PID （待开发） 
    - RSA （待开发） 
- 设备和仪器 (Device)
    - Fct
        - 盟讯电子 (MengXunFct)
    - 激光刻印 (Mark)
        - 大族激光刻印 (DaZhuTcpMark)
    - 无线射频 (Rfid)
        - 倍加福Rfid (BeiJiaFuRfid)
        - 泰和森Rfid (TaiHeSenRfid)
        - 万全Rfid (WanQuanRfid)
    - 扫码枪 (Scanner)
        - 霍尼韦尔扫码器 (HoneywellScanner)
        - 民德扫码器 (MindeoScanner)
    - 螺丝机 (Screw)
        - 快克螺丝机 (KuaiKeDeskScrew,KuaiKeScrew,KuaiKeTcpScrew)
        - 米勒螺丝机 (MiLeScrew)
    - 温控 (TemperatureControl)
        - 快克温控 (KuaiKeTemperatureControl)
    - 焊接机 (Weld)
        - 快克焊接机 (KuaiKeWeld)