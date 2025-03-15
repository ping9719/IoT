using Ping9719.IoT.Common;
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

namespace Ping9719.IoT.Communication.SerialPort
{
    /// <summary>
    /// 串口客户端
    /// </summary>
    public class SerialPortClient : ClientBase
    {
        System.IO.Ports.SerialPort serialPort;
        object obj1 = new object();
        bool isSendReceive = false;//是否正在发送和接受中
        /// <summary>
        /// 是否链接
        /// </summary>
        public bool IsConnect = false;
        public override bool IsOpen => serialPort?.IsOpen ?? false;

        public SerialPortClient(string portName, int baudRate, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            serialPort = new System.IO.Ports.SerialPort();
            serialPort.PortName = portName;
            serialPort.BaudRate = baudRate;
            serialPort.DataBits = dataBits;
            serialPort.StopBits = stopBits;
            serialPort.Encoding = Encoding;
            serialPort.Parity = parity;
        }

        public override IoTResult Open()
        {
            var result = new IoTResult();
            try
            {
                lock (obj1)
                {
                    var aa = Opening?.Invoke(this);
                    if (aa == false)
                        throw new Exception("用户已拒绝链接");

                    Open2(false);
                }
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        public override IoTResult Close()
        {
            var result = new IoTResult();
            try
            {
                if (IsOpen)
                {
                    var aa = Closing?.Invoke(this);
                    if (aa == false)
                        throw new Exception("用户已拒绝断开");

                    Close2(true);
                }
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        public override IoTResult Send(byte[] data, int offset = 0, int count = -1)
        {
            var result = new IoTResult();
            var isHmOpen = false;
            isSendReceive = true;
            try
            {
                if (!IsOpen && ConnectionMode == ConnectionMode.AutoOpen)
                { result = Open(); isHmOpen = true; }
                if (!result.IsSucceed)
                    return result;

                result.Requests.Add(data);

                lock (obj1)
                {
                    Send2(data, offset, count);
                }
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            finally
            {
                if (IsOpen && ConnectionMode == ConnectionMode.AutoOpen && isHmOpen)
                    Close();

                isSendReceive = false;
            }
            return result.ToEnd();
        }

        public override IoTResult<byte[]> Receive(ReceiveMode receiveMode = null)
        {
            var result = new IoTResult<byte[]>();
            var isHmOpen = false;
            isSendReceive = true;
            try
            {
                if (!IsOpen && ConnectionMode == ConnectionMode.AutoOpen)
                { result = Open().ToVal<byte[]>(); isHmOpen = true; }
                if (!result.IsSucceed)
                    return result;

                lock (obj1)
                {
                    result.Value = Receive2(receiveMode);
                }


            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            finally
            {
                if (IsOpen && ConnectionMode == ConnectionMode && isHmOpen)
                    Close();

                isSendReceive = false;
            }
            return result.ToEnd();
        }

        public override IoTResult<byte[]> SendReceive(byte[] data, ReceiveMode receiveMode = null)
        {
            var result = new IoTResult<byte[]>();
            var isHmOpen = false;
            isSendReceive = true;
            try
            {
                if (!IsOpen && ConnectionMode == ConnectionMode.AutoOpen)
                { result = Open().ToVal<byte[]>(); isHmOpen = true; }
                if (!result.IsSucceed)
                    return result;

                lock (obj1)
                {
                    if (IsAutoDiscard && serialPort.BytesToRead > 0)
                    {
                        var bytes = new byte[serialPort.BytesToRead];
                        serialPort.Read(bytes, 0, bytes.Length);
                    }

                    Send2(data);
                    result.Value = Receive2(receiveMode);
                }
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }
            finally
            {
                if (IsOpen && ConnectionMode == ConnectionMode.AutoOpen && isHmOpen)
                    Close();

                isSendReceive = false;
            }
            return result.ToEnd();
        }

        #region 内部
        void Open2(bool IsReconnection)
        {
            //serialPort?.Close();
            serialPort?.Open();

            if (!IsReconnection)
                Monitor2();
        }

        void Monitor2()
        {
            Task.Run(() =>
            {
                byte[] data = new byte[0];
                while (true)
                {
                    //System.Threading.Thread.Sleep(1);
                    try
                    {
                        if (IsConnect && IsOpen)
                        {
                            //等待消息
                            var receiveResult = serialPort.Read(data, 0, 0);
                            if (IsConnect && serialPort.BytesToRead == 0 && !isSendReceive)//可能已经断开
                            {
                                Close2(false);
                            }
                            else if (IsConnect && Received != null && serialPort.BytesToRead > 0 && !isSendReceive)//有新信息
                            {
                                lock (obj1)
                                {
                                    var bytes = Receive2(ReceiveModeReceived);
                                    Received?.Invoke(this, bytes);
                                }
                            }

                            ////可能已经断开
                            //if (IsPoll && IsConnect && Socket.Poll(1000, SelectMode.SelectRead) && Socket.Available == 0)
                            //{
                            //    Close2(false);
                            //}
                        }
                        else if (ConnectionMode == ConnectionMode.AutoReconnection)
                        {
                            Open2(true);
                        }
                        else
                        {
                            return;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {

                    }
                }
            });
        }

        /// <summary>
        /// 断开
        /// </summary>
        /// <param name="isUser">是否用户自己断开的</param>
        void Close2(bool isUser)
        {
            try
            {
                serialPort?.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                IsConnect = false;
                Closed?.BeginInvoke(this, isUser, null, null);
            }
        }

        void Send2(byte[] data, int offset = 0, int count = -1)
        {
            serialPort.Write(data, offset, count < 0 ? data.Length : count);
        }

        byte[] Receive2(ReceiveMode receiveMode = null)
        {
            receiveMode ??= ReceiveMode;

            byte[] value = new byte[0];
            DateTime beginTime = DateTime.Now;
            if (receiveMode.Type == ReceiveModeEnum.Byte)
            {
                var count = 0;
                var countMax = (int)receiveMode.Data;
                value = new byte[countMax];

                while (countMax - count > 0)
                {
                    if (IsOutTime(beginTime, receiveMode.TimeOut))
                        throw new TimeoutException("已超时");

                    count += serialPort.Read(value, count, countMax - count);
                }
            }
            else if (receiveMode.Type == ReceiveModeEnum.ByteAll)
            {
                while (serialPort.BytesToRead == 0)
                {
                    if (IsOutTime(beginTime, receiveMode.TimeOut))
                        throw new TimeoutException("已超时");
                    Thread.Sleep(10);
                }
                value = new byte[serialPort.BytesToRead];
                var count = serialPort.Read(value, 0, value.Length);
            }
            else if (receiveMode.Type == ReceiveModeEnum.Char)
            {
                var count = 0;
                var countMax = (int)receiveMode.Data * 2;
                value = new byte[countMax];

                while (countMax - count > 0)
                {
                    if (IsOutTime(beginTime, receiveMode.TimeOut))
                        throw new TimeoutException("已超时");

                    count += serialPort.Read(value, count, countMax - count);
                }
            }
            else if (receiveMode.Type == ReceiveModeEnum.Time)
            {
                var countMax = (int)receiveMode.Data;
                var tempBufferLength = serialPort.BytesToRead;
                while (serialPort.BytesToRead == 0 || tempBufferLength != serialPort.BytesToRead)
                {
                    if (IsOutTime(beginTime, receiveMode.TimeOut))
                        throw new TimeoutException("已超时");

                    tempBufferLength = serialPort.BytesToRead;
                    Thread.Sleep(countMax);
                }
                value = new byte[serialPort.BytesToRead];
                var count = serialPort.Read(value, 0, value.Length);
            }
            else if (receiveMode.Type == ReceiveModeEnum.ToString)
            {
                //value = new byte[0];
                var zfc = Encoding.GetBytes(receiveMode.Data.ToString());
                List<byte> buffer = new List<byte>();
                while (true)
                {
                    if (serialPort.BytesToRead == 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    if (IsOutTime(beginTime, receiveMode.TimeOut))
                        throw new TimeoutException("已超时");

                    var a1 = new byte[serialPort.BytesToRead];
                    var count = serialPort.Read(a1, 0, a1.Length);
                    buffer.AddRange(a1);
                    if (buffer.ToArray().EndsWith(zfc))
                    {
                        break;
                    }
                }
                value = buffer.ToArray();
            }

            return value;
        }
        #endregion
    }
}
