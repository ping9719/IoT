using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Device.Rfid
{
    /// <summary>
    /// rfid地址
    /// </summary>
    public class RfidAddress
    {
        /// <summary>
        /// 区域地址
        /// </summary>
        public RfidArea Area { get; set; } = RfidArea.EPC;
        /// <summary>
        /// 密码
        /// </summary>
        public byte[] Pass { get; set; } = null;
        /// <summary>
        /// 天线号
        /// </summary>
        public int AntennaNum { get; set; } = 1;

        /// <summary>
        /// 解析地址
        /// </summary>
        /// <param name="address">地址，a天线号，p密码。比如“EPC”，“EPC;a=1;p=00 00 00 00”</param>
        /// <returns></returns>
        public static RfidAddress GetRfidAddress(string address)
        {
            var info = address?.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[] { };
            RfidAddress rfidAddress = new RfidAddress();
            foreach (var item in info)
            {
                if (item.Contains('='))
                {
                    var info2 = item?.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[] { };
                    if (info2.Length != 2)
                        continue;
                    //解析密码
                    if (info2[0].ToLower().StartsWith("p"))
                    {
                        rfidAddress.Pass = info2[1].Split(' ').Select(o => Convert.ToByte(o, 16)).ToArray();
                    }
                    else if (info2[0].ToLower().StartsWith("a"))
                    {
                        rfidAddress.AntennaNum = Convert.ToInt32(info2[1]);
                    }
                }
                else
                {
                    if (Enum.TryParse<RfidArea>(item, true, out var area))
                        rfidAddress.Area = area;
                }
            }
            return rfidAddress;
        }
    }
}
