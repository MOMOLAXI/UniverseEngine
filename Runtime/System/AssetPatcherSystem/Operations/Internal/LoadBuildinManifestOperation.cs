
namespace Universe
{
	internal class LoadBuildinManifestOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			LoadBuildinManifest,
			CheckDeserializeManifest,
			Done,
		}

		private readonly string m_BuildinPackageName;
		private readonly string m_BuildinPackageVersion;
		private UnityWebDataRequester m_Downloader;
		private DeserializeManifestOperation m_Deserializer;
		private ESteps m_Steps = ESteps.None;

		/// <summary>
		/// 加载的清单实例
		/// </summary>
		public PatchManifest Manifest { private set; get; }


		public LoadBuildinManifestOperation(string buildinPackageName, string buildinPackageVersion)
		{
			m_BuildinPackageName = buildinPackageName;
			m_BuildinPackageVersion = buildinPackageVersion;
		}
		internal override void Start()
		{
			m_Steps = ESteps.LoadBuildinManifest;
		}
		internal override void Update()
		{
			if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
				return;

			if (m_Steps == ESteps.LoadBuildinManifest)
			{
				if (m_Downloader == null)
				{
					string fileName = AssetSystemNameGetter.GetManifestBinaryFileName(m_BuildinPackageName, m_BuildinPackageVersion);
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
					byte[] bytesData = m_Downloader.GetData();
					m_Deserializer = new(bytesData);
					Engine.StartAsyncOperation(m_Deserializer);
					m_Steps = ESteps.CheckDeserializeManifest;
				}

				m_Downloader.Dispose();
			}

			if (m_Steps == ESteps.CheckDeserializeManifest)
			{
				Progress = m_Deserializer.Progress;
				if (m_Deserializer.IsDone == false)
					return;

				if (m_Deserializer.Status == EOperationStatus.Succeed)
				{
					Manifest = m_Deserializer.Manifest;
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
				else
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = m_Deserializer.Error;
				}
			}
		}
	}
}