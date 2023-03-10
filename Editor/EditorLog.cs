using System;
using UnityEngine;

namespace Universe
{
    public static class EditorLog
    {
        /// <summary>
        /// 日志前缀
        /// </summary>
        static readonly string s_Editor_LOG_Tag = $"[<color=#FFCC99>{nameof(UniverseEditor)}</color>]";

        public static bool EnableLog;
        
        public static void Info(object message)
        {
            Debug.Log($"{s_Editor_LOG_Tag}{message}");
        }

        public static void Warning(object message)
        {

            Debug.LogWarning($"{s_Editor_LOG_Tag}{message}");
        }

        public static void Error(object message)
        {
            Debug.LogError($"{s_Editor_LOG_Tag}{message}");
        }

        public static void Exception(Exception ex)
        {
            Debug.LogError($"================={s_Editor_LOG_Tag} Exception Start ===============");
            Debug.LogException(ex);
            Debug.LogError($"================={s_Editor_LOG_Tag} Exception End ===============");
        }
    }
}