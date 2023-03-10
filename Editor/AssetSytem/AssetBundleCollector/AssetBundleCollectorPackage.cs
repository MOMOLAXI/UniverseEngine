using System;
using System.Collections.Generic;
using System.Linq;

namespace Universe
{
	[Serializable]
	public class AssetBundleCollectorPackage
	{
		/// <summary>
		/// 包裹名称
		/// </summary>
		public string PackageName = string.Empty;

		/// <summary>
		/// 包裹描述
		/// </summary>
		public string PackageDesc = string.Empty;

		/// <summary>
		/// 分组列表
		/// </summary>
		public List<AssetBundleCollectorGroup> Groups = new();


		/// <summary>
		/// 检测配置错误
		/// </summary>
		public void CheckConfigError()
		{
			foreach (AssetBundleCollectorGroup group in Groups)
			{
				group.CheckConfigError();
			}
		}

		/// <summary>
		/// 修复配置错误
		/// </summary>
		public bool FixConfigError()
		{
			bool isFixed = false;
			foreach (AssetBundleCollectorGroup group in Groups)
			{
				if (group.FixConfigError())
				{
					isFixed = true;
				}
			}
			return isFixed;
		}

		/// <summary>
		/// 获取打包收集的资源文件
		/// </summary>
		public List<CollectAssetInfo> GetAllCollectAssets(CollectCommand command)
		{
			Dictionary<string, CollectAssetInfo> result = new(10000);

			// 收集打包资源
			foreach (AssetBundleCollectorGroup group in Groups)
			{
				List<CollectAssetInfo> temper = group.GetAllCollectAssets(command);
				foreach (CollectAssetInfo assetInfo in temper)
				{
					if (result.ContainsKey(assetInfo.AssetPath) == false)
						result.Add(assetInfo.AssetPath, assetInfo);
					else
						throw new($"The collecting asset file is existed : {assetInfo.AssetPath}");
				}
			}

			// 检测可寻址地址是否重复
			if (command.EnableAddressable)
			{
				HashSet<string> adressTemper = new();
				foreach (KeyValuePair<string, CollectAssetInfo> collectInfoPair in result)
				{
					if (collectInfoPair.Value.CollectorType == ECollectorType.MainAssetCollector)
					{
						string address = collectInfoPair.Value.Address;
						if (adressTemper.Contains(address) == false)
							adressTemper.Add(address);
						else
							throw new($"The address is existed : {address}");
					}
				}
			}

			// 返回列表
			return result.Values.ToList();
		}

		/// <summary>
		/// 获取所有的资源标签
		/// </summary>
		public List<string> GetAllTags()
		{
			HashSet<string> result = new();
			foreach (AssetBundleCollectorGroup group in Groups)
			{
				List<string> groupTags = group.AssetTags.StringToStringList(';');
				foreach (string tag in groupTags)
				{
					if (result.Contains(tag) == false)
						result.Add(tag);
				}

				foreach (AssetBundleCollector collector in group.Collectors)
				{
					List<string> collectorTags = collector.AssetTags.StringToStringList(';');
					foreach (string tag in collectorTags)
					{
						if (result.Contains(tag) == false)
							result.Add(tag);
					}
				}
			}
			return result.ToList();
		}
	}
}