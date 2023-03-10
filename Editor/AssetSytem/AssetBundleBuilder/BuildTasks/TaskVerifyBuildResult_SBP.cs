using System.Linq;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline.Interfaces;

namespace Universe
{
    [UniverseBuildTask("验证构建结果")]
    public class TaskVerifyBuildResultSbp : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();

            // 模拟构建模式下跳过验证
            if (buildParametersContext.Parameters.BuildMode == EBuildMode.SimulateBuild)
                return;

            // 验证构建结果
            if (buildParametersContext.Parameters.VerifyBuildingResult)
            {
                TaskBuildingSbp.BuildResultContext buildResultContext = context.GetContextObject<TaskBuildingSbp.BuildResultContext>();
                VerifyingBuildingResult(context, buildResultContext.Results);
            }
        }

        /// <summary>
        /// 验证构建结果
        /// </summary>
        static void VerifyingBuildingResult(BuildContext context, IBundleBuildResults buildResults)
        {
            BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();
            List<string> buildedBundles = buildResults.BundleInfos.Keys.ToList();

            // 1. 过滤掉原生Bundle
            List<string> expectBundles = buildMapContext.BundleInfos.Where(t => t.IsRawFile == false).Select(t => t.BundleName).ToList();

            // 2. 验证Bundle
            List<string> exceptBundleList1 = buildedBundles.Except(expectBundles).ToList();
            if (exceptBundleList1.Count > 0)
            {
                foreach (string exceptBundle in exceptBundleList1)
                {
                    EditorLog.Warning($"差异资源包: {exceptBundle}");
                }
                
                throw new("存在差异资源包！请查看警告信息！");
            }

            // 3. 验证Bundle
            List<string> exceptBundleList2 = expectBundles.Except(buildedBundles).ToList();
            if (exceptBundleList2.Count > 0)
            {
                foreach (string exceptBundle in exceptBundleList2)
                {
                    EditorLog.Warning($"差异资源包: {exceptBundle}");
                }
                
                throw new("存在差异资源包！请查看警告信息！");
            }

            EditorLog.Info("构建结果验证成功！");
        }
    }
}