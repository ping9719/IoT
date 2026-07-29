using HidSharp;
using Ping9719.IoT.Common;
using Ping9719.IoT.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ping9719.IoT.Hid
{
    /// <summary>
    /// 蓝牙客户端
    /// </summary>
    public class BleClient : ClientBase
    {
        public static string[] GetNames => DeviceList.Local.GetBleDevices().Select(o => o.DevicePath).ToArray();
        public override bool IsOpen => base.IsOpen;
        string devicePath;
        public HidSharp.Experimental.BleDevice Device;

        public BleClient(string devicePath)
        {
            this.devicePath = devicePath;

            ConnectionMode = ConnectionMode.Manual;
            Encoding = Encoding.ASCII;
            ReceiveMode = ReceiveMode.ParseByteAll();
            ReceiveModeReceived = ReceiveMode.ParseByteAll();
        }

        protected override OpenClientData OpenCore()
        {
            Device = DeviceList.Local.GetBleDevices().FirstOrDefault(o => o.DevicePath == devicePath);
            if (Device == null)
                throw new InvalidOperationException($"无法找到设备[{devicePath}]");

            return new OpenClientData(Device.Open());
        }
    }
}
