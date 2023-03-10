namespace Universe
{
    public enum EntityEvent
    {
        /// <summary>
        /// 创建
        /// </summary>
        OnCreate,
        
        /// <summary>
        /// 销毁
        /// </summary>
        OnDestroy,
        
        /// <summary>
        /// 添加子对象
        /// </summary>
        OnAddChild,
        
        /// <summary>
        /// 添加子对象
        /// </summary>
        OnAddChildren,
        
        /// <summary>
        /// 进入视野
        /// </summary>
        OnEntryOther,
        
        /// <summary>
        /// 离开视野
        /// </summary>
        OnLeaveOther,
        
        /// <summary>
        /// 移除子对象
        /// </summary>
        OnRemoveChild,
        
        /// <summary>
        /// 移除子对象
        /// </summary>
        OnRemoveChildren,
        
        /// <summary>
        /// 容器数量变化
        /// </summary>
        OnCapacityChange,
        Max,
    }
}