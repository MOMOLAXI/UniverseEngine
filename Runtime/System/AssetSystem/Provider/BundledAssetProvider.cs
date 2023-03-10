using UnityEngine;

namespace Universe
{
	sealed internal class BundledAssetProvider : BundledProvider
	{
		private AssetBundleRequest m_CacheRequest;

		public BundledAssetProvider(AssetPackageProxy proxy, string providerGuid, AssetInfo assetInfo) : base(proxy, providerGuid, assetInfo)
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

				if (OwnerBundle.CacheBundle == null)
				{
					if (OwnerBundle.IsDestroyed)
						throw new("Should never get here !");
					Status = EStatus.Failed;
					LastError = $"The bundle {OwnerBundle.MainBundleInfo.Bundle.BundleName} has been destroyed by unity bugs !";
					Log.Error(LastError);
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
						AssetObject = OwnerBundle.CacheBundle.LoadAsset(MainAssetInfo.AssetPath);
					else
						AssetObject = OwnerBundle.CacheBundle.LoadAsset(MainAssetInfo.AssetPath, MainAssetInfo.AssetType);
				}
				else
				{
					if (MainAssetInfo.AssetType == null)
						m_CacheRequest = OwnerBundle.CacheBundle.LoadAssetAsync(MainAssetInfo.AssetPath);
					else
						m_CacheRequest = OwnerBundle.CacheBundle.LoadAssetAsync(MainAssetInfo.AssetPath, MainAssetInfo.AssetType);
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
						AssetObject = m_CacheRequest.asset;
					}
					else
					{
						Progress = m_CacheRequest.progress;
						if (m_CacheRequest.isDone == false)
							return;
						AssetObject = m_CacheRequest.asset;
					}
				}

				Status = AssetObject == null ? EStatus.Failed : EStatus.Succeed;
				if (Status == EStatus.Failed)
				{
					if (MainAssetInfo.AssetType == null)
						LastError = $"Failed to load asset : {MainAssetInfo.AssetPath} AssetType : null AssetBundle : {OwnerBundle.MainBundleInfo.Bundle.BundleName}";
					else
						LastError = $"Failed to load asset : {MainAssetInfo.AssetPath} AssetType : {MainAssetInfo.AssetType} AssetBundle : {OwnerBundle.MainBundleInfo.Bundle.BundleName}";
					Log.Error(LastError);
				}
				InvokeCompletion();
			}
		}
	}
}