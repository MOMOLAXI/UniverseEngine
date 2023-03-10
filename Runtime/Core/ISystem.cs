using System;
using System.Collections.Generic;

namespace Universe
{
    public abstract class ISystem
    {
        readonly Dictionary<Type, SystemComponent> m_Components = new();
        readonly List<SystemComponent> m_ComponentList = new();

        public virtual int InitializePriority => 0;
        public virtual void OnRegisterComponents() { }
        public virtual void Init() { }
        public virtual void Update(float dt) { }
        public virtual void FixedUpdate(float dt) { }
        public virtual void LateUpdate(float dt) { }
        public virtual void Reset() { }
        public virtual void Destroy() { }
        public virtual void ApplicationFocus(bool hasFocus) { }
        public virtual void ApplicationPause(bool pauseStatus) { }
        public virtual void ApplicationQuit() { }
        
        public void OnComponentsInitialize()
        {
            m_ComponentList.Sort((c1, c2) => c1.InitializePriority - c2.InitializePriority);
            for (int i = 0; i < m_ComponentList.Count; i++)
            {
                SystemComponent component = m_ComponentList[i];
                component.OnInit();
            }
        }

        public void OnCompoenntUpdate(float dt)
        {
            for (int i = 0; i < m_ComponentList.Count; i++)
            {
                SystemComponent component = m_ComponentList[i];
                component.OnUpdate(dt);
            }
        }

        public void OnComponentFixedUpdate(float dt)
        {
            for (int i = 0; i < m_ComponentList.Count; i++)
            {
                SystemComponent component = m_ComponentList[i];
                component.OnFixedUpdate(dt);
            }
        }

        public void OnComponentLateUpdate(float dt)
        {
            for (int i = 0; i < m_ComponentList.Count; i++)
            {
                SystemComponent component = m_ComponentList[i];
                component.OnLateUpdate(dt);
            }
        }

        public void OnComponentDestroy()
        {
            for (int i = 0; i < m_ComponentList.Count; i++)
            {
                SystemComponent component = m_ComponentList[i];
                component.OnDestroy();
            }
        }

        protected T RegisterComponent<T>() where T : SystemComponent, new()
        {
            if (m_Components.ContainsKey(typeof(T)))
            {
                return null;
            }

            ComponentGetter<T>.Component = new();
            m_Components[typeof(T)] = ComponentGetter<T>.Component;
            m_ComponentList.Add(ComponentGetter<T>.Component);
            return ComponentGetter<T>.Component;
        }

        public static T GetComponent<T>() where T : SystemComponent
        {
            return ComponentGetter<T>.Component;
        }

        private static class ComponentGetter<TComponent> where TComponent : SystemComponent
        {
            public static TComponent Component;
        }
    }
}