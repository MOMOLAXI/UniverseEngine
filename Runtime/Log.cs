using System;
using UnityEngine;

namespace Universe
{
    public enum ELogLevel
    {
        None,

        Exception,
        Error,
        Info,
        Warning,
        Debug,

        Max,
    }

    public static class Log
    {
        /// <summary>
        /// 日志等级
        /// 修改等级来屏蔽日志
        /// </summary>
        public static ELogLevel LogLevel => ELogLevel.Max;

        /// <summary>
        /// 日志前缀
        /// </summary>
        const string RUNTIME_LOG_TAG = "[<color=#33CC33>UniverseEngine</color>]";

        public static void Info(object message)
        {
            if (LogLevel < ELogLevel.Info)
            {
                return;
            }

            Debug.Log($"{RUNTIME_LOG_TAG}{message}");
        }

        public static void Warning(object message)
        {
            if (LogLevel < ELogLevel.Warning)
            {
                return;
            }

            Debug.LogWarning($"{RUNTIME_LOG_TAG}{message}");
        }

        public static void Error(object message)
        {
            if (LogLevel < ELogLevel.Error)
            {
                return;
            }

            Debug.LogError($"{RUNTIME_LOG_TAG}{message}");
        }

        public static void Exception(Exception ex)
        {
            if (LogLevel < ELogLevel.Exception)
            {
                return;
            }

            Debug.LogError($"================={RUNTIME_LOG_TAG} Exception Start ===============");
            Debug.LogException(ex);
            Debug.LogError($"================={RUNTIME_LOG_TAG} Exception End ===============");
        }
    }
}