using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Communication.SerialPort
{
    public class SerialPortClient : ClientBase
    {
        public override bool IsOpen => throw new NotImplementedException();

        public SerialPortClient(string name)
        { 
        
        }

        public override IoTResult Close()
        {
            throw new NotImplementedException();
        }

        public override IoTResult Open()
        {
            throw new NotImplementedException();
        }

        public override IoTResult<byte[]> Receive(ReceiveMode receiveMode = null)
        {
            throw new NotImplementedException();
        }

        public override IoTResult Send(byte[] data, int offset = 0, int count = -1)
        {
            throw new NotImplementedException();
        }

        public override IoTResult<byte[]> SendReceive(byte[] data, ReceiveMode receiveMode = null)
        {
            throw new NotImplementedException();
        }
    }
}
