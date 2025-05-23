using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Ping9719.IoT.Algorithm
{
    /// <summary>
    /// 稳定婚姻配对项
    /// </summary>
    /// <typeparam name="T">项的类型</typeparam>
    public class GaleShapleyItem<T> where T : class
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="item">待配对者</param>
        public GaleShapleyItem(T item)
        {
            Item = item;
        }

        /// <summary>
        /// 待配对者
        /// </summary>
        public T Item { get; set; }
        /// <summary>
        /// 偏好列表
        /// </summary>
        public List<GaleShapleyItem<T>> Preferences { get; set; } = new List<GaleShapleyItem<T>>();
        /// <summary>
        /// 匹配完成的对象
        /// </summary>
        public GaleShapleyItem<T> Match { get; set; }
    }

    /// <summary>
    /// 稳定婚姻配对算法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class GaleShapleyAlgorithm
    {
        /// <summary>
        /// 开始计算
        /// </summary>
        public static void Run<T>(IEnumerable<GaleShapleyItem<T>> items) where T : class
        {
            foreach (var man in items)
            {
                man.Match = null;
            }

            while (true)
            {
                bool anyMatchMade = false;
                foreach (var man in items)
                {
                    if (man.Match == null)
                    {
                        anyMatchMade |= Propose(man);//任意一个是true，则是true
                    }
                }

                if (!anyMatchMade)
                {
                    break; // 如果当前循环中没有人成功配对，则所有可能的稳定匹配已完成
                }
            }
        }

        private static bool Propose<T>(GaleShapleyItem<T> man) where T : class
        {
            var nextPreference = man.Preferences.Find(woman => woman.Match == null || woman.Preferences.IndexOf(man) < woman.Preferences.IndexOf(woman.Match));
            if (nextPreference != null)
            {
                if (nextPreference.Match != null)
                {
                    nextPreference.Match.Match = null; // 解除当前匹配
                }

                man.Match = nextPreference;
                nextPreference.Match = man;

                return true;
            }

            return false;
        }
    }
}
