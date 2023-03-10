
namespace Universe
{
	internal class BundledRawFileProvider : BundledProvider
	{
		public BundledRawFileProvider(AssetPackageProxy proxy, string providerGuid, AssetInfo assetInfo) : base(proxy, providerGuid, assetInfo)
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

			if (Status == EStatus.CheckBundle)
			{
				if (IsWaitForAsyncComplete)
				{
					OwnerBundle.WaitForAsyncComplete();
				}

				if (OwnerBundle.IsDone() == false)
					return;

				if (OwnerBundle.Status != BundleLoaderBase.EStatus.Succeed)
				{
					Status = EStatus.Failed;
					LastError = OwnerBundle.LastError;
					InvokeCompletion();
					return;
				}

				Status = EStatus.Checking;
			}

			if (Status == EStatus.Checking)
			{
				RawFilePath = OwnerBundle.FileLoadPath;
				Status = EStatus.Succeed;
				InvokeCompletion();
			}
		}
	}
}