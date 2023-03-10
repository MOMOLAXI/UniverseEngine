using System;
using System.Collections.Generic;
using System.Reflection;

namespace Universe
{
    internal static class Kernel
    {
        static readonly Dictionary<Type, EngineSystem> s_Type2System = new();
        static readonly List<EngineSystem> s_SystemInstance = new();
        static readonly List<EngineSystem> s_UpdateSystems = new();
        static readonly List<EngineSystem> s_LateUpdateSystems = new();
        static readonly List<EngineSystem> s_FixedUpdateSystems = new();

        /// <summary>
        /// 注册游戏逻辑系统
        /// </summary>
        /// <param name="systems"></param>
        internal static void Register(IList<EngineSystem> systems)
        {
            if (systems == null)
            {
                Log.Error("Input System Collection is null");
                return;
            }

            for (int i = 0; i < systems.Count; ++i)
            {
                EngineSystem system = systems[i];
                Register(system);
            }
        }

        /// <summary>
        /// 注册游戏逻辑系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Register<T>() where T : EngineSystem, new()
        {
            T module = new();
            Register(module);
            return module;
        }

        /// <summary>
        /// 注册游戏逻辑系统
        /// </summary>
        /// <param name="system"></param>
        internal static void Register(EngineSystem system)
        {
            if (system == null)
            {
                return;
            }

            Type type = system.GetType();
            //类型映射
            s_Type2System[type] = system;

            //实例缓存
            s_SystemInstance.Add(system);

            //将覆盖过的Update加入列表
            MethodInfo updateMethod = type.GetMethod(nameof(system.Update), (BindingFlags)(-1));
            if (updateMethod != null && updateMethod != updateMethod.GetBaseDefinition())
            {
                s_UpdateSystems.Add(system);
            }

            //将覆盖过的LateUpdate加入列表
            MethodInfo lateUpdateMethod = type.GetMethod(nameof(system.LateUpdate), (BindingFlags)(-1));
            if (lateUpdateMethod != null && lateUpdateMethod != lateUpdateMethod.GetBaseDefinition())
            {
                s_LateUpdateSystems.Add(system);
            }

            //将覆盖过的LateUpdate加入列表
            MethodInfo fixedUpdateMethod = type.GetMethod(nameof(system.FixedUpdate), (BindingFlags)(-1));
            if (fixedUpdateMethod != null && fixedUpdateMethod != fixedUpdateMethod.GetBaseDefinition())
            {
                s_FixedUpdateSystems.Add(system);
            }
        }

        /// <summary>
        /// 获取系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static T Get<T>() where T : EngineSystem, new()
        {
            Type type = typeof(T);
            return s_Type2System.TryGetValue(type, out EngineSystem module) ? module as T : Register<T>();
        }

        internal static void Init()
        {
            s_SystemInstance.Sort((s1, s2) => s1.InitializePriority - s2.InitializePriority);
            for (int i = 0; i < s_SystemInstance.Count; i++)
            {
                EngineSystem system = s_SystemInstance[i];
                try
                {
                    //初始化系统
                    system.OnRegisterComponents();
                    system.OnComponentsInitialize();
                    system.Init();
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        internal static void Update(float dt)
        {
            for (int i = 0; i < s_UpdateSystems.Count; ++i)
            {
                EngineSystem system = s_UpdateSystems[i];
                try
                {
                    system.Update(dt);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        internal static void FixedUpdate(float dt)
        {
            for (int i = 0; i < s_FixedUpdateSystems.Count; ++i)
            {
                EngineSystem system = s_FixedUpdateSystems[i];
                try
                {
                    system.FixedUpdate(dt);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        internal static void LateUpdate(float dt)
        {
            for (int i = 0; i < s_LateUpdateSystems.Count; ++i)
            {
                EngineSystem system = s_LateUpdateSystems[i];
                try
                {
                    system.LateUpdate(dt);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        internal static void Reset()
        {
            for (int i = 0; i < s_SystemInstance.Count; ++i)
            {
                EngineSystem system = s_SystemInstance[i];
                try
                {
                    system.Reset();
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        internal static void Destroy()
        {
            for (int i = 0; i < s_SystemInstance.Count; ++i)
            {
                EngineSystem system = s_SystemInstance[i];
                try
                {
                    system.Destroy();
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        internal static void ApplicationFocus(bool hasFocus)
        {
            for (int i = 0; i < s_SystemInstance.Count; ++i)
            {
                EngineSystem system = s_SystemInstance[i];
                try
                {
                    system.ApplicationFocus(hasFocus);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        internal static void ApplicationPause(bool pauseStatus)
        {
            for (int i = 0; i < s_SystemInstance.Count; ++i)
            {
                EngineSystem system = s_SystemInstance[i];
                try
                {
                    system.ApplicationPause(pauseStatus);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        internal static void ApplicationQuit()
        {
            for (int i = 0; i < s_SystemInstance.Count; ++i)
            {
                EngineSystem system = s_SystemInstance[i];
                try
                {
                    system.ApplicationQuit();
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }
    }
}