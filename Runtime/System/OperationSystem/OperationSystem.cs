using System.Collections.Generic;
using System.Diagnostics;

namespace Universe
{
    internal class OperationSystem : EngineSystem
    {
        readonly List<AsyncOperationBase> m_Operations = new(100);
        readonly Queue<AsyncOperationBase> m_EnterQueue = new();
        readonly Queue<AsyncOperationBase> m_ExitQueue = new();

        // 计时器相关
        long m_FrameTime;
        Stopwatch m_Watcher;

        /// <summary>
        /// 异步操作的最小时间片段
        /// </summary>
        public long MaxSliceTimeMs { set; get; } = 30;

        /// <summary>
        /// 处理器是否繁忙
        /// </summary>
        public bool IsBusy => m_Watcher.ElapsedMilliseconds - m_FrameTime >= MaxSliceTimeMs;

        public override void Init()
        {
            m_Watcher = Stopwatch.StartNew();
        }

        /// <summary>
        /// 更新异步操作系统
        /// </summary>
        public override void Update(float dt)
        {
            m_FrameTime = m_Watcher.ElapsedMilliseconds;

            // 添加新的异步操作
            while (m_EnterQueue.Count > 0)
            {
                AsyncOperationBase operation = m_EnterQueue.Dequeue();
                m_Operations.Add(operation);
            }

            // 更新所有的异步操作
            if (IsBusy)
            {
                return;
            }

            for (int i = 0; i < m_Operations.Count; i++)
            {
                AsyncOperationBase operation = m_Operations[i];
                operation.Update();
                if (operation.IsDone)
                {
                    m_ExitQueue.Enqueue(operation);
                    operation.Finish();
                }
            }

            // 移除已经完成的异步操作
            while (m_ExitQueue.Count > 0)
            {
                AsyncOperationBase operation = m_ExitQueue.Dequeue();
                m_Operations.Remove(operation);
            }
        }

        /// <summary>
        /// 销毁异步操作系统
        /// </summary>
        public override void Destroy()
        {
            m_Operations.Clear();
            m_EnterQueue.Clear();
            m_ExitQueue.Clear();
            m_Watcher = null;
            m_FrameTime = 0;
            MaxSliceTimeMs = long.MaxValue;
        }

        /// <summary>
        /// 开始处理异步操作类
        /// </summary>
        public void StartOperation(AsyncOperationBase operation)
        {
            m_EnterQueue.Enqueue(operation);
            operation.Start();
        }
    }
}