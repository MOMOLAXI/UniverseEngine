using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.SceneManagement;

namespace Universe
{
    public class AssetsPackage
    {
        bool m_IsInitialize;
        string m_InitializeError = string.Empty;
        EPlayMode m_PlayMode;
        IBundleServices m_BundleServices;
        IPlayModeServices m_PlayModeServices;
        AssetPackageProxy m_AssetPackageProxy;

        /// <summary>
        /// 资源包名
        /// </summary>
        public string PackageName { get; }

        /// <summary>
        /// 初始化状态
        /// </summary>
        public EOperationStatus InitializeStatus { get; private set; } = EOperationStatus.None;

        public static readonly AssetsPackage Empty = new("Empty");

        internal AssetsPackage(string packageName)
        {
            PackageName = packageName;
        }

        /// <summary>
        /// 更新资源包裹
        /// </summary>
        internal void UpdatePackage()
        {
            m_AssetPackageProxy?.Update();
        }

        /// <summary>
        /// 销毁资源包裹
        /// </summary>
        internal void DestroyPackage()
        {
            if (!m_IsInitialize)
            {
                return;
            }
            
            m_IsInitialize = false;
            m_InitializeError = string.Empty;
            InitializeStatus = EOperationStatus.None;
            m_BundleServices = null;
            m_PlayModeServices = null;

            if (m_AssetPackageProxy != null)
            {
                m_AssetPackageProxy.DestroyAll();
                m_AssetPackageProxy = null;
            }
        }

        /// <summary>
        /// 异步初始化
        /// </summary>
        public InitializationOperation InitializeAsync(InitializeParameters parameters)
        {
            // 注意：WebGL平台因为网络原因可能会初始化失败！
            ResetInitializeAfterFailed();

            // 检测初始化参数合法性
            CheckInitializeParameters(parameters);

            // 初始化资源系统
            InitializationOperation initializeOperation;
            m_AssetPackageProxy = new();
            switch (m_PlayMode)
            {
                case EPlayMode.EditorSimulateMode:
                {
                    EditorSimulateModeImpl editorSimulateModeImpl = new();
                    m_BundleServices = editorSimulateModeImpl;
                    m_PlayModeServices = editorSimulateModeImpl;
                    m_AssetPackageProxy.Initialize(PackageName, true, parameters.AssetLoadingMaxNumber, parameters.DecryptionServices, m_BundleServices);

                    EditorSimulateModeParameters initializeParameters = parameters as EditorSimulateModeParameters;
                    initializeOperation = editorSimulateModeImpl.InitializeAsync(initializeParameters.LocationToLower, initializeParameters.SimulatePatchManifestPath);
                    break;
                }
                case EPlayMode.OfflinePlayMode:
                {
                    OfflinePlayModeImpl offlinePlayModeImpl = new();
                    m_BundleServices = offlinePlayModeImpl;
                    m_PlayModeServices = offlinePlayModeImpl;
                    m_AssetPackageProxy.Initialize(PackageName, false, parameters.AssetLoadingMaxNumber, parameters.DecryptionServices, m_BundleServices);

                    OfflinePlayModeParameters initializeParameters = parameters as OfflinePlayModeParameters;
                    initializeOperation = offlinePlayModeImpl.InitializeAsync(PackageName, initializeParameters.LocationToLower);
                    break;
                }
                case EPlayMode.HostPlayMode:
                {
                    HostPlayModeImpl hostPlayModeImpl = new();
                    m_BundleServices = hostPlayModeImpl;
                    m_PlayModeServices = hostPlayModeImpl;
                    m_AssetPackageProxy.Initialize(PackageName, false, parameters.AssetLoadingMaxNumber, parameters.DecryptionServices, m_BundleServices);

                    HostPlayModeParameters initializeParameters = parameters as HostPlayModeParameters;
                    initializeOperation = hostPlayModeImpl.InitializeAsync(PackageName,
                                                                           initializeParameters.LocationToLower,
                                                                           initializeParameters.DefaultHostServer,
                                                                           initializeParameters.FallbackHostServer,
                                                                           initializeParameters.QueryServices);
                    break;
                }
                default: throw new NotImplementedException();
            }

            // 监听初始化结果
            m_IsInitialize = true;
            initializeOperation.Completed += InitializeOperation_Completed;
            return initializeOperation;
        }
        private void ResetInitializeAfterFailed()
        {
            if (m_IsInitialize && InitializeStatus == EOperationStatus.Failed)
            {
                m_IsInitialize = false;
                InitializeStatus = EOperationStatus.None;
                m_InitializeError = string.Empty;
                m_BundleServices = null;
                m_PlayModeServices = null;
                m_AssetPackageProxy = null;
            }
        }
        private void CheckInitializeParameters(InitializeParameters parameters)
        {
            if (m_IsInitialize)
            {
                throw new($"{nameof(AssetsPackage)} is initialized yet.");
            }

            if (parameters == null)
            {
                throw new($"{nameof(AssetsPackage)} create parameters is null.");
            }

        #if !UNITY_EDITOR
			if (parameters is EditorSimulateModeParameters)
				throw new Exception($"Editor simulate mode only support unity editor.");
        #endif

            switch (parameters)
            {
                case EditorSimulateModeParameters modeParameters when string.IsNullOrEmpty(modeParameters.SimulatePatchManifestPath): throw new($"{nameof(modeParameters.SimulatePatchManifestPath)} is null or empty.");
                case HostPlayModeParameters playModeParameters:
                {
                    if (string.IsNullOrEmpty(playModeParameters.DefaultHostServer))
                    {
                        throw new($"${playModeParameters.DefaultHostServer} is null or empty.");
                    }
                    if (string.IsNullOrEmpty(playModeParameters.FallbackHostServer))
                    {
                        throw new($"${playModeParameters.FallbackHostServer} is null or empty.");
                    }
                    if (playModeParameters.QueryServices == null)
                    {
                        throw new($"{nameof(IQueryServices)} is null.");
                    }
                    break;
                }
            }

            // 鉴定运行模式
            m_PlayMode = parameters switch
            {
                EditorSimulateModeParameters => EPlayMode.EditorSimulateMode,
                OfflinePlayModeParameters => EPlayMode.OfflinePlayMode,
                HostPlayModeParameters => EPlayMode.HostPlayMode,
                _ => throw new NotImplementedException()
            };

            // 检测参数范围
            if (parameters.AssetLoadingMaxNumber < 1)
            {
                parameters.AssetLoadingMaxNumber = 1;
                Log.Warning($"{nameof(parameters.AssetLoadingMaxNumber)} minimum value is 1");
            }
        }
        private void InitializeOperation_Completed(AsyncOperationBase op)
        {
            InitializeStatus = op.Status;
            m_InitializeError = op.Error;
        }

        /// <summary>
        /// 向网络端请求最新的资源版本
        /// </summary>
        /// <param name="appendTimeTicks">在URL末尾添加时间戳</param>
        /// <param name="timeout">超时时间（默认值：60秒）</param>
        public UpdatePackageVersionOperation UpdatePackageVersionAsync(bool appendTimeTicks = true, int timeout = 60)
        {
            DebugCheckInitialize();
            return m_PlayModeServices.UpdatePackageVersionAsync(appendTimeTicks, timeout);
        }

        /// <summary>
        /// 向网络端请求并更新补丁清单
        /// </summary>
        /// <param name="packageVersion">更新的包裹版本</param>
        /// <param name="timeout">超时时间（默认值：60秒）</param>
        public UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, int timeout = 60)
        {
            DebugCheckInitialize();
            DebugCheckUpdateManifest();
            return m_PlayModeServices.UpdatePackageManifestAsync(packageVersion, timeout);
        }

        /// <summary>
        /// 预下载指定版本的包裹资源
        /// </summary>
        /// <param name="packageVersion">下载的包裹版本</param>
        /// <param name="timeout">超时时间（默认值：60秒）</param>
        public PreDownloadPackageOperation PreDownloadPackageAsync(string packageVersion, int timeout = 60)
        {
            DebugCheckInitialize();
            return m_PlayModeServices.PreDownloadPackageAsync(packageVersion, timeout);
        }

        /// <summary>
        /// 清理包裹未使用的缓存文件
        /// </summary>
        public ClearUnusedCacheFilesOperation ClearUnusedCacheFilesAsync()
        {
            DebugCheckInitialize();
            ClearUnusedCacheFilesOperation operation = new(this);
            Engine.StartAsyncOperation(operation);
            return operation;
        }

        /// <summary>
        /// 获取本地包裹的版本信息
        /// </summary>
        public string GetPackageVersion()
        {
            DebugCheckInitialize();
            if (m_PlayModeServices.ActiveManifest == null)
                return string.Empty;
            return m_PlayModeServices.ActiveManifest.PackageVersion;
        }

        /// <summary>
        /// 资源回收（卸载引用计数为零的资源）
        /// </summary>
        public void UnloadUnusedAssets()
        {
            DebugCheckInitialize();
            m_AssetPackageProxy.Update();
            m_AssetPackageProxy.UnloadUnusedAssets();
        }

        /// <summary>
        /// 强制回收所有资源
        /// </summary>
        public void ForceUnloadAllAssets()
        {
            DebugCheckInitialize();
            m_AssetPackageProxy.ForceUnloadAllAssets();
        }


    #region 资源信息

        /// <summary>
        /// 是否需要从远端更新下载
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public bool IsNeedDownloadFromRemote(string location)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            if (assetInfo.IsInvalid)
            {
                Log.Warning(assetInfo.Error);
                return false;
            }

            BundleInfo bundleInfo = m_BundleServices.GetBundleInfo(assetInfo);
            if (bundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromRemote)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 是否需要从远端更新下载
        /// </summary>
        /// <param name="assetInfo"></param>
        public bool IsNeedDownloadFromRemote(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            if (assetInfo.IsInvalid)
            {
                Log.Warning(assetInfo.Error);
                return false;
            }

            BundleInfo bundleInfo = m_BundleServices.GetBundleInfo(assetInfo);
            if (bundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromRemote)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        /// <param name="tag">资源标签</param>
        public AssetInfo[] GetAssetInfos(string tag)
        {
            DebugCheckInitialize();
            string[] tags = new string[] { tag };
            return m_PlayModeServices.ActiveManifest.GetAssetsInfoByTags(tags);
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        public AssetInfo[] GetAssetInfos(string[] tags)
        {
            DebugCheckInitialize();
            return m_PlayModeServices.ActiveManifest.GetAssetsInfoByTags(tags);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public AssetInfo GetAssetInfo(string location)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            return assetInfo;
        }

        /// <summary>
        /// 检查资源定位地址是否有效
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public bool CheckLocationValid(string location)
        {
            DebugCheckInitialize();
            string assetPath = m_PlayModeServices.ActiveManifest.TryMappingToAssetPath(location);
            return string.IsNullOrEmpty(assetPath) == false;
        }

    #endregion

    #region 原生文件

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public RawFileOperationHandle LoadRawFileSync(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            return LoadRawFileInternal(assetInfo, true);
        }

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public RawFileOperationHandle LoadRawFileSync(string location)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            return LoadRawFileInternal(assetInfo, true);
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public RawFileOperationHandle LoadRawFileAsync(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            return LoadRawFileInternal(assetInfo, false);
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public RawFileOperationHandle LoadRawFileAsync(string location)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            return LoadRawFileInternal(assetInfo, false);
        }


        private RawFileOperationHandle LoadRawFileInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
        {
        #if UNITY_EDITOR
            if (assetInfo.IsInvalid == false)
            {
                BundleInfo bundleInfo = m_BundleServices.GetBundleInfo(assetInfo);
                if (bundleInfo.Bundle.IsRawFile == false)
                    throw new($"Cannot load asset bundle file using {nameof(LoadRawFileAsync)} method !");
            }
        #endif

            RawFileOperationHandle handle = m_AssetPackageProxy.LoadRawFileAsync(assetInfo);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }

    #endregion

    #region 场景加载

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="location">场景的定位地址</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="activateOnLoad">加载完毕时是否主动激活</param>
        /// <param name="priority">优先级</param>
        public SceneOperationHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            SceneOperationHandle handle = m_AssetPackageProxy.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad, priority);
            return handle;
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetInfo">场景的资源信息</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="activateOnLoad">加载完毕时是否主动激活</param>
        /// <param name="priority">优先级</param>
        public SceneOperationHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            DebugCheckInitialize();
            SceneOperationHandle handle = m_AssetPackageProxy.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad, priority);
            return handle;
        }

    #endregion

    #region 资源加载

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public AssetOperationHandle LoadAssetSync(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            return LoadAssetInternal(assetInfo, true);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        public AssetOperationHandle LoadAssetSync<TObject>(string location) where TObject : UnityEngine.Object
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadAssetInternal(assetInfo, true);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        public AssetOperationHandle LoadAssetSync(string location, Type type)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAssetInternal(assetInfo, true);
        }


        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public AssetOperationHandle LoadAssetAsync(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            return LoadAssetInternal(assetInfo, false);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        public AssetOperationHandle LoadAssetAsync<TObject>(string location) where TObject : UnityEngine.Object
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadAssetInternal(assetInfo, false);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        public AssetOperationHandle LoadAssetAsync(string location, Type type)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAssetInternal(assetInfo, false);
        }


        private AssetOperationHandle LoadAssetInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
        {
        #if UNITY_EDITOR
            if (assetInfo.IsInvalid == false)
            {
                BundleInfo bundleInfo = m_BundleServices.GetBundleInfo(assetInfo);
                if (bundleInfo.Bundle.IsRawFile)
                    throw new($"Cannot load raw file using {nameof(LoadAssetAsync)} method !");
            }
        #endif

            AssetOperationHandle handle = m_AssetPackageProxy.LoadAssetAsync(assetInfo);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }

    #endregion

    #region 资源加载

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public SubAssetsOperationHandle LoadSubAssetsSync(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            return LoadSubAssetsInternal(assetInfo, true);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        public SubAssetsOperationHandle LoadSubAssetsSync<TObject>(string location) where TObject : UnityEngine.Object
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadSubAssetsInternal(assetInfo, true);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        public SubAssetsOperationHandle LoadSubAssetsSync(string location, Type type)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadSubAssetsInternal(assetInfo, true);
        }


        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public SubAssetsOperationHandle LoadSubAssetsAsync(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            return LoadSubAssetsInternal(assetInfo, false);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        public SubAssetsOperationHandle LoadSubAssetsAsync<TObject>(string location) where TObject : UnityEngine.Object
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadSubAssetsInternal(assetInfo, false);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        public SubAssetsOperationHandle LoadSubAssetsAsync(string location, Type type)
        {
            DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadSubAssetsInternal(assetInfo, false);
        }


        private SubAssetsOperationHandle LoadSubAssetsInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
        {
        #if UNITY_EDITOR
            if (assetInfo.IsInvalid == false)
            {
                BundleInfo bundleInfo = m_BundleServices.GetBundleInfo(assetInfo);
                if (bundleInfo.Bundle.IsRawFile)
                {
                    throw new($"Cannot load raw file using {nameof(LoadSubAssetsAsync)} method !");
                }
            }
        #endif

            SubAssetsOperationHandle handle = m_AssetPackageProxy.LoadSubAssetsAsync(assetInfo);
            if (waitForAsyncComplete)
            {
                handle.WaitForAsyncComplete();
            }
            return handle;
        }

    #endregion

    #region 资源下载

        /// <summary>
        /// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
        /// </summary>
        /// <param name="tag">资源标签</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public PatchDownloaderOperation CreatePatchDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            FixedArray<string> array = FixedArray<string>.Get(1);
            array[0] = tag;
            return m_PlayModeServices.CreatePatchDownloaderByTags(array, downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public PatchDownloaderOperation CreatePatchDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            return m_PlayModeServices.CreatePatchDownloaderByTags(tags, downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建补丁下载器，用于下载更新当前资源版本所有的资源包文件
        /// </summary>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public PatchDownloaderOperation CreatePatchDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            return m_PlayModeServices.CreatePatchDownloaderByAll(downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建补丁下载器，用于下载更新指定的资源列表依赖的资源包文件
        /// </summary>
        /// <param name="assetInfos">资源信息列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public PatchDownloaderOperation CreateBundleDownloader(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            return m_PlayModeServices.CreatePatchDownloaderByPaths(assetInfos, downloadingMaxNumber, failedTryAgain, timeout);
        }

    #endregion

    #region 资源解压

        /// <summary>
        /// 创建补丁解压器
        /// </summary>
        /// <param name="tag">资源标签</param>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        public PatchUnpackerOperation CreatePatchUnpacker(string tag, int unpackingMaxNumber, int failedTryAgain)
        {
            DebugCheckInitialize();
            FixedArray<string> array = FixedArray<string>.Get(1);
            array[0] = tag;
            return m_PlayModeServices.CreatePatchUnpackerByTags(array, unpackingMaxNumber, failedTryAgain, int.MaxValue);
        }

        /// <summary>
        /// 创建补丁解压器
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        public PatchUnpackerOperation CreatePatchUnpacker(string[] tags, int unpackingMaxNumber, int failedTryAgain)
        {
            DebugCheckInitialize();
            return m_PlayModeServices.CreatePatchUnpackerByTags(tags, unpackingMaxNumber, failedTryAgain, int.MaxValue);
        }

        /// <summary>
        /// 创建补丁解压器
        /// </summary>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        public PatchUnpackerOperation CreatePatchUnpacker(int unpackingMaxNumber, int failedTryAgain)
        {
            DebugCheckInitialize();
            return m_PlayModeServices.CreatePatchUnpackerByAll(unpackingMaxNumber, failedTryAgain, int.MaxValue);
        }

    #endregion

    #region 内部方法

        /// <summary>
        /// 是否包含资源文件
        /// </summary>
        internal bool IsIncludeBundleFile(string cacheGuid)
        {
            // NOTE : 编辑器模拟模式下始终返回TRUE
            if (m_PlayMode == EPlayMode.EditorSimulateMode)
                return true;
            return m_PlayModeServices.ActiveManifest.IsIncludeBundleFile(cacheGuid);
        }

        /// <summary>
        /// 资源定位地址转换为资源信息类
        /// </summary>
        AssetInfo ConvertLocationToAssetInfo(string location, Type assetType)
        {
            return m_PlayModeServices.ActiveManifest.ConvertLocationToAssetInfo(location, assetType);
        }

    #endregion

    #region 调试方法

        [Conditional("DEBUG")]
        void DebugCheckInitialize()
        {
            switch (InitializeStatus)
            {
                case EOperationStatus.None: throw new("Package initialize not completed !");
                case EOperationStatus.Failed: throw new($"Package initialize failed ! {m_InitializeError}");
            }
        }

        [Conditional("DEBUG")]
        void DebugCheckUpdateManifest()
        {
            List<BundleInfo> loadedBundleInfos = m_AssetPackageProxy.GetLoadedBundleInfos();
            if (loadedBundleInfos.Count > 0)
            {
                Log.Warning($"Found loaded bundle before update manifest ! Recommended to call the  {nameof(ForceUnloadAllAssets)} method to release loaded bundle !");
            }
        }

    #endregion

    #region 调试信息

        internal DebugPackageData GetDebugPackageData()
        {
            DebugPackageData data = new()
            {
                PackageName = PackageName,
                ProviderInfos = m_AssetPackageProxy.GetDebugReportInfos()
            };
            return data;
        }

    #endregion

    }
}