using Ping9719.IoT;
using Ping9719.IoT.Modbus;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Screw
{
    /// <summary>
    /// 快克螺丝机
    /// Modbus指令整理.xlsx
    /// </summary>
    public class KuaiKeScrew : ModbusRtuClient, IClientData
    {
        public KuaiKeScrew(string portName, int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, EndianFormat format = EndianFormat.BADC, byte stationNumber = 1)
            : base(portName, baudRate, parity, dataBits, stopBits, format, stationNumber)
        {

        }

        /// <summary>
        /// 写入状态
        /// </summary>
        /// <param name="val">0.无按键/清除按键 1.运行 2.暂停 3.停止 4.复位(按键寄存器无读取功能，切只响应当次写入，仅写入一次按键响应一次)</param>
        /// <returns></returns>
        public IoTResult WriteState(int val, bool isLeft = true)
        {
            return Write<Int16>(isLeft ? "s=1;x=6;12290" : "s=1;x=6;12291", Convert.ToInt16(val));
        }

        /// <summary>
        /// 读取信息
        /// </summary>
        /// <param name="sleep">内部延迟，可能还是提高通信的正确率</param>
        /// <returns></returns>
        public IoTResult<KuaiKeScrewInfo> ReadInfo(int sleep = 0)
        {
            var result = new IoTResult<KuaiKeScrewInfo>();
            result.Value = new KuaiKeScrewInfo();
            try
            {
                //bool isopen = false;
                //if (isAutoOpen)
                //{
                //    Open();
                //    isopen = true;
                //}

                var data1 = Read<short>("12289");
                if (!data1.IsSucceed)
                {
                    data1.AddError (data1.Error);
                    return new IoTResult<KuaiKeScrewInfo>(data1).ToEnd();
                }
                result.Value.螺丝锁附调用的文件号 = data1.Value;

                Thread.Sleep(sleep);

                data1 = Read<short>("12290");
                if (!data1.IsSucceed)
                {
                    data1.AddError(data1.Error);
                    return new IoTResult<KuaiKeScrewInfo>(data1).ToEnd();
                }
                result.Value.左机按键值 = data1.Value;
                Thread.Sleep(sleep);

                var data = Read<short>("16385", 35);
                if (!data.IsSucceed)
                {
                    data1.AddError(data.Error);
                    return new IoTResult<KuaiKeScrewInfo>(data).ToEnd();
                }

                var valList = data.Value.ToList();
                result.Value.螺丝锁附状态1 = valList[0];
                result.Value.螺丝锁附状态2 = valList[1];
                result.Value.设备复位状态1 = valList[2];
                result.Value.设备复位状态2 = valList[3];
                result.Value.设备状态1 = valList[4];
                result.Value.设备状态2 = valList[5];
                result.Value.轴伺服报警信号 = valList[10];
                result.Value.锁附报警1 = valList[11];
                result.Value.锁附报警2 = valList[12];
                result.Value.通讯报警1 = valList[13];
                result.Value.通讯报警2 = valList[14];
                result.Value.运行状态报警1 = valList[15];
                result.Value.运行状态报警2 = valList[16];
                result.Value.左机加工完成标志 = valList[33];
                result.Value.右机加工完成标志 = valList[34];

                //if (isopen)
                //{
                //    Close();
                //}
            }
            catch (Exception ex)
            {
                
                result.AddError(ex);
            }
            //finally
            //{
            //    if (isAutoOpen)
            //        Dispose();
            //}
            return result.ToEnd();
        }
    }

    public class KuaiKeScrewInfo
    {
        public int 螺丝锁附调用的文件号 { get; set; }
        /// <summary>
        /// 0.无按键/清除按键 1.运行 2.暂停 3.停止 4.复位(按键寄存器无读取功能，切只响应当次写入，仅写入一次按键响应一次)
        /// </summary>
        public int 左机按键值 { get; set; }
        //public int 右机按键值 { get; set; }
        /// <summary>
        /// 0停机空闲 1组别参数载入中 2编程点合法性验证 3等待工件装载 4工件夹紧中 5正在加工 6加工结束设备停止 7工件松开中 8等待工件取走 9Y轴复位中 10等待启动键按下
        /// </summary>
        public int 螺丝锁附状态1 { get; set; }
        public int 螺丝锁附状态2 { get; set; }
        /// <summary>
        /// 0.未复位1.复位中2.复位异常3复位完成
        /// </summary>
        public int 设备复位状态1 { get; set; }
        public int 设备复位状态2 { get; set; }
        /// <summary>
        /// 1.运行 2.暂停 3.复位中 4.报警 5.未复位 6.停止/空闲 7.急停
        /// </summary>
        public int 设备状态1 { get; set; }
        public int 设备状态2 { get; set; }
        /// <summary>
        /// bit0——bit5 对应轴1——6 状态0无报警 状态1报警
        /// </summary>
        public int 轴伺服报警信号 { get; set; }
        /// <summary>
        /// 0.无报警 1.滑牙 2.浮锁 3.深度异常 4.下压气缸返回故障 5.完成停留时间太小 6.真空检测失败 7吸气/吹出螺丝失败 8.供料器分料超时
        /// </summary>
        public int 锁附报警1 { get; set; }
        public int 锁附报警2 { get; set; }
        /// <summary>
        /// 0.无报警 1.读取失败 2.写入失败 3.任务号切换失败
        /// </summary>
        public int 通讯报警1 { get; set; }
        public int 通讯报警2 { get; set; }
        /// <summary>
        /// 0.无报警 1.夹紧异常 2.光栅报警
        /// </summary>
        public int 运行状态报警1 { get; set; }
        public int 运行状态报警2 { get; set; }
        /// <summary>
        /// 0.未加工/加工中 1.加工完成 2.异常中止
        /// </summary>
        public int 左机加工完成标志 { get; set; }
        public int 右机加工完成标志 { get; set; }
    }
}
