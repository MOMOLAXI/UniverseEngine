using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Universe
{
    /// <summary>
    /// 同步其它线程里的回调到主线程里
    /// 注意：Unity3D中需要设置Scripting Runtime Version为.NET4.6
    /// </summary>
    sealed internal class ThreadSyncContext : SynchronizationContext
    {
        private readonly ConcurrentQueue<Action> m_ConcurrentQueue = new();

        /// <summary>
        /// 更新同步队列
        /// </summary>
        public void Update()
        {
            while (true)
            {
                if (m_ConcurrentQueue.TryDequeue(out Action action) == false)
                {
                    return;
                }

                action.Invoke();
            }
        }

        /// <summary>
        /// 向同步队列里投递一个回调方法
        /// </summary>
        public override void Post(SendOrPostCallback callback, object state)
        {
            void Action()
            {
                callback(state);
            }

            m_ConcurrentQueue.Enqueue(Action);
        }
    }
}