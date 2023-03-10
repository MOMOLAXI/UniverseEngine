using System.Collections.Generic;

namespace Universe
{
	internal abstract class BundledProvider : ProviderBase
	{
		protected BundleLoaderBase OwnerBundle { private set; get; }
		protected DependAssetBundleGroup DependBundleGroup { private set; get; }

		public BundledProvider(AssetPackageProxy proxy, string providerGuid, AssetInfo assetInfo) : base(proxy, providerGuid, assetInfo)
		{
			OwnerBundle = proxy.CreateOwnerAssetBundleLoader(assetInfo);
			OwnerBundle.Reference();
			OwnerBundle.AddProvider(this);

			var dependBundles = proxy.CreateDependAssetBundleLoaders(assetInfo);
			DependBundleGroup = new(dependBundles);
			DependBundleGroup.Reference();
		}
		public override void Destroy()
		{
			base.Destroy();

			// 释放资源包
			if (OwnerBundle != null)
			{
				OwnerBundle.Release();
				OwnerBundle = null;
			}
			if (DependBundleGroup != null)
			{
				DependBundleGroup.Release();
				DependBundleGroup = null;
			}
		}

		/// <summary>
		/// 获取下载报告
		/// </summary>
		public override DownloadReport GetDownloadReport()
		{
			DownloadReport result = new()
			{
				TotalSize = (ulong)OwnerBundle.MainBundleInfo.Bundle.FileSize,
				DownloadedBytes = OwnerBundle.DownloadedBytes
			};
			foreach (var dependBundle in DependBundleGroup.DependBundles)
			{
				result.TotalSize += (ulong)dependBundle.MainBundleInfo.Bundle.FileSize;
				result.DownloadedBytes += dependBundle.DownloadedBytes;
			}
			result.Progress = (float)result.DownloadedBytes / result.TotalSize;
			return result;
		}

		/// <summary>
		/// 获取资源包的调试信息列表
		/// </summary>
		internal void GetBundleDebugInfos(List<DebugBundleInfo> output)
		{
			var bundleInfo = new DebugBundleInfo
			{
				BundleName = OwnerBundle.MainBundleInfo.Bundle.BundleName,
				RefCount = OwnerBundle.RefCount,
				Status = OwnerBundle.Status.ToString()
			};
			output.Add(bundleInfo);

			DependBundleGroup.GetBundleDebugInfos(output);
		}
	}
}