using System.Collections.Generic;
using UnityEditor;

namespace Universe
{
    public class BuildMapContext : IContextObject
    {
        /// <summary>
        /// 参与构建的资源总数
        /// 说明：包括主动收集的资源以及其依赖的所有资源
        /// </summary>
        public int AssetFileCount;

        /// <summary>
        /// 是否启用可寻址资源定位
        /// </summary>
        public bool EnableAddressable;

        /// <summary>
        /// 资源包名唯一化
        /// </summary>
        public bool UniqueBundleName;

        /// <summary>
        /// 着色器统一的全名称
        /// </summary>
        public string ShadersBundleName;

        /// <summary>
        /// 资源包列表
        /// </summary>
        public readonly List<BuildBundleInfo> BundleInfos = new(1000);


        /// <summary>
        /// 添加一个打包资源
        /// </summary>
        public void PackAsset(BuildAssetInfo assetInfo)
        {
            string bundleName = assetInfo.BundleName;
            if (string.IsNullOrEmpty(bundleName))
            {
                throw new("Should never get here !");
            }

            if (TryGetBundleInfo(bundleName, out BuildBundleInfo bundleInfo))
            {
                bundleInfo.PackAsset(assetInfo);
            }
            else
            {
                BuildBundleInfo newBundleInfo = new(bundleName);
                newBundleInfo.PackAsset(assetInfo);
                BundleInfos.Add(newBundleInfo);
            }
        }

        /// <summary>
        /// 获取所有的打包资源
        /// </summary>
        public List<BuildAssetInfo> GetAllAssets()
        {
            List<BuildAssetInfo> result = new(BundleInfos.Count);
            foreach (BuildBundleInfo bundleInfo in BundleInfos)
            {
                result.AddRange(bundleInfo.BuildinAssets);
            }
            return result;
        }

        /// <summary>
        /// 获取AssetBundle内构建的资源路径列表
        /// </summary>
        public string[] GetBuildinAssetPaths(string bundleName)
        {
            if (TryGetBundleInfo(bundleName, out BuildBundleInfo bundleInfo))
            {
                return bundleInfo.GetBuildinAssetPaths();
            }
            throw new($"Not found {nameof(BuildBundleInfo)} : {bundleName}");
        }

        /// <summary>
        /// 获取构建管线里需要的数据
        /// </summary>
        public AssetBundleBuild[] GetPipelineBuilds()
        {
            List<AssetBundleBuild> builds = new(BundleInfos.Count);
            foreach (BuildBundleInfo bundleInfo in BundleInfos)
            {
                if (!bundleInfo.IsRawFile)
                {
                    builds.Add(bundleInfo.CreatePipelineBuild());
                }
            }
            return builds.ToArray();
        }

        /// <summary>
        /// 是否包含资源包
        /// </summary>
        public bool IsContainsBundle(string bundleName)
        {
            return TryGetBundleInfo(bundleName, out BuildBundleInfo bundleInfo);
        }

        public bool TryGetBundleInfo(string bundleName, out BuildBundleInfo result)
        {
            foreach (BuildBundleInfo bundleInfo in BundleInfos)
            {
                if (bundleInfo.BundleName == bundleName)
                {
                    result = bundleInfo;
                    return true;
                }
            }
            result = null;
            return false;
        }
    }
}