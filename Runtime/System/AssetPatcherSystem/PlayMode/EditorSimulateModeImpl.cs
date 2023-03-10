using System;

namespace Universe
{
    internal class EditorSimulateModeImpl : IPlayModeServices, IBundleServices
    {
        private PatchManifest m_ActiveManifest;
        private bool m_LocationToLower;

        /// <summary>
        /// 异步初始化
        /// </summary>
        public InitializationOperation InitializeAsync(bool locationToLower, string simulatePatchManifestPath)
        {
            m_LocationToLower = locationToLower;
            EditorSimulateModeInitializationOperation operation = new(this, simulatePatchManifestPath);
            Engine.StartAsyncOperation(operation);
            return operation;
        }

    #region IPlayModeServices接口

        public PatchManifest ActiveManifest
        {
            set
            {
                m_ActiveManifest = value;
                m_ActiveManifest.InitAssetPathMapping(m_LocationToLower);
            }
            get
            {
                return m_ActiveManifest;
            }
        }
        public bool IsBuildinPatchBundle(PatchBundle patchBundle)
        {
            return true;
        }

        UpdatePackageVersionOperation IPlayModeServices.UpdatePackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            EditorPlayModeUpdatePackageVersionOperation operation = new();
            Engine.StartAsyncOperation(operation);
            return operation;
        }
        UpdatePackageManifestOperation IPlayModeServices.UpdatePackageManifestAsync(string packageVersion, int timeout)
        {
            EditorPlayModeUpdatePackageManifestOperation operation = new();
            Engine.StartAsyncOperation(operation);
            return operation;
        }
        PreDownloadPackageOperation IPlayModeServices.PreDownloadPackageAsync(string packageVersion, int timeout)
        {
            EditorPlayModePreDownloadPackageOperation operation = new();
            Engine.StartAsyncOperation(operation);
            return operation;
        }

        PatchDownloaderOperation IPlayModeServices.CreatePatchDownloaderByAll(int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
        }
        PatchDownloaderOperation IPlayModeServices.CreatePatchDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
        }
        PatchDownloaderOperation IPlayModeServices.CreatePatchDownloaderByPaths(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
        }

        PatchUnpackerOperation IPlayModeServices.CreatePatchUnpackerByAll(int upackingMaxNumber, int failedTryAgain, int timeout)
        {
            return PatchUnpackerOperation.CreateEmptyUnpacker(upackingMaxNumber, failedTryAgain, timeout);
        }
        PatchUnpackerOperation IPlayModeServices.CreatePatchUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout)
        {
            return PatchUnpackerOperation.CreateEmptyUnpacker(upackingMaxNumber, failedTryAgain, timeout);
        }

    #endregion

    #region IBundleServices接口

        BundleInfo IBundleServices.GetBundleInfo(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
            {
                throw new("Should never get here !");
            }

            // 注意：如果补丁清单里未找到资源包会抛出异常！
            PatchBundle patchBundle = m_ActiveManifest.GetMainPatchBundle(assetInfo.AssetPath);
            BundleInfo bundleInfo = new(patchBundle, BundleInfo.ELoadMode.LoadFromEditor, assetInfo.AssetPath);
            return bundleInfo;
        }
        BundleInfo[] IBundleServices.GetAllDependBundleInfos(AssetInfo assetInfo)
        {
            throw new NotImplementedException();
        }
        string IBundleServices.GetBundleName(int bundleID)
        {
            throw new NotImplementedException();
        }
        bool IBundleServices.IsServicesValid()
        {
            return m_ActiveManifest != null;
        }

    #endregion

    }
}