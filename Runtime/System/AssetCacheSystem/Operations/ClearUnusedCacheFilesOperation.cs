using System.Collections.Generic;

namespace Universe
{
    /// <summary>
    /// 清理本地包裹未使用的缓存文件
    /// </summary>
    public sealed class ClearUnusedCacheFilesOperation : AsyncOperationBase
    {
        enum ESteps
        {
            None,
            GetUnusedCacheFiles,
            ClearUnusedCacheFiles,
            Done,
        }

        int m_UnusedFileTotalCount;
        ESteps m_Steps = ESteps.None;
        readonly AssetsPackage m_Package;
        readonly List<string> m_UnusedCacheGUIDs = new();
        
        internal ClearUnusedCacheFilesOperation(AssetsPackage package)
        {
            m_Package = package;
        }
        internal override void Start()
        {
            m_Steps = ESteps.GetUnusedCacheFiles;
        }
        internal override void Update()
        {
            switch (m_Steps)
            {
                case ESteps.None or ESteps.Done: return;
                case ESteps.GetUnusedCacheFiles:
                {
                    CacheSystem.GetUnusedCacheGUIDs(m_Package, m_UnusedCacheGUIDs);
                    m_UnusedFileTotalCount = m_UnusedCacheGUIDs.Count;
                    Log.Info($"Found unused cache file count : {m_UnusedFileTotalCount}");
                    m_Steps = ESteps.ClearUnusedCacheFiles;
                    break;
                }
                case ESteps.ClearUnusedCacheFiles:
                {
                    for (int i = m_UnusedCacheGUIDs.Count - 1; i >= 0; i--)
                    {
                        string cacheGuid = m_UnusedCacheGUIDs[i];
                        CacheSystem.DiscardFile(m_Package.PackageName, cacheGuid);
                        m_UnusedCacheGUIDs.RemoveAt(i);

                        if (Engine.IsOperationBusy)
                        {
                            break;
                        }
                    }

                    if (m_UnusedFileTotalCount == 0)
                    {
                        Progress = 1.0f;
                    }
                    else
                    {
                        Progress = 1.0f - m_UnusedCacheGUIDs.Count / (float)m_UnusedFileTotalCount;
                    }

                    if (m_UnusedCacheGUIDs.Count == 0)
                    {
                        m_Steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                    }
                    break;
                }
            }
        }
    }
}