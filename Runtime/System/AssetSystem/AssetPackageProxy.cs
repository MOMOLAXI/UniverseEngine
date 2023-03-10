using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Universe
{
    public class AssetPackageProxy
    {
        readonly Dictionary<string, BundleLoaderBase> m_Loaders = new(5000);
        readonly List<BundleLoaderBase> m_LoaderList = new(5000);

        readonly Dictionary<string, ProviderBase> m_Providers = new(5000);
        readonly List<ProviderBase> m_ProviderList = new(5000);

        static readonly Dictionary<string, SceneOperationHandle> s_SceneHandles = new(100);
        static long s_SceenCreateCount;

        bool m_IsUnloadSafe = true;
        string m_PackageName;
        bool m_IsSimulationOnEditor;
        int m_LoadingMaxNumber;

        public IDecryptionServices DecryptionServices { get; private set; }
        public IBundleServices BundleServices { get; private set; }

        /// <summary>
        /// 初始化
        /// 注意：在使用AssetSystem之前需要初始化
        /// </summary>
        public void Initialize(string packageName,
                               bool simulationOnEditor,
                               int loadingMaxNumber,
                               IDecryptionServices decryptionServices,
                               IBundleServices bundleServices)
        {
            m_PackageName = packageName;
            m_IsSimulationOnEditor = simulationOnEditor;
            m_LoadingMaxNumber = loadingMaxNumber;
            DecryptionServices = decryptionServices;
            BundleServices = bundleServices;
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            // 更新加载器	
            foreach (BundleLoaderBase loader in m_LoaderList)
            {
                loader.Update();
            }

            // 更新资源提供者
            // 注意：循环更新的时候，可能会扩展列表
            // 注意：不能限制场景对象的加载
            m_IsUnloadSafe = false;
            int loadingCount = 0;
            for (int i = 0; i < m_ProviderList.Count; i++)
            {
                ProviderBase provider = m_ProviderList[i];
                if (provider.IsSceneProvider())
                {
                    provider.Update();
                }
                else
                {
                    if (loadingCount < m_LoadingMaxNumber)
                    {
                        provider.Update();
                    }

                    if (provider.IsDone == false)
                    {
                        loadingCount++;
                    }
                }
            }

            m_IsUnloadSafe = true;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void DestroyAll()
        {
            foreach (ProviderBase provider in m_ProviderList)
            {
                provider.Destroy();
            }

            m_ProviderList.Clear();
            m_Providers.Clear();

            foreach (BundleLoaderBase loader in m_LoaderList)
            {
                loader.Destroy(true);
            }

            m_LoaderList.Clear();
            m_Loaders.Clear();

            ClearSceneHandle();

            DecryptionServices = null;
            BundleServices = null;
        }

        /// <summary>
        /// 资源回收（卸载引用计数为零的资源）
        /// </summary>
        public void UnloadUnusedAssets()
        {
            if (m_IsUnloadSafe == false)
            {
                Log.Warning("Can not unload unused assets when processing resource loading !");
                return;
            }

            // 注意：资源包之间可能存在多层深层嵌套，需要多次循环释放。
            const int loopCount = 10;
            for (int i = 0; i < loopCount; i++)
            {
                UnloadUnusedAssetsInternal();
            }
        }

        void UnloadUnusedAssetsInternal()
        {
            if (m_IsSimulationOnEditor)
            {
                for (int i = m_ProviderList.Count - 1; i >= 0; i--)
                {
                    ProviderBase provider = m_ProviderList[i];
                    if (provider.CanDestroy())
                    {
                        provider.Destroy();
                        m_ProviderList.RemoveAt(i);
                        m_Providers.Remove(provider.ProviderGuid);
                    }
                }
            }
            else
            {
                for (int i = m_LoaderList.Count - 1; i >= 0; i--)
                {
                    BundleLoaderBase loader = m_LoaderList[i];
                    loader.TryDestroyAllProviders();
                }
                for (int i = m_LoaderList.Count - 1; i >= 0; i--)
                {
                    BundleLoaderBase loader = m_LoaderList[i];
                    if (loader.CanDestroy())
                    {
                        string bundleName = loader.MainBundleInfo.Bundle.BundleName;
                        loader.Destroy(false);
                        m_LoaderList.RemoveAt(i);
                        m_Loaders.Remove(bundleName);
                    }
                }
            }
        }

        /// <summary>
        /// 强制回收所有资源
        /// </summary>
        public void ForceUnloadAllAssets()
        {
            foreach (ProviderBase provider in m_ProviderList)
            {
                provider.Destroy();
            }

            foreach (BundleLoaderBase loader in m_LoaderList)
            {
                loader.Destroy(true);
            }

            m_ProviderList.Clear();
            m_Providers.Clear();
            m_LoaderList.Clear();
            m_Loaders.Clear();
            ClearSceneHandle();

            // 注意：调用底层接口释放所有资源
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public SceneOperationHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode, bool activateOnLoad, int priority)
        {
            if (assetInfo.IsInvalid)
            {
                Log.Error($"Failed to load scene ! {assetInfo.Error}");
                CompletedProvider completedProvider = new(assetInfo);
                completedProvider.SetCompleted(assetInfo.Error);
                return completedProvider.CreateHandle<SceneOperationHandle>();
            }

            // 如果加载的是主场景，则卸载所有缓存的场景
            if (sceneMode == LoadSceneMode.Single)
            {
                UnloadAllScene();
            }

            // 注意：同一个场景的ProviderGUID每次加载都会变化
            string providerGuid = $"{assetInfo.Guid}-{++s_SceenCreateCount}";
            ProviderBase provider;
            {
                if (m_IsSimulationOnEditor)
                {
                    provider = new DatabaseSceneProvider(this, providerGuid, assetInfo, sceneMode, activateOnLoad, priority);
                }
                else
                {
                    provider = new BundledSceneProvider(this, providerGuid, assetInfo, sceneMode, activateOnLoad, priority);
                }

                provider.InitSpawnDebugInfo();
                m_ProviderList.Add(provider);
                m_Providers.Add(providerGuid, provider);
            }

            SceneOperationHandle handle = provider.CreateHandle<SceneOperationHandle>();
            handle.PackageName = m_PackageName;
            s_SceneHandles.Add(providerGuid, handle);
            return handle;
        }

        /// <summary>
        /// 加载资源对象
        /// </summary>
        public AssetOperationHandle LoadAssetAsync(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
            {
                Log.Error($"Failed to load asset ! {assetInfo.Error}");
                CompletedProvider completedProvider = new(assetInfo);
                completedProvider.SetCompleted(assetInfo.Error);
                return completedProvider.CreateHandle<AssetOperationHandle>();
            }

            string providerGuid = assetInfo.Guid;
            ProviderBase provider = TryGetProvider(providerGuid);
            if (provider == null)
            {
                if (m_IsSimulationOnEditor)
                {
                    provider = new DatabaseAssetProvider(this, providerGuid, assetInfo);
                }
                else
                {
                    provider = new BundledAssetProvider(this, providerGuid, assetInfo);
                }

                provider.InitSpawnDebugInfo();
                m_ProviderList.Add(provider);
                m_Providers.Add(providerGuid, provider);
            }
            return provider.CreateHandle<AssetOperationHandle>();
        }

        /// <summary>
        /// 加载子资源对象
        /// </summary>
        public SubAssetsOperationHandle LoadSubAssetsAsync(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
            {
                Log.Error($"Failed to load sub assets ! {assetInfo.Error}");
                CompletedProvider completedProvider = new(assetInfo);
                completedProvider.SetCompleted(assetInfo.Error);
                return completedProvider.CreateHandle<SubAssetsOperationHandle>();
            }

            string providerGuid = assetInfo.Guid;
            ProviderBase provider = TryGetProvider(providerGuid);
            if (provider == null)
            {
                if (m_IsSimulationOnEditor)
                {
                    provider = new DatabaseSubAssetsProvider(this, providerGuid, assetInfo);
                }
                else
                {
                    provider = new BundledSubAssetsProvider(this, providerGuid, assetInfo);
                }

                provider.InitSpawnDebugInfo();
                m_ProviderList.Add(provider);
                m_Providers.Add(providerGuid, provider);
            }
            return provider.CreateHandle<SubAssetsOperationHandle>();
        }

        /// <summary>
        /// 加载原生文件
        /// </summary>
        public RawFileOperationHandle LoadRawFileAsync(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
            {
                Log.Error($"Failed to load raw file ! {assetInfo.Error}");
                CompletedProvider completedProvider = new(assetInfo);
                completedProvider.SetCompleted(assetInfo.Error);
                return completedProvider.CreateHandle<RawFileOperationHandle>();
            }

            string providerGuid = assetInfo.Guid;
            ProviderBase provider = TryGetProvider(providerGuid);
            if (provider == null)
            {
                if (m_IsSimulationOnEditor)
                {
                    provider = new DatabaseRawFileProvider(this, providerGuid, assetInfo);
                }
                else
                {
                    provider = new BundledRawFileProvider(this, providerGuid, assetInfo);
                }

                provider.InitSpawnDebugInfo();
                m_ProviderList.Add(provider);
                m_Providers.Add(providerGuid, provider);
            }
            return provider.CreateHandle<RawFileOperationHandle>();
        }

        internal void UnloadSubScene(ProviderBase provider)
        {
            string providerGuid = provider.ProviderGuid;
            if (!s_SceneHandles.ContainsKey(providerGuid))
            {
                throw new("Should never get here !");
            }

            // 释放子场景句柄
            s_SceneHandles[providerGuid].ReleaseInternal();
            s_SceneHandles.Remove(providerGuid);

            // 卸载未被使用的资源（包括场景）
            UnloadUnusedAssets();
        }
        internal void UnloadAllScene()
        {
            // 释放所有场景句柄
            foreach (KeyValuePair<string, SceneOperationHandle> valuePair in s_SceneHandles)
            {
                valuePair.Value.ReleaseInternal();
            }

            s_SceneHandles.Clear();

            // 卸载未被使用的资源（包括场景）
            UnloadUnusedAssets();
        }

        internal void ClearSceneHandle()
        {
            // 释放资源包下的所有场景
            if (BundleServices.IsServicesValid())
            {
                string packageName = m_PackageName;
                List<string> removeList = new();
                foreach (KeyValuePair<string, SceneOperationHandle> valuePair in s_SceneHandles)
                {
                    if (valuePair.Value.PackageName == packageName)
                    {
                        removeList.Add(valuePair.Key);
                    }
                }
                foreach (string key in removeList)
                {
                    s_SceneHandles.Remove(key);
                }
            }
        }

        internal BundleLoaderBase CreateOwnerAssetBundleLoader(AssetInfo assetInfo)
        {
            BundleInfo bundleInfo = BundleServices.GetBundleInfo(assetInfo);
            return CreateAssetBundleLoaderInternal(bundleInfo);
        }

        internal List<BundleLoaderBase> CreateDependAssetBundleLoaders(AssetInfo assetInfo)
        {
            BundleInfo[] depends = BundleServices.GetAllDependBundleInfos(assetInfo);
            List<BundleLoaderBase> result = new(depends.Length);
            foreach (BundleInfo bundleInfo in depends)
            {
                BundleLoaderBase dependLoader = CreateAssetBundleLoaderInternal(bundleInfo);
                result.Add(dependLoader);
            }
            return result;
        }

        internal void RemoveBundleProviders(List<ProviderBase> providers)
        {
            foreach (ProviderBase provider in providers)
            {
                m_ProviderList.Remove(provider);
                m_Providers.Remove(provider.ProviderGuid);
            }
        }
        internal bool CheckBundleDestroyed(int bundleID)
        {
            string bundleName = BundleServices.GetBundleName(bundleID);
            BundleLoaderBase loader = TryGetAssetBundleLoader(bundleName);
            if (loader == null)
            {
                return true;
            }

            return loader.IsDestroyed;
        }

        BundleLoaderBase CreateAssetBundleLoaderInternal(BundleInfo bundleInfo)
        {
            // 如果加载器已经存在
            string bundleName = bundleInfo.Bundle.BundleName;
            BundleLoaderBase loader = TryGetAssetBundleLoader(bundleName);
            if (loader != null)
            {
                return loader;
            }

            // 新增下载需求
            if (bundleInfo.Bundle.IsRawFile)
            {
                loader = new RawBundleFileLoader(this, bundleInfo);
            }
            else
            {
            #if UNITY_WEBGL
                loader = new AssetBundleWebLoader(this, bundleInfo);
            #else
                loader = new AssetBundleFileLoader(this, bundleInfo);
            #endif
            }

            m_LoaderList.Add(loader);
            m_Loaders.Add(bundleName, loader);
            return loader;
        }
        BundleLoaderBase TryGetAssetBundleLoader(string bundleName)
        {
            if (m_Loaders.TryGetValue(bundleName, out BundleLoaderBase value))
            {
                return value;
            }

            return null;
        }
        ProviderBase TryGetProvider(string providerGuid)
        {
            if (m_Providers.TryGetValue(providerGuid, out ProviderBase value))
            {
                return value;
            }

            return null;
        }

    #region 调试信息

        internal List<DebugProviderInfo> GetDebugReportInfos()
        {
            List<DebugProviderInfo> result = new(m_ProviderList.Count);
            foreach (ProviderBase provider in m_ProviderList)
            {
                DebugProviderInfo providerInfo = new()
                {
                    AssetPath = provider.MainAssetInfo.AssetPath,
                    SpawnScene = provider.SpawnScene,
                    SpawnTime = provider.SpawnTime,
                    LoadingTime = provider.LoadingTime,
                    RefCount = provider.RefCount,
                    Status = provider.Status.ToString(),
                    DependBundleInfos = new()
                };
                result.Add(providerInfo);

                if (provider is BundledProvider)
                {
                    BundledProvider temp = provider as BundledProvider;
                    temp.GetBundleDebugInfos(providerInfo.DependBundleInfos);
                }
            }
            return result;
        }
        internal List<BundleInfo> GetLoadedBundleInfos()
        {
            List<BundleInfo> result = new(100);
            result.AddRange(m_LoaderList.Select(loader => loader.MainBundleInfo));
            return result;
        }

    #endregion

    }
}