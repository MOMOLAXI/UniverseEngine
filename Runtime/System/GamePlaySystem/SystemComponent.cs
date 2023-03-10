namespace Universe
{
    /// <summary>
    /// 系统组件, 声明周期被托管在GameSystem
    /// </summary>
    public abstract class SystemComponent
    {
        public virtual int InitializePriority { get; } = 0;

        public virtual void OnInit() { }
        public virtual void OnUpdate(float dt) { }
        public virtual void OnFixedUpdate(float dt) { }
        public virtual void OnLateUpdate(float dt) { }
        public virtual void OnDestroy() { }
    }
}