using UnityEngine;

namespace Universe
{
	sealed internal class BundledSubAssetsProvider : BundledProvider
	{
		private AssetBundleRequest m_CacheRequest;

		public BundledSubAssetsProvider(AssetPackageProxy proxy, string providerGuid, AssetInfo assetInfo) : base(proxy, providerGuid, assetInfo)
		{
		}
		public override void Update()
		{
			DebugBeginRecording();

			if (IsDone)
				return;

			if (Status == EStatus.None)
			{
				Status = EStatus.CheckBundle;
			}

			// 1. 检测资源包
			if (Status == EStatus.CheckBundle)
			{
				if (IsWaitForAsyncComplete)
				{
					DependBundleGroup.WaitForAsyncComplete();
					OwnerBundle.WaitForAsyncComplete();
				}

				if (DependBundleGroup.IsDone() == false)
					return;
				if (OwnerBundle.IsDone() == false)
					return;

				if (DependBundleGroup.IsSucceed() == false)
				{
					Status = EStatus.Failed;
					LastError = DependBundleGroup.GetLastError();
					InvokeCompletion();
					return;
				}

				if (OwnerBundle.Status != BundleLoaderBase.EStatus.Succeed)
				{
					Status = EStatus.Failed;
					LastError = OwnerBundle.LastError;
					InvokeCompletion();
					return;
				}

				Status = EStatus.Loading;
			}

			// 2. 加载资源对象
			if (Status == EStatus.Loading)
			{
				if (IsWaitForAsyncComplete)
				{
					if (MainAssetInfo.AssetType == null)
						AllAssetObjects = OwnerBundle.CacheBundle.LoadAssetWithSubAssets(MainAssetInfo.AssetPath);
					else
						AllAssetObjects = OwnerBundle.CacheBundle.LoadAssetWithSubAssets(MainAssetInfo.AssetPath, MainAssetInfo.AssetType);
				}
				else
				{
					if (MainAssetInfo.AssetType == null)
						m_CacheRequest = OwnerBundle.CacheBundle.LoadAssetWithSubAssetsAsync(MainAssetInfo.AssetPath);
					else
						m_CacheRequest = OwnerBundle.CacheBundle.LoadAssetWithSubAssetsAsync(MainAssetInfo.AssetPath, MainAssetInfo.AssetType);
				}
				Status = EStatus.Checking;
			}

			// 3. 检测加载结果
			if (Status == EStatus.Checking)
			{
				if (m_CacheRequest != null)
				{
					if (IsWaitForAsyncComplete)
					{
						// 强制挂起主线程（注意：该操作会很耗时）
						Log.Warning("Suspend the main thread to load unity asset.");
						AllAssetObjects = m_CacheRequest.allAssets;
					}
					else
					{
						Progress = m_CacheRequest.progress;
						if (m_CacheRequest.isDone == false)
							return;
						AllAssetObjects = m_CacheRequest.allAssets;
					}
				}

				Status = AllAssetObjects == null ? EStatus.Failed : EStatus.Succeed;
				if (Status == EStatus.Failed)
				{
					if (MainAssetInfo.AssetType == null)
						LastError = $"Failed to load sub assets : {MainAssetInfo.AssetPath} AssetType : null AssetBundle : {OwnerBundle.MainBundleInfo.Bundle.BundleName}";
					else
						LastError = $"Failed to load sub assets : {MainAssetInfo.AssetPath} AssetType : {MainAssetInfo.AssetType} AssetBundle : {OwnerBundle.MainBundleInfo.Bundle.BundleName}";
					Log.Error(LastError);
				}
				InvokeCompletion();
			}
		}
	}
}