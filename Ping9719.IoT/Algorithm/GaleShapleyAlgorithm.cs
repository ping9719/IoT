using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Ping9719.IoT.Algorithm
{
    //public class Person2<T> where T : class
    //{
    //    /// <summary>
    //    /// 待配对者
    //    /// </summary>
    //    public T Me { get; set; }
    //    /// <summary>
    //    /// 偏好列表
    //    /// </summary>
    //    public List<T> Preferences { get; set; } = new List<T>();
    //    /// <summary>
    //    /// 匹配完成的对象
    //    /// </summary>
    //    public T Match { get; set; }
    //}

    public class Person<T> where T : class
    {
        /// <summary>
        /// 待配对者
        /// </summary>
        public T Me { get; set; }
        /// <summary>
        /// 偏好列表
        /// </summary>
        public List<Person<T>> Preferences { get; set; } = new List<Person<T>>();
        /// <summary>
        /// 匹配完成的对象
        /// </summary>
        public Person<T> Match { get; set; }
    }

    /// <summary>
    /// 稳定婚姻配对算法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GaleShapleyAlgorithm<T> where T : class
    {
        private List<Person<T>> mans, womens;

        public GaleShapleyAlgorithm(List<Person<T>> mans)
        {
            this.mans = mans;
        }

        //public GaleShapleyAlgorithm(List<Person<T>> mans, List<Person<T>> womens)
        //{
        //    this.mans = mans;
        //    this.womens = womens;
        //}

        public void Run()
        {
            foreach (var man in mans)
            {
                man.Match = null;
            }

            while (true)
            {
                bool anyMatchMade = false;
                foreach (var man in mans)
                {
                    if (man.Match == null)
                    {
                        anyMatchMade |= Propose(man);
                    }
                }

                if (!anyMatchMade)
                {
                    break; // 如果当前循环中没有人成功配对，则所有可能的稳定匹配已完成
                }
            }
        }

        private bool Propose(Person<T> mans)
        {
            var nextPreference = mans.Preferences.Find(woman => woman.Match == null || woman.Preferences.IndexOf(mans) < woman.Preferences.IndexOf(woman.Match));
            if (nextPreference != null)
            {
                if (nextPreference.Match != null)
                {
                    nextPreference.Match.Match = null; // 解除当前匹配
                }

                mans.Match = nextPreference;
                nextPreference.Match = mans;

                return true;
            }

            return false;
        }
    }
}
