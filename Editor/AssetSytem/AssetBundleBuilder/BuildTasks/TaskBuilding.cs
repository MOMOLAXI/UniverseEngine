using System.IO;
using UnityEditor;
using UnityEngine;

namespace Universe
{
    [UniverseBuildTask("资源构建内容打包")]
    public class TaskBuilding : IBuildTask
    {
        public class BuildResultContext : IContextObject
        {
            public AssetBundleManifest UnityManifest;
        }

        void IBuildTask.Run(BuildContext context)
        {
            BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
            BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();

            // 模拟构建模式下跳过引擎构建
            EBuildMode buildMode = buildParametersContext.Parameters.BuildMode;
            if (buildMode == EBuildMode.SimulateBuild)
            {
                EditorLog.Info("模拟构建模式下跳过引擎构建");
                return;
            }

            // 开始构建
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            BuildAssetBundleOptions buildOptions = buildParametersContext.GetPipelineBuildOptions();
            AssetBundleManifest buildResults = BuildPipeline.BuildAssetBundles(pipelineOutputDirectory, buildMapContext.GetPipelineBuilds(), buildOptions, buildParametersContext.Parameters.BuildTarget);
            if (buildResults == null)
            {
                throw new("构建过程中发生错误！");
            }

            if (buildMode is EBuildMode.ForceRebuild or EBuildMode.IncrementalBuild)
            {
                string unityOutputManifestFilePath = $"{pipelineOutputDirectory}/{UniverseConstant.OUTPUT_FOLDER_NAME}";
                if (!File.Exists(unityOutputManifestFilePath))
                {
                    throw new("构建过程中发生严重错误！请查阅上下文日志！");
                }
            }

            EditorLog.Info("Unity引擎打包成功！");
            BuildResultContext buildResultContext = new()
            {
                UnityManifest = buildResults
            };
            
            context.SetContextObject(buildResultContext);
        }
    }
}