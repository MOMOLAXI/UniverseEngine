namespace Universe
{
    /// <summary>
    /// 请求远端包裹的最新版本
    /// </summary>
    public abstract class UpdatePackageVersionOperation : AsyncOperationBase
    {
        /// <summary>
        /// 当前最新的包裹版本
        /// </summary>
        public string PackageVersion { protected set; get; }
    }

    /// <summary>
    /// 编辑器下模拟模式的请求远端包裹的最新版本
    /// </summary>
    sealed internal class EditorPlayModeUpdatePackageVersionOperation : UpdatePackageVersionOperation
    {
        internal override void Start()
        {
            Status = EOperationStatus.Succeed;
        }
        internal override void Update()
        {
        }
    }

    /// <summary>
    /// 离线模式的请求远端包裹的最新版本
    /// </summary>
    sealed internal class OfflinePlayModeUpdatePackageVersionOperation : UpdatePackageVersionOperation
    {
        internal override void Start()
        {
            Status = EOperationStatus.Succeed;
        }
        internal override void Update()
        {
        }
    }

    /// <summary>
    /// 联机模式的请求远端包裹的最新版本
    /// </summary>
    sealed internal class HostPlayModeUpdatePackageVersionOperation : UpdatePackageVersionOperation
    {
        enum ESteps
        {
            None,
            QueryRemotePackageVersion,
            Done,
        }

        readonly HostPlayModeImpl m_Impl;
        readonly string m_PackageName;
        readonly bool m_AppendTimeTicks;
        readonly int m_Timeout;
        QueryRemotePackageVersionOperation m_QueryRemotePackageVersionOp;
        ESteps m_Steps = ESteps.None;

        internal HostPlayModeUpdatePackageVersionOperation(HostPlayModeImpl impl, string packageName, bool appendTimeTicks, int timeout)
        {
            m_Impl = impl;
            m_PackageName = packageName;
            m_AppendTimeTicks = appendTimeTicks;
            m_Timeout = timeout;
        }
        internal override void Start()
        {
            m_Steps = ESteps.QueryRemotePackageVersion;
        }
        internal override void Update()
        {
            if (m_Steps is ESteps.None or ESteps.Done)
            {
                return;
            }

            if (m_Steps == ESteps.QueryRemotePackageVersion)
            {
                if (m_QueryRemotePackageVersionOp == null)
                {
                    m_QueryRemotePackageVersionOp = new(m_Impl, m_PackageName, m_AppendTimeTicks, m_Timeout);
                    Engine.StartAsyncOperation(m_QueryRemotePackageVersionOp);
                }

                if (m_QueryRemotePackageVersionOp.IsDone == false)
                {
                    return;
                }

                if (m_QueryRemotePackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    PackageVersion = m_QueryRemotePackageVersionOp.PackageVersion;
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = m_QueryRemotePackageVersionOp.Error;
                }
            }
        }
    }
}