using System;
using System.Collections.Generic;
using System.Reflection;

namespace Universe
{
    internal class GameplaySystem : EngineSystem
    {
        readonly Dictionary<Type, GameSystem> m_Type2System = new();
        readonly List<GameSystem> m_Systems = new();
        readonly List<GameSystem> m_UpdateSystems = new();
        readonly List<GameSystem> m_LateUpdateSystems = new();
        readonly List<GameSystem> m_FixedUpdateSystems = new();

        /// <summary>
        /// 注册游戏逻辑系统
        /// </summary>
        /// <param name="systems"></param>
        public void Register(List<GameSystem> systems)
        {
            if (systems == null)
            {
                Log.Error("Input System Collection is null");
                return;
            }

            for (int i = 0; i < systems.Count; ++i)
            {
                GameSystem system = systems[i];
                Register(system);
            }
        }

        /// <summary>
        /// 注册游戏逻辑系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void Register<T>() where T : GameSystem, new()
        {
            Register(new T());
        }

        /// <summary>
        /// 注册游戏逻辑系统
        /// </summary>
        /// <param name="system"></param>
        public void Register(GameSystem system)
        {
            if (system == null)
            {
                return;
            }

            Type type = system.GetType();
            //类型映射
            m_Type2System[type] = system;

            //实例缓存
            m_Systems.Add(system);

            //将覆盖过的Update加入列表
            MethodInfo updateMethod = type.GetMethod(nameof(system.Update), (BindingFlags)(-1));
            if (updateMethod != null && updateMethod != updateMethod.GetBaseDefinition())
            {
                m_UpdateSystems.Add(system);
            }

            //将覆盖过的LateUpdate加入列表
            MethodInfo lateUpdateMethod = type.GetMethod(nameof(system.LateUpdate), (BindingFlags)(-1));
            if (lateUpdateMethod != null && lateUpdateMethod != lateUpdateMethod.GetBaseDefinition())
            {
                m_LateUpdateSystems.Add(system);
            }

            //将覆盖过的LateUpdate加入列表
            MethodInfo fixedUpdateMethod = type.GetMethod(nameof(system.FixedUpdate), (BindingFlags)(-1));
            if (fixedUpdateMethod != null && fixedUpdateMethod != fixedUpdateMethod.GetBaseDefinition())
            {
                m_FixedUpdateSystems.Add(system);
            }

            Function.Run(() =>
            {
                //初始化系统
                system.Tag = $"[<color=#{Guid.NewGuid().ToString()[..6]}>{type.Name}</color>]";
                system.OnRegisterComponents();
                system.OnComponentsInitialize();
                system.Info("Start Initialize");
                system.Init();
            }, out float seconds);
            system.Info("Initialize done");
        }

        /// <summary>
        /// 获取系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>() where T : GameSystem, new()
        {
            Type type = typeof(T);
            return m_Type2System.TryGetValue(type, out GameSystem module) ? module as T : default;
        }

        public override void Update(float dt)
        {
            for (int i = 0; i < m_UpdateSystems.Count; ++i)
            {
                GameSystem system = m_UpdateSystems[i];
                try
                {
                    system.OnCompoenntUpdate(dt);
                    system.Update(dt);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        public override void FixedUpdate(float dt)
        {
            for (int i = 0; i < m_FixedUpdateSystems.Count; ++i)
            {
                GameSystem system = m_FixedUpdateSystems[i];
                try
                {
                    system.OnComponentFixedUpdate(dt);
                    system.FixedUpdate(dt);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        public override void LateUpdate(float dt)
        {
            for (int i = 0; i < m_LateUpdateSystems.Count; ++i)
            {
                GameSystem system = m_LateUpdateSystems[i];
                try
                {
                    system.OnComponentLateUpdate(dt);
                    system.LateUpdate(dt);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        public override void Reset()
        {
            for (int i = 0; i < m_Systems.Count; ++i)
            {
                GameSystem system = m_Systems[i];
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

        public override void Destroy()
        {
            for (int i = 0; i < m_Systems.Count; ++i)
            {
                GameSystem system = m_Systems[i];
                try
                {
                    system.Destroy();
                    system.OnComponentDestroy();
                    // Log.Info($"Destroy Game System {system.GetType().Name}");
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        public override void ApplicationFocus(bool hasFocus)
        {
            for (int i = 0; i < m_Systems.Count; ++i)
            {
                GameSystem system = m_Systems[i];
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

        public override void ApplicationPause(bool pauseStatus)
        {
            for (int i = 0; i < m_Systems.Count; ++i)
            {
                GameSystem system = m_Systems[i];
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

        public override void ApplicationQuit()
        {
            for (int i = 0; i < m_Systems.Count; ++i)
            {
                GameSystem system = m_Systems[i];
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