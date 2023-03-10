using System.IO;
using UnityEngine.Networking;

namespace Universe
{
    /// <summary>
    /// 支持Unity2018版本的断点续传下载器
    /// </summary>
    internal class DownloadHandlerFileRange : DownloadHandlerScript
    {
        readonly long m_FileTotalSize;
        readonly long m_LocalFileSize;

        readonly UnityWebRequest m_Request;
        FileStream m_FileStream;
        long m_CurFileSize;

        public string FileSavePath { get; }

        public DownloadHandlerFileRange(string fileSavePath, long fileTotalSize, UnityWebRequest webRequest) : base(new byte[1024 * 1024])
        {
            FileSavePath = fileSavePath;
            m_FileTotalSize = fileTotalSize;
            m_Request = webRequest;

            if (File.Exists(fileSavePath))
            {
                FileInfo fileInfo = new(fileSavePath);
                m_LocalFileSize = fileInfo.Length;
            }

            m_FileStream = new(FileSavePath, FileMode.Append, FileAccess.Write);
            m_CurFileSize = m_LocalFileSize;
        }

        protected override bool ReceiveData(byte[] receiveData, int dataLength)
        {
            if (receiveData == null || dataLength == 0 || m_Request.responseCode >= 400)
            {
                return false;
            }

            if (m_FileStream == null)
            {
                return false;
            }

            m_FileStream.Write(receiveData, 0, dataLength);
            m_CurFileSize += dataLength;
            return true;
        }

        /// <summary>
        /// UnityWebRequest.downloadHandler.data
        /// </summary>
        protected override byte[] GetData()
        {
            return null;
        }

        /// <summary>
        /// UnityWebRequest.downloadHandler.text
        /// </summary>
        protected override string GetText()
        {
            return null;
        }

        /// <summary>
        /// UnityWebRequest.downloadProgress
        /// </summary>
        protected override float GetProgress()
        {
            return m_FileTotalSize == 0 ? 0 : ((float)m_CurFileSize) / m_FileTotalSize;
        }

        /// <summary>
        /// 释放下载句柄
        /// </summary>
        public void Cleanup()
        {
            if (m_FileStream != null)
            {
                m_FileStream.Flush();
                m_FileStream.Dispose();
                m_FileStream = null;
            }
        }
    }
}