using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Ping9719.IoT.Communication.SerialPort
{
    /// <summary>
    /// SerialPort基类
    /// </summary>
    public abstract class SerialPortBase
    {
        /// <summary>
        /// 串行端口对象
        /// </summary>
        protected System.IO.Ports.SerialPort serialPort;

        /// <summary>
        /// 是否自动打开关闭
        /// </summary>
        protected bool isAutoOpen = true;

        /// <summary>
        /// 获取设备上的COM端口集合
        /// </summary>
        /// <returns></returns>
        public static string[] GetPortNames()
        {
            return System.IO.Ports.SerialPort.GetPortNames();
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        protected IoTResult Connect()
        {
            var result = new IoTResult();
            serialPort?.Close();
            try
            {
                serialPort.Open();
            }
            catch (Exception ex)
            {
                if (serialPort?.IsOpen ?? false) serialPort?.Close();
                result.AddError(ex);
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 打开连接
        /// </summary>
        /// <returns></returns>
        public IoTResult Open()
        {
            isAutoOpen = false;
            return Connect();
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        protected IoTResult Dispose()
        {
            var result = new IoTResult();
            try
            {
                serialPort.Close();
            } 
            catch (Exception ex)
            {
                
                result.AddError(ex);
            }
            return result;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        public IoTResult Close()
        {
            isAutoOpen = true;
            return Dispose();
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="serialPort"></param>
        /// <returns></returns>
        protected virtual IoTResult<byte[]> SerialPortRead()
        {
            IoTResult<byte[]> result = new IoTResult<byte[]>();
            DateTime beginTime = DateTime.Now;
            var tempBufferLength = serialPort.BytesToRead;
            //在(没有取到数据或BytesToRead在继续读取)且没有超时的情况，延时处理
            while ((serialPort.BytesToRead == 0 || tempBufferLength != serialPort.BytesToRead) && DateTime.Now - beginTime <= TimeSpan.FromMilliseconds(serialPort.ReadTimeout))
            {
                tempBufferLength = serialPort.BytesToRead;
                //延时处理
                Thread.Sleep(20);
            }
            byte[] buffer = new byte[serialPort.BytesToRead];
            var receiveFinish = 0;
            while (receiveFinish < buffer.Length)
            {
                var readLeng = serialPort.Read(buffer, receiveFinish, buffer.Length);
                if (readLeng == 0)
                {
                    result.Value = null;
                    return result.ToEnd();
                }
                receiveFinish += readLeng;
            }
            result.Value = buffer;
            return result.ToEnd();
        }

        /// <summary>
        /// 发送报文，并获取响应报文
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual IoTResult<byte[]> SendPackageReliable(byte[] command)
        {
            IoTResult<byte[]> _sendPackage()
            {
                //从发送命令到读取响应为最小单元，避免多线程执行串数据（可线程安全执行）
                lock (this)
                {
                    //发送命令
                    serialPort.Write(command, 0, command.Length);
                    //获取响应报文
                    return SerialPortRead();
                }
            }

            try
            {
                var result = _sendPackage();
                if (!result.IsSucceed)
                {
                    //WarningLog?.Invoke(result.Err, result.Exception);
                    //如果出现异常，则进行一次重试         
                    var conentResult = Connect();
                    if (!conentResult.IsSucceed)
                        return new IoTResult<byte[]>(conentResult);

                    return _sendPackage();
                }
                else
                    return result;
            }
            catch (Exception)
            {
                //WarningLog?.Invoke(ex.Message, ex);
                //如果出现异常，则进行一次重试
                //重新打开连接
                var conentResult = Connect();
                if (!conentResult.IsSucceed)
                    return new IoTResult<byte[]>(conentResult);

                return _sendPackage();
            }
        }
    }
}
