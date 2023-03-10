using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Universe
{
    public abstract class ProviderBase
    {
        readonly List<OperationHandleBase> m_Handles = new();

        public enum EStatus
        {
            None = 0,
            CheckBundle,
            Loading,
            Checking,
            Succeed,
            Failed,
        }

        /// <summary>
        /// 资源提供者唯一标识符
        /// </summary>
        public string ProviderGuid { get; }

        /// <summary>
        /// 所属资源系统
        /// </summary>
        public AssetPackageProxy Proxy { get; }

        /// <summary>
        /// 资源信息
        /// </summary>
        public AssetInfo MainAssetInfo { get; }

        /// <summary>
        /// 获取的资源对象
        /// </summary>
        public UnityEngine.Object AssetObject { protected set; get; }

        /// <summary>
        /// 获取的资源对象集合
        /// </summary>
        public UnityEngine.Object[] AllAssetObjects { protected set; get; }

        /// <summary>
        /// 获取的场景对象
        /// </summary>
        public UnityEngine.SceneManagement.Scene SceneObject { protected set; get; }

        /// <summary>
        /// 原生文件路径
        /// </summary>
        public string RawFilePath { protected set; get; }


        /// <summary>
        /// 当前的加载状态
        /// </summary>
        public EStatus Status { protected set; get; } = EStatus.None;

        /// <summary>
        /// 最近的错误信息
        /// </summary>
        public string LastError { protected set; get; } = string.Empty;

        /// <summary>
        /// 加载进度
        /// </summary>
        public float Progress { protected set; get; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { private set; get; }

        /// <summary>
        /// 是否已经销毁
        /// </summary>
        public bool IsDestroyed { private set; get; }

        /// <summary>
        /// 是否完毕（成功或失败）
        /// </summary>
        public bool IsDone => Status is EStatus.Succeed or EStatus.Failed;

        protected bool IsWaitForAsyncComplete { private set; get; }


        protected ProviderBase(AssetPackageProxy proxy, string providerGuid, AssetInfo assetInfo)
        {
            Proxy = proxy;
            ProviderGuid = providerGuid;
            MainAssetInfo = assetInfo;
        }

        /// <summary>
        /// 轮询更新方法
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// 销毁资源对象
        /// </summary>
        public virtual void Destroy()
        {
            IsDestroyed = true;
        }

        /// <summary>
        /// 获取下载进度
        /// </summary>
        public virtual DownloadReport GetDownloadReport()
        {
            return DownloadReport.CreateDefaultReport();
        }

        /// <summary>
        /// 是否可以销毁
        /// </summary>
        public bool CanDestroy()
        {
            return IsDone && RefCount <= 0;

        }

        /// <summary>
        /// 是否为场景提供者
        /// </summary>
        public bool IsSceneProvider()
        {
            return this is BundledSceneProvider || this is DatabaseSceneProvider;
        }

        /// <summary>
        /// 创建操作句柄
        /// </summary>
        public T CreateHandle<T>() where T : OperationHandleBase
        {
            // 引用计数增加
            RefCount++;

            OperationHandleBase handle;
            if (typeof(T) == typeof(AssetOperationHandle))
            {
                handle = new AssetOperationHandle(this);
            }
            else if (typeof(T) == typeof(SceneOperationHandle))
            {
                handle = new SceneOperationHandle(this);
            }
            else if (typeof(T) == typeof(SubAssetsOperationHandle))
            {
                handle = new SubAssetsOperationHandle(this);
            }
            else if (typeof(T) == typeof(RawFileOperationHandle))
            {
                handle = new RawFileOperationHandle(this);
            }
            else
            {
                throw new System.NotImplementedException();
            }

            m_Handles.Add(handle);
            return handle as T;
        }

        /// <summary>
        /// 释放操作句柄
        /// </summary>
        public void ReleaseHandle(OperationHandleBase handle)
        {
            if (RefCount <= 0)
            {
                Log.Warning("Asset provider reference count is already zero. There may be resource leaks !");
            }

            if (m_Handles.Remove(handle) == false)
            {
                throw new("Should never get here !");
            }

            // 引用计数减少
            RefCount--;
        }

        /// <summary>
        /// 等待异步执行完毕
        /// </summary>
        public void WaitForAsyncComplete()
        {
            IsWaitForAsyncComplete = true;

            // 注意：主动轮询更新完成同步加载
            Update();

            // 验证结果
            if (IsDone)
            {
                return;
            }

            Log.Warning($"WaitForAsyncComplete failed to loading : {MainAssetInfo.AssetPath}");
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

    #region 异步编程相关

        TaskCompletionSource<object> m_TaskCompletionSource;
        protected void InvokeCompletion()
        {
            DebugEndRecording();

            // 进度百分百完成
            Progress = 1f;

            // 注意：创建临时列表是为了防止外部逻辑在回调函数内创建或者释放资源句柄。
            List<OperationHandleBase> tempers = new(m_Handles);
            foreach (OperationHandleBase hande in tempers)
            {
                if (hande.IsValid)
                {
                    hande.InvokeCallback();
                }
            }

            m_TaskCompletionSource?.TrySetResult(null);
        }

    #endregion

    #region 调试信息相关

        /// <summary>
        /// 出生的场景
        /// </summary>
        public string SpawnScene = string.Empty;

        /// <summary>
        /// 出生的时间
        /// </summary>
        public string SpawnTime = string.Empty;

        /// <summary>
        /// 加载耗时（单位：毫秒）
        /// </summary>
        public long LoadingTime { protected set; get; }

        // 加载耗时统计
        private Stopwatch m_Watch;

        [Conditional("DEBUG")]
        public void InitSpawnDebugInfo()
        {
            SpawnScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            SpawnTime = SpawnTimeToString(UnityEngine.Time.realtimeSinceStartup);
        }
        static string SpawnTimeToString(float spawnTime)
        {
            float h = UnityEngine.Mathf.FloorToInt(spawnTime / 3600f);
            float m = UnityEngine.Mathf.FloorToInt(spawnTime / 60f - h * 60f);
            float s = UnityEngine.Mathf.FloorToInt(spawnTime - m * 60f - h * 3600f);
            return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
        }

        [Conditional("DEBUG")]
        protected void DebugBeginRecording()
        {
            m_Watch ??= Stopwatch.StartNew();
        }

        [Conditional("DEBUG")]
        private void DebugEndRecording()
        {
            if (m_Watch != null)
            {
                LoadingTime = m_Watch.ElapsedMilliseconds;
                m_Watch = null;
            }
        }

    #endregion

    }
}