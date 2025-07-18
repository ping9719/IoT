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
    /// UsbHid客户端
    /// </summary>
    public class UsbHidClient : ClientBase
    {
        public static string[] GetNames => DeviceList.Local.GetHidDevices().Select(o => o.DevicePath).ToArray();
        public override bool IsOpen => IsOpen2;// hidDevice?.IsOpen ?? false && IsOpen2;
        string devicePath;
        private HidSharp.HidDevice hidDevice;

        public UsbHidClient(string devicePath)
        {
            this.devicePath = devicePath;

            ConnectionMode = ConnectionMode.Manual;
            Encoding = Encoding.ASCII;
            ReceiveMode = ReceiveMode.ParseByteAll();
            ReceiveModeReceived = ReceiveMode.ParseByteAll();
        }

        protected override OpenClientData Open2()
        {
            dataEri = new QueueByteFixed(ReceiveBufferSize, true);
            hidDevice = DeviceList.Local.GetHidDevices().FirstOrDefault(o => o.DevicePath == devicePath);
            if (hidDevice == null)
                throw new InvalidOperationException($"无法找到设备[{devicePath}]");

            return new OpenClientData(hidDevice.Open());
        }
    }
}
