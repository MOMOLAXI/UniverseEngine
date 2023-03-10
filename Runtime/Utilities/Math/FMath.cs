using System;
using System.Collections.Generic;
using UnityEngine;

namespace Universe
{
    public static class FMath
    {
        static readonly Dictionary<int, int> s_Temp = new();

        /// <summary>
        /// 获取两数之和等于target的下标列表
        /// </summary>
        /// <param name="target"></param>
        /// <param name="nums"></param>
        /// <param name="result"></param>
        public static void TwoSum(int target, List<int> nums, List<int> result)
        {
            if (nums == null || result == null)
            {
                return;
            }

            result.Clear();

            for (int i = 0; i < nums.Count; i++)
            {
                int num = nums[i];

                if (s_Temp.TryGetValue(target - num, out int index))
                {
                    result.Add(index);
                    result.Add(i);
                    break;
                }

                s_Temp[num] = i;
            }
        }

        public static float Normalize(float target, float left, float right)
        {
            float range = Mathf.Abs(left - right);
            if (range <= 0)
            {
                return 0;
            }

            float t = Mathf.Abs(target);
            float value = t - left;
            if (value <= 0)
            {
                return 0;
            }

            return value / range;
        }

        public static Vector3 Add(this Vector3 a, Vector3 b, bool ignoreZ = true)
        {
            a.x += b.x;
            a.y += b.y;

            if (!ignoreZ)
            {
                a.z += b.z;
            }

            return a;
        }

        public static Vector3 Substract(this Vector3 a, Vector3 b, bool ignoreZ = true)
        {
            a.x -= b.x;
            a.y -= b.y;

            if (!ignoreZ)
            {
                a.z -= b.z;
            }

            return a;
        }

        public static Vector3 Multiply(this Vector3 a, float value)
        {
            a.x *= value;
            a.y *= value;
            a.z *= value;
            return a;
        }

        public static Vector3 Multiply(Vector3 a, float floatValueA, float floatValueB)
        {
            a.x *= floatValueA * floatValueB;
            a.y *= floatValueA * floatValueB;
            a.z *= floatValueA * floatValueB;
            return a;
        }

        public static void AddMagnitude(ref Vector3 vector, float magnitude)
        {
            if (vector == Vector3.zero)
            {
                return;
            }

            float vectorMagnitude = Vector3.Magnitude(vector);
            Vector3 vectorDirection = vector / vectorMagnitude;
            vector += vectorDirection * magnitude;
        }

        public static void ChangeMagnitude(ref Vector3 vector, float magnitude)
        {
            if (vector == Vector3.zero)
            {
                return;
            }

            Vector3 vectorDirection = Vector3.Normalize(vector);
            vector = vectorDirection * magnitude;
        }

        public static void ChangeDirection(ref Vector3 vector, Vector3 direction)
        {
            if (vector == Vector3.zero)
            {
                return;
            }

            float vectorMagnitude = Vector3.Magnitude(vector);
            vector = direction * vectorMagnitude;
        }

        public static void ChangeDirectionOntoPlane(ref Vector3 vector, Vector3 planeNormal)
        {
            if (vector == Vector3.zero)
            {
                return;
            }

            Vector3 direction = Vector3.Normalize(Vector3.ProjectOnPlane(vector, planeNormal));
            float vectorMagnitude = Vector3.Magnitude(vector);
            vector = direction * vectorMagnitude;
        }

        public static void GetMagnitudeAndDirection(this Vector3 vector, out Vector3 direction, out float magnitude)
        {
            magnitude = Vector3.Magnitude(vector);
            direction = Vector3.Normalize(vector);
        }

        /// <summary>
        /// 将输入向量投影到给定平面的切线上(由其法线定义)。
        /// </summary>
        public static Vector3 ProjectOnTangent(Vector3 inputVector, Vector3 planeNormal, Vector3 up)
        {
            Vector3 inputVectorDirection = Vector3.Normalize(inputVector);

            if (inputVectorDirection == -up)
            {
                inputVector += planeNormal * 0.01f;
            }
            else if (inputVectorDirection == up)
            {
                return Vector3.zero;
            }

            Vector3 rotationAxis = GetPerpendicularDirection(inputVector, up);
            Vector3 tangent = GetPerpendicularDirection(planeNormal, rotationAxis);
            return Multiply(tangent, Vector3.Magnitude(inputVector));
        }

        /// <summary>
        /// 将一个输入向量投影到平面A和平面B的标准正交方向上。
        /// </summary>
        public static Vector3 DeflectVector(Vector3 inputVector, Vector3 planeA, Vector3 planeB, bool maintainMagnitude = false)
        {
            Vector3 direction = GetPerpendicularDirection(planeA, planeB);

            if (maintainMagnitude)
            {
                return direction * inputVector.magnitude;
            }

            return Vector3.Project(inputVector, direction);
        }

        public static Vector3 GetPerpendicularDirection(Vector3 vectorA, Vector3 vectorB)
        {
            return Vector3.Normalize(Vector3.Cross(vectorA, vectorB));
        }

        public static float GetTriangleValue(float center, float height, float width, float independentVariable, float minIndependentVariableLimit = Mathf.NegativeInfinity, float maxIndependentVariableLimit = Mathf.Infinity)
        {
            float minValue = center - width / 2f;
            float maxValue = center + width / 2f;

            if (independentVariable < minValue || independentVariable > maxValue)
            {
                return 0f;
            }

            if (independentVariable < center)
            {
                return height * (independentVariable - minValue) / (center - minValue);
            }

            return -height * (independentVariable - center) / (maxValue - center) + height;
        }


        /// <summary>
        /// 使一个值大于或等于零(默认值)。
        /// </summary>
        public static void SetPositive<T>(ref T value) where T : IComparable<T>
        {
            SetMin(ref value, default);
        }

        /// <summary>
        /// 使值小于或等于零(默认值)。
        /// </summary>
        public static void SetNegative<T>(ref T value) where T : IComparable<T>
        {
            SetMax(ref value, default);
        }

        /// <summary>
        /// 使值大于或等于最小值。
        /// </summary>
        public static void SetMin<T>(ref T value, T minValue) where T : IComparable<T>
        {
            bool isLess = value.CompareTo(minValue) < 0;
            if (isLess)
            {
                value = minValue;
            }
        }

        /// <summary>
        /// 使值小于或等于最大值。
        /// </summary>
        public static void SetMax<T>(ref T value, T maxValue) where T : IComparable<T>
        {
            bool isGreater = value.CompareTo(maxValue) > 0;
            if (isGreater)
            {
                value = maxValue;
            }
        }

        /// <summary>
        /// 将值范围从最小值限制为最大值(类似于Mathf.Clamp)。
        /// </summary>
        public static void SetRange<T>(ref T value, T minValue, T maxValue) where T : IComparable<T>
        {
            SetMin(ref value, minValue);
            SetMax(ref value, maxValue);
        }


        /// <summary>
        /// 如果目标值在a和b之间(都是排他的)，则返回true。
        /// 包含限制值，将"inclusive"参数设置为true。
        /// </summary>
        public static bool IsBetween(float target, float a, float b, bool inclusive = false)
        {
            if (b > a)
            {
                return (inclusive ? target >= a : target > a) && (inclusive ? target <= b : target < b);
            }

            return (inclusive ? target >= b : target > b) && (inclusive ? target <= a : target < a);
        }

        /// <summary>
        /// 如果目标值在a和b之间(都是排他的)，则返回true。
        /// 包含限制值，将"inclusive"参数设置为true。
        /// </summary>
        public static bool IsBetween(int target, int a, int b, bool inclusive = false)
        {
            if (b > a)
            {
                return (inclusive ? target >= a : target > a) && (inclusive ? target <= b : target < b);
            }

            return (inclusive ? target >= b : target > b) && (inclusive ? target <= a : target < a);
        }


        public static bool IsCloseTo(Vector3 input, Vector3 target, float tolerance)
        {
            return Vector3.Distance(input, target) <= tolerance;
        }

        public static bool IsCloseTo(float input, float target, float tolerance)
        {
            return Mathf.Abs(target - input) <= tolerance;
        }

        public static Vector3 TransformVectorUnscaled(this Transform transform, Vector3 vector)
        {
            return transform.rotation * vector;
        }

        public static Vector3 InverseTransformVectorUnscaled(this Transform transform, Vector3 vector)
        {
            return Quaternion.Inverse(transform.rotation) * vector;
        }

        public static Vector3 RotatePointAround(Vector3 point, Vector3 center, float angle, Vector3 axis)
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, axis);
            Vector3 pointToCenter = center - point;
            Vector3 rotatedPointToCenter = rotation * pointToCenter;
            return center - rotatedPointToCenter;
        }

        public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
        {
            float angle = Vector3.Angle(from, to);
            Vector3 cross = Vector3.Cross(from, to);
            cross.Normalize();
            float sign = cross == axis ? 1f : -1f;
            return sign * angle;
        }

        public static bool IsInCloseRange(int start, int end, int value)
        {
            return value >= start && value <= end;
        }

        public static bool IsInCloseRange(uint start, uint end, uint value)
        {
            return value >= start && value <= end;
        }

        public static bool IsInOpenRange(int start, int end, int value)
        {
            return value > start && value < end;
        }

        public static bool IsInOpenRange(uint start, uint end, uint value)
        {
            return value > start && value < end;
        }

        public static int GetNearestPowValue(int target)
        {
            if (target >= int.MaxValue)
            {
                return int.MaxValue;
            }

            int result = 1;
            while (result < target)
            {
                result *= 2;
            }

            return result;
        }
    }
}