namespace Universe
{
    [UniverseBuildTask("拷贝原生文件")]
    public class TaskCopyRawFile : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
            BuildParametersContext buildParameters = context.GetContextObject<BuildParametersContext>();
            BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();

            EBuildMode buildMode = buildParameters.Parameters.BuildMode;
            if (buildMode is EBuildMode.ForceRebuild or EBuildMode.IncrementalBuild)
            {
                CopyRawBundle(buildMapContext, buildParametersContext);
            }
        }

        /// <summary>
        /// 拷贝原生文件
        /// </summary>
        static void CopyRawBundle(BuildMapContext buildMapContext, BuildParametersContext buildParametersContext)
        {
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            foreach (BuildBundleInfo bundleInfo in buildMapContext.BundleInfos)
            {
                if (bundleInfo.IsRawFile)
                {
                    string dest = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
                    foreach (BuildAssetInfo buildAsset in bundleInfo.BuildinAssets)
                    {
                        if (buildAsset.IsRawAsset)
                            FileUtility.CopyFile(buildAsset.AssetPath, dest, true);
                    }
                }
            }
        }
    }
}