using System;

namespace Universe
{
    public abstract class EntityComponent : ICacheAble
    {
        bool m_IsActive = true;

        //高频度调用属性，不使用属性方法
        internal bool IsStarted;
        internal bool IsDestroyed;

        public EntityID Owner { get; private set; }

        public bool IsActive
        {
            get => m_IsActive;
            set
            {
                if (m_IsActive == value)
                {
                    return;
                }

                m_IsActive = value;

                try
                {
                    if (m_IsActive)
                    {
                        OnEnable();
                    }
                    else
                    {
                        OnDisable();
                    }
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
        }

        internal void Awake()
        {
            OnAwake();
        }

        internal void Start()
        {
            OnStart();
        }

        internal void Update()
        {
            OnUpdate();
        }

        internal void LateUpdate()
        {
            OnLateUpdate();
        }

        internal void FixedUpdate()
        {
            OnFixedUpdate();
        }

        protected virtual void OnAwake() { }
        protected virtual void OnStart() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLateUpdate() { }
        protected virtual void OnFixedUpdate() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnDestroy() { }

        public void DestroySelf()
        {
            if (IsDestroyed)
            {
                return;
            }

            IsActive = false;
            IsDestroyed = true;

            try
            {
                OnDestroy();
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        internal void Init(EntityID id)
        {
            Owner = id;
            m_IsActive = true;
            IsStarted = false;
            IsDestroyed = false;
        }

        public bool IsInCache { get; set; }
        public virtual void Reset() { }
    }
}