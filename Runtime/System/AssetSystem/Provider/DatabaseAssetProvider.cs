namespace Universe
{
	sealed internal class DatabaseAssetProvider : ProviderBase
	{
		public DatabaseAssetProvider(AssetPackageProxy proxy, string providerGuid, AssetInfo assetInfo) : base(proxy, providerGuid, assetInfo)
		{
		}
		public override void Update()
		{
#if UNITY_EDITOR
			if (IsDone)
				return;

			if (Status == EStatus.None)
			{
				// 检测资源文件是否存在
				string guid = UnityEditor.AssetDatabase.AssetPathToGUID(MainAssetInfo.AssetPath);
				if (string.IsNullOrEmpty(guid))
				{
					Status = EStatus.Failed;
					LastError = $"Not found asset : {MainAssetInfo.AssetPath}";
					Log.Error(LastError);
					InvokeCompletion();
					return;
				}

				Status = EStatus.Loading;

				// 注意：模拟异步加载效果提前返回
				if (IsWaitForAsyncComplete == false)
					return;
			}

			// 1. 加载资源对象
			if (Status == EStatus.Loading)
			{
				if (MainAssetInfo.AssetType == null)
					AssetObject = UnityEditor.AssetDatabase.LoadMainAssetAtPath(MainAssetInfo.AssetPath);
				else
					AssetObject = UnityEditor.AssetDatabase.LoadAssetAtPath(MainAssetInfo.AssetPath, MainAssetInfo.AssetType);
				Status = EStatus.Checking;
			}

			// 2. 检测加载结果
			if (Status == EStatus.Checking)
			{
				Status = AssetObject == null ? EStatus.Failed : EStatus.Succeed;
				if (Status == EStatus.Failed)
				{
					if (MainAssetInfo.AssetType == null)
						LastError = $"Failed to load asset object : {MainAssetInfo.AssetPath} AssetType : null";
					else
						LastError = $"Failed to load asset object : {MainAssetInfo.AssetPath} AssetType : {MainAssetInfo.AssetType}";
					Log.Error(LastError);
				}
				InvokeCompletion();
			}
#endif
		}
	}
}