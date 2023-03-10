using System;
using UnityEngine;
using UnityEngine.Build.Pipeline;
using UniverseEngineEditor;

namespace Universe
{
    [UniverseBuildTask("更新构建信息")]
    public class TaskUpdateBuildInfo : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
            BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            int outputNameStyle = (int)buildParametersContext.Parameters.OutputNameStyle;

            // 1.检测路径长度
            foreach (BuildBundleInfo bundleInfo in buildMapContext.BundleInfos)
            {
                // NOTE：检测路径长度不要超过260字符。
                string filePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
                if (filePath.Length >= 260)
                {
                    throw new($"The output bundle name is too long {filePath.Length} chars : {filePath}");
                }
            }

            // 2.更新构建输出的文件路径
            foreach (BuildBundleInfo bundleInfo in buildMapContext.BundleInfos)
            {
                bundleInfo.PatchInfo.BuildOutputFilePath = bundleInfo.IsEncryptedFile
                                                               ? bundleInfo.EncryptedFilePath
                                                               : $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
            }

            // 3.更新文件其它信息
            foreach (BuildBundleInfo bundleInfo in buildMapContext.BundleInfos)
            {
                string buildOutputFilePath = bundleInfo.PatchInfo.BuildOutputFilePath;
                bundleInfo.PatchInfo.ContentHash = GetBundleContentHash(bundleInfo, context);
                bundleInfo.PatchInfo.PatchFileHash = GetBundleFileHash(buildOutputFilePath, buildParametersContext);
                bundleInfo.PatchInfo.PatchFileCRC = GetBundleFileCRC(buildOutputFilePath, buildParametersContext);
                bundleInfo.PatchInfo.PatchFileSize = GetBundleFileSize(buildOutputFilePath, buildParametersContext);
            }

            // 4.更新补丁包输出的文件路径
            foreach (BuildBundleInfo bundleInfo in buildMapContext.BundleInfos)
            {
                string patchFileExtension = AssetManifestPatcher.GetRemoteBundleFileExtension(bundleInfo.BundleName);
                string patchFileName = AssetManifestPatcher.GetRemoteBundleFileName(outputNameStyle, bundleInfo.BundleName, patchFileExtension, bundleInfo.PatchInfo.PatchFileHash);
                bundleInfo.PatchInfo.PatchOutputFilePath = $"{packageOutputDirectory}/{patchFileName}";
            }
        }

        private string GetBundleContentHash(BuildBundleInfo bundleInfo, BuildContext context)
        {
            BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
            BuildParameters parameters = buildParametersContext.Parameters;
            EBuildMode buildMode = parameters.BuildMode;
            if (buildMode is EBuildMode.DryRunBuild or EBuildMode.SimulateBuild)
            {
                return "00000000000000000000000000000000"; //32位
            }

            if (bundleInfo.IsRawFile)
            {
                string filePath = bundleInfo.PatchInfo.BuildOutputFilePath;
                return HashUtility.FileMD5(filePath);
            }

            switch (parameters.BuildPipeline)
            {
                case EBuildPipeline.BuiltinBuildPipeline:
                {
                    TaskBuilding.BuildResultContext buildResult = context.GetContextObject<TaskBuilding.BuildResultContext>();
                    Hash128 hash = buildResult.UnityManifest.GetAssetBundleHash(bundleInfo.BundleName);
                    if (hash.isValid)
                        return hash.ToString();
                    else
                        throw new($"Not found bundle in build result : {bundleInfo.BundleName}");
                }
                case EBuildPipeline.ScriptableBuildPipeline:
                {
                    // 注意：当资源包的依赖列表发生变化的时候，ContentHash也会发生变化！
                    TaskBuildingSbp.BuildResultContext buildResult = context.GetContextObject<TaskBuildingSbp.BuildResultContext>();
                    if (buildResult.Results.BundleInfos.TryGetValue(bundleInfo.BundleName, out BundleDetails value))
                    {
                        return value.Hash.ToString();
                    }

                    throw new($"Not found bundle in build result : {bundleInfo.BundleName}");
                }
                default: throw new NotImplementedException();
            }
        }
        static string GetBundleFileHash(string filePath, BuildParametersContext buildParametersContext)
        {
            EBuildMode buildMode = buildParametersContext.Parameters.BuildMode;
            if (buildMode is EBuildMode.DryRunBuild or EBuildMode.SimulateBuild)
            {
                return "00000000000000000000000000000000"; //32位
            }

            return HashUtility.FileMD5(filePath);
        }
        static string GetBundleFileCRC(string filePath, BuildParametersContext buildParametersContext)
        {
            EBuildMode buildMode = buildParametersContext.Parameters.BuildMode;
            if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
            {
                return "00000000"; //8位
            }

            return HashUtility.FileCRC32(filePath);
        }
        static long GetBundleFileSize(string filePath, BuildParametersContext buildParametersContext)
        {
            EBuildMode buildMode = buildParametersContext.Parameters.BuildMode;
            if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
            {
                return 0;
            }
            
            return FileUtility.GetFileSize(filePath);
        }
    }
}