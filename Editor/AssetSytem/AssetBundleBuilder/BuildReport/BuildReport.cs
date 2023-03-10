using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Universe
{
    /// <summary>
    /// 构建报告
    /// </summary>
    [Serializable]
    public class BuildReport
    {
        /// <summary>
        /// 汇总信息
        /// </summary>
        public ReportSummary Summary = new();

        /// <summary>
        /// 资源对象列表
        /// </summary>
        public List<ReportAssetInfo> AssetInfos = new();

        /// <summary>
        /// 资源包列表
        /// </summary>
        public List<ReportBundleInfo> BundleInfos = new();

        /// <summary>
        /// 获取资源包信息类
        /// </summary>
        public ReportBundleInfo GetBundleInfo(string bundleName)
        {
            foreach (ReportBundleInfo bundleInfo in BundleInfos)
            {
                if (bundleInfo.BundleName == bundleName)
                    return bundleInfo;
            }

            throw new($"Not found bundle : {bundleName}");
        }

        /// <summary>
        /// 获取资源信息类
        /// </summary>
        public ReportAssetInfo GetAssetInfo(string assetPath)
        {
            foreach (ReportAssetInfo assetInfo in AssetInfos)
            {
                if (assetInfo.AssetPath == assetPath)
                    return assetInfo;
            }

            throw new($"Not found asset : {assetPath}");
        }

        public static void Serialize(string savePath, BuildReport buildReport)
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            string json = JsonUtility.ToJson(buildReport, true);
            FileUtility.CreateFile(savePath, json);
        }
        public static BuildReport Deserialize(string jsonData)
        {
            BuildReport report = JsonUtility.FromJson<BuildReport>(jsonData);
            return report;
        }
    }
}