using UnityEngine;

namespace Universe
{
    public class UniverseEngine : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void InitializeEngine()
        {
            if (Engine.UnityObject != null)
            {
                return;
            }

            Engine.UnityObject = new(Engine.FormatName(nameof(UniverseEngine)));
            DontDestroyOnLoad(Engine.UnityObject);
            Function.Run(Engine.Start, out float s1);
            Log.Info($"Initialize [UniverseEngine] ... using {s1} seconds");
        }
        
        void Reset()
        {
            Engine.Reset();
        }

        void Update()
        {
            Engine.Update(Time.deltaTime);
        }

        void FixedUpdate()
        {
            Engine.FixedUpdate(Time.fixedDeltaTime);
        }

        void LateUpdate()
        {
            Engine.LateUpdate(Time.deltaTime);
        }

        void OnDestroy()
        {
            Engine.Destroy();
        }

        void OnApplicationFocus(bool hasFocus)
        {
            Engine.ApplicationFocus(hasFocus);
        }

        void OnApplicationPause(bool pauseStatus)
        {
            Engine.ApplicationPause(pauseStatus);
        }

        void OnApplicationQuit()
        {
            Engine.ApplicationQuit();
        }
    }
}