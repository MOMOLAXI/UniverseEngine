using System.Linq;
using System.Collections.Generic;

namespace Universe
{
    public static class BuildMapCreater
    {
        /// <summary>
        /// 执行资源构建上下文
        /// </summary>
        public static BuildMapContext CreateBuildMap(EBuildMode buildMode, string packageName)
        {
            Dictionary<string, BuildAssetInfo> buildAssetDic = new(1000);

            // 1. 检测配置合法性
            AssetBundleCollectorSettingData.Setting.CheckConfigError();

            // 2. 获取所有收集器收集的资源
            CollectResult buildResult = AssetBundleCollectorSettingData.Setting.GetPackageAssets(buildMode, packageName);
            List<CollectAssetInfo> allCollectAssets = buildResult.CollectAssets;

            // 3. 剔除未被引用的依赖项资源
            List<CollectAssetInfo> removeDependList = new();
            for (int i = 0; i < allCollectAssets.Count; i++)
            {
                CollectAssetInfo collectAssetInfo = allCollectAssets[i];
                if (collectAssetInfo.CollectorType == ECollectorType.DependAssetCollector)
                {
                    if (IsRemoveDependAsset(allCollectAssets, collectAssetInfo.AssetPath))
                        removeDependList.Add(collectAssetInfo);
                }
            }

            for (int i = 0; i < removeDependList.Count; i++)
            {
                CollectAssetInfo removeValue = removeDependList[i];
                allCollectAssets.Remove(removeValue);
            }

            // 4. 录入所有收集器收集的资源
            for (int i = 0; i < allCollectAssets.Count; i++)
            {
                CollectAssetInfo collectAssetInfo = allCollectAssets[i];
                if (buildAssetDic.ContainsKey(collectAssetInfo.AssetPath) == false)
                {
                    BuildAssetInfo buildAssetInfo = new(
                                                        collectAssetInfo.CollectorType, collectAssetInfo.BundleName,
                                                        collectAssetInfo.Address, collectAssetInfo.AssetPath, collectAssetInfo.IsRawAsset);
                    buildAssetInfo.AddAssetTags(collectAssetInfo.AssetTags);
                    buildAssetInfo.AddBundleTags(collectAssetInfo.AssetTags);
                    buildAssetDic.Add(collectAssetInfo.AssetPath, buildAssetInfo);
                }
                else
                {
                    throw new("Should never get here !");
                }
            }

            // 5. 录入所有收集资源的依赖资源
            for (int i = 0; i < allCollectAssets.Count; i++)
            {
                CollectAssetInfo collectAssetInfo = allCollectAssets[i];
                string collectAssetBundleName = collectAssetInfo.BundleName;
                for (int j = 0; j < collectAssetInfo.DependAssets.Count; j++)
                {
                    string dependAssetPath = collectAssetInfo.DependAssets[j];
                    if (buildAssetDic.ContainsKey(dependAssetPath))
                    {
                        buildAssetDic[dependAssetPath].AddBundleTags(collectAssetInfo.AssetTags);
                        buildAssetDic[dependAssetPath].AddReferenceBundleName(collectAssetBundleName);
                    }
                    else
                    {
                        BuildAssetInfo buildAssetInfo = new(dependAssetPath);
                        buildAssetInfo.AddBundleTags(collectAssetInfo.AssetTags);
                        buildAssetInfo.AddReferenceBundleName(collectAssetBundleName);
                        buildAssetDic.Add(dependAssetPath, buildAssetInfo);
                    }
                }
            }

            // 6. 填充所有收集资源的依赖列表
            for (int i = 0; i < allCollectAssets.Count; i++)
            {
                CollectAssetInfo collectAssetInfo = allCollectAssets[i];
                List<BuildAssetInfo> dependAssetInfos = new(collectAssetInfo.DependAssets.Count);
                for (int j = 0; j < collectAssetInfo.DependAssets.Count; j++)
                {
                    string dependAssetPath = collectAssetInfo.DependAssets[j];
                    if (buildAssetDic.TryGetValue(dependAssetPath, out BuildAssetInfo value))
                        dependAssetInfos.Add(value);
                    else
                        throw new("Should never get here !");
                }
                buildAssetDic[collectAssetInfo.AssetPath].SetAllDependAssetInfos(dependAssetInfos);
            }

            // 7. 记录关键信息
            BuildMapContext context = new()
            {
                AssetFileCount = buildAssetDic.Count,
                EnableAddressable = buildResult.Command.EnableAddressable,
                UniqueBundleName = buildResult.Command.UniqueBundleName,
                ShadersBundleName = buildResult.ShadersBundleName
            };

            // 8. 计算共享的资源包名
            CollectCommand command = buildResult.Command;
            foreach (KeyValuePair<string, BuildAssetInfo> pair in buildAssetDic)
            {
                pair.Value.CalculateShareBundleName(command.UniqueBundleName, command.PackageName, buildResult.ShadersBundleName);
            }

            // 9. 移除不参与构建的资源
            List<BuildAssetInfo> removeBuildList = new();
            foreach (KeyValuePair<string, BuildAssetInfo> pair in buildAssetDic)
            {
                BuildAssetInfo buildAssetInfo = pair.Value;
                if (buildAssetInfo.HasBundleName() == false)
                {
                    removeBuildList.Add(buildAssetInfo);
                }
            }

            foreach (BuildAssetInfo removeValue in removeBuildList)
            {
                buildAssetDic.Remove(removeValue.AssetPath);
            }

            // 10. 构建资源包
            List<BuildAssetInfo> allBuildinAssets = buildAssetDic.Values.ToList();
            if (allBuildinAssets.Count == 0)
            {
                throw new("构建的资源列表不能为空");
            }

            foreach (BuildAssetInfo assetInfo in allBuildinAssets)
            {
                context.PackAsset(assetInfo);
            }

            return context;
        }
        static bool IsRemoveDependAsset(List<CollectAssetInfo> allCollectAssets, string dependAssetPath)
        {
            for (int i = 0; i < allCollectAssets.Count; i++)
            {
                CollectAssetInfo collectAssetInfo = allCollectAssets[i];
                ECollectorType collectorType = collectAssetInfo.CollectorType;
                if (collectorType is ECollectorType.MainAssetCollector or ECollectorType.StaticAssetCollector)
                {
                    if (collectAssetInfo.DependAssets.Contains(dependAssetPath))
                    {
                        return false;
                    }
                }
            }

            EditorLog.Info($"发现未被依赖的资源并自动移除 : {dependAssetPath}");
            return true;
        }
    }
}