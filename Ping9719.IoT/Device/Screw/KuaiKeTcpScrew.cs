using Ping9719.IoT;
using Ping9719.IoT.Modbus;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Screw
{
    /// <summary>
    /// 快克螺丝机（智能电批）
    /// 设置IP方式：开机按住屏幕-网络设置
    /// Quick--Modbus TCP智能电批.xls
    /// </summary>
    public class KuaiKeTcpScrew : ModbusTcpClient, IIoT
    {
        public KuaiKeTcpScrew(string ip, int port = 502, int timeout = 1500) : base(ip, port, timeout)
        {

        }

        /// <summary>
        /// 委托(累计数量,当前加工完成的信息)
        /// </summary>
        public Action<int, KuaiKeScrewElectricBatchInfo> MonitorInfo;

        /// <summary>
        /// 读取信息
        /// </summary>
        /// <returns></returns>
        public IoTResult<KuaiKeScrewElectricBatchInfo> ReadInfo()
        {
            var result = new IoTResult<KuaiKeScrewElectricBatchInfo>();
            try
            {
                var data = Read<short>("0", 11);
                if (!data.IsSucceed)
                    return new IoTResult<KuaiKeScrewElectricBatchInfo>(data).ToEnd();

                var valList = data.Value.ToList();
                if (valList.Count != 11)
                    return new IoTResult<KuaiKeScrewElectricBatchInfo>(data).ToEnd();

                result.Value = new KuaiKeScrewElectricBatchInfo();
                result.Value.任务号 = valList[0];
                result.Value.圈数 = Convert.ToDouble(valList[1]) / 100.00;
                result.Value.扭力 = valList[2];
                result.Value.耗时 = valList[3];
                switch (valList[4])
                {
                    case 0X4AAA:
                        result.Value.结果 = new KeyValuePair<int, string>(valList[4], "NG");
                        break;
                    case 0X4BBB:
                        result.Value.结果 = new KeyValuePair<int, string>(valList[4], "OK");
                        break;
                    case 0X4CCC:
                        result.Value.结果 = new KeyValuePair<int, string>(valList[4], "未完成");
                        break;
                    default:
                        result.Value.结果 = new KeyValuePair<int, string>(valList[4], "未知");
                        break;
                }
                switch (valList[5])
                {
                    case 1:
                        result.Value.报警 = new KeyValuePair<int, string>(valList[5], "浮高");
                        break;
                    case 2:
                        result.Value.报警 = new KeyValuePair<int, string>(valList[5], "滑牙");
                        break;
                    case 3:
                        result.Value.报警 = new KeyValuePair<int, string>(valList[5], "过流（断电重启）");
                        break;
                    case 4:
                        result.Value.报警 = new KeyValuePair<int, string>(valList[5], "过压（检查供电电压是否偏高）");
                        break;
                    case 5:
                        result.Value.报警 = new KeyValuePair<int, string>(valList[5], "欠压（检查供电电压是否偏低）");
                        break;
                    case 6:
                        result.Value.报警 = new KeyValuePair<int, string>(valList[5], "飞车");
                        break;
                    case 7:
                        result.Value.报警 = new KeyValuePair<int, string>(valList[5], "I2T过热（检查批头、螺丝是否打滑）");
                        break;
                    case 8:
                        result.Value.报警 = new KeyValuePair<int, string>(valList[5], "反转不到位");
                        break;
                    case 9:
                        result.Value.报警 = new KeyValuePair<int, string>(valList[5], "位置偏差过大（检查电机线与encoder线的接触是否良好）");
                        break;
                    case 10:
                        result.Value.报警 = new KeyValuePair<int, string>(valList[5], "电批断线");
                        break;
                    case 11:
                        result.Value.报警 = new KeyValuePair<int, string>(valList[5], "力矩偏差异常");
                        break;
                    case 12:
                        result.Value.报警 = new KeyValuePair<int, string>(valList[5], "拧松失败");
                        break;
                    case 32:
                        result.Value.报警 = new KeyValuePair<int, string>(valList[5], "超时");
                        break;
                    default:
                        result.Value.报警 = new KeyValuePair<int, string>(valList[5], string.Empty);
                        break;
                }
                result.Value.实时扭矩 = valList[6];
                switch (valList[7])
                {
                    case 1:
                        result.Value.电批状态 = new KeyValuePair<int, string>(valList[7], "STOP");
                        break;
                    case 2:
                        result.Value.电批状态 = new KeyValuePair<int, string>(valList[7], "RUN");
                        break;
                    default:
                        result.Value.电批状态 = new KeyValuePair<int, string>(valList[7], string.Empty);
                        break;
                }
                switch (valList[8])
                {
                    case 0:
                        result.Value.机台状态 = new KeyValuePair<int, string>(valList[8], "STOP");
                        break;
                    case 1:
                        result.Value.机台状态 = new KeyValuePair<int, string>(valList[8], "RUN");
                        break;
                    default:
                        result.Value.机台状态 = new KeyValuePair<int, string>(valList[8], string.Empty);
                        break;
                }
                result.Value.螺丝号 = valList[9];
                result.Value.完成标志 = valList[10];
            }
            catch (Exception ex)
            {

                result.AddError(ex);
            }
            finally
            {

            }
            return result.ToEnd();
        }

        bool isTopMonitor = false;//是否触发停止
        /// <summary>
        /// 是否正在监听
        /// </summary>
        public bool IsMonitor { get; private set; } = false;
        /// <summary>
        /// 监听到的全部信息
        /// </summary>
        public List<KuaiKeScrewElectricBatchInfo> MonitorInfos { get; private set; } = new List<KuaiKeScrewElectricBatchInfo>();

        /// <summary>
        /// 重置并开始监听
        /// </summary>
        public void StartMonitor()
        {
            MonitorInfos = new List<KuaiKeScrewElectricBatchInfo>();
            Task.Run(() =>
            {
                //终止
                if (IsMonitor)
                {
                    StopMonitor();
                    while (true)
                    {
                        if (!IsMonitor)
                            break;
                    }
                }

                //开始
                IsMonitor = true;
                //bool isOpen = false;
                //if (isAutoOpen || IsConnected == false)
                //{
                //    Open();
                //    isOpen = true;
                //}

                int sta = -1;
                while (true)
                {
                    if (isTopMonitor)
                        break;

                    var aa = ReadInfo();
                    if (aa.IsSucceed && sta != aa.Value.电批状态.Key)
                    {
                        MonitorInfos.Add(aa.Value);
                        sta = aa.Value.电批状态.Key;
                    }

                    Thread.Sleep(1);
                }

                //if (isOpen)
                //    Close();
                IsMonitor = false;
                isTopMonitor = false;
            });
        }

        /// <summary>
        /// 重置并开始监听（以完成标志变化为监听）
        /// </summary>
        public void StartMonitorMark()
        {
            MonitorInfos = new List<KuaiKeScrewElectricBatchInfo>();
            Task.Run(() =>
            {
                //终止
                if (IsMonitor)
                {
                    StopMonitor();
                    while (true)
                    {
                        if (!IsMonitor)
                            break;
                    }
                }

                //开始
                IsMonitor = true;
                //bool isOpen = false;
                //if (isAutoOpen || IsConnected == false)
                //{
                //    Open();
                //    isOpen = true;
                //}

                int sta = 1;
                while (true)
                {
                    if (isTopMonitor)
                        break;

                    var aa = ReadInfo();
                    if (aa.IsSucceed && aa.Value.完成标志 == 1 && sta != aa.Value.完成标志)
                    {
                        MonitorInfos.Add(aa.Value);
                        MonitorInfo?.Invoke(MonitorInfos.Count(), aa.Value);
                    }
                    if (aa.IsSucceed)
                        sta = aa.Value.完成标志;

                    Thread.Sleep(1);
                }

                //if (isOpen)
                //    Close();
                IsMonitor = false;
                isTopMonitor = false;
            });
        }

        /// <summary>
        /// 停止监听
        /// </summary>
        public void StopMonitor()
        {
            isTopMonitor = true;
        }
    }

    public class KuaiKeScrewElectricBatchInfo
    {
        /// <summary>
        /// 任务号(0-7)
        /// </summary>
        public int 任务号 { get; set; }
        /// <summary>
        /// 圈数(r)
        /// </summary>
        public double 圈数 { get; set; }
        /// <summary>
        /// 扭力(mM.m)
        /// </summary>
        public int 扭力 { get; set; }
        /// <summary>
        /// 耗时（ms）
        /// </summary>
        public int 耗时 { get; set; }
        /// <summary>
        /// 结果 19114:NG;19387:OK;19660:未完成
        /// </summary>
        public KeyValuePair<int, string> 结果 { get; set; }
        /// <summary>
        /// 报警信息
        /// </summary>
        public KeyValuePair<int, string> 报警 { get; set; }
        /// <summary>
        /// 实时扭矩(mM.m)
        /// </summary>
        public int 实时扭矩 { get; set; }
        /// <summary>
        /// 电批运行状态（2 Run;1 Stop）
        /// </summary>
        public KeyValuePair<int, string> 电批状态 { get; set; }
        /// <summary>
        /// 机台运行状态（1 Run;0 Stop）
        /// </summary>
        public KeyValuePair<int, string> 机台状态 { get; set; }
        /// <summary>
        /// 螺丝编号
        /// </summary>
        public int 螺丝号 { get; set; }
        /// <summary>
        /// 完成标志 运行0，打完保存1
        /// </summary>
        public int 完成标志 { get; set; }

        /// <summary>
        /// 筛选出停止点位
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static List<KuaiKeScrewElectricBatchInfo> WhereStop(List<KuaiKeScrewElectricBatchInfo> data, bool isClre = false)
        {
            List<KuaiKeScrewElectricBatchInfo> rdata = new List<KuaiKeScrewElectricBatchInfo>();
            if (data == null)
                return rdata;

            int i = 0;
            foreach (var item in data)
            {
                if (i > 0)
                {
                    //1STOP 2RUN
                    if (item.电批状态.Key == 1 && data[i - 1].电批状态.Key == 2)
                        rdata.Add(item);
                }
                i++;
            }

            if (isClre)
            {
                List<KuaiKeScrewElectricBatchInfo> rdata2 = new List<KuaiKeScrewElectricBatchInfo>();
                foreach (var item in rdata)
                {
                    if (!rdata2.Any(o => o.圈数 == item.圈数
                    && o.扭力 == item.扭力
                    && o.耗时 == item.耗时
                    && o.结果.Key == item.结果.Key
                    && o.电批状态.Key == item.电批状态.Key
                    && o.机台状态.Key == item.机台状态.Key))
                    {
                        rdata2.Add(item);
                    }
                }
                return rdata2;
            }


            return rdata;
        }
    }
}
