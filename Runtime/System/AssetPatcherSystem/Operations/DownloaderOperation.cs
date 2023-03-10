using System.Collections.Generic;

namespace Universe
{
	public abstract class DownloaderOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			Check,
			Loading,
			Done,
		}

		private const int MAX_LOADER_COUNT = 64;

		public delegate void OnDownloadOver(bool isSucceed);
		public delegate void OnDownloadProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes);
		public delegate void OnDownloadError(string fileName, string error);
		public delegate void OnStartDownloadFile(string fileName, long sizeBytes);

		private readonly int m_DownloadingMaxNumber;
		private readonly int m_FailedTryAgain;
		private readonly int m_Timeout;
		private readonly List<BundleInfo> m_DownloadList;
		private readonly List<DownloaderBase> m_Downloaders = new(MAX_LOADER_COUNT);
		private readonly List<DownloaderBase> m_RemoveList = new(MAX_LOADER_COUNT);
		private readonly List<DownloaderBase> m_FailedList = new(MAX_LOADER_COUNT);

		// 数据相关
		private bool m_IsPause;
		private long m_LastDownloadBytes;
		private int m_LastDownloadCount;
		private long m_CachedDownloadBytes;
		private int m_CachedDownloadCount;
		private ESteps m_Steps = ESteps.None;
		

		/// <summary>
		/// 统计的下载文件总数量
		/// </summary>
		public int TotalDownloadCount { private set; get; }
		
		/// <summary>
		/// 统计的下载文件的总大小
		/// </summary>
		public long TotalDownloadBytes { private set; get; }
		
		/// <summary>
		/// 当前已经完成的下载总数量
		/// </summary>
		public int CurrentDownloadCount 
		{
			get { return m_LastDownloadCount; }
		}

		/// <summary>
		/// 当前已经完成的下载总大小
		/// </summary>
		public long CurrentDownloadBytes
		{
			get { return m_LastDownloadBytes; }
		}

		/// <summary>
		/// 当下载器结束（无论成功或失败）
		/// </summary>
		public OnDownloadOver OnDownloadOverCallback { set; get; }

		/// <summary>
		/// 当下载进度发生变化
		/// </summary>
		public OnDownloadProgress OnDownloadProgressCallback { set; get; }

		/// <summary>
		/// 当某个文件下载失败
		/// </summary>
		public OnDownloadError OnDownloadErrorCallback { set; get; }

		/// <summary>
		/// 当开始下载某个文件
		/// </summary>
		public OnStartDownloadFile OnStartDownloadFileCallback { set; get; }


		internal DownloaderOperation(List<BundleInfo> downloadList, int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			m_DownloadList = downloadList;
			m_DownloadingMaxNumber = UnityEngine.Mathf.Clamp(downloadingMaxNumber, 1, MAX_LOADER_COUNT); ;
			m_FailedTryAgain = failedTryAgain;
			m_Timeout = timeout;

			if (downloadList != null)
			{
				TotalDownloadCount = downloadList.Count;
				foreach (BundleInfo patchBundle in downloadList)
				{
					TotalDownloadBytes += patchBundle.Bundle.FileSize;
				}
			}
		}
		internal override void Start()
		{
			Log.Info($"Begine to download : {TotalDownloadCount} files and {TotalDownloadBytes} bytes");
			m_Steps = ESteps.Check;
		}
		internal override void Update()
		{
			if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
				return;

			if (m_Steps == ESteps.Check)
			{
				if (m_DownloadList == null)
				{
					m_Steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = "Download list is null.";
				}
				else
				{
					m_Steps = ESteps.Loading;
				}
			}

			if (m_Steps == ESteps.Loading)
			{
				// 检测下载器结果
				m_RemoveList.Clear();
				long downloadBytes = m_CachedDownloadBytes;
				foreach (DownloaderBase downloader in m_Downloaders)
				{
					downloadBytes += (long)downloader.DownloadedBytes;
					if (downloader.IsDone() == false)
						continue;

					BundleInfo bundleInfo = downloader.GetBundleInfo();

					// 检测是否下载失败
					if (downloader.HasError())
					{
						m_RemoveList.Add(downloader);
						m_FailedList.Add(downloader);
						continue;
					}

					// 下载成功
					m_RemoveList.Add(downloader);
					m_CachedDownloadCount++;
					m_CachedDownloadBytes += bundleInfo.Bundle.FileSize;
				}

				// 移除已经完成的下载器（无论成功或失败）
				foreach (DownloaderBase loader in m_RemoveList)
				{
					m_Downloaders.Remove(loader);
				}

				// 如果下载进度发生变化
				if (m_LastDownloadBytes != downloadBytes || m_LastDownloadCount != m_CachedDownloadCount)
				{
					m_LastDownloadBytes = downloadBytes;
					m_LastDownloadCount = m_CachedDownloadCount;
					Progress = (float)m_LastDownloadBytes / TotalDownloadBytes;
					OnDownloadProgressCallback?.Invoke(TotalDownloadCount, m_LastDownloadCount, TotalDownloadBytes, m_LastDownloadBytes);
				}

				// 动态创建新的下载器到最大数量限制
				// 注意：如果期间有下载失败的文件，暂停动态创建下载器
				if (m_DownloadList.Count > 0 && m_FailedList.Count == 0)
				{
					if (m_IsPause)
						return;

					if (m_Downloaders.Count < m_DownloadingMaxNumber)
					{
						int index = m_DownloadList.Count - 1;
						BundleInfo bundleInfo = m_DownloadList[index];
						DownloaderBase operation = AssetDownloadSystem.BeginDownload(bundleInfo, m_FailedTryAgain, m_Timeout);
						m_Downloaders.Add(operation);
						m_DownloadList.RemoveAt(index);
						OnStartDownloadFileCallback?.Invoke(bundleInfo.Bundle.BundleName, bundleInfo.Bundle.FileSize);
					}
				}

				// 下载结算
				if (m_Downloaders.Count == 0)
				{
					if (m_FailedList.Count > 0)
					{
						DownloaderBase failedDownloader = m_FailedList[0];
						string fileName = failedDownloader.GetBundleInfo().Bundle.BundleName;
						m_Steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"Failed to download file : {fileName}";
						OnDownloadErrorCallback?.Invoke(fileName, failedDownloader.GetLastError());
						OnDownloadOverCallback?.Invoke(false);
					}
					else
					{
						// 结算成功
						m_Steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
						OnDownloadOverCallback?.Invoke(true);
					}
				}
			}
		}

		/// <summary>
		/// 开始下载
		/// </summary>
		public void BeginDownload()
		{
			if (m_Steps == ESteps.None)
			{
				Engine.StartAsyncOperation(this);
			}
		}

		/// <summary>
		/// 暂停下载
		/// </summary>
		public void PauseDownload()
		{
			m_IsPause = true;
		}

		/// <summary>
		/// 恢复下载
		/// </summary>
		public void ResumeDownload()
		{
			m_IsPause = false;
		}

		/// <summary>
		/// 取消下载
		/// </summary>
		public void CancelDownload()
		{
			if (m_Steps != ESteps.Done)
			{
				m_Steps = ESteps.Done;
				Status = EOperationStatus.Failed;
				Error = "User cancel.";
			}
		}
	}

	public sealed class PatchDownloaderOperation : DownloaderOperation
	{
		internal PatchDownloaderOperation(List<BundleInfo> downloadList, int downloadingMaxNumber, int failedTryAgain, int timeout)
			: base(downloadList, downloadingMaxNumber, failedTryAgain, timeout)
		{
		}

		/// <summary>
		/// 创建空的下载器
		/// </summary>
		internal static PatchDownloaderOperation CreateEmptyDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			List<BundleInfo> downloadList = new();
			PatchDownloaderOperation operation = new(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
			return operation;
		}
	}
	public sealed class PatchUnpackerOperation : DownloaderOperation
	{
		internal PatchUnpackerOperation(List<BundleInfo> downloadList, int downloadingMaxNumber, int failedTryAgain, int timeout)
			: base(downloadList, downloadingMaxNumber, failedTryAgain, timeout)
		{
		}

		/// <summary>
		/// 创建空的解压器
		/// </summary>
		internal static PatchUnpackerOperation CreateEmptyUnpacker(int upackingMaxNumber, int failedTryAgain, int timeout)
		{
			List<BundleInfo> downloadList = new();
			PatchUnpackerOperation operation = new(downloadList, upackingMaxNumber, failedTryAgain, int.MaxValue);
			return operation;
		}
	}
}