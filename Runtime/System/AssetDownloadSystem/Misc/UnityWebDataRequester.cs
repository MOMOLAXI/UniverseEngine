using UnityEngine.Networking;

namespace Universe
{
    /// <summary>
    /// 下载器
    /// 说明：UnityWebRequest(UWR) supports reading streaming assets since 2017.1
    /// </summary>
    internal class UnityWebDataRequester
    {
        private UnityWebRequest m_Request;
        private UnityWebRequestAsyncOperation m_Handle;

        /// <summary>
        /// 请求URL地址
        /// </summary>
        public string URL { private set; get; }

        /// <summary>
        /// 发送GET请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeout">超时：从请求开始计时</param>
        public void SendRequest(string url, int timeout = 0)
        {
            if (m_Request != null)
            {
                return;
            }

            URL = url;
            m_Request = AssetDownloadSystem.NewRequest(URL);
            DownloadHandlerBuffer handler = new();
            m_Request.downloadHandler = handler;
            m_Request.disposeDownloadHandlerOnDispose = true;
            m_Request.timeout = timeout;
            m_Handle = m_Request.SendWebRequest();
        }

        /// <summary>
        /// 获取下载的字节数据
        /// </summary>
        public byte[] GetData()
        {
            if (m_Request != null && IsDone())
            {
                return m_Request.downloadHandler.data;
            }

            return null;
        }

        /// <summary>
        /// 获取下载的文本数据
        /// </summary>
        public string GetText()
        {
            if (m_Request != null && IsDone())
            {
                return m_Request.downloadHandler.text;
            }

            return null;
        }

        /// <summary>
        /// 释放下载器
        /// </summary>
        public void Dispose()
        {
            if (m_Request == null)
            {
                return;
            }

            m_Request.Dispose();
            m_Request = null;
            m_Handle = null;
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
    }
}