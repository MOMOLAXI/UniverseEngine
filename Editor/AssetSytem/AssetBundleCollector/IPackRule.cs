
namespace Universe
{
	public struct PackRuleData
	{
		public string AssetPath;
		public string CollectPath;
		public string GroupName;
		public string UserData;

		public PackRuleData(string assetPath)
		{
			AssetPath = assetPath;
			CollectPath = string.Empty;
			GroupName = string.Empty;
			UserData = string.Empty;
		}
		public PackRuleData(string assetPath, string collectPath, string groupName, string userData)
		{
			AssetPath = assetPath;
			CollectPath = collectPath;
			GroupName = groupName;
			UserData = userData;
		}
	}

	public struct PackRuleResult
	{
		private readonly string m_BundleName;
		private readonly string m_BundleExtension;

		public PackRuleResult(string bundleName, string bundleExtension)
		{
			m_BundleName = bundleName;
			m_BundleExtension = bundleExtension;
		}

		/// <summary>
		/// 获取主资源包全名称
		/// </summary>
		public string GetMainBundleName(string packageName, bool uniqueBundleName)
		{
			string fullName;
			string bundleName = FileUtility.GetRegularPath(m_BundleName).Replace('/', '_').Replace('.', '_').ToLower();
			if (uniqueBundleName)
				fullName = $"{packageName}_{bundleName}.{m_BundleExtension}";
			else
				fullName = $"{bundleName}.{m_BundleExtension}";
			return fullName.ToLower();
		}
		
		/// <summary>
		/// 获取共享资源包全名称
		/// </summary>
		public string GetShareBundleName(string packageName, bool uniqueBundleName)
		{
			string fullName;
			string bundleName = FileUtility.GetRegularPath(m_BundleName).Replace('/', '_').Replace('.', '_').ToLower();
			if (uniqueBundleName)
				fullName = $"{packageName}_share_{bundleName}.{m_BundleExtension}";
			else
				fullName = $"share_{bundleName}.{m_BundleExtension}";
			return fullName.ToLower();
		}
	}

	/// <summary>
	/// 资源打包规则接口
	/// </summary>
	public interface IPackRule
	{
		/// <summary>
		/// 获取打包规则结果
		/// </summary>
		PackRuleResult GetPackRuleResult(PackRuleData data);

		/// <summary>
		/// 是否为原生文件打包规则
		/// </summary>
		bool IsRawFilePackRule();
	}
}