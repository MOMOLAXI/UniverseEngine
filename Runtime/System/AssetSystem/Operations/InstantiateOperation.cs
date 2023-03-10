using UnityEngine;
using Object = UnityEngine.Object;

namespace Universe
{
    public sealed class InstantiateOperation : AsyncOperationBase
    {
        enum ESteps
        {
            None,
            Clone,
            Done,
        }

        readonly AssetOperationHandle m_Handle;
        readonly Vector3 m_Position;
        readonly Quaternion m_Rotation;
        readonly Transform m_Parent;
        ESteps m_Steps = ESteps.None;

        /// <summary>
        /// 实例化的游戏对象
        /// </summary>
        public GameObject Result;

        internal InstantiateOperation(AssetOperationHandle handle, Vector3 position, Quaternion rotation, Transform parent)
        {
            m_Handle = handle;
            m_Position = position;
            m_Rotation = rotation;
            m_Parent = parent;
        }
        internal override void Start()
        {
            m_Steps = ESteps.Clone;
        }

        internal override void Update()
        {
            switch (m_Steps)
            {
                case ESteps.None:
                case ESteps.Done:
                {
                    return;
                }
                case ESteps.Clone when m_Handle.IsValidWithWarning == false:
                {
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"{nameof(AssetOperationHandle)} is invalid.";
                    return;
                }
                case ESteps.Clone when m_Handle.IsDone == false: return;
                case ESteps.Clone when m_Handle.AssetObject == null:
                {
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"{nameof(AssetOperationHandle.AssetObject)} is null.";
                    return;
                }
                // 实例化游戏对象
                case ESteps.Clone:
                {
                    Result = Object.Instantiate(m_Handle.AssetObject as GameObject, m_Position, m_Rotation, m_Parent);
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                    break;
                }
            }
        }

        /// <summary>
        /// 取消实例化对象操作
        /// </summary>
        public void Cancel()
        {
            if (IsDone == false)
            {
                m_Steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = "User cancelled !";
            }
        }

        /// <summary>
        /// 等待异步实例化结束
        /// </summary>
        public void WaitForAsyncComplete()
        {
            if (m_Steps == ESteps.Done)
            {
                return;
            }

            m_Handle.WaitForAsyncComplete();
            Update();
        }
    }
}