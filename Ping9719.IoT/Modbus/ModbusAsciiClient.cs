using Ping9719.IoT.Common;
using System;
using System.IO.Ports;
using System.Linq;
using Ping9719.IoT.Algorithm;
using Ping9719.IoT;
using Ping9719.IoT.Communication;
using System.Text;

namespace Ping9719.IoT.Modbus
{
    /// <summary>
    /// ModbusAscii
    /// </summary>
    public class ModbusAsciiClient : ModbusSerialBase, IIoT
    {
        public ModbusAsciiClient(ClientBase client, int timeout = 1500, EndianFormat format = EndianFormat.ABCD, byte stationNumber = 1, bool plcAddresses = false) : base(client, timeout, format, stationNumber, plcAddresses) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="portName">COM端口名称</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="parity">奇偶校验</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <param name="format">大小端设置</param>
        /// <param name="plcAddresses">PLC地址</param>
        public ModbusAsciiClient(string portName, int baudRate, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, int timeout = 1500, EndianFormat format = EndianFormat.ABCD, byte stationNumber = 1, bool plcAddresses = false) : base(new SerialPortClient(portName, baudRate, parity, dataBits, stopBits), timeout, format, stationNumber, plcAddresses) { }


        #region  Read 读取
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="address">寄存器起始地址</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="readLength">读取长度</param>
        /// <returns></returns>
        public override IoTResult<byte[]> Read(string address, byte stationNumber = 1, byte functionCode = 3, ushort readLength = 1, bool byteFormatting = true)
        {
            //if (isAutoOpen) Connect();

            var result = new IoTResult<byte[]>();
            try
            {
                //获取命令（组装报文）
                byte[] command = GetReadCommand(address, stationNumber, functionCode, readLength);
                var commandLRC = LRC.GetLRC(command).ByteArrayToAsciiArray();

                var finalCommand = new byte[commandLRC.Length + 3];
                Buffer.BlockCopy(commandLRC, 0, finalCommand, 1, commandLRC.Length);
                finalCommand[0] = 0x3A;
                finalCommand[finalCommand.Length - 2] = 0x0D;
                finalCommand[finalCommand.Length - 1] = 0x0A;

                result.Requests.Add(finalCommand);

                //发送命令并获取响应报文
                var sendResult = Client.SendReceive(finalCommand);
                if (!sendResult.IsSucceed)
                    return result.AddError(sendResult.Error).ToEnd();
                var responsePackage = sendResult.Value;

                if (!responsePackage.Any())
                {

                    result.AddError("响应结果为空");
                    return result.ToEnd();
                }

                byte[] resultLRC = new byte[responsePackage.Length - 3];
                Array.Copy(responsePackage, 1, resultLRC, 0, resultLRC.Length);
                var resultByte = resultLRC.AsciiArrayToByteArray();
                if (!LRC.CheckLRC(resultByte))
                {

                    result.AddError("响应结果LRC验证失败");
                    //return result.ToEnd();
                }
                var resultData = new byte[resultByte[2]];
                Buffer.BlockCopy(resultByte, 3, resultData, 0, resultData.Length);
                result.Responses.Add(responsePackage);
                //4 获取响应报文数据（字节数组形式）         
                if (byteFormatting)
                    result.Value = resultData.Reverse().ToArray().ByteFormatting(format);
                else
                    result.Value = resultData.Reverse().ToArray();
            }
            catch (Exception ex)
            {

                result.AddError(ex);
            }
            finally
            {
                //if (isAutoOpen) Dispose();
            }
            return result.ToEnd();
        }
        #endregion

        #region Write 写入
        /// <summary>
        /// 线圈写入
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <param name="stationNumber"></param>
        /// <param name="functionCode"></param>
        public override IoTResult Write(string address, bool value, byte stationNumber = 1, byte functionCode = 5)
        {
            //if (isAutoOpen) Connect();
            var result = new IoTResult();
            try
            {
                var command = GetWriteCoilCommand(address, value, stationNumber, functionCode);

                var commandAscii = LRC.GetLRC(command).ByteArrayToAsciiArray();
                var finalCommand = new byte[commandAscii.Length + 3];
                Buffer.BlockCopy(commandAscii, 0, finalCommand, 1, commandAscii.Length);
                finalCommand[0] = 0x3A;
                finalCommand[finalCommand.Length - 2] = 0x0D;
                finalCommand[finalCommand.Length - 1] = 0x0A;

                result.Requests.Add(finalCommand);
                //发送命令并获取响应报文
                var sendResult = Client.SendReceive(finalCommand);
                if (!sendResult.IsSucceed)
                    return result.AddError(sendResult.Error).ToEnd();
                var responsePackage = sendResult.Value;
                if (!responsePackage.Any())
                {

                    result.AddError("响应结果为空");
                    return result.ToEnd();
                }

                byte[] resultLRC = new byte[responsePackage.Length - 3];
                Array.Copy(responsePackage, 1, resultLRC, 0, resultLRC.Length);
                var resultByte = resultLRC.AsciiArrayToByteArray();
                if (!LRC.CheckLRC(resultByte))
                {

                    result.AddError("响应结果LRC验证失败");
                    //return result.ToEnd();
                }

                result.Responses.Add(responsePackage);
            }
            catch (Exception ex)
            {

                result.AddError(ex);
            }
            finally
            {
                //if (isAutoOpen) Dispose();
            }
            return result.ToEnd();
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address"></param>
        /// <param name="values"></param>
        /// <param name="stationNumber"></param>
        /// <param name="functionCode"></param>
        /// <returns></returns>
        public override IoTResult Write(string address, byte[] values, byte stationNumber = 1, byte functionCode = 16, bool byteFormatting = true)
        {
            //if (isAutoOpen) Connect();

            var result = new IoTResult();
            try
            {
                values = values.ByteFormatting(format);
                var command = GetWriteCommand(address, values, stationNumber, functionCode);

                var commandAscii = LRC.GetLRC(command).ByteArrayToAsciiArray();
                var finalCommand = new byte[commandAscii.Length + 3];
                Buffer.BlockCopy(commandAscii, 0, finalCommand, 1, commandAscii.Length);
                finalCommand[0] = 0x3A;
                finalCommand[finalCommand.Length - 2] = 0x0D;
                finalCommand[finalCommand.Length - 1] = 0x0A;

                result.Requests.Add(finalCommand);
                var sendResult = Client.SendReceive(finalCommand);
                if (!sendResult.IsSucceed)
                    return result.AddError(sendResult.Error).ToEnd();
                var responsePackage = sendResult.Value;
                if (!responsePackage.Any())
                {

                    result.AddError("响应结果为空");
                    return result.ToEnd();
                }

                byte[] resultLRC = new byte[responsePackage.Length - 3];
                Array.Copy(responsePackage, 1, resultLRC, 0, resultLRC.Length);
                var resultByte = resultLRC.AsciiArrayToByteArray();
                if (!LRC.CheckLRC(resultByte))
                {

                    result.AddError("响应结果LRC验证失败");
                    //return result.ToEnd();
                }

                result.Responses.Add(responsePackage);
            }
            catch (Exception ex)
            {

                result.AddError(ex);
            }
            finally
            {
                //if (isAutoOpen) Dispose();
            }
            return result.ToEnd();
        }

        #endregion
    }
}
