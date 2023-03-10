
namespace Universe
{
	internal class UnpackBuildinManifestOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			UnpackManifestHashFile,
			UnpackManifestFile,
			Done,
		}

		private readonly string m_BuildinPackageName;
		private readonly string m_BuildinPackageVersion;
		private UnityWebFileRequester m_Downloader1;
		private UnityWebFileRequester m_Downloader2;
		private ESteps m_Steps = ESteps.None;

		public UnpackBuildinManifestOperation(string buildinPackageName, string buildinPackageVersion)
		{
			m_BuildinPackageName = buildinPackageName;
			m_BuildinPackageVersion = buildinPackageVersion;
		}
		internal override void Start()
		{
			m_Steps = ESteps.UnpackManifestHashFile;
		}
		internal override void Update()
		{
			if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
				return;

			if(m_Steps == ESteps.UnpackManifestHashFile)
			{
				if (m_Downloader1 == null)
				{
					string savePath = PersistentHelper.GetCachePackageHashFilePath(m_BuildinPackageName, m_BuildinPackageVersion);
					string fileName = AssetSystemNameGetter.GetPackageHashFileName(m_BuildinPackageName, m_BuildinPackageVersion);
					string filePath = AssetPath.MakeStreamingLoadPath(fileName);
					string url = AssetPath.ConvertToWWWPath(filePath);
					m_Downloader1 = new();
					m_Downloader1.SendRequest(url, savePath);
				}

				if (m_Downloader1.IsDone() == false)
					return;

				if (m_Downloader1.HasError())
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = m_Downloader1.GetError();
				}
				else
				{
					m_Steps = ESteps.UnpackManifestFile;
				}

				m_Downloader1.Dispose();
			}

			if (m_Steps == ESteps.UnpackManifestFile)
			{
				if (m_Downloader2 == null)
				{
					string savePath = PersistentHelper.GetCacheManifestFilePath(m_BuildinPackageName, m_BuildinPackageVersion);
					string fileName = AssetSystemNameGetter.GetManifestBinaryFileName(m_BuildinPackageName, m_BuildinPackageVersion);
					string filePath = AssetPath.MakeStreamingLoadPath(fileName);
					string url = AssetPath.ConvertToWWWPath(filePath);
					m_Downloader2 = new();
					m_Downloader2.SendRequest(url, savePath);
				}

				if (m_Downloader2.IsDone() == false)
					return;

				if (m_Downloader2.HasError())
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = m_Downloader2.GetError();
				}
				else
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}

				m_Downloader2.Dispose();
			}
		}
	}
}