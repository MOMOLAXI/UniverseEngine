using System;
using System.Collections.Generic;

namespace Universe
{
    public class BuildAssetInfo
    {
        private bool m_IsAddAssetTags;
        private readonly HashSet<string> m_ReferenceBundleNames = new();

        /// <summary>
        /// 收集器类型
        /// </summary>
        public ECollectorType CollectorType { get; }

        /// <summary>
        /// 资源包完整名称
        /// </summary>
        public string BundleName { private set; get; }

        /// <summary>
        /// 可寻址地址
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath { get; }

        /// <summary>
        /// 是否为原生资源
        /// </summary>
        public bool IsRawAsset { get; }

        /// <summary>
        /// 是否为着色器资源
        /// </summary>
        public bool IsShaderAsset { get; }

        /// <summary>
        /// 资源的分类标签
        /// </summary>
        public readonly List<string> AssetTags = new();

        /// <summary>
        /// 资源包的分类标签
        /// </summary>
        public readonly List<string> BundleTags = new();

        /// <summary>
        /// 依赖的所有资源
        /// 注意：包括零依赖资源和冗余资源（资源包名无效）
        /// </summary>
        public List<BuildAssetInfo> AllDependAssetInfos { private set; get; }


        public BuildAssetInfo(ECollectorType collectorType, string bundleName, string address, string assetPath, bool isRawAsset)
        {
            CollectorType = collectorType;
            BundleName = bundleName;
            Address = address;
            AssetPath = assetPath;
            IsRawAsset = isRawAsset;

            Type assetType = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if (assetType == typeof(UnityEngine.Shader) || assetType == typeof(UnityEngine.ShaderVariantCollection))
            {
                IsShaderAsset = true;
            }
            else
            {
                IsShaderAsset = false;
            }
        }
        public BuildAssetInfo(string assetPath)
        {
            CollectorType = ECollectorType.None;
            Address = string.Empty;
            AssetPath = assetPath;
            IsRawAsset = false;

            Type assetType = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if (assetType == typeof(UnityEngine.Shader) || assetType == typeof(UnityEngine.ShaderVariantCollection))
                IsShaderAsset = true;
            else
                IsShaderAsset = false;
        }


        /// <summary>
        /// 设置所有依赖的资源
        /// </summary>
        public void SetAllDependAssetInfos(List<BuildAssetInfo> dependAssetInfos)
        {
            if (AllDependAssetInfos != null)
                throw new("Should never get here !");

            AllDependAssetInfos = dependAssetInfos;
        }

        /// <summary>
        /// 添加资源的分类标签
        /// 说明：原始定义的资源分类标签
        /// </summary>
        public void AddAssetTags(List<string> tags)
        {
            if (m_IsAddAssetTags)
                throw new("Should never get here !");
            m_IsAddAssetTags = true;

            foreach (string tag in tags)
            {
                if (AssetTags.Contains(tag) == false)
                {
                    AssetTags.Add(tag);
                }
            }
        }

        /// <summary>
        /// 添加资源包的分类标签
        /// 说明：传染算法统计到的分类标签
        /// </summary>
        public void AddBundleTags(List<string> tags)
        {
            foreach (string tag in tags)
            {
                if (BundleTags.Contains(tag) == false)
                {
                    BundleTags.Add(tag);
                }
            }
        }

        /// <summary>
        /// 资源包名是否存在
        /// </summary>
        public bool HasBundleName()
        {
            if (string.IsNullOrEmpty(BundleName))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 添加关联的资源包名称
        /// </summary>
        public void AddReferenceBundleName(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                throw new("Should never get here !");
            }

            if (m_ReferenceBundleNames.Contains(bundleName) == false)
            {
                m_ReferenceBundleNames.Add(bundleName);
            }
        }

        /// <summary>
        /// 计算共享资源包的完整包名
        /// </summary>
        public void CalculateShareBundleName(bool uniqueBundleName, string packageName, string shadersBundleName)
        {
            if (CollectorType != ECollectorType.None)
            {
                return;
            }

            if (IsRawAsset)
            {
                throw new("Should never get here !");
            }

            if (IsShaderAsset)
            {
                BundleName = shadersBundleName;
            }
            else
            {
                if (m_ReferenceBundleNames.Count > 1)
                {
                    IPackRule packRule = PackDirectory.StaticPackRule;
                    PackRuleResult packRuleResult = packRule.GetPackRuleResult(new(AssetPath));
                    BundleName = packRuleResult.GetShareBundleName(packageName, uniqueBundleName);
                }
                else
                {
                    // 注意：被引用次数小于1的资源不需要设置资源包名称
                    BundleName = string.Empty;
                }
            }
        }
    }
}