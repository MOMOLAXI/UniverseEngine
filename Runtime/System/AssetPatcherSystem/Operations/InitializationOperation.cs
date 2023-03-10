using System;
using System.IO;
using UnityEngine;

namespace Universe
{
    /// <summary>
    /// 初始化操作
    /// </summary>
    public abstract class InitializationOperation : AsyncOperationBase
    {
        public string PackageVersion { protected set; get; }
    }

    /// <summary>
    /// 编辑器下模拟模式的初始化操作
    /// </summary>
    sealed internal class EditorSimulateModeInitializationOperation : InitializationOperation
    {
        enum ESteps
        {
            None,
            LoadEditorManifest,
            Done,
        }

        readonly EditorSimulateModeImpl m_Impl;
        readonly string m_SimulateManifestPath;
        LoadEditorManifestOperation m_LoadEditorManifestOp;
        ESteps m_Steps = ESteps.None;

        internal EditorSimulateModeInitializationOperation(EditorSimulateModeImpl impl, string simulateManifestPath)
        {
            m_Impl = impl;
            m_SimulateManifestPath = simulateManifestPath;
        }
        internal override void Start()
        {
            m_Steps = ESteps.LoadEditorManifest;
        }
        internal override void Update()
        {
            if (m_Steps == ESteps.LoadEditorManifest)
            {
                if (m_LoadEditorManifestOp == null)
                {
                    m_LoadEditorManifestOp = new(m_SimulateManifestPath);
                    Engine.StartAsyncOperation(m_LoadEditorManifestOp);
                }

                if (m_LoadEditorManifestOp.IsDone == false)
                    return;

                if (m_LoadEditorManifestOp.Status == EOperationStatus.Succeed)
                {
                    PackageVersion = m_LoadEditorManifestOp.Manifest.PackageVersion;
                    m_Impl.ActiveManifest = m_LoadEditorManifestOp.Manifest;
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = m_LoadEditorManifestOp.Error;
                }
            }
        }
    }

    /// <summary>
    /// 离线运行模式的初始化操作
    /// </summary>
    sealed internal class OfflinePlayModeInitializationOperation : InitializationOperation
    {
        enum ESteps
        {
            None,
            QueryBuildinPackageVersion,
            LoadBuildinManifest,
            PackageCaching,
            Done,
        }

        readonly OfflinePlayModeImpl m_Impl;
        readonly string m_PackageName;
        QueryBuildinPackageVersionOperation m_QueryBuildinPackageVersionOp;
        LoadBuildinManifestOperation m_LoadBuildinManifestOp;
        PackageCachingOperation m_CachingOperation;
        ESteps m_Steps = ESteps.None;

        internal OfflinePlayModeInitializationOperation(OfflinePlayModeImpl impl, string packageName)
        {
            m_Impl = impl;
            m_PackageName = packageName;
        }

        internal override void Start()
        {
            m_Steps = ESteps.QueryBuildinPackageVersion;
        }

        internal override void Update()
        {
            switch (m_Steps)
            {
                case ESteps.None:
                case ESteps.Done:
                    return;
                case ESteps.QueryBuildinPackageVersion:
                {
                    if (m_QueryBuildinPackageVersionOp == null)
                    {
                        m_QueryBuildinPackageVersionOp = new(m_PackageName);
                        Engine.StartAsyncOperation(m_QueryBuildinPackageVersionOp);
                    }

                    if (m_QueryBuildinPackageVersionOp.IsDone == false)
                        return;

                    if (m_QueryBuildinPackageVersionOp.Status == EOperationStatus.Succeed)
                    {
                        m_Steps = ESteps.LoadBuildinManifest;
                    }
                    else
                    {
                        m_Steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = m_QueryBuildinPackageVersionOp.Error;
                    }
                    break;
                }
                case ESteps.LoadBuildinManifest:
                {
                    if (m_LoadBuildinManifestOp == null)
                    {
                        m_LoadBuildinManifestOp = new(m_PackageName, m_QueryBuildinPackageVersionOp.PackageVersion);
                        Engine.StartAsyncOperation(m_LoadBuildinManifestOp);
                    }

                    Progress = m_LoadBuildinManifestOp.Progress;
                    if (m_LoadBuildinManifestOp.IsDone == false)
                        return;

                    if (m_LoadBuildinManifestOp.Status == EOperationStatus.Succeed)
                    {
                        PackageVersion = m_LoadBuildinManifestOp.Manifest.PackageVersion;
                        m_Impl.ActiveManifest = m_LoadBuildinManifestOp.Manifest;
                        m_Steps = ESteps.PackageCaching;
                    }
                    else
                    {
                        m_Steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = m_LoadBuildinManifestOp.Error;
                    }
                    break;
                }
                case ESteps.PackageCaching:
                {
                    if (m_CachingOperation == null)
                    {
                        m_CachingOperation = new(m_PackageName);
                        Engine.StartAsyncOperation(m_CachingOperation);
                    }

                    Progress = m_CachingOperation.Progress;
                    if (m_CachingOperation.IsDone)
                    {
                        m_Steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                    }
                    break;
                }
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// 联机运行模式的初始化操作
    /// 注意：优先从沙盒里加载清单，如果沙盒里不存在就尝试把内置清单拷贝到沙盒并加载该清单。
    /// </summary>
    sealed internal class HostPlayModeInitializationOperation : InitializationOperation
    {
        enum ESteps
        {
            None,
            CheckAppFootPrint,
            QueryCachePackageVersion,
            TryLoadCacheManifest,
            QueryBuildinPackageVersion,
            UnpackBuildinManifest,
            LoadBuildinManifest,
            PackageCaching,
            Done,
        }

        readonly HostPlayModeImpl m_Impl;
        readonly string m_PackageName;
        QueryBuildinPackageVersionOperation m_QueryBuildinPackageVersionOp;
        QueryCachePackageVersionOperation m_QueryCachePackageVersionOp;
        UnpackBuildinManifestOperation m_UnpackBuildinManifestOp;
        LoadBuildinManifestOperation m_LoadBuildinManifestOp;
        LoadCacheManifestOperation m_LoadCacheManifestOp;
        PackageCachingOperation m_CachingOperation;
        ESteps m_Steps = ESteps.None;

        internal HostPlayModeInitializationOperation(HostPlayModeImpl impl, string packageName)
        {
            m_Impl = impl;
            m_PackageName = packageName;
        }
        internal override void Start()
        {
            m_Steps = ESteps.CheckAppFootPrint;
        }
        internal override void Update()
        {
            if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
                return;

            if (m_Steps == ESteps.CheckAppFootPrint)
            {
                AppFootPrint appFootPrint = new();
                appFootPrint.Load();

                // 如果水印发生变化，则说明覆盖安装后首次打开游戏
                if (appFootPrint.IsDirty())
                {
                    PersistentHelper.DeleteManifestFolder();
                    appFootPrint.Coverage();
                    Log.Info("Delete manifest files when application foot print dirty !");
                }
                m_Steps = ESteps.QueryCachePackageVersion;
            }

            if (m_Steps == ESteps.QueryCachePackageVersion)
            {
                if (m_QueryCachePackageVersionOp == null)
                {
                    m_QueryCachePackageVersionOp = new(m_PackageName);
                    Engine.StartAsyncOperation(m_QueryCachePackageVersionOp);
                }

                if (m_QueryCachePackageVersionOp.IsDone == false)
                    return;

                if (m_QueryCachePackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    m_Steps = ESteps.TryLoadCacheManifest;
                }
                else
                {
                    m_Steps = ESteps.QueryBuildinPackageVersion;
                }
            }

            if (m_Steps == ESteps.TryLoadCacheManifest)
            {
                if (m_LoadCacheManifestOp == null)
                {
                    m_LoadCacheManifestOp = new(m_PackageName, m_QueryCachePackageVersionOp.PackageVersion);
                    Engine.StartAsyncOperation(m_LoadCacheManifestOp);
                }

                if (m_LoadCacheManifestOp.IsDone == false)
                    return;

                if (m_LoadCacheManifestOp.Status == EOperationStatus.Succeed)
                {
                    PackageVersion = m_LoadCacheManifestOp.Manifest.PackageVersion;
                    m_Impl.ActiveManifest = m_LoadCacheManifestOp.Manifest;
                    m_Steps = ESteps.PackageCaching;
                }
                else
                {
                    m_Steps = ESteps.QueryBuildinPackageVersion;
                }
            }

            if (m_Steps == ESteps.QueryBuildinPackageVersion)
            {
                if (m_QueryBuildinPackageVersionOp == null)
                {
                    m_QueryBuildinPackageVersionOp = new(m_PackageName);
                    Engine.StartAsyncOperation(m_QueryBuildinPackageVersionOp);
                }

                if (m_QueryBuildinPackageVersionOp.IsDone == false)
                    return;

                if (m_QueryBuildinPackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    m_Steps = ESteps.UnpackBuildinManifest;
                }
                else
                {
                    // 注意：为了兼容MOD模式，初始化动态新增的包裹的时候，如果内置清单不存在也不需要报错！
                    m_Steps = ESteps.PackageCaching;
                    string error = m_QueryBuildinPackageVersionOp.Error;
                    Log.Info($"Failed to load buildin package version file : {error}");
                }
            }

            if (m_Steps == ESteps.UnpackBuildinManifest)
            {
                if (m_UnpackBuildinManifestOp == null)
                {
                    m_UnpackBuildinManifestOp = new(m_PackageName, m_QueryBuildinPackageVersionOp.PackageVersion);
                    Engine.StartAsyncOperation(m_UnpackBuildinManifestOp);
                }

                if (m_UnpackBuildinManifestOp.IsDone == false)
                    return;

                if (m_UnpackBuildinManifestOp.Status == EOperationStatus.Succeed)
                {
                    m_Steps = ESteps.LoadBuildinManifest;
                }
                else
                {
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = m_UnpackBuildinManifestOp.Error;
                }
            }

            if (m_Steps == ESteps.LoadBuildinManifest)
            {
                if (m_LoadBuildinManifestOp == null)
                {
                    m_LoadBuildinManifestOp = new(m_PackageName, m_QueryBuildinPackageVersionOp.PackageVersion);
                    Engine.StartAsyncOperation(m_LoadBuildinManifestOp);
                }

                Progress = m_LoadBuildinManifestOp.Progress;
                if (m_LoadBuildinManifestOp.IsDone == false)
                    return;

                if (m_LoadBuildinManifestOp.Status == EOperationStatus.Succeed)
                {
                    PackageVersion = m_LoadBuildinManifestOp.Manifest.PackageVersion;
                    m_Impl.ActiveManifest = m_LoadBuildinManifestOp.Manifest;
                    m_Steps = ESteps.PackageCaching;
                }
                else
                {
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = m_LoadBuildinManifestOp.Error;
                }
            }

            if (m_Steps == ESteps.PackageCaching)
            {
                if (m_CachingOperation == null)
                {
                    m_CachingOperation = new(m_PackageName);
                    Engine.StartAsyncOperation(m_CachingOperation);
                }

                Progress = m_CachingOperation.Progress;
                if (m_CachingOperation.IsDone)
                {
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
            }
        }
    }

    /// <summary>
    /// 应用程序水印
    /// </summary>
    internal class AppFootPrint
    {
        string m_FootPrint;

        /// <summary>
        /// 读取应用程序水印
        /// </summary>
        public void Load()
        {
            string footPrintFilePath = PersistentHelper.GetAppFootPrintFilePath();
            if (File.Exists(footPrintFilePath))
            {
                m_FootPrint = FileUtility.ReadAllText(footPrintFilePath);
            }
            else
            {
                Coverage();
            }
        }

        /// <summary>
        /// 检测水印是否发生变化
        /// </summary>
        public bool IsDirty()
        {
        #if UNITY_EDITOR
            return m_FootPrint != Application.version;
        #else
			return m_FootPrint != Application.buildGUID;
        #endif
        }

        /// <summary>
        /// 覆盖掉水印
        /// </summary>
        public void Coverage()
        {
        #if UNITY_EDITOR
            m_FootPrint = Application.version;
        #else
			m_FootPrint = Application.buildGUID;
        #endif
            string footPrintFilePath = PersistentHelper.GetAppFootPrintFilePath();
            FileUtility.CreateFile(footPrintFilePath, m_FootPrint);
            Log.Info($"Save application foot print : {m_FootPrint}");
        }
    }
}