using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping9719.IoT.Algorithm
{
    /// <summary>
    /// 采用直接计算的方式计算CRC（有直接计算和查表法两种方式）
    /// https://gitee.com/anyangchina/crc_all
    /// </summary>
    public static class CRC
    {
        /// <summary>
        /// 验证crc8算法
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="poly">多项式</param>
        /// <param name="init">初始值</param>
        /// <param name="ref_in">输入反转</param>
        /// <param name="ref_out">输出反转</param>
        /// <param name="xor_out">结果异或</param>
        /// <returns>追加的结果</returns>
        public static bool CheckCrc8(byte[] data, byte poly = 0x07, byte init = 0x00, bool ref_in = false, bool ref_out = false, byte xor_out = 0x00)
        {
            var aa = Crc8Base(data, 0, data.Length - 1, 0, poly, init, ref_in, ref_out, xor_out);
            return aa[data.Length - 1] == data[data.Length - 1];
        }
        public static bool CheckCrc8Itu(byte[] data) => CheckCrc8(data, 0x07, 0x00, false, false, 0x55);
        public static bool CheckCrc8Rohc(byte[] data) => CheckCrc8(data, 0x07, 0xFF, true, true, 0x00);
        public static bool CheckCrc8Maxim(byte[] data) => CheckCrc8(data, 0x31, 0x00, true, true, 0x00);
        /// <summary>
        /// crc8算法
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="poly">多项式</param>
        /// <param name="init">初始值</param>
        /// <param name="ref_in">输入反转</param>
        /// <param name="ref_out">输出反转</param>
        /// <param name="xor_out">结果异或</param>
        /// <returns>追加的结果</returns>
        public static byte[] Crc8(byte[] data, byte poly = 0x07, byte init = 0x00, bool ref_in = false, bool ref_out = false, byte xor_out = 0x00)
        {
            return Crc8Base(data, 0, data.Length, 0, poly, init, ref_in, ref_out, xor_out);
        }
        public static byte[] Crc8Itu(byte[] data) => Crc8(data, 0x07, 0x00, false, false, 0x55);
        public static byte[] Crc8Rohc(byte[] data) => Crc8(data, 0x07, 0xFF, true, true, 0x00);
        public static byte[] Crc8Maxim(byte[] data) => Crc8(data, 0x31, 0x00, true, true, 0x00);
        private static byte[] Crc8Base(byte[] data, int start_index, int len, byte shift, byte poly, byte init, bool ref_in, bool ref_out, byte xor_out)
        {
            byte crc = (byte)(init << shift);
            poly = (byte)(poly << shift);
            byte data_byte;

            for (int j = start_index; j < start_index + len; j++)
            {
                data_byte = ref_in ? Reverse8(data[j]) : data[j];
                crc = (byte)(crc ^ data_byte);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x80) != 0)
                    {
                        crc = (byte)(crc << 1 ^ poly);
                    }
                    else
                    {
                        crc = (byte)(crc << 1);
                    }
                }
            }
            crc = ref_out ? Reverse8(crc) : (byte)(crc >> shift);
            var crc8 = (byte)(crc ^ xor_out);
            return data.Skip(start_index).Take(len).Concat(new byte[] { crc8 }).ToArray();
        }


        /// <summary>
        /// 验证crc16算法
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="poly">多项式</param>
        /// <param name="init">初始值</param>
        /// <param name="ref_in">输入反转</param>
        /// <param name="ref_out">输出反转</param>
        /// <param name="xor_out">结果异或</param>
        /// <param name="is_little_endian">小端序</param>
        /// <returns>追加的结果</returns>
        public static bool CheckCrc16(byte[] data, ushort poly = 0x8005, ushort init = 0xFFFF, bool ref_in = true, bool ref_out = true, ushort xor_out = 0x0000, bool is_little_endian = true)
        {
            var aa = Crc16Base(data, 0, data.Length - 2, 0, poly, init, ref_in, ref_out, xor_out, is_little_endian);
            return aa[data.Length - 1] == data[data.Length - 1] && aa[data.Length - 2] == data[data.Length - 2];
        }
        public static bool CheckCrc16Ibm(byte[] data, bool is_little_endian = true) => CheckCrc16(data, 0x8005, 0x0000, true, true, 0x0000, is_little_endian);
        public static bool CheckCrc16Maxim(byte[] data, bool is_little_endian = true) => CheckCrc16(data, 0x8005, 0x0000, true, true, 0xFFFF, is_little_endian);
        public static bool CheckCrc16Usb(byte[] data, bool is_little_endian = true) => CheckCrc16(data, 0x8005, 0xFFFF, true, true, 0xFFFF, is_little_endian);
        public static bool CheckCrc16Modbus(byte[] data, bool is_little_endian = true) => CheckCrc16(data, 0x8005, 0xFFFF, true, true, 0x0000, is_little_endian);
        public static bool CheckCrc16Ccitt(byte[] data, bool is_little_endian = true) => CheckCrc16(data, 0x1021, 0x0000, true, true, 0x0000, is_little_endian);
        public static bool CheckCrc16CcittFalse(byte[] data, bool is_little_endian = true) => CheckCrc16(data, 0x1021, 0xFFFF, false, false, 0x0000, is_little_endian);
        public static bool CheckCrc16X25(byte[] data, bool is_little_endian = true) => CheckCrc16(data, 0x1021, 0xFFFF, true, true, 0xFFFF, is_little_endian);
        public static bool CheckCrc16Ymodem(byte[] data, bool is_little_endian = true) => CheckCrc16(data, 0x1021, 0x0000, false, false, 0x0000, is_little_endian);
        public static bool CheckCrc16Dnp(byte[] data, bool is_little_endian = true) => CheckCrc16(data, 0x3d65, 0x0000, true, true, 0xFFFF, is_little_endian);
        /// <summary>
        /// crc16算法
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="poly">多项式</param>
        /// <param name="init">初始值</param>
        /// <param name="ref_in">输入反转</param>
        /// <param name="ref_out">输出反转</param>
        /// <param name="xor_out">结果异或</param>
        /// <param name="is_little_endian">小端序</param>
        /// <returns>追加的结果</returns>
        public static byte[] Crc16(byte[] data, ushort poly = 0x8005, ushort init = 0xFFFF, bool ref_in = true, bool ref_out = true, ushort xor_out = 0x0000, bool is_little_endian = true)
        {
            return Crc16Base(data, 0, data.Length, 0, poly, init, ref_in, ref_out, xor_out, is_little_endian);
        }
        public static byte[] Crc16Ibm(byte[] data, bool is_little_endian = true) => Crc16(data, 0x8005, 0x0000, true, true, 0x0000, is_little_endian);
        public static byte[] Crc16Maxim(byte[] data, bool is_little_endian = true) => Crc16(data, 0x8005, 0x0000, true, true, 0xFFFF, is_little_endian);
        public static byte[] Crc16Usb(byte[] data, bool is_little_endian = true) => Crc16(data, 0x8005, 0xFFFF, true, true, 0xFFFF, is_little_endian);
        public static byte[] Crc16Modbus(byte[] data, bool is_little_endian = true) => Crc16(data, 0x8005, 0xFFFF, true, true, 0x0000, is_little_endian);
        public static byte[] Crc16Ccitt(byte[] data, bool is_little_endian = true) => Crc16(data, 0x1021, 0x0000, true, true, 0x0000, is_little_endian);
        public static byte[] Crc16CcittFalse(byte[] data, bool is_little_endian = true) => Crc16(data, 0x1021, 0xFFFF, false, false, 0x0000, is_little_endian);
        public static byte[] Crc16X25(byte[] data, bool is_little_endian = true) => Crc16(data, 0x1021, 0xFFFF, true, true, 0xFFFF, is_little_endian);
        public static byte[] Crc16Ymodem(byte[] data, bool is_little_endian = true) => Crc16(data, 0x1021, 0x0000, false, false, 0x0000, is_little_endian);
        public static byte[] Crc16Dnp(byte[] data, bool is_little_endian = true) => Crc16(data, 0x3d65, 0x0000, true, true, 0xFFFF, is_little_endian);
        private static byte[] Crc16Base(byte[] data, int start_index, int len, ushort shift, ushort poly, ushort init, bool ref_in, bool ref_out, ushort xor_out, bool is_little_endian)
        {
            ushort crc = (ushort)(init << shift);
            poly = (ushort)(poly << shift);
            byte data_byte;

            for (int j = start_index; j < start_index + len; j++)
            {
                data_byte = ref_in ? Reverse8(data[j]) : data[j];
                crc = (ushort)(crc ^ data_byte << 8);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                    {
                        crc = (ushort)(crc << 1 ^ poly);
                    }
                    else
                    {
                        crc = (ushort)(crc << 1);
                    }
                }
            }
            crc = ref_out ? Reverse16(crc) : (ushort)(crc >> shift);
            var crc16 = (ushort)(crc ^ xor_out);
            var crc16byte = (is_little_endian && BitConverter.IsLittleEndian) ? BitConverter.GetBytes(crc16) : BitConverter.GetBytes(crc16).Reverse();
            return data.Skip(start_index).Take(len).Concat(crc16byte).ToArray();
        }

        /// <summary>
        /// 验证crc32算法
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="poly">多项式</param>
        /// <param name="init">初始值</param>
        /// <param name="ref_in">输入反转</param>
        /// <param name="ref_out">输出反转</param>
        /// <param name="xor_out">结果异或</param>
        /// <param name="is_little_endian">小端序</param>
        /// <returns>追加的结果</returns>
        public static bool CheckCrc32(byte[] data, uint poly = 0x04C11DB7, uint init = 0xFFFFFFFF, bool ref_in = true, bool ref_out = true, uint xor_out = 0xFFFFFFFF, bool is_little_endian = true)
        {
            var aa = Crc32Base(data, 0, data.Length - 4, poly, init, ref_in, ref_out, xor_out, is_little_endian);
            return aa[data.Length - 1] == data[data.Length - 1] && aa[data.Length - 2] == data[data.Length - 2] && aa[data.Length - 3] == data[data.Length - 3] && aa[data.Length - 4] == data[data.Length - 4];
        }
        public static bool CheckCrc32Mpeg2(byte[] data, bool is_little_endian = true) => CheckCrc32(data, 0x04c11db7, 0xFFFFFFFF, false, false, 0x00000000, is_little_endian);
        public static bool CheckCrc32Sata(byte[] data, bool is_little_endian = true) => CheckCrc32(data, 0x04C11DB7, 0x52325032, false, false, 0x00000000, is_little_endian);
        public static bool CheckCrc32Q(byte[] data, bool is_little_endian = true) => CheckCrc32(data, 0x814141AB, 0x00000000, false, false, 0x00000000, is_little_endian);
        /// <summary>
        /// crc32算法
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="poly">多项式</param>
        /// <param name="init">初始值</param>
        /// <param name="ref_in">输入反转</param>
        /// <param name="ref_out">输出反转</param>
        /// <param name="xor_out">结果异或</param>
        /// <param name="is_little_endian">小端序</param>
        /// <returns>追加的结果</returns>
        public static byte[] Crc32(byte[] data, uint poly = 0x04C11DB7, uint init = 0xFFFFFFFF, bool ref_in = true, bool ref_out = true, uint xor_out = 0xFFFFFFFF, bool is_little_endian = true)
        {
            return Crc32Base(data, 0, data.Length, poly, init, ref_in, ref_out, xor_out, is_little_endian);
        }
        public static byte[] Crc32Mpeg2(byte[] data, bool is_little_endian = true) => Crc32(data, 0x04c11db7, 0xFFFFFFFF, false, false, 0x00000000, is_little_endian);
        public static byte[] Crc32Sata(byte[] data, bool is_little_endian = true) => Crc32(data, 0x04C11DB7, 0x52325032, false, false, 0x00000000, is_little_endian);
        public static byte[] Crc32Q(byte[] data, bool is_little_endian = true) => Crc32(data, 0x814141AB, 0x00000000, false, false, 0x00000000,is_little_endian);
        private static byte[] Crc32Base(byte[] data, int start_index, int len, uint poly, uint init, bool ref_in, bool ref_out, uint xor_out, bool is_little_endian)
        {
            uint crc = init;
            byte data_byte;

            for (int j = start_index; j < start_index + len; j++)
            {
                data_byte = ref_in ? Reverse8(data[j]) : data[j];
                crc = (uint)(crc ^ data_byte << 24);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x80000000) != 0)
                    {
                        crc = crc << 1 ^ poly;
                    }
                    else
                    {
                        crc = crc << 1;
                    }
                }
            }
            crc = ref_out ? Reverse32(crc) : crc;
            var crc32 = crc ^ xor_out;
            var crc16byte = (is_little_endian && BitConverter.IsLittleEndian) ? BitConverter.GetBytes(crc32) : BitConverter.GetBytes(crc32).Reverse();
            return data.Skip(start_index).Take(len).Concat(crc16byte).ToArray();
        }


        private static byte Reverse8(byte data)
        {
            byte i;
            byte temp = 0;
            for (i = 0; i < 8; i++)
                temp |= (byte)((data >> i & 0x01) << 7 - i);
            return temp;
        }
        private static ushort Reverse16(ushort data)
        {
            ushort i;
            ushort temp = 0;
            for (i = 0; i < 16; i++)
                temp |= (ushort)((data >> i & 0x01) << 15 - i);
            return temp;
        }
        private static uint Reverse32(uint data)
        {
            int i;
            uint temp = 0;
            for (i = 0; i < 32; i++)
                temp |= (data >> i & 0x01) << 31 - i;
            return temp;
        }
    }
}
