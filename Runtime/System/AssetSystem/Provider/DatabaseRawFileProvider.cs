
namespace Universe
{
	internal class DatabaseRawFileProvider : ProviderBase
	{
		public DatabaseRawFileProvider(AssetPackageProxy proxy, string providerGuid, AssetInfo assetInfo) : base(proxy, providerGuid, assetInfo)
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

				Status = EStatus.Checking;

				// 注意：模拟异步加载效果提前返回
				if (IsWaitForAsyncComplete == false)
					return;
			}

			if(Status == EStatus.Checking)
			{
				RawFilePath = MainAssetInfo.AssetPath;
				Status = EStatus.Succeed;
				InvokeCompletion();
			}
#endif
		}
	}
}