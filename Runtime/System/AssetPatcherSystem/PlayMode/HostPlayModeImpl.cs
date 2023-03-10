using System.Collections.Generic;
using System.Linq;

namespace Universe
{
    internal class HostPlayModeImpl : IPlayModeServices, IBundleServices, IRemoteServices
    {
        PatchManifest m_ActiveManifest;

        // 参数相关
        string m_PackageName;
        bool m_LocationToLower;
        string m_DefaultHostServer;
        string m_FallbackHostServer;
        IQueryServices m_QueryServices;

        /// <summary>
        /// 异步初始化
        /// </summary>
        public InitializationOperation InitializeAsync(string packageName, bool locationToLower, string defaultHostServer, string fallbackHostServer, IQueryServices queryServices)
        {
            m_PackageName = packageName;
            m_LocationToLower = locationToLower;
            m_DefaultHostServer = defaultHostServer;
            m_FallbackHostServer = fallbackHostServer;
            m_QueryServices = queryServices;

            HostPlayModeInitializationOperation operation = new(this, packageName);
            Engine.StartAsyncOperation(operation);
            return operation;
        }

        // 下载相关
        List<BundleInfo> ConvertToDownloadList(IReadOnlyCollection<PatchBundle> downloadList)
        {
            List<BundleInfo> result = new(downloadList.Count);
            result.AddRange(downloadList.Select(ConvertToDownloadInfo));
            return result;
        }
        
        BundleInfo ConvertToDownloadInfo(PatchBundle patchBundle)
        {
            string remoteMainURL = GetRemoteMainURL(patchBundle.FileName);
            string remoteFallbackURL = GetRemoteFallbackURL(patchBundle.FileName);
            BundleInfo bundleInfo = new(patchBundle, BundleInfo.ELoadMode.LoadFromRemote, remoteMainURL, remoteFallbackURL);
            return bundleInfo;
        }

        // 解压相关
        List<BundleInfo> ConvertToUnpackList(IReadOnlyList<PatchBundle> unpackList)
        {
            List<BundleInfo> result = new(unpackList.Count);
            for (int i = 0; i < unpackList.Count; i++)
            {
                PatchBundle patchBundle = unpackList[i];
                BundleInfo bundleInfo = ConvertToUnpackInfo(patchBundle);
                result.Add(bundleInfo);
            }
            return result;
        }
        BundleInfo ConvertToUnpackInfo(PatchBundle patchBundle)
        {
            return AssetManifestPatcher.GetUnpackInfo(patchBundle);
        }

    #region IRemoteServices接口

        public string GetRemoteMainURL(string fileName)
        {
            return $"{m_DefaultHostServer}/{fileName}";
        }
        public string GetRemoteFallbackURL(string fileName)
        {
            return $"{m_FallbackHostServer}/{fileName}";
        }

    #endregion

    #region IPlayModeServices接口

        public PatchManifest ActiveManifest
        {
            set
            {
                m_ActiveManifest = value;
                m_ActiveManifest.InitAssetPathMapping(m_LocationToLower);
                PersistentHelper.SaveCachePackageVersionFile(m_PackageName, m_ActiveManifest.PackageVersion);
            }
            get => m_ActiveManifest;
        }
        public bool IsBuildinPatchBundle(PatchBundle patchBundle)
        {
            return m_QueryServices.QueryStreamingAssets(patchBundle.FileName);
        }
        public bool IsCachedPatchBundle(PatchBundle patchBundle)
        {
            return CacheSystem.IsCached(patchBundle.PackageName, patchBundle.CacheGuid);
        }

        UpdatePackageVersionOperation IPlayModeServices.UpdatePackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            HostPlayModeUpdatePackageVersionOperation operation = new(this, m_PackageName, appendTimeTicks, timeout);
            Engine.StartAsyncOperation(operation);
            return operation;
        }
        UpdatePackageManifestOperation IPlayModeServices.UpdatePackageManifestAsync(string packageVersion, int timeout)
        {
            HostPlayModeUpdatePackageManifestOperation operation = new(this, m_PackageName, packageVersion, timeout);
            Engine.StartAsyncOperation(operation);
            return operation;
        }
        PreDownloadPackageOperation IPlayModeServices.PreDownloadPackageAsync(string packageVersion, int timeout)
        {
            HostPlayModePreDownloadPackageOperation operation = new(this, m_PackageName, packageVersion, timeout);
            Engine.StartAsyncOperation(operation);
            return operation;
        }

        PatchDownloaderOperation IPlayModeServices.CreatePatchDownloaderByAll(int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> downloadList = GetDownloadListByAll(m_ActiveManifest);
            PatchDownloaderOperation operation = new(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        
        public List<BundleInfo> GetDownloadListByAll(PatchManifest patchManifest)
        {
            List<PatchBundle> downloadList = new(1000);
            for (int i = 0; i < patchManifest.BundleList.Count; i++)
            {
                PatchBundle patchBundle = patchManifest.BundleList[i];
                // 忽略缓存文件
                if (IsCachedPatchBundle(patchBundle))
                {
                    continue;
                }

                // 忽略APP资源
                if (IsBuildinPatchBundle(patchBundle))
                {
                    continue;
                }

                downloadList.Add(patchBundle);
            }

            return ConvertToDownloadList(downloadList);
        }

        PatchDownloaderOperation IPlayModeServices.CreatePatchDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> downloadList = GetDownloadListByTags(m_ActiveManifest, tags);
            PatchDownloaderOperation operation = new(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        public List<BundleInfo> GetDownloadListByTags(PatchManifest patchManifest, string[] tags)
        {
            List<PatchBundle> downloadList = new(1000);
            foreach (PatchBundle patchBundle in patchManifest.BundleList)
            {
                // 忽略缓存文件
                if (IsCachedPatchBundle(patchBundle))
                    continue;

                // 忽略APP资源
                if (IsBuildinPatchBundle(patchBundle))
                    continue;

                // 如果未带任何标记，则统一下载
                if (patchBundle.HasAnyTags() == false)
                {
                    downloadList.Add(patchBundle);
                }
                else
                {
                    // 查询DLC资源
                    if (patchBundle.HasTag(tags))
                    {
                        downloadList.Add(patchBundle);
                    }
                }
            }

            return ConvertToDownloadList(downloadList);
        }

        PatchDownloaderOperation IPlayModeServices.CreatePatchDownloaderByPaths(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> downloadList = GetDownloadListByPaths(m_ActiveManifest, assetInfos);
            PatchDownloaderOperation operation = new(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        public List<BundleInfo> GetDownloadListByPaths(PatchManifest patchManifest, AssetInfo[] assetInfos)
        {
            // 获取资源对象的资源包和所有依赖资源包
            List<PatchBundle> checkList = new();
            foreach (AssetInfo assetInfo in assetInfos)
            {
                if (assetInfo.IsInvalid)
                {
                    Log.Warning(assetInfo.Error);
                    continue;
                }

                // 注意：如果补丁清单里未找到资源包会抛出异常！
                PatchBundle mainBundle = patchManifest.GetMainPatchBundle(assetInfo.AssetPath);
                if (checkList.Contains(mainBundle) == false)
                    checkList.Add(mainBundle);

                // 注意：如果补丁清单里未找到资源包会抛出异常！
                PatchBundle[] dependBundles = patchManifest.GetAllDependencies(assetInfo.AssetPath);
                foreach (PatchBundle dependBundle in dependBundles)
                {
                    if (checkList.Contains(dependBundle) == false)
                        checkList.Add(dependBundle);
                }
            }

            List<PatchBundle> downloadList = new(1000);
            foreach (PatchBundle patchBundle in checkList)
            {
                // 忽略缓存文件
                if (IsCachedPatchBundle(patchBundle))
                    continue;

                // 忽略APP资源
                if (IsBuildinPatchBundle(patchBundle))
                    continue;

                downloadList.Add(patchBundle);
            }

            return ConvertToDownloadList(downloadList);
        }

        PatchUnpackerOperation IPlayModeServices.CreatePatchUnpackerByAll(int upackingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> unpcakList = GetUnpackListByAll(m_ActiveManifest);
            PatchUnpackerOperation operation = new(unpcakList, upackingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        List<BundleInfo> GetUnpackListByAll(PatchManifest patchManifest)
        {
            List<PatchBundle> downloadList = new(1000);
            foreach (PatchBundle patchBundle in patchManifest.BundleList)
            {
                // 忽略缓存文件
                if (IsCachedPatchBundle(patchBundle))
                    continue;

                if (IsBuildinPatchBundle(patchBundle))
                {
                    downloadList.Add(patchBundle);
                }
            }

            return ConvertToUnpackList(downloadList);
        }

        PatchUnpackerOperation IPlayModeServices.CreatePatchUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> unpcakList = GetUnpackListByTags(m_ActiveManifest, tags);
            PatchUnpackerOperation operation = new(unpcakList, upackingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        List<BundleInfo> GetUnpackListByTags(PatchManifest patchManifest, string[] tags)
        {
            List<PatchBundle> downloadList = new(1000);
            foreach (PatchBundle patchBundle in patchManifest.BundleList)
            {
                // 忽略缓存文件
                if (IsCachedPatchBundle(patchBundle))
                    continue;

                // 查询DLC资源
                if (IsBuildinPatchBundle(patchBundle))
                {
                    if (patchBundle.HasTag(tags))
                    {
                        downloadList.Add(patchBundle);
                    }
                }
            }

            return ConvertToUnpackList(downloadList);
        }

    #endregion

    #region IBundleServices接口

        BundleInfo CreateBundleInfo(PatchBundle patchBundle)
        {
            if (patchBundle == null)
                throw new("Should never get here !");

            // 查询沙盒资源
            if (IsCachedPatchBundle(patchBundle))
            {
                BundleInfo bundleInfo = new(patchBundle, BundleInfo.ELoadMode.LoadFromCache);
                return bundleInfo;
            }

            // 查询APP资源
            if (IsBuildinPatchBundle(patchBundle))
            {
                BundleInfo bundleInfo = new(patchBundle, BundleInfo.ELoadMode.LoadFromStreaming);
                return bundleInfo;
            }

            // 从服务端下载
            return ConvertToDownloadInfo(patchBundle);
        }
        BundleInfo IBundleServices.GetBundleInfo(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new("Should never get here !");

            // 注意：如果补丁清单里未找到资源包会抛出异常！
            PatchBundle patchBundle = m_ActiveManifest.GetMainPatchBundle(assetInfo.AssetPath);
            return CreateBundleInfo(patchBundle);
        }
        BundleInfo[] IBundleServices.GetAllDependBundleInfos(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new("Should never get here !");

            // 注意：如果补丁清单里未找到资源包会抛出异常！
            PatchBundle[] depends = m_ActiveManifest.GetAllDependencies(assetInfo.AssetPath);
            List<BundleInfo> result = new(depends.Length);
            foreach (PatchBundle patchBundle in depends)
            {
                BundleInfo bundleInfo = CreateBundleInfo(patchBundle);
                result.Add(bundleInfo);
            }
            return result.ToArray();
        }
        string IBundleServices.GetBundleName(int bundleID)
        {
            return m_ActiveManifest.GetBundleName(bundleID);
        }
        bool IBundleServices.IsServicesValid()
        {
            return m_ActiveManifest != null;
        }

    #endregion

    }
}