using UniverseEngineEditor;

namespace Universe
{
    [UniverseBuildTask("制作补丁包")]
    public class TaskCreatePatchPackage : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            BuildParametersContext buildParameters = context.GetContextObject<BuildParametersContext>();
            BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();
            EBuildMode buildMode = buildParameters.Parameters.BuildMode;
            if (buildMode is EBuildMode.ForceRebuild or EBuildMode.IncrementalBuild)
            {
                CopyPatchFiles(buildParameters, buildMapContext);
            }
        }

        /// <summary>
        /// 拷贝补丁文件到补丁包目录
        /// </summary>
        static void CopyPatchFiles(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext)
        {
            BuildParameters buildParameters = buildParametersContext.Parameters;
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            EditorLog.Info($"开始拷贝补丁文件到补丁包目录：{packageOutputDirectory}");

            switch (buildParameters.BuildPipeline)
            {
                case EBuildPipeline.ScriptableBuildPipeline:
                {
                    // 拷贝构建日志
                    {
                        string sourcePath = $"{pipelineOutputDirectory}/buildlogtep.json";
                        string destPath = $"{packageOutputDirectory}/buildlogtep.json";
                        FileUtility.CopyFile(sourcePath, destPath, true);
                    }

                    // 拷贝代码防裁剪配置
                    if (buildParameters.SbpParameters.WriteLinkXML)
                    {
                        string sourcePath = $"{pipelineOutputDirectory}/link.xml";
                        string destPath = $"{packageOutputDirectory}/link.xml";
                        FileUtility.CopyFile(sourcePath, destPath, true);
                    }
                    break;
                }
                case EBuildPipeline.BuiltinBuildPipeline:
                {
                    // 拷贝UnityManifest序列化文件
                    {
                        string sourcePath = $"{pipelineOutputDirectory}/{UniverseConstant.OUTPUT_FOLDER_NAME}";
                        string destPath = $"{packageOutputDirectory}/{UniverseConstant.OUTPUT_FOLDER_NAME}";
                        FileUtility.CopyFile(sourcePath, destPath, true);
                    }

                    // 拷贝UnityManifest文本文件
                    {
                        string sourcePath = $"{pipelineOutputDirectory}/{UniverseConstant.OUTPUT_FOLDER_NAME}.manifest";
                        string destPath = $"{packageOutputDirectory}/{UniverseConstant.OUTPUT_FOLDER_NAME}.manifest";
                        FileUtility.CopyFile(sourcePath, destPath, true);
                    }
                    break;
                }
                default: throw new System.NotImplementedException();
            }

            // 拷贝所有补丁文件
            int progressValue = 0;
            int patchFileTotalCount = buildMapContext.BundleInfos.Count;
            foreach (BuildBundleInfo bundleInfo in buildMapContext.BundleInfos)
            {
                FileUtility.CopyFile(bundleInfo.PatchInfo.BuildOutputFilePath, bundleInfo.PatchInfo.PatchOutputFilePath, true);
                UniverseEditor.DisplayProgressBar("拷贝补丁文件", ++progressValue, patchFileTotalCount);
            }
            
            UniverseEditor.ClearProgressBar();
        }
    }
}