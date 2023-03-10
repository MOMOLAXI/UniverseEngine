
namespace Universe
{
	internal class DownloadManifestOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			DownloadPackageHashFile,
			DownloadManifestFile,
			Done,
		}

		private static int RequestCount;
		private readonly IRemoteServices m_RemoteServices;
		private readonly string m_PackageName;
		private readonly string m_PackageVersion;
		private readonly int m_Timeout;
		private UnityWebFileRequester m_Downloader1;
		private UnityWebFileRequester m_Downloader2;
		private ESteps m_Steps = ESteps.None;

		internal DownloadManifestOperation(IRemoteServices remoteServices, string packageName, string packageVersion, int timeout)
		{
			m_RemoteServices = remoteServices;
			m_PackageName = packageName;
			m_PackageVersion = packageVersion;
			m_Timeout = timeout;
		}
		internal override void Start()
		{
			RequestCount++;
			m_Steps = ESteps.DownloadPackageHashFile;
		}
		internal override void Update()
		{
			if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
				return;

			if (m_Steps == ESteps.DownloadPackageHashFile)
			{
				if (m_Downloader1 == null)
				{
					string savePath = PersistentHelper.GetCachePackageHashFilePath(m_PackageName, m_PackageVersion);
					string fileName = AssetSystemNameGetter.GetPackageHashFileName(m_PackageName, m_PackageVersion);
					string webURL = GetDownloadRequestURL(fileName);
					Log.Info($"Beginning to download package hash file : {webURL}");
					m_Downloader1 = new();
					m_Downloader1.SendRequest(webURL, savePath, m_Timeout);
				}

				m_Downloader1.CheckTimeout();
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
					m_Steps = ESteps.DownloadManifestFile;
				}

				m_Downloader1.Dispose();
			}

			if (m_Steps == ESteps.DownloadManifestFile)
			{
				if (m_Downloader2 == null)
				{
					string savePath = PersistentHelper.GetCacheManifestFilePath(m_PackageName, m_PackageVersion);
					string fileName = AssetSystemNameGetter.GetManifestBinaryFileName(m_PackageName, m_PackageVersion);
					string webURL = GetDownloadRequestURL(fileName);
					Log.Info($"Beginning to download manifest file : {webURL}");
					m_Downloader2 = new();
					m_Downloader2.SendRequest(webURL, savePath, m_Timeout);
				}

				m_Downloader2.CheckTimeout();
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

		private string GetDownloadRequestURL(string fileName)
		{
			// 轮流返回请求地址
			if (RequestCount % 2 == 0)
				return m_RemoteServices.GetRemoteFallbackURL(fileName);
			return m_RemoteServices.GetRemoteMainURL(fileName);
		}
	}
}