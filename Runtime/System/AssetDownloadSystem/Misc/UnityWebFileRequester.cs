using UnityEngine.Networking;
using UnityEngine;

namespace Universe
{
    /// <summary>
    /// 下载器
    /// 说明：UnityWebRequest(UWR) supports reading streaming assets since 2017.1
    /// </summary>
    internal class UnityWebFileRequester
    {
        UnityWebRequest m_Request;
        UnityWebRequestAsyncOperation m_Handle;

        // 超时相关
        float m_TimeOut;
        bool m_IsAbort;
        ulong m_LatestDownloadBytes;
        float m_LatestDownloadRealtime;

        /// <summary>
        /// 请求URL地址
        /// </summary>
        public string URL { private set; get; }

        /// <summary>
        /// 发送GET请求
        /// </summary>
        public void SendRequest(string url, string savePath, float timeout = 60)
        {
            if (m_Request != null)
            {
                return;
            }

            URL = url;
            m_TimeOut = timeout;
            m_LatestDownloadBytes = 0;
            m_LatestDownloadRealtime = Time.realtimeSinceStartup;

            m_Request = AssetDownloadSystem.NewRequest(URL);
            DownloadHandlerFile handler = new(savePath);
            handler.removeFileOnAbort = true;
            m_Request.downloadHandler = handler;
            m_Request.disposeDownloadHandlerOnDispose = true;
            m_Handle = m_Request.SendWebRequest();
        }

        /// <summary>
        /// 释放下载器
        /// </summary>
        public void Dispose()
        {
            if (m_Request != null)
            {
                m_Request.Dispose();
                m_Request = null;
                m_Handle = null;
            }
        }

        /// <summary>
        /// 是否完毕（无论成功失败）
        /// </summary>
        public bool IsDone()
        {
            if (m_Handle == null)
            {
                return false;
            }

            return m_Handle.isDone;
        }

        /// <summary>
        /// 下载进度
        /// </summary>
        public float Progress()
        {
            if (m_Handle == null)
            {
                return 0;
            }

            return m_Handle.progress;
        }

        /// <summary>
        /// 下载是否发生错误
        /// </summary>
        public bool HasError()
        {
            return m_Request.result != UnityWebRequest.Result.Success;
        }

        /// <summary>
        /// 获取错误信息
        /// </summary>
        public string GetError()
        {
            if (m_Request != null)
            {
                return $"URL : {URL} Error : {m_Request.error}";
            }

            return string.Empty;
        }

        /// <summary>
        /// 检测超时
        /// </summary>
        public void CheckTimeout()
        {
            if (m_IsAbort)
            {
                return;
            }
            
            // 注意：在连续时间段内无新增下载数据及判定为超时
            if (m_LatestDownloadBytes != m_Request.downloadedBytes)
            {
                m_LatestDownloadBytes = m_Request.downloadedBytes;
                m_LatestDownloadRealtime = Time.realtimeSinceStartup;
            }

            float offset = Time.realtimeSinceStartup - m_LatestDownloadRealtime;
            if (offset > m_TimeOut)
            {
                m_Request.Abort();
                m_IsAbort = true;
            }
        }
    }
}