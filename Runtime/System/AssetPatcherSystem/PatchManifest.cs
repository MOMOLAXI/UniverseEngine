using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace Universe
{
    /// <summary>
    /// 补丁清单文件
    /// </summary>
    [Serializable]
    public class PatchManifest
    {
        /// <summary>
        /// 文件版本
        /// </summary>
        public string FileVersion;

        /// <summary>
        /// 启用可寻址资源定位
        /// </summary>
        public bool EnableAddressable;

        /// <summary>
        /// 文件名称样式
        /// </summary>
        public int OutputNameStyle;

        /// <summary>
        /// 资源包裹名称
        /// </summary>
        public string PackageName;

        /// <summary>
        /// 资源包裹的版本信息
        /// </summary>
        public string PackageVersion;

        /// <summary>
        /// 资源列表（主动收集的资源列表）
        /// </summary>
        public List<PatchAsset> AssetList = new();

        /// <summary>
        /// 资源包列表
        /// </summary>
        public List<PatchBundle> BundleList = new();

        /// <summary>
        /// 资源包集合（提供BundleName获取PatchBundle）
        /// </summary>
        [NonSerialized]
        public Dictionary<string, PatchBundle> BundleDic;

        /// <summary>
        /// 资源映射集合（提供AssetPath获取PatchAsset）
        /// </summary>
        [NonSerialized]
        public Dictionary<string, PatchAsset> AssetDic;

        /// <summary>
        /// 资源路径映射集合
        /// </summary>
        [NonSerialized]
        public Dictionary<string, string> AssetPathMapping;

        // 资源路径映射相关
        private bool m_IsInitAssetPathMapping;
        private bool m_LocationToLower;


        /// <summary>
        /// 初始化资源路径映射
        /// </summary>
        public void InitAssetPathMapping(bool locationToLower)
        {
            if (m_IsInitAssetPathMapping)
            {
                return;
            }

            m_IsInitAssetPathMapping = true;
            if (EnableAddressable)
            {
                if (locationToLower)
                {
                    Log.Error("Addressable not support location to lower !");
                }

                AssetPathMapping = new(AssetList.Count);
                foreach (PatchAsset patchAsset in AssetList)
                {
                    string location = patchAsset.Address;
                    if (AssetPathMapping.ContainsKey(location))
                    {
                        throw new($"Address have existed : {location}");
                    }

                    AssetPathMapping.Add(location, patchAsset.AssetPath);
                }
            }
            else
            {
                m_LocationToLower = locationToLower;
                AssetPathMapping = new(AssetList.Count * 2);
                foreach (PatchAsset patchAsset in AssetList)
                {
                    string location = patchAsset.AssetPath;
                    if (locationToLower)
                    {
                        location = location.ToLower();
                    }

                    // 添加原生路径的映射
                    if (AssetPathMapping.ContainsKey(location))
                    {
                        throw new($"AssetPath have existed : {location}");
                    }

                    AssetPathMapping.Add(location, patchAsset.AssetPath);

                    // 添加无后缀名路径的映射
                    if (Path.HasExtension(location))
                    {
                        string locationWithoutExtension = StringUtility.RemoveExtension(location);
                        if (AssetPathMapping.ContainsKey(locationWithoutExtension))
                        {
                            Log.Warning($"AssetPath have existed : {locationWithoutExtension}");
                        }
                        else
                        {
                            AssetPathMapping.Add(locationWithoutExtension, patchAsset.AssetPath);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 映射为资源路径
        /// </summary>
        public string MappingToAssetPath(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                Log.Error("Failed to mapping location to asset path, The location is null or empty.");
                return string.Empty;
            }

            if (m_LocationToLower)
            {
                location = location.ToLower();
            }

            if (AssetPathMapping.TryGetValue(location, out string assetPath))
            {
                return assetPath;
            }
            Log.Warning($"Failed to mapping location to asset path : {location}");
            return string.Empty;
        }

        /// <summary>
        /// 尝试映射为资源路径
        /// </summary>
        public string TryMappingToAssetPath(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                return string.Empty;
            }

            if (m_LocationToLower)
            {
                location = location.ToLower();
            }

            if (AssetPathMapping.TryGetValue(location, out string assetPath))
            {
                return assetPath;
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取主资源包
        /// 注意：传入的资源路径一定合法有效！
        /// </summary>
        public PatchBundle GetMainPatchBundle(string assetPath)
        {
            if (AssetDic.TryGetValue(assetPath, out PatchAsset patchAsset))
            {
                int bundleID = patchAsset.BundleID;
                if (bundleID >= 0 && bundleID < BundleList.Count)
                {
                    PatchBundle patchBundle = BundleList[bundleID];
                    return patchBundle;
                }

                throw new($"Invalid bundle id : {bundleID} Asset path : {assetPath}");
            }

            throw new("Should never get here !");
        }

        /// <summary>
        /// 获取资源依赖列表
        /// 注意：传入的资源路径一定合法有效！
        /// </summary>
        public PatchBundle[] GetAllDependencies(string assetPath)
        {
            if (AssetDic.TryGetValue(assetPath, out PatchAsset patchAsset))
            {
                List<PatchBundle> result = new(patchAsset.DependIDs.Length);
                foreach (int dependID in patchAsset.DependIDs)
                {
                    if (dependID >= 0 && dependID < BundleList.Count)
                    {
                        PatchBundle dependPatchBundle = BundleList[dependID];
                        result.Add(dependPatchBundle);
                    }
                    else
                    {
                        throw new($"Invalid bundle id : {dependID} Asset path : {assetPath}");
                    }
                }
                return result.ToArray();
            }

            throw new("Should never get here !");
        }

        /// <summary>
        /// 获取资源包名称
        /// </summary>
        public string GetBundleName(int bundleID)
        {
            if (bundleID >= 0 && bundleID < BundleList.Count)
            {
                PatchBundle patchBundle = BundleList[bundleID];
                return patchBundle.BundleName;
            }

            throw new($"Invalid bundle id : {bundleID}");
        }

        /// <summary>
        /// 尝试获取补丁资源
        /// </summary>
        public bool TryGetPatchAsset(string assetPath, out PatchAsset result)
        {
            return AssetDic.TryGetValue(assetPath, out result);
        }

        /// <summary>
        /// 尝试获取补丁资源包
        /// </summary>
        public bool TryGetPatchBundle(string bundleName, out PatchBundle result)
        {
            return BundleDic.TryGetValue(bundleName, out result);
        }

        /// <summary>
        /// 是否包含资源文件
        /// </summary>
        public bool IsIncludeBundleFile(string cacheGuid)
        {
            foreach (PatchBundle patchBundle in BundleList)
            {
                if (patchBundle.CacheGuid == cacheGuid)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        public AssetInfo[] GetAssetsInfoByTags(string[] tags)
        {
            List<AssetInfo> result = new(100);
            foreach (PatchAsset patchAsset in AssetList)
            {
                if (patchAsset.HasTag(tags))
                {
                    AssetInfo assetInfo = new(patchAsset);
                    result.Add(assetInfo);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// 资源定位地址转换为资源信息类，失败时内部会发出错误日志。
        /// </summary>
        /// <returns>如果转换失败会返回一个无效的资源信息类</returns>
        public AssetInfo ConvertLocationToAssetInfo(string location, Type assetType)
        {
            DebugCheckLocation(location);

            string assetPath = MappingToAssetPath(location);
            if (TryGetPatchAsset(assetPath, out PatchAsset patchAsset))
            {
                AssetInfo assetInfo = new(patchAsset, assetType);
                return assetInfo;
            }
            else
            {
                string error = string.IsNullOrEmpty(location) ? "The location is null or empty !" : $"The location is invalid : {location}";
                AssetInfo assetInfo = new(error);
                return assetInfo;
            }
        }

    #region 调试方法

        [Conditional("DEBUG")]
        private void DebugCheckLocation(string location)
        {
            if (string.IsNullOrEmpty(location) == false)
            {
                // 检查路径末尾是否有空格
                int index = location.LastIndexOf(" ", StringComparison.Ordinal);
                if (index != -1)
                {
                    if (location.Length == index + 1)
                    {
                        Log.Warning($"Found blank character in location : \"{location}\"");
                    }
                }

                if (location.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                {
                    Log.Warning($"Found illegal character in location : \"{location}\"");
                }
            }
        }

    #endregion

    }
}