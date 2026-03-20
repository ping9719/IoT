using HidSharp;
using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Hid
{
    /// <summary>
    /// 串口客户端
    /// </summary>
    public class SerialClient : ClientBase
    {
        public static string[] GetNames => DeviceList.Local.GetSerialDevices().Select(o => o.DevicePath).ToArray();
        public override bool IsOpen => base.IsOpen;
        string devicePath;
        public SerialDevice Device;

        public SerialClient(string devicePath)
        {
            this.devicePath = devicePath;

            ConnectionMode = ConnectionMode.Manual;
            Encoding = Encoding.ASCII;
            ReceiveMode = ReceiveMode.ParseByteAll();
            ReceiveModeReceived = ReceiveMode.ParseByteAll();
        }

        protected override OpenClientData Open2()
        {
            Device = DeviceList.Local.GetSerialDevices().FirstOrDefault(o => o.DevicePath == devicePath);
            if (Device == null)
                throw new InvalidOperationException($"无法找到设备[{devicePath}]");

            return new OpenClientData(Device.Open());
        }
    }
}
