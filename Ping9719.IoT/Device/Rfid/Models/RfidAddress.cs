using Ping9719.IoT.Common;
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
        /// 密码,默认0X00000000
        /// </summary>
        public byte[] Pass { get; set; } = new byte[] { 0, 0, 0, 0 };
        /// <summary>
        /// 天线号
        /// </summary>
        public int AntennaNum { get; set; } = 1;

        /// <summary>
        /// 解析地址。比如“EPC”，“EPC;a=1;p=00 00 00 00”
        /// </summary>
        /// <param name="address">地址，a天线号，p密码。比如“EPC”，“EPC;a=1;p=00 00 00 00”</param>
        /// <returns></returns>
        public static RfidAddress GetRfidAddress(string address = "")
        {
            if (string.IsNullOrEmpty(address))
                return new RfidAddress();

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

        /// <summary>
        /// 得到rfid地址
        /// </summary>
        public static RfidAddress GetRfidAddress(RfidArea Area, byte[] Pass = null, int AntennaNum = 1)
        {
            return new RfidAddress() { Area = Area, Pass = Pass, AntennaNum = AntennaNum };
        }

        /// <summary>
        /// 得到rfid地址字符串
        /// </summary>
        public static string GetRfidAddressStr(RfidArea Area, byte[] Pass = null, int AntennaNum = 1)
        {
            List<string> str = new List<string>(3);
            str.Add(Area.ToString());
            str.Add($"a={AntennaNum.ToString()}");
            if (Pass != null && Pass.Any())
            {
                str.Add($"p={DataConvert.ByteArrayToString(Pass)}");
            }
            return string.Join(";", str);
        }
    }
}
