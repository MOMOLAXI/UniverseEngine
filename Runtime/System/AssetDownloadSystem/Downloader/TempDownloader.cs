
namespace Universe
{
	sealed internal class TempDownloader : DownloaderBase
	{
		public TempDownloader(BundleInfo bundleInfo) : base(bundleInfo)
		{
			m_DownloadProgress = 1f;
			m_DownloadedBytes = (ulong)bundleInfo.Bundle.FileSize;
			m_Steps = ESteps.Succeed;
		}

		public override void Update()
		{
		}
		public override void Abort()
		{
		}
	}
}