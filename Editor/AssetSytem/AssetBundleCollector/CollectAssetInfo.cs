using System.Collections.Generic;

namespace Universe
{
	public class CollectAssetInfo
	{
		/// <summary>
		/// 收集器类型
		/// </summary>
		public ECollectorType CollectorType { get; }

		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName { get; }
		
		/// <summary>
		/// 可寻址地址
		/// </summary>
		public string Address { get; }

		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath { get; }

		/// <summary>
		/// 是否为原生资源
		/// </summary>
		public bool IsRawAsset { get; }

		/// <summary>
		/// 资源分类标签
		/// </summary>
		public List<string> AssetTags { get; }

		/// <summary>
		/// 依赖的资源列表
		/// </summary>
		public List<string> DependAssets = new();


		public CollectAssetInfo(ECollectorType collectorType, string bundleName, string address, string assetPath, bool isRawAsset, List<string> assetTags)
		{
			CollectorType = collectorType;
			BundleName = bundleName;
			Address = address;
			AssetPath = assetPath;
			IsRawAsset = isRawAsset;
			AssetTags = assetTags;		
		}
	}
}