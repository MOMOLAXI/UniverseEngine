using System.IO;

namespace Universe
{
	internal class LoadCacheManifestOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			QueryCachePackageHash,
			VerifyFileHash,
			LoadCacheManifest,
			CheckDeserializeManifest,
			Done,
		}

		private readonly string m_PackageName;
		private readonly string m_PackageVersion;
		private QueryCachePackageHashOperation m_QueryCachePackageHashOp;
		private DeserializeManifestOperation m_Deserializer;
		private string m_ManifestFilePath;
		private ESteps m_Steps = ESteps.None;

		/// <summary>
		/// 加载的清单实例
		/// </summary>
		public PatchManifest Manifest { private set; get; }


		public LoadCacheManifestOperation(string packageName, string packageVersion)
		{
			m_PackageName = packageName;
			m_PackageVersion = packageVersion;
		}
		internal override void Start()
		{
			m_Steps = ESteps.QueryCachePackageHash;
		}
		internal override void Update()
		{
			if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
				return;

			if (m_Steps == ESteps.QueryCachePackageHash)
			{
				if (m_QueryCachePackageHashOp == null)
				{
					m_QueryCachePackageHashOp = new(m_PackageName, m_PackageVersion);
					Engine.StartAsyncOperation(m_QueryCachePackageHashOp);
				}

				if (m_QueryCachePackageHashOp.IsDone == false)
					return;

				if (m_QueryCachePackageHashOp.Status == EOperationStatus.Succeed)
				{
					m_Steps = ESteps.VerifyFileHash;
				}
				else
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = m_QueryCachePackageHashOp.Error;
					ClearCacheFile();
				}
			}

			if (m_Steps == ESteps.VerifyFileHash)
			{
				m_ManifestFilePath = PersistentHelper.GetCacheManifestFilePath(m_PackageName, m_PackageVersion);
				if (File.Exists(m_ManifestFilePath) == false)
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Not found cache manifest file : {m_ManifestFilePath}";
					ClearCacheFile();
					return;
				}

				string fileHash = HashUtility.FileMD5(m_ManifestFilePath);
				if (fileHash != m_QueryCachePackageHashOp.PackageHash)
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = "Failed to verify cache manifest file hash !";
					ClearCacheFile();
				}
				else
				{
					m_Steps = ESteps.LoadCacheManifest;
				}
			}

			if (m_Steps == ESteps.LoadCacheManifest)
			{
				byte[] bytesData = File.ReadAllBytes(m_ManifestFilePath);
				m_Deserializer = new(bytesData);
				Engine.StartAsyncOperation(m_Deserializer);
				m_Steps = ESteps.CheckDeserializeManifest;
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
					ClearCacheFile();
				}
			}
		}

		private void ClearCacheFile()
		{
			// 注意：如果加载沙盒内的清单报错，为了避免流程被卡住，主动把损坏的文件删除。
			if (File.Exists(m_ManifestFilePath))
			{
				Log.Warning($"Failed to load cache manifest file : {Error}");
				Log.Warning($"Invalid cache manifest file have been removed : {m_ManifestFilePath}");
				File.Delete(m_ManifestFilePath);
			}

			string hashFilePath = PersistentHelper.GetCachePackageHashFilePath(m_PackageName, m_PackageVersion);
			if (File.Exists(hashFilePath))
			{
				File.Delete(hashFilePath);
			}
		}
	}
}