using System.Collections;

namespace Universe
{
    public abstract class OperationHandleBase : IEnumerator
    {
        private readonly AssetInfo m_AssetInfo;
        internal ProviderBase Provider { private set; get; }

        internal OperationHandleBase(ProviderBase provider)
        {
            Provider = provider;
            m_AssetInfo = provider.MainAssetInfo;
        }

        internal abstract void InvokeCallback();

        /// <summary>
        /// 获取资源信息
        /// </summary>
        public AssetInfo GetAssetInfo()
        {
            return m_AssetInfo;
        }

        /// <summary>
        /// 获取下载报告
        /// </summary>
        public DownloadReport GetDownloadReport()
        {
            return IsValidWithWarning ? Provider.GetDownloadReport() : DownloadReport.CreateDefaultReport();
        }

        /// <summary>
        /// 当前状态
        /// </summary>
        public EOperationStatus Status
        {
            get
            {
                if (IsValidWithWarning == false)
                {
                    return EOperationStatus.None;
                }

                return Provider.Status switch
                {
                    ProviderBase.EStatus.Failed => EOperationStatus.Failed,
                    ProviderBase.EStatus.Succeed => EOperationStatus.Succeed,
                    _ => EOperationStatus.None
                };
            }
        }

        /// <summary>
        /// 最近的错误信息
        /// </summary>
        public string LastError => IsValidWithWarning ? Provider.LastError : string.Empty;

        /// <summary>
        /// 加载进度
        /// </summary>
        public float Progress
        {
            get
            {
                if (IsValidWithWarning == false)
                {
                    return 0;
                }

                return Provider.Progress;
            }
        }

        /// <summary>
        /// 是否加载完毕
        /// </summary>
        public bool IsDone => IsValidWithWarning && Provider.IsDone;

        /// <summary>
        /// 句柄是否有效
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (Provider is { IsDestroyed: false })
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 句柄是否有效
        /// </summary>
        internal bool IsValidWithWarning
        {
            get
            {
                if (Provider is { IsDestroyed: false })
                {
                    return true;
                }
                
                if (Provider == null)
                {
                    Log.Warning($"Operation handle is released : {m_AssetInfo.AssetPath}");
                }
                else
                {
                    if (Provider.IsDestroyed)
                    {
                        Log.Warning($"Provider is destroyed : {m_AssetInfo.AssetPath}");
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 释放句柄
        /// </summary>
        internal void ReleaseInternal()
        {
            if (!IsValidWithWarning)
            {
                return;
            }
            Provider.ReleaseHandle(this);
            Provider = null;
        }

    #region 异步操作相关

        /// <summary>
        /// 异步操作任务
        /// </summary>
        public System.Threading.Tasks.Task Task => Provider.Task;

        // 协程相关
        bool IEnumerator.MoveNext()
        {
            return !IsDone;
        }
        void IEnumerator.Reset()
        {
        }
        object IEnumerator.Current => Provider;

    #endregion

    }
}