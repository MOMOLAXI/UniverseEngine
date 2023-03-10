using System;

namespace Universe
{
    public static class ParamUtility
    {
        public static int QueryIntArgs(object[] args, int index, int defaultVal = 0)
        {
            object obj = args.SafeGet(index);
            return obj.ToInt(defaultVal);
        }

        public static int QueryIntArgs(string[] args, int index, int defaultVal = 0)
        {
            if (index < 0 || null == args || args.Length <= index)
            {
                return defaultVal;
            }

            return Utilities.ToInt(args[index]);
        }

        public static long QueryInt64Args(object[] args, int index, long defaultVal = 0)
        {
            if (index < 0 || null == args || args.Length <= index)
            {
                return defaultVal;
            }

            object obj = args[index];
            return obj.ToLong(defaultVal);
        }

        public static bool QueryBoolArgs(object[] args, int index, bool defaultVal = false)
        {
            if (index < 0 || null == args || args.Length <= index)
            {
                return defaultVal;
            }

            object obj = args[index];

            switch (obj)
            {
                case bool b:
                {
                    return b;
                }
                case int v:
                {
                    return v != 0;
                }
                case string s:
                {
                    return s.ParseBool();
                }
                default:
                {
                    float value = obj.ToFloat();
                    return Math.Abs(value) > float.Epsilon || defaultVal;
                }
            }
        }

        public static T QueryStructArgs<T>(object[] args, int index, T defaultVal = default(T)) where T : struct
        {
            if (index < 0 || null == args || args.Length <= index)
            {
                return defaultVal;
            }

            if (args[index] is T)
            {
                return (T)args[index];
            }

            return defaultVal;
        }

        public static bool QueryBoolArgs(string[] args, int index, bool defaultVal = false)
        {
            if (index < 0 || null == args || args.Length <= index)
            {
                return defaultVal;
            }

            return args[index].ToBool();
        }

        public static float QueryFloatArgs(object[] args, int index, float defaultVal = 0)
        {
            if (index < 0 || null == args || args.Length <= index)
            {
                return defaultVal;
            }

            object obj = args[index];
            return obj.ToFloat(defaultVal);
        }

        /// <summary>
        /// 查询对象参数
        /// </summary>
        /// <param name="args"></param>
        /// <param name="index"></param>
        /// <param name="defaultVal"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T QueryClassArgs<T>(object[] args, int index, T defaultVal = default(T)) where T : class
        {
            if (args == null)
            {
                return default;
            }

            if (!Utilities.IsValidIndex(index, args.Length))
            {
                return defaultVal;
            }

            if (args[index] is T t)
            {
                return t;
            }

            return defaultVal;
        }
    }
}