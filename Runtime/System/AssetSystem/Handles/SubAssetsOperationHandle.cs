using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Universe
{
    public sealed class SubAssetsOperationHandle : OperationHandleBase, IDisposable
    {
        private Action<SubAssetsOperationHandle> m_Callback;

        internal SubAssetsOperationHandle(ProviderBase provider) : base(provider)
        {
        }
        internal override void InvokeCallback()
        {
            m_Callback?.Invoke(this);
        }

        /// <summary>
        /// 完成委托
        /// </summary>
        public event Action<SubAssetsOperationHandle> Completed
        {
            add
            {
                if (IsValidWithWarning)
                {
                    if (Provider.IsDone)
                    {
                        value.Invoke(this);
                    }
                    else
                    {
                        m_Callback += value;
                    }
                }
                else
                {
                    throw new($"{nameof(SubAssetsOperationHandle)} is invalid");
                }
            }
            remove
            {
                if (IsValidWithWarning)
                {
                    m_Callback -= value;
                }
                else
                {
                    throw new($"{nameof(SubAssetsOperationHandle)} is invalid");
                }
            }
        }

        /// <summary>
        /// 等待异步执行完毕
        /// </summary>
        public void WaitForAsyncComplete()
        {
            if (IsValidWithWarning == false)
            {
                return;
            }
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
        /// 子资源对象集合
        /// </summary>
        public Object[] AllAssetObjects
        {
            get
            {
                if (IsValidWithWarning == false)
                {
                    return null;
                }
                return Provider.AllAssetObjects;
            }
        }

        /// <summary>
        /// 获取子资源对象
        /// </summary>
        /// <typeparam name="TObject">子资源对象类型</typeparam>
        /// <param name="assetName">子资源对象名称</param>
        public TObject GetSubAssetObject<TObject>(string assetName) where TObject : Object
        {
            if (IsValidWithWarning == false)
            {
                return null;
            }

            foreach (Object assetObject in Provider.AllAssetObjects)
            {
                if (assetObject.name == assetName)
                {
                    return assetObject as TObject;
                }
            }

            Log.Warning($"Not found sub asset object : {assetName}");
            return null;
        }

        /// <summary>
        /// 获取所有的子资源对象集合
        /// </summary>
        /// <typeparam name="TObject">子资源对象类型</typeparam>
        public void GetSubAssetObjects<TObject>(List<TObject> result) where TObject : Object
        {
            if (!IsValidWithWarning)
            {
                return;
            }
            
            result.Clear();

            // List<TObject> ret = new(Provider.AllAssetObjects.Length);
            foreach (Object assetObject in Provider.AllAssetObjects)
            {
                if (assetObject is TObject retObject)
                {
                    result.Add(retObject);
                }
                else
                {
                    Log.Warning($"The type conversion failed : {assetObject.name}");
                }
            }
        }
    }
}