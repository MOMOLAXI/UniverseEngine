using System;
using System.Collections.Generic;

namespace Universe
{
    [Serializable]
    public class EntityClassEventContext : ICacheAble
    {
        public List<EntityClassEventContext> Children = new();

        public EntityClassEvent[] EventSet = new EntityClassEvent[(int)EntityEvent.Max];

        public EntityClassEventContext()
        {
            for (int i = 0; i < (int)EntityEvent.Max; ++i)
            {
                EventSet[i] = new();
            }
        }

        /// <summary>
        /// 添加子类
        /// </summary>
        /// <param name="child"></param>
        public void AddChildClass(EntityClassEventContext child)
        {
            if (child == null)
            {
                return;
            }

            Children.Add(child);
            child.Inherit(this);
        }

        /// <summary>
        /// 添加类事件回调
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="eventCallBack"></param>
        public void RegisterEvent(EntityEvent eventType, EntityEventCallBack eventCallBack)
        {
            if (eventCallBack == null)
            {
                Log.Error("event can not be null while add event to ClassEventContext");
                return;
            }

            int index = (int)eventType;
            if (!Utilities.IsValidIndex(index, EventSet.Length))
            {
                Log.Error($"event type is invalid : {eventType.ToString()}, while add class event to ClassEventContext");
                return;
            }

            EntityClassEvent entityClassEvent = EventSet[index];
            entityClassEvent.Add(eventCallBack);

            for (int i = 0; i < Children.Count; ++i)
            {
                EntityClassEventContext child = Children[i];
                child.RegisterEvent(eventType, eventCallBack);
            }
        }

        /// <summary>
        /// 执行类事件
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="self"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ExecuteEvent(EntityEvent eventType, EntityID self, EntityID sender, Variables args)
        {
            int index = (int)eventType;
            if (!Utilities.IsValidIndex(index, EventSet.Length))
            {
                Log.Error($"logic event type is not valid {eventType.ToString()} while execute");
                return;
            }

            EntityClassEvent entityClassEvent = EventSet[index];
            entityClassEvent.Execute(self, sender, args);
        }

        /// <summary>
        /// 继承父类所有逻辑事件
        /// </summary>
        /// <param name="parent"></param>
        public void Inherit(EntityClassEventContext parent)
        {
            if (parent == null)
            {
                return;
            }

            for (int i = 0; i < parent.EventSet.Length; i++)
            {
                EntityClassEvent parentEvent = parent.EventSet[i];
                EntityClassEvent thisEvent = EventSet[i];
                thisEvent.Add(parentEvent);
            }
        }

        public bool IsInCache { get; set; }

        public void Reset()
        {
            foreach (EntityClassEvent classEvent in EventSet)
            {
                classEvent.Reset();
            }
        }
    }
}