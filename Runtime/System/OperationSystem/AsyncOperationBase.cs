using System;
using System.Collections;
using System.Threading.Tasks;

namespace Universe
{
    public abstract class AsyncOperationBase : IEnumerator
    {
        TaskCompletionSource<object> m_TaskCompletionSource;

        // 用户请求的回调
        Action<AsyncOperationBase> m_Callback;

        /// <summary>
        /// 状态
        /// </summary>
        public EOperationStatus Status { get; protected set; } = EOperationStatus.None;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; protected set; }

        /// <summary>
        /// 处理进度
        /// </summary>
        public float Progress { get; protected set; }

        /// <summary>
        /// 是否已经完成
        /// </summary>
        public bool IsDone => Status is EOperationStatus.Failed or EOperationStatus.Succeed;

        /// <summary>
        /// 完成事件
        /// </summary>
        public event Action<AsyncOperationBase> Completed
        {
            add
            {
                if (IsDone)
                {
                    value.Invoke(this);
                }
                else
                {
                    m_Callback += value;
                }
            }
            remove => m_Callback -= value;
        }

        /// <summary>
        /// 异步操作任务
        /// </summary>
        public Task Task
        {
            get
            {
                if (m_TaskCompletionSource == null)
                {
                    m_TaskCompletionSource = new();
                    if (IsDone)
                    {
                        m_TaskCompletionSource.SetResult(null);
                    }
                }
                return m_TaskCompletionSource.Task;
            }
        }

        internal abstract void Start();
        internal abstract void Update();
        internal void Finish()
        {
            Progress = 1f;
            m_Callback?.Invoke(this);
            m_TaskCompletionSource?.TrySetResult(null);
        }

        /// <summary>
        /// 清空完成回调
        /// </summary>
        protected void ClearCompletedCallback()
        {
            m_Callback = null;
        }

        bool IEnumerator.MoveNext()
        {
            return !IsDone;
        }

        void IEnumerator.Reset() { }
        object IEnumerator.Current => null;
    }
}