using System.Collections.Generic;
using UnityEngine.Networking;

namespace Universe
{
    /// <summary>
    /// 自定义下载器的请求委托
    /// </summary>
    public delegate UnityWebRequest DownloadRequestDelegate(string url);

    /// <summary>
    /// 1. 保证每一时刻资源文件只存在一个下载器
    /// 2. 保证下载器下载完成后立刻验证并缓存
    /// 3. 保证资源文件不会被重复下载
    /// </summary>
    internal class AssetDownloadSystem : EngineSystem
    {
        static AssetDownloadSystem s_Instance;
        static AssetDownloadSystem Instance => s_Instance ??= Engine.GetEngineSystem<AssetDownloadSystem>();

        readonly Dictionary<string, DownloaderBase> m_Downloaders = new();
        readonly List<string> m_RemoveList = new(100);

        /// <summary>
        /// 线程同步
        /// </summary>
        public static ThreadSyncContext SyncContext { set; get; } = new();

        /// <summary>
        /// 自定义下载器的请求委托
        /// </summary>
        public static DownloadRequestDelegate RequestDelegate;

        /// <summary>
        /// 自定义的证书认证实例
        /// </summary>
        public static CertificateHandler CertificateHandlerInstance;

        /// <summary>
        /// 启用断点续传功能文件的最小字节数
        /// </summary>
        public static int BreakpointResumeFileSize { set; get; } = int.MaxValue;

        /// <summary>
        /// 下载失败后清理文件的HTTP错误码
        /// </summary>
        public static List<long> ClearFileResponseCodes { set; get; }

        /// <summary>
        /// 更新下载器
        /// </summary>
        public override void Update(float dt)
        {
            SyncContext?.Update();

            // 更新下载器
            m_RemoveList.Clear();
            foreach ((string key, DownloaderBase downloader) in m_Downloaders)
            {
                downloader.Update();
                if (downloader.IsDone())
                {
                    m_RemoveList.Add(key);
                }
            }

            // 移除下载器
            foreach (string key in m_RemoveList)
            {
                m_Downloaders.Remove(key);
            }
        }

        /// <summary>
        /// 销毁所有下载器
        /// </summary>
        public override void Destroy()
        {
            foreach (KeyValuePair<string, DownloaderBase> valuePair in m_Downloaders)
            {
                DownloaderBase downloader = valuePair.Value;
                downloader.Abort();
            }

            m_Downloaders.Clear();
            m_RemoveList.Clear();

            SyncContext = null;
            RequestDelegate = null;
            CertificateHandlerInstance = null;
            BreakpointResumeFileSize = int.MaxValue;
            ClearFileResponseCodes = null;
        }

        /// <summary>
        /// 开始下载资源文件
        /// 注意：只有第一次请求的参数才是有效的
        /// </summary>
        public static DownloaderBase BeginDownload(BundleInfo bundleInfo, int failedTryAgain, int timeout = 60)
        {
            return Instance.InternalBeginDownload(bundleInfo, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建一个新的网络请求
        /// </summary>
        public static UnityWebRequest NewRequest(string requestURL)
        {
            return Instance.InternalNewRequest(requestURL);
        }

        /// <summary>
        /// 获取下载器的总数
        /// </summary>
        public static int GetDownloaderTotalCount()
        {
            return Instance.InternalGetDownloaderTotalCount();
        }

        DownloaderBase InternalBeginDownload(BundleInfo bundleInfo, int failedTryAgain, int timeout = 60)
        {
            // 查询存在的下载器
            if (m_Downloaders.TryGetValue(bundleInfo.Bundle.CachedDataFilePath, out DownloaderBase downloader))
            {
                return downloader;
            }

            // 如果资源已经缓存
            if (CacheSystem.IsCached(bundleInfo.Bundle.PackageName, bundleInfo.Bundle.CacheGuid))
            {
                TempDownloader tempDownloader = new(bundleInfo);
                return tempDownloader;
            }

            // 创建新的下载器	
            {
                Log.Info($"Beginning to download file : {bundleInfo.Bundle.FileName} URL : {bundleInfo.RemoteMainURL}");
                FileUtility.CreateFileDirectory(bundleInfo.Bundle.CachedDataFilePath);
                bool breakDownload = bundleInfo.Bundle.FileSize >= BreakpointResumeFileSize;
                DownloaderBase newDownloader = new FileDownloader(bundleInfo, breakDownload);
                newDownloader.SendRequest(failedTryAgain, timeout);
                m_Downloaders.Add(bundleInfo.Bundle.CachedDataFilePath, newDownloader);
                return newDownloader;
            }
        }


        UnityWebRequest InternalNewRequest(string requestURL)
        {
            if (RequestDelegate != null)
            {
                return RequestDelegate.Invoke(requestURL);
            }

            UnityWebRequest request = new(requestURL, UnityWebRequest.kHttpVerbGET);
            return request;
        }

        /// <summary>
        /// 获取下载器的总数
        /// </summary>
        int InternalGetDownloaderTotalCount()
        {
            return m_Downloaders.Count;
        }
    }
}