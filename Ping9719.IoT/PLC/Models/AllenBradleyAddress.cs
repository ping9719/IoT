using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Ping9719.IoT.PLC
{
    public class AllenBradleyAddress
    {
        public string AddressSource { get; set; }
        public List<string> Address { get; set; }
        public List<int> Index { get; set; }

        public static AllenBradleyAddress Parse(string address)
        {
            AllenBradleyAddress allenBradleyAddress = new AllenBradleyAddress()
            {
                AddressSource = address,
                Address = new List<string>(),
                Index = new List<int>(2)
            };

            var vLen = address.Length;
            if (address.EndsWith("]"))
            {
                var aaa = address.LastIndexOf('[');
                if (aaa >= 0)
                {
                    vLen = aaa;
                    var bbbb = address.Substring(aaa + 1, address.Length - aaa - 2).Split(',').Select(o => Convert.ToInt32(o));
                    allenBradleyAddress.Index = bbbb.ToList();
                }
            }
            allenBradleyAddress.Address = address.Substring(0, vLen).Split('.').ToList();
            return allenBradleyAddress;
        }

        public byte[] GetCip(Encoding encoding)
        {
            List<byte> bytes = new List<byte>();
            //A
            //4C 02 91 01 41 00 01 00
            //D[9]
            //4C 04 91 01 44 00 29 00 09 00 01 00
            //4C 03 91 01 44 00 28 09 01 00
            //A.B
            //4C 04 91 01 41 00 91 01 42 00 01 00
            //4C 04 91 01 41 00 91 01 42 00 01 00
            //A.B[9]
            //4C 06 91 01 41 00 91 01 42 00 29 00 09 00 01 00
            foreach (var item in Address)
            {
                var addData = encoding.GetBytes(item).ToList();
                byte length = (byte)addData.Count;
                if (length % 2 == 1)
                {
                    addData.Add(0);
                }
                bytes.AddRange(new byte[] { 0x91, length });
                bytes.AddRange(addData);
            }
            foreach (var item in Index)
            {
                bytes.AddRange(new byte[] { 0x29, 0x00 });
                bytes.AddRange(BitConverter.GetBytes((ushort)item));
            }
            return bytes.ToArray();
        }
    }
}
