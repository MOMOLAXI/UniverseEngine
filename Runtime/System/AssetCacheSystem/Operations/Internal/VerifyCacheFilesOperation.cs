using System;
using System.Collections.Generic;
using System.Threading;

namespace Universe
{
    internal abstract class VerifyCacheFilesOperation : AsyncOperationBase
    {
        public static VerifyCacheFilesOperation CreateOperation(List<VerifyCacheElement> elements)
        {
            return new VerifyCacheFilesWithThreadOperation(elements);
        }
    }

    /// <summary>
    /// 本地缓存文件验证（线程版）
    /// </summary>
    internal class VerifyCacheFilesWithThreadOperation : VerifyCacheFilesOperation
    {
        enum ESteps
        {
            None,
            InitVerify,
            UpdateVerify,
            Done,
        }

        readonly ThreadSyncContext m_SyncContext = new();
        readonly List<VerifyCacheElement> m_WaitingList;
        List<VerifyCacheElement> m_VerifyingList;
        int m_VerifyMaxNum;
        int m_VerifyTotalCount;
        float m_VerifyStartTime;
        int m_SucceedCount;
        int m_FailedCount;
        ESteps m_Steps = ESteps.None;

        public VerifyCacheFilesWithThreadOperation(List<VerifyCacheElement> elements)
        {
            m_WaitingList = elements;
        }

        internal override void Start()
        {
            m_Steps = ESteps.InitVerify;
            m_VerifyStartTime = UnityEngine.Time.realtimeSinceStartup;
        }

        internal override void Update()
        {
            switch (m_Steps)
            {

                case ESteps.InitVerify:
                {
                    int fileCount = m_WaitingList.Count;

                    // 设置同时验证的最大数
                    ThreadPool.GetMaxThreads(out int workerThreads, out int ioThreads);
                    Log.Info($"Work threads : {workerThreads}, IO threads : {ioThreads}");
                    m_VerifyMaxNum = Math.Min(workerThreads, ioThreads);
                    m_VerifyTotalCount = fileCount;
                    if (m_VerifyMaxNum < 1)
                    {
                        m_VerifyMaxNum = 1;
                    }

                    m_VerifyingList = new(m_VerifyMaxNum);
                    m_Steps = ESteps.UpdateVerify;
                    break;
                }
                case ESteps.UpdateVerify:
                {
                    m_SyncContext.Update();

                    Progress = GetProgress();
                    if (m_WaitingList.Count == 0 && m_VerifyingList.Count == 0)
                    {
                        m_Steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                        float costTime = UnityEngine.Time.realtimeSinceStartup - m_VerifyStartTime;
                        Log.Info($"Verify cache files elapsed time {costTime:f1} seconds");
                    }

                    for (int i = m_WaitingList.Count - 1; i >= 0; i--)
                    {
                        if (Engine.IsOperationBusy)
                            break;

                        if (m_VerifyingList.Count >= m_VerifyMaxNum)
                            break;

                        VerifyCacheElement element = m_WaitingList[i];
                        if (BeginVerifyFileWithThread(element))
                        {
                            m_WaitingList.RemoveAt(i);
                            m_VerifyingList.Add(element);
                        }
                        else
                        {
                            Log.Warning("The thread pool is failed queued.");
                            break;
                        }
                    }
                    break;
                }
                case ESteps.None:
                case ESteps.Done:
                default: return;
            }
        }

        float GetProgress()
        {
            if (m_VerifyTotalCount == 0)
            {
                return 1f;
            }

            return (float)(m_SucceedCount + m_FailedCount) / m_VerifyTotalCount;
        }
        
        bool BeginVerifyFileWithThread(VerifyCacheElement element)
        {
            return ThreadPool.QueueUserWorkItem(VerifyInThread, element);
        }

        void VerifyInThread(object obj)
        {
            VerifyCacheElement element = (VerifyCacheElement)obj;
            element.Result = CacheSystem.VerifyingCacheFile(element);
            m_SyncContext.Post(VerifyCallback, element);
        }

        void VerifyCallback(object obj)
        {
            VerifyCacheElement element = (VerifyCacheElement)obj;
            m_VerifyingList.Remove(element);

            if (element.Result == EVerifyResult.Succeed)
            {
                m_SucceedCount++;
                PackageCache.RecordWrapper wrapper = new(element.InfoFilePath, element.DataFilePath, element.DataFileCRC, element.DataFileSize);
                CacheSystem.RecordFile(element.PackageName, element.CacheGUID, wrapper);
            }
            else
            {
                m_FailedCount++;
                Log.Warning($"Failed verify file and delete files : {element.FileRootPath}");
                element.DeleteFiles();
            }
        }
    }

    /// <summary>
    /// 本地缓存文件验证（非线程版）
    /// </summary>
    internal class VerifyCacheFilesWithoutThreadOperation : VerifyCacheFilesOperation
    {
        enum ESteps
        {
            None,
            InitVerify,
            UpdateVerify,
            Done,
        }

        readonly List<VerifyCacheElement> m_WaitingList;
        List<VerifyCacheElement> m_VerifyingList;
        int m_VerifyMaxNum;
        int m_VerifyTotalCount;
        float m_VerifyStartTime;
        int m_SucceedCount;
        int m_FailedCount;
        ESteps m_Steps = ESteps.None;

        public VerifyCacheFilesWithoutThreadOperation(List<VerifyCacheElement> elements)
        {
            m_WaitingList = elements;
        }

        internal override void Start()
        {
            m_Steps = ESteps.InitVerify;
            m_VerifyStartTime = UnityEngine.Time.realtimeSinceStartup;
        }

        internal override void Update()
        {
            switch (m_Steps)
            {
                case ESteps.None or ESteps.Done: return;
                case ESteps.InitVerify:
                {
                    int fileCount = m_WaitingList.Count;

                    // 设置同时验证的最大数
                    m_VerifyMaxNum = fileCount;
                    m_VerifyTotalCount = fileCount;

                    m_VerifyingList = new(m_VerifyMaxNum);
                    m_Steps = ESteps.UpdateVerify;
                    break;
                }
                case ESteps.UpdateVerify:
                {
                    Progress = GetProgress();
                    if (m_WaitingList.Count == 0 && m_VerifyingList.Count == 0)
                    {
                        m_Steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                        float costTime = UnityEngine.Time.realtimeSinceStartup - m_VerifyStartTime;
                        Log.Info($"Package verify elapsed time {costTime:f1} seconds");
                    }

                    for (int i = m_WaitingList.Count - 1; i >= 0; i--)
                    {
                        if (Engine.IsOperationBusy)
                            break;

                        if (m_VerifyingList.Count >= m_VerifyMaxNum)
                            break;

                        VerifyCacheElement element = m_WaitingList[i];
                        BeginVerifyFileWithoutThread(element);
                        m_WaitingList.RemoveAt(i);
                        m_VerifyingList.Add(element);
                    }

                    // 主线程内验证，可以清空列表
                    m_VerifyingList.Clear();
                    break;
                }
            }
        }

        float GetProgress()
        {
            if (m_VerifyTotalCount == 0)
            {
                return 1f;
            }

            return (float)(m_SucceedCount + m_FailedCount) / m_VerifyTotalCount;
        }
        void BeginVerifyFileWithoutThread(VerifyCacheElement element)
        {
            element.Result = CacheSystem.VerifyingCacheFile(element);
            if (element.Result == EVerifyResult.Succeed)
            {
                m_SucceedCount++;
                PackageCache.RecordWrapper wrapper = new(element.InfoFilePath, element.DataFilePath, element.DataFileCRC, element.DataFileSize);
                CacheSystem.RecordFile(element.PackageName, element.CacheGUID, wrapper);
            }
            else
            {
                m_FailedCount++;

                Log.Warning($"Failed verify file and delete files : {element.FileRootPath}");
                element.DeleteFiles();
            }
        }
    }
}