using System.IO;
using UnityEditor;
using UniverseEngineEditor;

namespace Universe
{
    [UniverseBuildTask("资源构建准备工作")]
    public class TaskPrepare : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
            BuildParameters buildParameters = buildParametersContext.Parameters;

            // 检测构建参数合法性
            if (buildParameters.BuildTarget == BuildTarget.NoTarget)
            {
                throw new("请选择目标平台");
            }

            if (string.IsNullOrEmpty(buildParameters.PackageName))
            {
                throw new("资源包名称不能为空");
            }

            if (string.IsNullOrEmpty(buildParameters.PackageVersion))
            {
                throw new("资源包版本不能为空");
            }

            if (buildParameters.BuildMode != EBuildMode.SimulateBuild)
            {
                // 检测当前是否正在构建资源包
                if (BuildPipeline.isBuildingPlayer)
                {
                    throw new("当前正在构建资源包，请结束后再试");
                }

                // 检测是否有未保存场景
                if (UniverseEditor.HasDirtyScenes())
                {
                    throw new("检测到未保存的场景文件");
                }

                // 检测首包资源标签
                if (buildParameters.CopyBuildinFileOption is ECopyBuildinFileOption.ClearAndCopyByTags or ECopyBuildinFileOption.OnlyCopyByTags)
                {
                    if (string.IsNullOrEmpty(buildParameters.CopyBuildinFileTags))
                    {
                        throw new("首包资源标签不能为空！");
                    }
                }

                // 检测包裹输出目录是否存在
                string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
                if (Directory.Exists(packageOutputDirectory))
                {
                    throw new($"本次构建的补丁目录已经存在：{packageOutputDirectory}");
                }

                // 保存改动的资源
                AssetDatabase.SaveAssets();
            }

            if (buildParameters.BuildMode == EBuildMode.ForceRebuild)
            {
                // 删除平台总目录
                string platformDirectory = $"{buildParameters.OutputRoot}/{buildParameters.PackageName}/{buildParameters.BuildTarget}";
                if (FileUtility.DeleteDirectory(platformDirectory, true))
                {
                    EditorLog.Info($"删除平台总目录：{platformDirectory}");
                }
            }

            // 如果输出目录不存在
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            if (FileUtility.CreateDirectory(pipelineOutputDirectory))
            {
                EditorLog.Info($"创建输出目录：{pipelineOutputDirectory}");
            }
        }
    }
}