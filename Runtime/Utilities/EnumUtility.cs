using System;

namespace Universe
{
    public static class EnumUtility
    {
        /// <summary>
        /// 使用频度高的地方慎用！
        /// 转换成枚举，因为当前C#版本没法限定为枚举，暂时限制为值类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool ParseEnum<T>(this string value, out T result) where T : Enum
        {
            result = default;
            try
            {
                result = (T)Enum.Parse(typeof(T), value);
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }
        }
        
        public static T NameToEnum<T>(string name)
        {
            if (Enum.IsDefined(typeof(T), name) == false)
            {
                throw new ArgumentException($"Enum {typeof(T)} is not defined name {name}");
            }
            
            return (T)Enum.Parse(typeof(T), name);
        }
    }
}