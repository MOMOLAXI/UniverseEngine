using System.IO;

namespace Universe
{
	internal class QueryCachePackageVersionOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			LoadCachePackageVersionFile,
			Done,
		}

		private readonly string m_PackageName;
		private ESteps m_Steps = ESteps.None;

		/// <summary>
		/// 包裹版本
		/// </summary>
		public string PackageVersion { private set; get; }


		public QueryCachePackageVersionOperation(string packageName)
		{
			m_PackageName = packageName;
		}
		internal override void Start()
		{
			m_Steps = ESteps.LoadCachePackageVersionFile;
		}
		internal override void Update()
		{
			if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
				return;

			if (m_Steps == ESteps.LoadCachePackageVersionFile)
			{
				string filePath = PersistentHelper.GetCachePackageVersionFilePath(m_PackageName);
				if (File.Exists(filePath) == false)
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Cache package version file not found : {filePath}";
					return;
				}

				PackageVersion = FileUtility.ReadAllText(filePath);
				if (string.IsNullOrEmpty(PackageVersion))
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Cache package version file content is empty !";
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