using System;
using System.Collections.Generic;

namespace Universe
{
    /// <summary>
    /// 单例组件容器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonContainer<T> where T : class
    {
        readonly Dictionary<Type, T> m_Instance = new();

        public List<T> Instance { get; } = new();

        public E Register<E>(Action<E> onRegister = null) where E : class, T, new()
        {
            Type type = typeof(E);
            if (m_Instance.ContainsKey(type))
            {
                return null;
            }

            E instance = new();
            m_Instance[type] = instance;
            Instance.Add(instance);
            GenericGetter<E>.Getter = InternalGet<E>;
            onRegister?.Invoke(instance);
            return instance;
        }

        public E Get<E>() where E : class, T
        {
            return GenericGetter<E>.Instance;
        }
        
        E InternalGet<E>() where E : class, T
        {
            if (m_Instance.TryGetValue(typeof(E), out T instance))
            {
                return instance as E;
            }

            return null;
        }

        private static class GenericGetter<E> where E : class, T
        {
            static E s_Instance;
            public static Func<E> Getter;
            public static E Instance => s_Instance ??= Getter?.Invoke();
        }
    }
}