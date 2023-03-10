using System;
using System.Collections.Generic;

namespace Universe
{
	[Serializable]
	public class ReportAssetInfo
	{
		/// <summary>
		/// 可寻址地址
		/// </summary>
		public string Address;

		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath;

		/// <summary>
		/// 资源GUID
		/// 说明：Meta文件记录的GUID
		/// </summary>
		public string AssetGuid;

		/// <summary>
		/// 资源的分类标签
		/// </summary>
		public string[] AssetTags;
		
		/// <summary>
		/// 所属资源包名称
		/// </summary>
		public string MainBundleName;

		/// <summary>
		/// 所属资源包的大小
		/// </summary>
		public long MainBundleSize;
		
		/// <summary>
		/// 依赖的资源包名称列表
		/// </summary>
		public List<string> DependBundles = new();

		/// <summary>
		/// 依赖的资源路径列表
		/// </summary>
		public List<string> DependAssets = new();
	}
}