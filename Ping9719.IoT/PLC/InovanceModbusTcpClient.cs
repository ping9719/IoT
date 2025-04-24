using Ping9719.IoT.Communication;
using Ping9719.IoT.Modbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.PLC
{
    /// <summary>
    /// 汇川plc
    /// </summary>
    public class InovanceModbusTcpClient : ModbusTcpClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="format"></param>
        /// <param name="stationNumber"></param>
        public InovanceModbusTcpClient(ClientBase client, EndianFormat format = EndianFormat.CDAB, byte stationNumber = 1) : base(client, format, stationNumber) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="format"></param>
        /// <param name="stationNumber"></param>
        public InovanceModbusTcpClient(string ip, int port = 502, EndianFormat format = EndianFormat.CDAB, byte stationNumber = 1) : base(ip, port, format, stationNumber) { }

        #region IIoTBase
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="address">D、R为寄存器，M、B、S、X、Y为线圈</param>
        public override IoTResult<T> Read<T>(string address)
        {
            var nAddress = AddressAnalysis(address);
            return base.Read<T>(nAddress);
        }

        /// <summary>
        /// 读取多个
        /// </summary>
        /// <param name="address">D、R为寄存器，M、B、S、X、Y为线圈</param>
        /// <param name="number">读取数量</param>
        public override IoTResult<IEnumerable<T>> Read<T>(string address, int number)
        {
            var nAddress = AddressAnalysis(address);
            return base.Read<T>(nAddress, number);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="address">D、R为寄存器，M、B、S、X、Y为线圈</param>
        public override IoTResult Write<T>(string address, T value)
        {
            var nAddress = AddressAnalysis(address);
            return base.Write(nAddress, value);
        }

        /// <summary>
        /// 写入，内部循环，失败了就跳出
        /// </summary>
        /// <param name="address">D、R为寄存器，M、B、S、X、Y为线圈</param>
        public override IoTResult Write<T>(string address, params T[] value)
        {
            var nAddress = AddressAnalysis(address);
            return base.Write(nAddress, value);
        }
        #endregion

        /// <summary>
        /// 解析地址
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        public static string AddressAnalysis(string address)
        {
            var ty = address.Trim().ToUpper().First();
            var readAddress = Convert.ToInt32(address.Trim().Substring(1));

            switch (ty)
            {
                case 'D'://0-7999
                    readAddress += 0;
                    break;
                case 'R'://12288-45055
                    readAddress += 12288;
                    break;
                case 'M'://0-7999
                    readAddress += 0;
                    break;
                case 'B'://12288-45055
                    readAddress += 12288;
                    break;
                case 'S'://57344-61439
                    readAddress += 57344;
                    break;
                case 'X'://63488-64511
                    readAddress = Convert.ToInt32(readAddress.ToString(), 8) + 63488;
                    break;
                case 'Y': //61512-65535
                    readAddress = Convert.ToInt32(readAddress.ToString(), 8) + 61512;
                    break;
                default:
                    throw new Exception("不支持的类型：" + ty);
            }

            return readAddress.ToString();
        }
    }
}
