using UnityEngine;
using UnityEngine.SceneManagement;

namespace Universe
{
    /// <summary>
    /// 场景卸载异步操作类
    /// </summary>
    public sealed class UnloadSceneOperation : AsyncOperationBase
    {
        enum EFlag
        {
            Normal,
            Error,
        }

        enum ESteps
        {
            None,
            UnLoad,
            Checking,
            Done,
        }

        readonly EFlag m_Flag;
        ESteps m_Steps = ESteps.None;
        Scene m_Scene;
        AsyncOperation m_AsyncOp;

        internal UnloadSceneOperation(string error)
        {
            m_Flag = EFlag.Error;
            Error = error;
        }
        internal UnloadSceneOperation(Scene scene)
        {
            m_Flag = EFlag.Normal;
            m_Scene = scene;
        }
        internal override void Start()
        {
            switch (m_Flag)
            {
                case EFlag.Normal:
                {
                    m_Steps = ESteps.UnLoad;
                    break;
                }
                case EFlag.Error:
                {
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    break;
                }
                default: throw new System.NotImplementedException(m_Flag.ToString());
            }
        }
        internal override void Update()
        {
            switch (m_Steps)
            {
                case ESteps.None or ESteps.Done: return;
                case ESteps.UnLoad when m_Scene.IsValid() && m_Scene.isLoaded:
                {
                    m_AsyncOp = SceneManager.UnloadSceneAsync(m_Scene);
                    m_Steps = ESteps.Checking;
                    break;
                }
                case ESteps.UnLoad:
                {
                    Error = "Scene is invalid or is not loaded.";
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    break;
                }
                case ESteps.Checking:
                {
                    Progress = m_AsyncOp.progress;
                    if (m_AsyncOp.isDone == false)
                    {
                        return;
                    }

                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                    break;
                }
            }
        }
    }
}