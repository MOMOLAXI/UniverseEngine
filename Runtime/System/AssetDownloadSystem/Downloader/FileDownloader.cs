using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Universe
{
    sealed internal class FileDownloader : DownloaderBase
    {
        readonly bool m_BreakResume;
        readonly string m_TempFilePath;
        UnityWebRequest m_WebRequest;
        DownloadHandlerFileRange m_DownloadHandle;
        VerifyTempFileOperation m_CheckFileOp;
        VerifyTempFileOperation m_VerifyFileOp;

        // 重置变量
        bool m_IsAbort;
        ulong m_FileOriginLength;
        ulong m_LatestDownloadBytes;
        float m_LatestDownloadRealtime;
        float m_TryAgainTimer;

        public FileDownloader(BundleInfo bundleInfo, bool breakResume) : base(bundleInfo)
        {
            m_BreakResume = breakResume;
            m_TempFilePath = bundleInfo.Bundle.TempDataFilePath;
        }

        public override void Update()
        {
            if (m_Steps == ESteps.None)
            {
                return;
            }
            if (IsDone())
            {
                return;
            }

            // 检测临时文件
            if (m_Steps == ESteps.CheckTempFile)
            {
                VerifyTempElement element = new(m_BundleInfo.Bundle.TempDataFilePath, m_BundleInfo.Bundle.FileCRC, m_BundleInfo.Bundle.FileSize);
                m_CheckFileOp = VerifyTempFileOperation.CreateOperation(element);
                Engine.StartAsyncOperation(m_CheckFileOp);
                m_Steps = ESteps.WaitingCheckTempFile;
            }

            // 等待检测结果
            if (m_Steps == ESteps.WaitingCheckTempFile)
            {
                if (m_CheckFileOp.IsDone == false)
                    return;

                if (m_CheckFileOp.Status == EOperationStatus.Succeed)
                {
                    m_Steps = ESteps.CachingFile;
                }
                else
                {
                    if (m_CheckFileOp.VerifyResult == EVerifyResult.FileOverflow)
                    {
                        if (File.Exists(m_TempFilePath))
                            File.Delete(m_TempFilePath);
                    }
                    m_Steps = ESteps.PrepareDownload;
                }
            }

            // 创建下载器
            if (m_Steps == ESteps.PrepareDownload)
            {
                // 重置变量
                m_DownloadProgress = 0f;
                m_DownloadedBytes = 0;
                m_IsAbort = false;
                m_FileOriginLength = 0;
                m_LatestDownloadBytes = 0;
                m_LatestDownloadRealtime = Time.realtimeSinceStartup;
                m_TryAgainTimer = 0f;

                // 获取请求地址
                m_RequestURL = GetRequestURL();

                if (m_BreakResume)
                    m_Steps = ESteps.CreateResumeDownloader;
                else
                    m_Steps = ESteps.CreateGeneralDownloader;
            }

            // 创建普通的下载器
            if (m_Steps == ESteps.CreateGeneralDownloader)
            {
                if (File.Exists(m_TempFilePath))
                    File.Delete(m_TempFilePath);

                m_WebRequest = AssetDownloadSystem.NewRequest(m_RequestURL);
                DownloadHandlerFile handler = new(m_TempFilePath);
                handler.removeFileOnAbort = true;
                m_WebRequest.downloadHandler = handler;
                m_WebRequest.disposeDownloadHandlerOnDispose = true;

                if (AssetDownloadSystem.CertificateHandlerInstance != null)
                {
                    m_WebRequest.certificateHandler = AssetDownloadSystem.CertificateHandlerInstance;
                    m_WebRequest.disposeCertificateHandlerOnDispose = false;
                }

                m_WebRequest.SendWebRequest();
                m_Steps = ESteps.CheckDownload;
            }

            // 创建断点续传下载器
            if (m_Steps == ESteps.CreateResumeDownloader)
            {
                long fileLength = -1;
                if (File.Exists(m_TempFilePath))
                {
                    FileInfo fileInfo = new(m_TempFilePath);
                    fileLength = fileInfo.Length;
                    m_FileOriginLength = (ulong)fileLength;
                    m_DownloadedBytes = m_FileOriginLength;
                }

            #if UNITY_2019_4_OR_NEWER
                m_WebRequest = AssetDownloadSystem.NewRequest(m_RequestURL);
                DownloadHandlerFile handler = new(m_TempFilePath, true);
                handler.removeFileOnAbort = false;
            #else
				_webRequest = DownloadSystem.NewRequest(_requestURL);
				var handler = new DownloadHandlerFileRange(_tempFilePath, _bundleInfo.Bundle.FileSize, _webRequest);
				_downloadHandle = handler;
            #endif
                m_WebRequest.downloadHandler = handler;
                m_WebRequest.disposeDownloadHandlerOnDispose = true;
                if (fileLength > 0)
                    m_WebRequest.SetRequestHeader("Range", $"bytes={fileLength}-");

                if (AssetDownloadSystem.CertificateHandlerInstance != null)
                {
                    m_WebRequest.certificateHandler = AssetDownloadSystem.CertificateHandlerInstance;
                    m_WebRequest.disposeCertificateHandlerOnDispose = false;
                }

                m_WebRequest.SendWebRequest();
                m_Steps = ESteps.CheckDownload;
            }

            // 检测下载结果
            if (m_Steps == ESteps.CheckDownload)
            {
                m_DownloadProgress = m_WebRequest.downloadProgress;
                m_DownloadedBytes = m_FileOriginLength + m_WebRequest.downloadedBytes;
                if (m_WebRequest.isDone == false)
                {
                    CheckTimeout();
                    return;
                }

                bool hasError = false;

                // 检查网络错误
            #if UNITY_2020_3_OR_NEWER
                if (m_WebRequest.result != UnityWebRequest.Result.Success)
                {
                    hasError = true;
                    m_LastError = m_WebRequest.error;
                    m_LastCode = m_WebRequest.responseCode;
                }
            #else
				if (_webRequest.isNetworkError || _webRequest.isHttpError)
				{
					hasError = true;
					_lastError = _webRequest.error;
					_lastCode = _webRequest.responseCode;
				}
            #endif

                // 如果网络异常
                if (hasError)
                {
                    if (m_BreakResume)
                    {
                        // 注意：下载断点续传文件发生特殊错误码之后删除文件
                        if (AssetDownloadSystem.ClearFileResponseCodes != null)
                        {
                            if (AssetDownloadSystem.ClearFileResponseCodes.Contains(m_WebRequest.responseCode))
                            {
                                FileUtility.DeleteFile(m_TempFilePath);
                            }
                        }
                    }
                    else
                    {
                        // 注意：非断点续传下载失败之后删除文件
                        FileUtility.DeleteFile(m_TempFilePath);
                    }

                    m_Steps = ESteps.TryAgain;
                }
                else
                {
                    m_Steps = ESteps.VerifyTempFile;
                }

                // 释放下载器
                DisposeWebRequest();
            }

            // 验证下载文件
            if (m_Steps == ESteps.VerifyTempFile)
            {
                VerifyTempElement element = new(m_BundleInfo.Bundle.TempDataFilePath, m_BundleInfo.Bundle.FileCRC, m_BundleInfo.Bundle.FileSize);
                m_VerifyFileOp = VerifyTempFileOperation.CreateOperation(element);
                Engine.StartAsyncOperation(m_VerifyFileOp);
                m_Steps = ESteps.WaitingVerifyTempFile;
            }

            // 等待验证完成
            if (m_Steps == ESteps.WaitingVerifyTempFile)
            {
                if (m_VerifyFileOp.IsDone == false)
                    return;

                if (m_VerifyFileOp.Status == EOperationStatus.Succeed)
                {
                    m_Steps = ESteps.CachingFile;
                }
                else
                {
                    if (File.Exists(m_TempFilePath))
                        File.Delete(m_TempFilePath);

                    m_LastError = m_VerifyFileOp.Error;
                    m_Steps = ESteps.TryAgain;
                }
            }

            // 缓存下载文件
            if (m_Steps == ESteps.CachingFile)
            {
                try
                {
                    string infoFilePath = m_BundleInfo.Bundle.CachedInfoFilePath;
                    string dataFilePath = m_BundleInfo.Bundle.CachedDataFilePath;
                    string dataFileCRC = m_BundleInfo.Bundle.FileCRC;
                    long dataFileSize = m_BundleInfo.Bundle.FileSize;
                    FileUtility.DeleteFile(infoFilePath);
                    FileUtility.DeleteFile(dataFilePath);

                    FileInfo fileInfo = new(m_TempFilePath);
                    fileInfo.MoveTo(dataFilePath);

                    // 写入信息文件记录验证数据
                    CacheFileInfo.WriteInfoToFile(infoFilePath, dataFileCRC, dataFileSize);

                    // 记录缓存文件
                    PackageCache.RecordWrapper wrapper = new(infoFilePath, dataFilePath, dataFileCRC, dataFileSize);
                    CacheSystem.RecordFile(m_BundleInfo.Bundle.PackageName, m_BundleInfo.Bundle.CacheGuid, wrapper);

                    m_LastError = string.Empty;
                    m_LastCode = 0;
                    m_Steps = ESteps.Succeed;
                }
                catch (Exception e)
                {
                    m_LastError = e.Message;
                    m_Steps = ESteps.TryAgain;
                }
            }

            // 重新尝试下载
            if (m_Steps == ESteps.TryAgain)
            {
                if (m_FailedTryAgain <= 0)
                {
                    ReportError();
                    m_Steps = ESteps.Failed;
                    return;
                }

                m_TryAgainTimer += Time.unscaledDeltaTime;
                if (m_TryAgainTimer > 1f)
                {
                    m_FailedTryAgain--;
                    m_Steps = ESteps.PrepareDownload;
                    ReportWarning();
                    Log.Warning($"Try again download : {m_RequestURL}");
                }
            }
        }
        public override void Abort()
        {
            if (IsDone() == false)
            {
                m_Steps = ESteps.Failed;
                m_LastError = "user abort";
                m_LastCode = 0;
                DisposeWebRequest();
            }
        }

        private void CheckTimeout()
        {
            // 注意：在连续时间段内无新增下载数据及判定为超时
            if (m_IsAbort == false)
            {
                if (m_LatestDownloadBytes != DownloadedBytes)
                {
                    m_LatestDownloadBytes = DownloadedBytes;
                    m_LatestDownloadRealtime = Time.realtimeSinceStartup;
                }

                float offset = Time.realtimeSinceStartup - m_LatestDownloadRealtime;
                if (offset > m_TimeOut)
                {
                    Log.Warning($"Web file request timeout : {m_RequestURL}");
                    m_WebRequest.Abort();
                    m_IsAbort = true;
                }
            }
        }
        private void DisposeWebRequest()
        {
            if (m_DownloadHandle != null)
            {
                m_DownloadHandle.Cleanup();
                m_DownloadHandle = null;
            }

            if (m_WebRequest != null)
            {
                m_WebRequest.Dispose();
                m_WebRequest = null;
            }
        }
    }
}