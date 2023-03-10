
namespace Universe
{
	internal class QueryBuildinPackageVersionOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			LoadBuildinPackageVersionFile,
			Done,
		}

		private readonly string m_PackageName;
		private UnityWebDataRequester m_Downloader;
		private ESteps m_Steps = ESteps.None;

		/// <summary>
		/// 包裹版本
		/// </summary>
		public string PackageVersion { private set; get; }


		public QueryBuildinPackageVersionOperation(string packageName)
		{
			m_PackageName = packageName;
		}
		internal override void Start()
		{
			m_Steps = ESteps.LoadBuildinPackageVersionFile;
		}
		internal override void Update()
		{
			if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
				return;

			if (m_Steps == ESteps.LoadBuildinPackageVersionFile)
			{
				if (m_Downloader == null)
				{
					string fileName = AssetSystemNameGetter.GetPackageVersionFileName(m_PackageName);
					string filePath = AssetPath.MakeStreamingLoadPath(fileName);
					string url = AssetPath.ConvertToWWWPath(filePath);
					m_Downloader = new();
					m_Downloader.SendRequest(url);
				}

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
						Error = $"Buildin package version file content is empty !";
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
	}
}