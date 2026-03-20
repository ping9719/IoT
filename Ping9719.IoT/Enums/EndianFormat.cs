namespace Ping9719.IoT
{
    /// <summary>
    /// 字节格式
    /// https://cloud.tencent.com/developer/article/1601823
    /// </summary>
    public enum EndianFormat
    {
        /// <summary>
        /// 大端序
        /// </summary>
        ABCD = 0,
        /// <summary>
        /// 中端序, PDP-11 风格
        /// </summary>
        BADC = 1,
        /// <summary>
        /// 中端序, Honeywell 316 风格
        /// </summary>
        CDAB = 2,
        /// <summary>
        /// 小端序，C#默认
        /// </summary>
        DCBA = 3,
    }
}
