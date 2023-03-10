using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Universe
{
    internal partial class AssetSystem : EngineSystem
    {
        int m_LastestUpdateFrame;
        readonly List<AssetsPackage> m_AssetsPackages = new();
        readonly Dictionary<string, AssetsPackage> m_Packages = new();

        /// <summary>
        /// 获取已经创建的资源包
        /// </summary>
        /// <param name="result"></param>
        public void GetCreatedPackages(List<string> result)
        {
            if (result == null)
            {
                return;
            }

            result.Clear();
            result.AddRange(m_AssetsPackages.Select(package => package.PackageName));
        }

        /// <summary>
        /// 创建资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public bool CreateAssetsPackage(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                Log.Error("Package name is null or empty !");
                return false;
            }

            if (ExistAssetsPackage(packageName))
            {
                Log.Error($"Package {packageName} already existed !");
                return false;
            }

            AssetsPackage assetsPackage = new(packageName);
            m_AssetsPackages.Add(assetsPackage);
            m_Packages[packageName] = assetsPackage;
            Log.Info($"Create Assets Package | {packageName} |");
            return true;
        }

        /// <summary>
        /// 获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <param name="package">资源包</param>
        public bool TryGetAssetsPackage(string packageName, out AssetsPackage package)
        {
            package = null;
            if (string.IsNullOrEmpty(packageName))
            {
                Log.Error("Package is null or empty");
                return false;
            }
            if (m_Packages.TryGetValue(packageName, out package))
            {
                return true;
            }

            Log.Error($"AssetsPackage {packageName} doesn't exist");
            return false;
        }

        /// <summary>
        /// 检测资源包是否存在
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public bool ExistAssetsPackage(string packageName)
        {
            return m_Packages.ContainsKey(packageName);
        }

        /// <summary>
        /// 资源是否需要从远端下载
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool IsNeedDownloadFromRemote(string packageName, string location)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                Log.Error("Package Name is null or empty");
                return false;
            }

            if (string.IsNullOrEmpty(location))
            {
                Log.Error("Package");
                return false;
            }

            if (TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return package.IsNeedDownloadFromRemote(location);
            }

            Log.Error($"Unknown AssetsPackage : {packageName}");
            return false;
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="tag">资源标签</param>
        public AssetInfo[] GetAssetInfos(string packageName, string tag)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return Array.Empty<AssetInfo>();
            }

            return package.GetAssetInfos(tag);
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="tags">资源标签列表</param>
        public AssetInfo[] GetAssetInfos(string packageName, string[] tags)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return Array.Empty<AssetInfo>();
            }

            return package.GetAssetInfos(tags);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        public AssetInfo GetAssetInfo(string packageName, string location)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return default;
            }

            return package.GetAssetInfo(location);
        }

        /// <summary>
        /// 检查资源定位地址是否有效
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        public bool CheckLocationValid(string packageName, string location)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return false;
            }

            return package.CheckLocationValid(location);
        }

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetInfo">资源信息</param>
        public RawFileOperationHandle LoadRawFileSync(string packageName, AssetInfo assetInfo)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadRawFileSync(assetInfo);
        }

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        public RawFileOperationHandle LoadRawFileSync(string packageName, string location)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadRawFileSync(location);
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetInfo">资源信息</param>
        public RawFileOperationHandle LoadRawFileAsync(string packageName, AssetInfo assetInfo)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadRawFileAsync(assetInfo);
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        public RawFileOperationHandle LoadRawFileAsync(string packageName, string location)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadRawFileAsync(location);
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">场景的定位地址</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="activateOnLoad">加载完毕时是否主动激活</param>
        /// <param name="priority">优先级</param>
        public SceneOperationHandle LoadSceneAsync(string packageName, string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadSceneAsync(location, sceneMode, activateOnLoad, priority);
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetInfo">场景的资源信息</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="activateOnLoad">加载完毕时是否主动激活</param>
        /// <param name="priority">优先级</param>
        public SceneOperationHandle LoadSceneAsync(string packageName, AssetInfo assetInfo, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad, priority);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetInfo">资源信息</param>
        public AssetOperationHandle LoadAssetSync(string packageName, AssetInfo assetInfo)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadAssetSync(assetInfo);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        public AssetOperationHandle LoadAssetSync<TObject>(string packageName, string location) where TObject : UnityEngine.Object
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadAssetSync<TObject>(location);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        public AssetOperationHandle LoadAssetSync(string packageName, string location, Type type)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadAssetSync(location, type);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetInfo">资源信息</param>
        public AssetOperationHandle LoadAssetAsync(string packageName, AssetInfo assetInfo)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadAssetAsync(assetInfo);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        public AssetOperationHandle LoadAssetAsync<TObject>(string packageName, string location) where TObject : UnityEngine.Object
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadAssetAsync<TObject>(location);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        public AssetOperationHandle LoadAssetAsync(string packageName, string location, Type type)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadAssetAsync(location, type);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetInfo">资源信息</param>
        public SubAssetsOperationHandle LoadSubAssetsSync(string packageName, AssetInfo assetInfo)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadSubAssetsSync(assetInfo);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        public SubAssetsOperationHandle LoadSubAssetsSync<TObject>(string packageName, string location) where TObject : UnityEngine.Object
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadSubAssetsSync<TObject>(location);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        public SubAssetsOperationHandle LoadSubAssetsSync(string packageName, string location, Type type)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadSubAssetsSync(location, type);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetInfo">资源信息</param>
        public SubAssetsOperationHandle LoadSubAssetsAsync(string packageName, AssetInfo assetInfo)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadSubAssetsAsync(assetInfo);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        public SubAssetsOperationHandle LoadSubAssetsAsync<TObject>(string packageName, string location) where TObject : UnityEngine.Object
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadSubAssetsAsync<TObject>(location);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        public SubAssetsOperationHandle LoadSubAssetsAsync(string packageName, string location, Type type)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.LoadSubAssetsAsync(location, type);
        }

        /// <summary>
        /// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="tag">资源标签</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public PatchDownloaderOperation CreatePatchDownloader(string packageName, string tag, int downloadingMaxNumber, int failedTryAgain)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            FixedArray<string> array = FixedArray<string>.Get(1);
            array[0] = tag;
            return package.CreatePatchDownloader(array, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="tags">资源标签列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public PatchDownloaderOperation CreatePatchDownloader(string packageName, string[] tags, int downloadingMaxNumber, int failedTryAgain)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.CreatePatchDownloader(tags, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建补丁下载器，用于下载更新当前资源版本所有的资源包文件
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public PatchDownloaderOperation CreatePatchDownloader(string packageName, int downloadingMaxNumber, int failedTryAgain)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.CreatePatchDownloader(downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建补丁下载器，用于下载更新指定的资源列表依赖的资源包文件
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetInfos">资源信息列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public PatchDownloaderOperation CreateBundleDownloader(string packageName, AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.CreateBundleDownloader(assetInfos, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建补丁解压器
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="tag">资源标签</param>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        public PatchUnpackerOperation CreatePatchUnpacker(string packageName, string tag, int unpackingMaxNumber, int failedTryAgain)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.CreatePatchUnpacker(tag, unpackingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建补丁解压器
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="tags">资源标签列表</param>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        public PatchUnpackerOperation CreatePatchUnpacker(string packageName, string[] tags, int unpackingMaxNumber, int failedTryAgain)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.CreatePatchUnpacker(tags, unpackingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建补丁解压器
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        public PatchUnpackerOperation CreatePatchUnpacker(string packageName, int unpackingMaxNumber, int failedTryAgain)
        {
            if (!TryGetAssetsPackage(packageName, out AssetsPackage package))
            {
                return null;
            }

            return package.CreatePatchUnpacker(unpackingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 获取资源包调试信息
        /// </summary>
        /// <returns></returns>
        public DebugReport GetDebugReport()
        {
            DebugReport report = new()
            {
                FrameCount = Time.frameCount
            };

            foreach (AssetsPackage package in m_AssetsPackages)
            {
                DebugPackageData packageData = package.GetDebugPackageData();
                report.PackageDatas.Add(packageData);
            }
            return report;
        }

        /// <summary>
        /// 更新资源系统
        /// </summary>
        public override void Update(float dt)
        {
            for (int i = 0; i < m_AssetsPackages.Count; i++)
            {
                AssetsPackage package = m_AssetsPackages[i];
                if (package == null)
                {
                    continue;
                }

                package.UpdatePackage();
            }
        }

        /// <summary>
        /// 销毁资源系统
        /// </summary>
        public override void Destroy()
        {
            CacheSystem.ClearAll();

            foreach (AssetsPackage package in m_AssetsPackages)
            {
                package.DestroyPackage();
            }

            m_AssetsPackages.Clear();
            Log.Info($"{nameof(AssetSystem)} Destroyed");
        }
    }
}