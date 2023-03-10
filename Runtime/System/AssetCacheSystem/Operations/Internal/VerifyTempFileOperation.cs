using System.Threading;

namespace Universe
{
    internal abstract class VerifyTempFileOperation : AsyncOperationBase
    {
        public EVerifyResult VerifyResult { protected set; get; }

        public static VerifyTempFileOperation CreateOperation(VerifyTempElement element)
        {
            return new VerifyTempFileWithThreadOperation(element);
        }
    }

    /// <summary>
    /// 下载文件验证（线程版）
    /// </summary>
    internal class VerifyTempFileWithThreadOperation : VerifyTempFileOperation
    {
        private enum ESteps
        {
            None,
            VerifyFile,
            Waiting,
            Done,
        }

        private readonly VerifyTempElement m_Element;
        private ESteps m_Steps = ESteps.None;

        public VerifyTempFileWithThreadOperation(VerifyTempElement element)
        {
            m_Element = element;
        }

        internal override void Start()
        {
            m_Steps = ESteps.VerifyFile;
        }

        internal override void Update()
        {
            switch (m_Steps)
            {
                case ESteps.None or ESteps.Done: return;
                case ESteps.VerifyFile:
                {
                    if (BeginVerifyFileWithThread(m_Element))
                    {
                        m_Steps = ESteps.Waiting;
                    }
                    break;
                }
            }

        }

        private bool BeginVerifyFileWithThread(VerifyTempElement element)
        {
            return ThreadPool.QueueUserWorkItem(VerifyInThread, element);
        }

        private void VerifyInThread(object obj)
        {
            VerifyTempElement element = (VerifyTempElement)obj;
            element.Result = CacheSystem.VerifyingTempFile(element);
            AssetDownloadSystem.SyncContext.Post(VerifyCallback, element);
        }

        private void VerifyCallback(object obj)
        {
            VerifyTempElement element = (VerifyTempElement)obj;
            VerifyResult = element.Result;
            if (element.Result == EVerifyResult.Succeed)
            {
                m_Steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
            else
            {
                m_Steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = $"Failed verify file : {element.TempDataFilePath} ! ErrorCode : {element.Result}";
            }
        }
    }

    /// <summary>
    /// 下载文件验证（非线程版）
    /// </summary>
    internal class VerifyTempFileWithoutThreadOperation : VerifyTempFileOperation
    {
        private enum ESteps
        {
            None,
            VerifyFile,
            Done,
        }

        private readonly VerifyTempElement m_Element;
        private ESteps m_Steps = ESteps.None;

        public VerifyTempFileWithoutThreadOperation(VerifyTempElement element)
        {
            m_Element = element;
        }
        internal override void Start()
        {
            m_Steps = ESteps.VerifyFile;
        }
        internal override void Update()
        {
            switch (m_Steps)
            {
                case ESteps.None or ESteps.Done: return;
                case ESteps.VerifyFile:
                {
                    m_Element.Result = CacheSystem.VerifyingTempFile(m_Element);
                    VerifyResult = m_Element.Result;
                    if (m_Element.Result == EVerifyResult.Succeed)
                    {
                        m_Steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                    }
                    else
                    {
                        m_Steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Failed verify file : {m_Element.TempDataFilePath} ! ErrorCode : {m_Element.Result}";
                    }
                    break;
                }
            }
        }
    }
}