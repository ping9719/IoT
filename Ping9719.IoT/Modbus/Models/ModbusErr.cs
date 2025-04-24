using System;

namespace Ping9719.IoT.Modbus
{
    /// <summary>
    /// 帮助类
    /// </summary>
    public class ModbusErr
    {
        /// <summary>
        /// 是否为异常功能码
        /// </summary>
        /// <param name="resultCode">请求的</param>
        /// <param name="responseCode">响应的</param>
        /// <returns></returns>
        public static bool VerifyFunctionCode(byte resultCode, byte responseCode)
        {
            //异常功能码：0x83（原功能码 0x03 + 0x80）
            //return responseCode - resultCode == 128;
            return resultCode != responseCode || responseCode >= 128;
        }

        /// <summary>
        /// 异常码描述
        /// https://www.likecs.com/show-204655077.html?sc=5546
        /// </summary>
        /// <param name="errCode"></param>
        public static string ErrMsg(byte errCode)
        {
            var err = $"异常码{errCode}：未知异常";
            switch (errCode)
            {
                case 0x01:
                    err = $"异常码{errCode}：非法功能码";
                    break;
                case 0x02:
                    err = $"异常码{errCode}：非法数据地址";
                    break;
                case 0x03:
                    err = $"异常码{errCode}：非法数据值";
                    break;
                case 0x04:
                    err = $"异常码{errCode}：服务器设备故障";
                    break;
                case 0x05:
                    err = $"异常码{errCode}：确认";
                    break;
                case 0x06:
                    err = $"异常码{errCode}：服务器忙";
                    break;
                case 0x08:
                    err = $"异常码{errCode}：内存奇偶校验错误";
                    break;
                case 0x0A:
                    err = $"异常码{errCode}：网关路径不可用";
                    break;
                case 0x0B:
                    err = $"异常码{errCode}：网关目标设备未响应";
                    break;
            }
            return err;
        }
    }
}
