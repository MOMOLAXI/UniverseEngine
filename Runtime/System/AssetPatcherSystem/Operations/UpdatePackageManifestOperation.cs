namespace Universe
{
    /// <summary>
    /// 向远端请求并更新补丁清单
    /// </summary>
    public abstract class UpdatePackageManifestOperation : AsyncOperationBase
    {
    }

    /// <summary>
    /// 编辑器下模拟运行的更新清单操作
    /// </summary>
    sealed internal class EditorPlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
    {
        public EditorPlayModeUpdatePackageManifestOperation()
        {
        }
        internal override void Start()
        {
            Status = EOperationStatus.Succeed;
        }
        internal override void Update()
        {
        }
    }

    /// <summary>
    /// 离线模式的更新清单操作
    /// </summary>
    sealed internal class OfflinePlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
    {
        public OfflinePlayModeUpdatePackageManifestOperation()
        {
        }
        internal override void Start()
        {
            Status = EOperationStatus.Succeed;
        }
        internal override void Update()
        {
        }
    }

    /// <summary>
    /// 联机模式的更新清单操作
    /// 注意：优先加载沙盒里缓存的清单文件，如果缓存没找到就下载远端清单文件，并保存到本地。
    /// </summary>
    sealed internal class HostPlayModeUpdatePackageManifestOperation : UpdatePackageManifestOperation
    {
        private enum ESteps
        {
            None,
            CheckActiveManifest,
            TryLoadCacheManifest,
            DownloadManifest,
            LoadCacheManifest,
            CheckDeserializeManifest,
            Done,
        }

        private readonly HostPlayModeImpl m_Impl;
        private readonly string m_PackageName;
        private readonly string m_PackageVersion;
        private readonly int m_Timeout;
        private LoadCacheManifestOperation m_TryLoadCacheManifestOp;
        private LoadCacheManifestOperation m_LoadCacheManifestOp;
        private DownloadManifestOperation m_DownloadManifestOp;
        private ESteps m_Steps = ESteps.None;


        internal HostPlayModeUpdatePackageManifestOperation(HostPlayModeImpl impl, string packageName, string packageVersion, int timeout)
        {
            m_Impl = impl;
            m_PackageName = packageName;
            m_PackageVersion = packageVersion;
            m_Timeout = timeout;
        }
        internal override void Start()
        {
            m_Steps = ESteps.CheckActiveManifest;
        }
        internal override void Update()
        {
            if (m_Steps is ESteps.None or ESteps.Done)
            {
                return;
            }

            if (m_Steps == ESteps.CheckActiveManifest)
            {
                // 检测当前激活的清单对象	
                if (m_Impl.ActiveManifest != null && m_Impl.ActiveManifest.PackageVersion == m_PackageVersion)
                {
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    m_Steps = ESteps.TryLoadCacheManifest;
                }
            }

            if (m_Steps == ESteps.TryLoadCacheManifest)
            {
                if (m_TryLoadCacheManifestOp == null)
                {
                    m_TryLoadCacheManifestOp = new(m_PackageName, m_PackageVersion);
                    Engine.StartAsyncOperation(m_TryLoadCacheManifestOp);
                }

                if (m_TryLoadCacheManifestOp.IsDone == false)
                    return;

                if (m_TryLoadCacheManifestOp.Status == EOperationStatus.Succeed)
                {
                    m_Impl.ActiveManifest = m_TryLoadCacheManifestOp.Manifest;
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    m_Steps = ESteps.DownloadManifest;
                }
            }

            if (m_Steps == ESteps.DownloadManifest)
            {
                if (m_DownloadManifestOp == null)
                {
                    m_DownloadManifestOp = new(m_Impl, m_PackageName, m_PackageVersion, m_Timeout);
                    Engine.StartAsyncOperation(m_DownloadManifestOp);
                }

                if (m_DownloadManifestOp.IsDone == false)
                    return;

                if (m_DownloadManifestOp.Status == EOperationStatus.Succeed)
                {
                    m_Steps = ESteps.LoadCacheManifest;
                }
                else
                {
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = m_DownloadManifestOp.Error;
                }
            }

            if (m_Steps == ESteps.LoadCacheManifest)
            {
                if (m_LoadCacheManifestOp == null)
                {
                    m_LoadCacheManifestOp = new(m_PackageName, m_PackageVersion);
                    Engine.StartAsyncOperation(m_LoadCacheManifestOp);
                }

                if (m_LoadCacheManifestOp.IsDone == false)
                    return;

                if (m_LoadCacheManifestOp.Status == EOperationStatus.Succeed)
                {
                    m_Impl.ActiveManifest = m_LoadCacheManifestOp.Manifest;
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = m_LoadCacheManifestOp.Error;
                }
            }
        }
    }
}