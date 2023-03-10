using System.Collections.Generic;

namespace Universe
{
    public readonly struct FixedArray<T>
    {
        readonly T[] m_ArrayInstance;
        static readonly Dictionary<int, FixedArray<T>> s_ArrayInstance = new();

        public static FixedArray<T> Get(int capacity)
        {
            if (capacity <= 0)
            {
                return default;
            }

            if (s_ArrayInstance.TryGetValue(capacity, out FixedArray<T> array))
            {
                return array;
            }

            array = new(new T[capacity]);
            s_ArrayInstance[capacity] = array;
            return array;
        }

        public FixedArray(T[] arrayInstance)
        {
            m_ArrayInstance = arrayInstance;
        }

        public T this[int index]
        {
            get => m_ArrayInstance[index];
            set => m_ArrayInstance[index] = value;
        }

        public static implicit operator T[](FixedArray<T> other)
        {
            return other.m_ArrayInstance;
        }

        public static explicit operator FixedArray<T>(T[] other)
        {
            return new(other);
        }
    }
}