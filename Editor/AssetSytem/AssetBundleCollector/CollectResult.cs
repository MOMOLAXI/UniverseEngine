using System.Collections.Generic;

namespace Universe
{
	public class CollectResult
	{
		/// <summary>
		/// 收集命令
		/// </summary>
		public CollectCommand Command { get; }

		/// <summary>
		/// 着色器统一全名称
		/// </summary>
		public string ShadersBundleName { get; }

		/// <summary>
		/// 收集的资源信息列表
		/// </summary>
		public List<CollectAssetInfo> CollectAssets { private set; get; }


		public CollectResult(CollectCommand command)
		{
			Command = command;

			// 着色器统一全名称
			PackRuleResult packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
			ShadersBundleName = packRuleResult.GetMainBundleName(command.PackageName, command.UniqueBundleName);
		}

		public void SetCollectAssets(List<CollectAssetInfo> collectAssets)
		{
			CollectAssets = collectAssets;
		}
	}
}