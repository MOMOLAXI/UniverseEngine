using System;
using UnityEngine;

namespace Universe
{
    public sealed class AssetOperationHandle : OperationHandleBase, IDisposable
    {
        private Action<AssetOperationHandle> m_Callback;

        internal AssetOperationHandle(ProviderBase provider) : base(provider)
        {
        }
        internal override void InvokeCallback()
        {
            m_Callback?.Invoke(this);
        }

        /// <summary>
        /// 完成委托
        /// </summary>
        public event Action<AssetOperationHandle> Completed
        {
            add
            {
                if (IsValidWithWarning == false)
                    throw new($"{nameof(AssetOperationHandle)} is invalid");
                if (Provider.IsDone)
                    value.Invoke(this);
                else
                    m_Callback += value;
            }
            remove
            {
                if (IsValidWithWarning == false)
                    throw new($"{nameof(AssetOperationHandle)} is invalid");
                m_Callback -= value;
            }
        }

        /// <summary>
        /// 等待异步执行完毕
        /// </summary>
        public void WaitForAsyncComplete()
        {
            if (IsValidWithWarning == false)
                return;
            Provider.WaitForAsyncComplete();
        }

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        public void Release()
        {
            ReleaseInternal();
        }

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        public void Dispose()
        {
            ReleaseInternal();
        }


        /// <summary>
        /// 资源对象
        /// </summary>
        public UnityEngine.Object AssetObject
        {
            get
            {
                if (IsValidWithWarning == false)
                    return null;
                return Provider.AssetObject;
            }
        }

        /// <summary>
        /// 获取资源对象
        /// </summary>
        /// <typeparam name="TAsset">资源类型</typeparam>
        public TAsset GetAssetObject<TAsset>() where TAsset : UnityEngine.Object
        {
            if (IsValidWithWarning == false)
                return null;
            return Provider.AssetObject as TAsset;
        }

        /// <summary>
        /// 同步初始化游戏对象
        /// </summary>
        /// <param name="parent">父类对象</param>
        /// <returns></returns>
        public GameObject InstantiateSync(Transform parent = null)
        {
            return InstantiateSyncInternal(Vector3.zero, Quaternion.identity, parent);
        }

        /// <summary>
        /// 同步初始化游戏对象
        /// </summary>
        /// <param name="position">坐标</param>
        /// <param name="rotation">角度</param>
        /// <param name="parent">父类对象</param>
        public GameObject InstantiateSync(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return InstantiateSyncInternal(position, rotation, parent);
        }

        /// <summary>
        /// 异步初始化游戏对象
        /// </summary>
        /// <param name="parent">父类对象</param>
        public InstantiateOperation InstantiateAsync(Transform parent = null)
        {
            return InstantiateAsyncInternal(Vector3.zero, Quaternion.identity, parent);
        }

        /// <summary>
        /// 异步初始化游戏对象
        /// </summary>
        /// <param name="position">坐标</param>
        /// <param name="rotation">角度</param>
        /// <param name="parent">父类对象</param>
        public InstantiateOperation InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return InstantiateAsyncInternal(position, rotation, parent);
        }


        private GameObject InstantiateSyncInternal(Vector3 position, Quaternion rotation, Transform parent)
        {
            if (IsValidWithWarning == false)
                return null;
            if (Provider.AssetObject == null)
                return null;

            GameObject clone = UnityEngine.Object.Instantiate(Provider.AssetObject as GameObject, position, rotation, parent);
            return clone;
        }
        private InstantiateOperation InstantiateAsyncInternal(Vector3 position, Quaternion rotation, Transform parent)
        {
            InstantiateOperation operation = new(this, position, rotation, parent);
            Engine.StartAsyncOperation(operation);
            return operation;
        }
    }
}