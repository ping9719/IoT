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
    }
}
