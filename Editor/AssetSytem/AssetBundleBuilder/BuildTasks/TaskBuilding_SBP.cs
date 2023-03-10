using System.Collections.Generic;

using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;

namespace Universe
{
	[UniverseBuildTask("资源构建内容打包")]
	public class TaskBuildingSbp : IBuildTask
	{
		public class BuildResultContext : IContextObject
		{
			public IBundleBuildResults Results;
		}

		void IBuildTask.Run(BuildContext context)
		{
			BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
			BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();

			// 模拟构建模式下跳过引擎构建
			EBuildMode buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.SimulateBuild)
				return;

			// 构建内容
			BundleBuildContent buildContent = new(buildMapContext.GetPipelineBuilds());

			// 开始构建
			BundleBuildParameters buildParameters = buildParametersContext.GetSbpBuildParameters();
			IList<UnityEditor.Build.Pipeline.Interfaces.IBuildTask> taskList = SbpBuildTasks.Create(buildMapContext.ShadersBundleName);
		
			ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParameters, buildContent, out IBundleBuildResults buildResults, taskList);
			if (exitCode < 0)
			{
				throw new($"构建过程中发生错误 : {exitCode}");
			}

			EditorLog.Info("Unity引擎打包成功！");
			BuildResultContext buildResultContext = new()
			{
				Results = buildResults
			};
			
			context.SetContextObject(buildResultContext);
		}
	}
}