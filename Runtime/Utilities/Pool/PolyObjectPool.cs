using System;
using System.Collections.Generic;

namespace Universe
{
    /// <summary>
    /// 多态对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PolyObjectPool<T> where T : class, ICacheAble
    {
        readonly Dictionary<Type, Stack<T>> m_TypeMapping = new();

        /// <summary>
        /// Current Count
        /// </summary>
        public int CachedCount { get; private set; }

        /// <summary>
        /// Max Count
        /// </summary>
        public int CaxCacheCount { get; }

        /// <summary>
        /// Construct with count
        /// </summary>
        /// <param name="caxCacheCount"></param>
        public PolyObjectPool(int caxCacheCount = Const.N_1024)
        {
            CaxCacheCount = caxCacheCount;
        }

        /// <summary>
        /// Get Generic
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <returns></returns>
        public E Get<E>() where E : class, T, new()
        {
            if (!m_TypeMapping.TryGetValue(typeof(E), out Stack<T> stack))
            {
                return new();
            }

            if (stack.Count <= 0)
            {
                return new();
            }

            --CachedCount;

            T element = stack.Pop();
            if (!(element is ICacheAble cacheAble))
            {
                return element as E;
            }

            cacheAble.IsInCache = false;
            cacheAble.Reset();

            return element as E;
        }

        /// <summary>
        /// Get By Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public T Get(Type type)
        {
            if (m_TypeMapping.TryGetValue(type, out Stack<T> stack) && stack.Count > 0)
            {
                --CachedCount;

                T element = stack.Pop();
                if (element is ICacheAble cacheAble)
                {
                    cacheAble.IsInCache = false;
                }

                return element;
            }

            T t = type.Assembly.CreateInstance(type.Name) as T;

            return t;
        }

        /// <summary>
        /// Release Generic
        /// </summary>
        /// <param name="tp"></param>
        /// <typeparam name="TE"></typeparam>
        public void Release<TE>(TE tp) where TE : T
        {
            if (tp == null)
            {
                return;
            }

            // 只缓存继承了ICacheAble接口的元素
            ICacheAble cacheAble = tp;
            if (cacheAble.IsInCache)
            {
                return;
            }

            Type type = cacheAble.GetType();

            if (!m_TypeMapping.TryGetValue(type, out Stack<T> stack))
            {
                stack = new();
                m_TypeMapping.Add(type, stack);
            }

            if (CachedCount >= CaxCacheCount)
            {
                cacheAble.IsInCache = true;
                return;
            }

            ++CachedCount;

            cacheAble.IsInCache = true;
            cacheAble.Reset();
            stack.Push(tp);
        }
    }
}