using System.Collections.Generic;

namespace Universe
{
    public class ObjectPool<T> where T : class, ICacheAble, new()
    {

        readonly Stack<T> m_Stack = new();

        /// <summary>
        /// 当前缓存数量
        /// </summary>
        public int CachedCount => m_Stack.Count;

        public static ObjectPool<T> Create()
        {
            return new();
        }

        private ObjectPool()
        {

        }

        public T Get()
        {
            T element = m_Stack.Count == 0 ? new() : m_Stack.Pop();
            element.Reset();
            element.IsInCache = false;
            return element;
        }

        // 释放进入缓存
        public void Release(T t)
        {
            if (t == null)
            {
                return;
            }

            t.IsInCache = true;
            t.Reset();
            m_Stack.Push(t);
        }
    }
}