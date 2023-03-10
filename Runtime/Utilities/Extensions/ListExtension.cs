using System;
using System.Collections.Generic;

namespace Universe
{
    public static class ListExtension
    {
        /// <summary>
        /// 安全获取数据
        /// </summary>
        public static T SafeGet<T>(this IReadOnlyList<T> dataList, int index)
        {
            return !Utilities.IsValidIndex(index, dataList.Count) ? default : dataList[index];

        }

        public static T Get<T>(this T[] target, int index)
        {
            return Utilities.IsValidIndex(target.Length, index) ? target[index] : default;

        }

        public static void Filter<T>(this List<T> target, Func<T, bool> filter)
        {
            if (filter == null || target == null)
            {
                return;
            }

            List<T> temp = ListPool<T>.Allocate();
            for (int i = 0; i < target.Count; i++)
            {
                T t = target[i];
                if (filter.Invoke(t))
                {
                    temp.Add(t);
                }
            }

            target.Clear();
            target.AddRange(temp);
            ListPool<T>.Release(temp);
        }

        public static void Filter<T>(this List<T> target, Func<T, bool> filter, List<T> result)
        {
            if (target == null || result == null || filter == null)
            {
                return;
            }

            result.Clear();
            for (int i = 0; i < target.Count; i++)
            {
                T t = target[i];
                if (filter.Invoke(t))
                {
                    result.Add(t);
                }
            }
        }
    }
}