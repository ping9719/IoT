using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Ping9719.IoT.Common
{
    /// <summary>
    /// IEnumerable扩展
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        /// 分块
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="size">块大小</param>
        /// <param name="isInsufficientDiscard">是否数量不足丢弃</param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<TSource>> Chunk<TSource>(this IEnumerable<TSource> source, int size, bool isInsufficientDiscard = true)
        {
            return source
                .Select((value, index) => new { value, index })
                .GroupBy(x => x.index / size)
                .Select(g => g.Select(x => x.value))
                .Where(o => isInsufficientDiscard ? o.Count() == size : true);
        }

        /// <summary>
        /// 分块，不足的进行补充
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source">数据</param>
        /// <param name="size">块大小</param>
        /// <param name="isSupplEnd">是否从结尾补充。false 从前面补充</param>
        /// <param name="supplVal">补充的值</param>
        /// <param name="isReverse">是否反转每一个里面的元素</param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<TSource>> ChunkSuppl<TSource>(this IEnumerable<TSource> source, int size, bool isSupplEnd = true, TSource supplVal = default, bool isReverse = false)
        {
            if (source == null)
                return null;

            var info = source
                .Select((value, index) => new { value, index })
                .GroupBy(x => x.index / size)
                .Select(g => g.Select(x => x.value).ToList()).ToList();

            if ((info.LastOrDefault()?.Count() ?? size) != size)
            {
                if (isSupplEnd)
                {
                    var abc = Enumerable.Repeat(supplVal, size - info.Last().Count());
                    info.Last().AddRange(abc);
                }
                else
                {
                    var abc = Enumerable.Repeat(supplVal, size - info.Last().Count());
                    info.Last().InsertRange(0, abc);
                }
            }

            if (isReverse)
            {
                foreach (var item in info)
                    item.Reverse();
            }
            return info;
        }
    }
}
