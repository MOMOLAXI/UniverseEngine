namespace Universe
{
    internal abstract class DownloaderBase
    {
        protected enum ESteps
        {
            None,
            CheckTempFile,
            WaitingCheckTempFile,
            PrepareDownload,
            CreateResumeDownloader,
            CreateGeneralDownloader,
            CheckDownload,
            VerifyTempFile,
            WaitingVerifyTempFile,
            CachingFile,
            TryAgain,
            Succeed,
            Failed,
        }

        protected readonly BundleInfo m_BundleInfo;

        protected ESteps m_Steps = ESteps.None;

        protected int m_TimeOut;
        protected int m_FailedTryAgain;
        protected int m_RequestCount;
        protected string m_RequestURL;

        protected string m_LastError = string.Empty;
        protected long m_LastCode = 0;
        protected float m_DownloadProgress = 0f;
        protected ulong m_DownloadedBytes = 0;


        /// <summary>
        /// 下载进度（0f~1f）
        /// </summary>
        public float DownloadProgress => m_DownloadProgress;

        /// <summary>
        /// 已经下载的总字节数
        /// </summary>
        public ulong DownloadedBytes => m_DownloadedBytes;

        protected DownloaderBase(BundleInfo bundleInfo)
        {
            m_BundleInfo = bundleInfo;
        }

        public void SendRequest(int failedTryAgain, int timeout)
        {
            if (m_Steps == ESteps.None)
            {
                m_FailedTryAgain = failedTryAgain;
                m_TimeOut = timeout;
                m_Steps = ESteps.CheckTempFile;
            }
        }

        public abstract void Update();
        public abstract void Abort();

        /// <summary>
        /// 获取网络请求地址
        /// </summary>
        protected string GetRequestURL()
        {
            // 轮流返回请求地址
            m_RequestCount++;
            if (m_RequestCount % 2 == 0)
            {
                return m_BundleInfo.RemoteFallbackURL;
            }

            return m_BundleInfo.RemoteMainURL;
        }

        /// <summary>
        /// 获取资源包信息
        /// </summary>
        public BundleInfo GetBundleInfo()
        {
            return m_BundleInfo;
        }

        /// <summary>
        /// 检测下载器是否已经完成（无论成功或失败）
        /// </summary>
        public bool IsDone()
        {
            return m_Steps is ESteps.Succeed or ESteps.Failed;
        }

        /// <summary>
        /// 下载过程是否发生错误
        /// </summary>
        public bool HasError()
        {
            return m_Steps == ESteps.Failed;
        }

        /// <summary>
        /// 按照错误级别打印错误
        /// </summary>
        public void ReportError()
        {
            Log.Error(GetLastError());
        }

        /// <summary>
        /// 按照警告级别打印错误
        /// </summary>
        public void ReportWarning()
        {
            Log.Warning(GetLastError());
        }

        /// <summary>
        /// 获取最近发生的错误信息
        /// </summary>
        public string GetLastError()
        {
            return $"Failed to download : {m_RequestURL} Error : {m_LastError} Code : {m_LastCode}";
        }
    }
}