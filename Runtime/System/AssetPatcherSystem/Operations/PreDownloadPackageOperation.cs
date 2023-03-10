using System.Collections.Generic;

namespace Universe
{
    public abstract class PreDownloadPackageOperation : AsyncOperationBase
    {
        /// <summary>
        /// 创建补丁下载器，用于下载更新指定资源版本所有的资源包文件
        /// </summary>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public virtual PatchDownloaderOperation CreatePatchDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
        /// </summary>
        /// <param name="tag">资源标签</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public virtual PatchDownloaderOperation CreatePatchDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public virtual PatchDownloaderOperation CreatePatchDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建补丁下载器，用于下载更新指定的资源列表依赖的资源包文件
        /// </summary>
        /// <param name="locations">资源定位列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public virtual PatchDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
        }
    }

    public class EditorPlayModePreDownloadPackageOperation : PreDownloadPackageOperation
    {
        internal override void Start()
        {
            Status = EOperationStatus.Succeed;
        }
        internal override void Update()
        {
        }
    }

    public class OfflinePlayModePreDownloadPackageOperation : PreDownloadPackageOperation
    {
        internal override void Start()
        {
            Status = EOperationStatus.Succeed;
        }
        internal override void Update()
        {
        }
    }

    public class HostPlayModePreDownloadPackageOperation : PreDownloadPackageOperation
    {
        enum ESteps
        {
            None,
            CheckActiveManifest,
            TryLoadCacheManifest,
            DownloadManifest,
            LoadCacheManifest,
            CheckDeserializeManifest,
            Done,
        }

        readonly HostPlayModeImpl m_Impl;
        readonly string m_PackageName;
        readonly string m_PackageVersion;
        readonly int m_Timeout;
        LoadCacheManifestOperation m_TryLoadCacheManifestOp;
        LoadCacheManifestOperation m_LoadCacheManifestOp;
        DownloadManifestOperation m_DownloadManifestOp;
        PatchManifest m_Manifest;
        ESteps m_Steps = ESteps.None;

        internal HostPlayModePreDownloadPackageOperation(HostPlayModeImpl impl, string packageName, string packageVersion, int timeout)
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
            if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
                return;

            if (m_Steps == ESteps.CheckActiveManifest)
            {
                // 检测当前激活的清单对象
                if (m_Impl.ActiveManifest != null)
                {
                    if (m_Impl.ActiveManifest.PackageVersion == m_PackageVersion)
                    {
                        m_Manifest = m_Impl.ActiveManifest;
                        m_Steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                        return;
                    }
                }
                m_Steps = ESteps.TryLoadCacheManifest;
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
                    m_Manifest = m_TryLoadCacheManifestOp.Manifest;
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
                    m_Manifest = m_LoadCacheManifestOp.Manifest;
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

        public override PatchDownloaderOperation CreatePatchDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            if (Status != EOperationStatus.Succeed)
            {
                Log.Warning($"{nameof(PreDownloadPackageOperation)} status is not succeed !");
                return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
            }

            List<BundleInfo> downloadList = m_Impl.GetDownloadListByAll(m_Manifest);
            PatchDownloaderOperation operation = new(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        public override PatchDownloaderOperation CreatePatchDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            if (Status != EOperationStatus.Succeed)
            {
                Log.Warning($"{nameof(PreDownloadPackageOperation)} status is not succeed !");
                return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
            }

            List<BundleInfo> downloadList = m_Impl.GetDownloadListByTags(m_Manifest, new string[] { tag });
            PatchDownloaderOperation operation = new(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        public override PatchDownloaderOperation CreatePatchDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            if (Status != EOperationStatus.Succeed)
            {
                Log.Warning($"{nameof(PreDownloadPackageOperation)} status is not succeed !");
                return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
            }

            List<BundleInfo> downloadList = m_Impl.GetDownloadListByTags(m_Manifest, tags);
            PatchDownloaderOperation operation = new(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        public override PatchDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            if (Status != EOperationStatus.Succeed)
            {
                Log.Warning($"{nameof(PreDownloadPackageOperation)} status is not succeed !");
                return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
            }

            List<AssetInfo> assetInfos = new(locations.Length);
            foreach (string location in locations)
            {
                AssetInfo assetInfo = m_Manifest.ConvertLocationToAssetInfo(location, null);
                assetInfos.Add(assetInfo);
            }

            List<BundleInfo> downloadList = m_Impl.GetDownloadListByPaths(m_Manifest, assetInfos.ToArray());
            PatchDownloaderOperation operation = new(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
    }
}