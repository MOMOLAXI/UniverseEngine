using System;
using UnityEngine;

namespace Universe
{
    public static class Utilities
    {
        public static bool IsValidIndex(int index, int count)
        {
            return index > -1 && index < count;
        }

        /// <summary>
        /// Swap Generic
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <typeparam name="T"></typeparam>
        public static void Swap<T>(ref T left, ref T right)
        {
            (left, right) = (right, left);
        }

        public static void Swap<T>(this T[] array, int left, int right)
        {
            if (!IsValidIndex(left, array.Length) || !IsValidIndex(right, array.Length))
            {
                return;
            }

            (array[left], array[right]) = (array[right], array[left]);
        }

        /// <summary>
        /// Swap
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static void PlusSwap(ref int left, ref int right)
        {
            left += right;
            right = left - right;
            left -= right;
        }

        /// <summary>
        /// Swap
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static void Swap(ref int left, ref int right)
        {
            left ^= right;
            right ^= left;
            left ^= right;
        }

        /// <summary>
        /// Swap
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static void Swap(ref long left, ref long right)
        {
            left ^= right;
            right ^= left;
            left ^= right;
        }

        /// <summary>
        /// Swap
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static void Swap(ref ulong left, ref ulong right)
        {
            left ^= right;
            right ^= left;
            left ^= right;
        }

        public static float ToFloat(this object obj, float defaultVal = 0f)
        {
            if (obj == null)
            {
                return defaultVal;
            }

            return obj switch
            {
                float result => float.IsNaN(result) ? defaultVal : result,
                int i => i,
                long l => l,
                string s => s.ParseFloat(),
                char c => c,
                short sh => sh,
                byte b => b,
                sbyte sb => sb,
                uint u => u,
                ulong ul => ul,
                _ => defaultVal
            };
        }

        public static long ToLong(this object obj, long defaultVal = 0)
        {
            if (obj == null)
            {
                return defaultVal;
            }

            return obj switch
            {
                long l => l,
                int i => i,
                string s => s.ParseInt64(),
                char c => c,
                short sh => sh,
                byte b => b,
                sbyte sb => sb,
                uint u => u,
                _ => defaultVal
            };
        }

        public static int ToInt(this object obj, int defaultVal = 0)
        {
            if (obj == null)
            {
                return defaultVal;
            }

            return obj switch
            {
                int i => i,
                Enum _ => (int)obj,
                string s1 => s1.ParseInt(),
                char c => c,
                short s => s,
                byte b => b,
                sbyte @sbyte => @sbyte,
                long l => (int)l,
                _ => defaultVal
            };
        }


        public static void DebugRay(Vector3 point, Vector3 direction = default, float duration = 2f, Color color = default)
        {
            Vector3 drawDirection = direction == default ? Vector3.up : direction;
            Color drawColor = color == default ? Color.blue : color;

            Debug.DrawRay(point, drawDirection, drawColor, duration);
        }

        public static void DrawArrowGizmo(Vector3 start, Vector3 end, Color color, float radius = 0.25f)
        {
            Gizmos.color = color;
            Gizmos.DrawLine(start, end);

            Gizmos.DrawRay(
                           end,
                           Quaternion.AngleAxis(45, Vector3.forward) * Vector3.Normalize(start - end) * radius
                          );

            Gizmos.DrawRay(
                           end,
                           Quaternion.AngleAxis(-45, Vector3.forward) * Vector3.Normalize(start - end) * radius
                          );
        }

        public static void DrawGizmoCross(Vector3 point, float radius, Color color)
        {
            Gizmos.color = color;

            Gizmos.DrawRay(
                           point + Vector3.up * 0.5f * radius,
                           Vector3.down * radius
                          );

            Gizmos.DrawRay(
                           point + Vector3.right * 0.5f * radius,
                           Vector3.left * radius
                          );
        }

        public static void DrawDebugCross(Vector3 point, float radius, Color color, float angleOffset = 0f)
        {
            Debug.DrawRay(
                          point + Quaternion.Euler(0, 0, angleOffset) * Vector3.up * 0.5f * radius,
                          Quaternion.Euler(0, 0, angleOffset) * Vector3.down * radius,
                          color
                         );

            Debug.DrawRay(
                          point + Quaternion.Euler(0, 0, angleOffset) * Vector3.right * 0.5f * radius,
                          Quaternion.Euler(0, 0, angleOffset) * Vector3.left * radius,
                          color
                         );
        }
    }
}