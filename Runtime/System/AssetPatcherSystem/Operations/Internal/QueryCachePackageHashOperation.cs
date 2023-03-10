using System.IO;

namespace Universe
{
	internal class QueryCachePackageHashOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			LoadCachePackageHashFile,
			Done,
		}

		private readonly string m_PackageName;
		private readonly string m_PackageVersion;
		private ESteps m_Steps = ESteps.None;

		/// <summary>
		/// 包裹哈希值
		/// </summary>
		public string PackageHash { private set; get; }


		public QueryCachePackageHashOperation(string packageName, string packageVersion)
		{
			m_PackageName = packageName;
			m_PackageVersion = packageVersion;
		}
		internal override void Start()
		{
			m_Steps = ESteps.LoadCachePackageHashFile;
		}
		internal override void Update()
		{
			if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
				return;

			if (m_Steps == ESteps.LoadCachePackageHashFile)
			{
				string filePath = PersistentHelper.GetCachePackageHashFilePath(m_PackageName, m_PackageVersion);
				if (File.Exists(filePath) == false)
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Cache package hash file not found : {filePath}";
					return;
				}

				PackageHash = FileUtility.ReadAllText(filePath);
				if (string.IsNullOrEmpty(PackageHash))
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Cache package hash file content is empty !";
				}
				else
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
			}
		}
	}
}