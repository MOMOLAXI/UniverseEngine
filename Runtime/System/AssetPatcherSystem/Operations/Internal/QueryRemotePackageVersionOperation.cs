namespace Universe
{
	internal class QueryRemotePackageVersionOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			DownloadPackageVersion,
			Done,
		}

		private static int RequestCount;
		private readonly IRemoteServices m_RemoteServices;
		private readonly string m_PackageName;
		private readonly bool m_AppendTimeTicks;
		private readonly int m_Timeout;
		private UnityWebDataRequester m_Downloader;
		private ESteps m_Steps = ESteps.None;

		/// <summary>
		/// 包裹版本
		/// </summary>
		public string PackageVersion { private set; get; }
		

		public QueryRemotePackageVersionOperation(IRemoteServices remoteServices, string packageName, bool appendTimeTicks, int timeout)
		{
			m_RemoteServices = remoteServices;
			m_PackageName = packageName;
			m_AppendTimeTicks = appendTimeTicks;
			m_Timeout = timeout;
		}
		internal override void Start()
		{
			RequestCount++;
			m_Steps = ESteps.DownloadPackageVersion;
		}
		internal override void Update()
		{
			if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
				return;

			if (m_Steps == ESteps.DownloadPackageVersion)
			{
				if (m_Downloader == null)
				{
					string fileName = AssetSystemNameGetter.GetPackageVersionFileName(m_PackageName);
					string webURL = GetPackageVersionRequestURL(fileName);
					Log.Info($"Beginning to request package version : {webURL}");
					m_Downloader = new();
					m_Downloader.SendRequest(webURL, m_Timeout);
				}

				Progress = m_Downloader.Progress();
				if (m_Downloader.IsDone() == false)
					return;

				if (m_Downloader.HasError())
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = m_Downloader.GetError();
				}
				else
				{
					PackageVersion = m_Downloader.GetText();
					if (string.IsNullOrEmpty(PackageVersion))
					{
						m_Steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"Remote package version is empty : {m_Downloader.URL}";
					}
					else
					{
						m_Steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
					}
				}

				m_Downloader.Dispose();
			}
		}

		private string GetPackageVersionRequestURL(string fileName)
		{
			string url;

			// 轮流返回请求地址
			if (RequestCount % 2 == 0)
				url = m_RemoteServices.GetRemoteFallbackURL(fileName);
			else
				url = m_RemoteServices.GetRemoteMainURL(fileName);

			// 在URL末尾添加时间戳
			if (m_AppendTimeTicks)
				return $"{url}?{System.DateTime.UtcNow.Ticks}";
			return url;
		}
	}
}