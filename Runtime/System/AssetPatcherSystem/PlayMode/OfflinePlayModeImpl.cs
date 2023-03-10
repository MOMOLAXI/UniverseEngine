using System.Collections.Generic;

namespace Universe
{
	internal class OfflinePlayModeImpl : IPlayModeServices, IBundleServices
	{
		private PatchManifest m_ActiveManifest;
		private bool m_LocationToLower;

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(string packageName, bool locationToLower)
		{
			m_LocationToLower = locationToLower;
			OfflinePlayModeInitializationOperation operation = new(this, packageName);
			Engine.StartAsyncOperation(operation);
			return operation;
		}

		#region IPlayModeServices接口
		public PatchManifest ActiveManifest
		{
			set
			{
				m_ActiveManifest = value;
				m_ActiveManifest.InitAssetPathMapping(m_LocationToLower);
			}
			get => m_ActiveManifest;
		}
		
		public bool IsBuildinPatchBundle(PatchBundle patchBundle)
		{
			return true;
		}

		UpdatePackageVersionOperation IPlayModeServices.UpdatePackageVersionAsync(bool appendTimeTicks, int timeout)
		{
			OfflinePlayModeUpdatePackageVersionOperation operation = new();
			Engine.StartAsyncOperation(operation);
			return operation;
		}
		UpdatePackageManifestOperation IPlayModeServices.UpdatePackageManifestAsync(string packageVersion, int timeout)
		{
			OfflinePlayModeUpdatePackageManifestOperation operation = new();
			Engine.StartAsyncOperation(operation);
			return operation;
		}
		PreDownloadPackageOperation IPlayModeServices.PreDownloadPackageAsync(string packageVersion, int timeout)
		{
			OfflinePlayModePreDownloadPackageOperation operation = new();
			Engine.StartAsyncOperation(operation);
			return operation;
		}

		PatchDownloaderOperation IPlayModeServices.CreatePatchDownloaderByAll(int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}
		PatchDownloaderOperation IPlayModeServices.CreatePatchDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}
		PatchDownloaderOperation IPlayModeServices.CreatePatchDownloaderByPaths(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			return PatchDownloaderOperation.CreateEmptyDownloader(downloadingMaxNumber, failedTryAgain, timeout);
		}

		PatchUnpackerOperation IPlayModeServices.CreatePatchUnpackerByAll(int upackingMaxNumber, int failedTryAgain, int timeout)
		{
			return PatchUnpackerOperation.CreateEmptyUnpacker(upackingMaxNumber, failedTryAgain, timeout);
		}
		PatchUnpackerOperation IPlayModeServices.CreatePatchUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout)
		{
			return PatchUnpackerOperation.CreateEmptyUnpacker(upackingMaxNumber, failedTryAgain, timeout);
		}
		#endregion

		#region IBundleServices接口
		private BundleInfo CreateBundleInfo(PatchBundle patchBundle)
		{
			if (patchBundle == null)
				throw new("Should never get here !");

			// 查询沙盒资源
			if (CacheSystem.IsCached(patchBundle.PackageName, patchBundle.CacheGuid))
			{
				BundleInfo bundleInfo = new(patchBundle, BundleInfo.ELoadMode.LoadFromCache);
				return bundleInfo;
			}

			// 查询APP资源
			{
				BundleInfo bundleInfo = new(patchBundle, BundleInfo.ELoadMode.LoadFromStreaming);
				return bundleInfo;
			}
		}
		BundleInfo IBundleServices.GetBundleInfo(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new("Should never get here !");

			// 注意：如果补丁清单里未找到资源包会抛出异常！
			PatchBundle patchBundle = m_ActiveManifest.GetMainPatchBundle(assetInfo.AssetPath);
			return CreateBundleInfo(patchBundle);
		}
		BundleInfo[] IBundleServices.GetAllDependBundleInfos(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new("Should never get here !");

			// 注意：如果补丁清单里未找到资源包会抛出异常！
			PatchBundle[] depends = m_ActiveManifest.GetAllDependencies(assetInfo.AssetPath);
			List<BundleInfo> result = new(depends.Length);
			foreach (PatchBundle patchBundle in depends)
			{
				BundleInfo bundleInfo = CreateBundleInfo(patchBundle);
				result.Add(bundleInfo);
			}
			return result.ToArray();
		}
		string IBundleServices.GetBundleName(int bundleID)
		{
			return m_ActiveManifest.GetBundleName(bundleID);
		}
		bool IBundleServices.IsServicesValid()
		{
			return m_ActiveManifest != null;
		}
		#endregion
	}
}