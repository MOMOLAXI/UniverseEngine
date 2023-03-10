namespace Universe
{
    [UniverseBuildTask("获取资源构建内容")]
    public class TaskGetBuildMap : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
            BuildMapContext buildMapContext = BuildMapCreater.CreateBuildMap(buildParametersContext.Parameters.BuildMode, buildParametersContext.Parameters.PackageName);
            context.SetContextObject(buildMapContext);
            EditorLog.Info("构建内容准备完毕！");

            // 检测构建结果
            CheckBuildMapContent(buildMapContext);
        }

        /// <summary>
        /// 检测构建结果
        /// </summary>
        static void CheckBuildMapContent(BuildMapContext buildMapContext)
        {
            for (int i = 0; i < buildMapContext.BundleInfos.Count; i++)
            {
                BuildBundleInfo bundleInfo = buildMapContext.BundleInfos[i];
                // 注意：原生文件资源包只能包含一个原生文件
                bool isRawFile = bundleInfo.IsRawFile;
                if (isRawFile)
                {
                    if (bundleInfo.BuildinAssets.Count != 1)
                        throw new($"The bundle does not support multiple raw asset : {bundleInfo.BundleName}");
                    continue;
                }

                // 注意：原生文件不能被其它资源文件依赖
                for (int j = 0; j < bundleInfo.BuildinAssets.Count; j++)
                {
                    BuildAssetInfo assetInfo = bundleInfo.BuildinAssets[j];
                    if (assetInfo.AllDependAssetInfos != null)
                    {
                        foreach (BuildAssetInfo dependAssetInfo in assetInfo.AllDependAssetInfos)
                        {
                            if (dependAssetInfo.IsRawAsset)
                                throw new($"{assetInfo.AssetPath} can not depend raw asset : {dependAssetInfo.AssetPath}");
                        }
                    }
                }
            }
        }
    }
}