using System;
using UnityEngine;

namespace Universe
{
    /// <summary>
    /// 工作节点基础类
    /// </summary>
    public abstract class WorkNode : ICacheAble
    {
        string m_WorkerName;
        float m_StartTime;
        
        /// <summary>
        /// 工作名称
        /// </summary>
        public string WorkerName
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                {
                    return m_WorkerName;
                }

                return Name;
            }
            set => m_WorkerName = value;
        }

        /// <summary>
        /// 是否工作结束
        /// </summary>
        public abstract bool IsDone { get; }

        public virtual string Name { get; }


        /// <summary>
        /// 启动
        /// </summary>
        protected virtual void OnStart() { }

        /// <summary>
        /// 结束
        /// </summary>
        protected virtual void OnEnd() { }

        /// <summary>
        /// Start Work
        /// </summary>
        public void Start()
        {
            m_StartTime = Time.time;
            Log.Info($"Sequencer [{WorkerName}] Start...");
            OnStart();
        }

        /// <summary>
        /// End Work
        /// </summary>
        public void End()
        {
            OnEnd();
            float costTime = Time.time - m_StartTime;
            Log.Info($"Sequencer [{WorkerName}] End... " +
                     $" Cost : {costTime}s");
        }

        protected virtual void ResetToCache() { }

        /// <summary>
        /// Cache Flag
        /// </summary>
        public bool IsInCache
        {
            get;
            set;
        }

        /// <summary>
        /// Reset before Release
        /// </summary>
        public void Reset()
        {
            ResetToCache();
        }
    }

}