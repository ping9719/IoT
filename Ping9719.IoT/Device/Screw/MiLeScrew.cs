using Ping9719.IoT;
using Ping9719.IoT.Enums;
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
    /// 米勒螺丝机
    /// 电批各阶段参数地址信息.xlsx
    /// 启动	53	"1.从0到1，并保持1，则启动。
    ///2.赋值1=启动，拧紧结束后，重新赋值0，然后重新赋值1，则再次启动。
    ///3.拧紧过程中，若赋值0，则结束拧紧动作。"
    ///清除报警	53	赋值2，则清除报警。清楚报警后需要赋值0。
    ///反转	54	值=0时，启动后是正转；值=2时，启动后是反转
    ///转速值	4096	单位RPM
    ///电批状态	4102	拧螺丝状态，0=待机，1=正在拧螺丝
    ///扭矩值	4103	单位0.01牛米，数值保持
    ///峰值扭矩值	4104	
    ///IN状态	4111	输入端口状态
    ///OUT状态	4112	二进制 0001，bit0=1则电批待机状态
    ///锁OK	4112	二进制 0100，bit2=1则拧紧OK
    ///锁NG	4112	二进制 1000，bit3=1则拧紧NG
    ///伺服报警	4112	二进制 0010，bit1=1则报警
    ///启动次数	4114	电批累计启动计数，重新上电归0并重新累加
    ///报警代码	4115	电批报警代码
    ///拧紧耗时	4116	电批从启动到结束或中途退出累计计时
    ///控制器版本	4120	控制器软件版本号
    ///角度值	4128	旋转角度，单位0.01圈,PA=14计算第2阶拧紧角度，PA=17计算开始终了角度
    ///当前通道号	4129	当前加载的通道号
    ///拧紧进程	4131	当前拧紧进程，此参数比较重要，能依此判断电批拧紧处于哪一步骤中
    ///历史力矩值	28672	记录当前工作一轮实时扭力值，每 PA0-67 毫秒保存一次，最大保存 6144点。
    ///历史角度值	35072	记录当前工作一轮实时角度值，每 PA0-67 毫秒保存一次，最大保存 6144点。
    /// </summary>
    public class MiLeScrew : ModbusRtuClient
    {
        public MiLeScrew(string portName, int baudRate = 9600, int dataBits = 8, StopBits stopBits = StopBits.One, Parity parity = Parity.None, int timeout = 1500, EndianFormat format = EndianFormat.BADC, byte stationNumber = 1, bool plcAddresses = false)
            : base(portName, baudRate, dataBits, stopBits, parity, timeout, format, stationNumber, plcAddresses)
        {

        }

        /// <summary>
        /// 扭矩
        /// </summary>
        public IoTResult<double> ReadTorque(int sleep = 0)
        {
            var aaa = Read<short>("4103");
            return new IoTResult<double>(aaa, aaa.Value / 100.00);
        }

        /// <summary>
        /// 圈数
        /// </summary>
        public IoTResult<double> ReadCycles(int sleep = 0)
        {
            var aaa = Read<short>("4128");
            return new IoTResult<double>(aaa, aaa.Value / 100.00);
        }

        /// <summary>
        /// 计时
        /// </summary>
        public IoTResult<double> ReadWorkTimes(int sleep = 0)
        {
            var tmp = Read<Int16>("4116");
            return new IoTResult<double>(tmp, ((double)tmp.Value) / 100.00);
        }

        /// <summary>
        /// 电批状态  拧螺丝状态，0=待机，1=正在拧螺丝
        /// </summary>
        public IoTResult<int> ReadScrewStatus(int sleep = 0)
        {
            var tmp = Read<Int16>("4102");
            return new IoTResult<int>(tmp);
        }

    }
}
