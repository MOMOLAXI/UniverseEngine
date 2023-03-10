namespace Universe
{
    internal class PackageCachingOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            FindCacheFiles,
            VerifyCacheFiles,
            Done,
        }

        private readonly string m_PackageName;
        private FindCacheFilesOperation m_FindCacheFilesOp;
        private VerifyCacheFilesOperation m_VerifyCacheFilesOp;
        private ESteps m_Steps = ESteps.None;

        public PackageCachingOperation(string packageName)
        {
            m_PackageName = packageName;
        }

        internal override void Start()
        {
            m_Steps = ESteps.FindCacheFiles;
        }

        internal override void Update()
        {
            if (m_Steps is ESteps.None or ESteps.Done)
            {
                return;
            }

            if (m_Steps == ESteps.FindCacheFiles)
            {
                if (m_FindCacheFilesOp == null)
                {
                    m_FindCacheFilesOp = new(m_PackageName);
                    Engine.StartAsyncOperation(m_FindCacheFilesOp);
                }

                Progress = m_FindCacheFilesOp.Progress;
                if (m_FindCacheFilesOp.IsDone == false)
                {
                    return;
                }

                m_Steps = ESteps.VerifyCacheFiles;
            }

            if (m_Steps == ESteps.VerifyCacheFiles)
            {
                if (m_VerifyCacheFilesOp == null)
                {
                    m_VerifyCacheFilesOp = VerifyCacheFilesOperation.CreateOperation(m_FindCacheFilesOp.VerifyElements);
                    Engine.StartAsyncOperation(m_VerifyCacheFilesOp);
                }

                Progress = m_VerifyCacheFilesOp.Progress;
                if (m_VerifyCacheFilesOp.IsDone == false)
                {
                    return;
                }

                // 注意：总是返回成功
                m_Steps = ESteps.Done;
                Status = EOperationStatus.Succeed;

                int totalCount = CacheSystem.GetCachedFilesCount(m_PackageName);
                Log.Info($"Package '{m_PackageName}' cached files count : {totalCount}");
            }
        }
    }
}