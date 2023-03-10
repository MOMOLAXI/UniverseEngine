using System;
using UnityEngine;

namespace Universe
{
    public abstract class GameSystem : ISystem
    {
        public string Tag { get; set; }

        public void Info(object message)
        {
            if (Log.LogLevel < ELogLevel.Info)
            {
                return;
            }

            Debug.Log($"{Tag}{message}");
        }

        public void Warning(object message)
        {
            if (Log.LogLevel < ELogLevel.Warning)
            {
                return;
            }

            Debug.LogWarning($"{Tag}{message}");
        }

        public void Error(object message)
        {
            if (Log.LogLevel < ELogLevel.Error)
            {
                return;
            }

            Debug.LogError($"{Tag}{message}");
        }

        public void Exception(Exception ex)
        {
            if (Log.LogLevel < ELogLevel.Exception)
            {
                return;
            }

            Debug.LogError($"================={Tag} Exception Start ===============");
            Debug.LogException(ex);
            Debug.LogError($"================={Tag} Exception End ===============");
        }
    }
}