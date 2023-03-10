using System.IO;

namespace Universe
{
	internal class LoadEditorManifestOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			LoadEditorManifest,
			CheckDeserializeManifest,
			Done,
		}

		private readonly string m_ManifestFilePath;
		private DeserializeManifestOperation m_Deserializer;
		private ESteps m_Steps = ESteps.None;

		/// <summary>
		/// 加载的清单实例
		/// </summary>
		public PatchManifest Manifest { private set; get; }


		public LoadEditorManifestOperation(string manifestFilePath)
		{
			m_ManifestFilePath = manifestFilePath;
		}
		internal override void Start()
		{
			m_Steps = ESteps.LoadEditorManifest;
		}
		internal override void Update()
		{
			if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
				return;

			if (m_Steps == ESteps.LoadEditorManifest)
			{
				if (File.Exists(m_ManifestFilePath) == false)
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Not found simulation manifest file : {m_ManifestFilePath}";
					return;
				}
				
				Log.Info($"Load editor manifest file : {m_ManifestFilePath}");
				byte[] bytesData = FileUtility.ReadAllBytes(m_ManifestFilePath);
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
				}
			}
		}
	}
}