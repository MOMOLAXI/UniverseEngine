using UnityEngine.SceneManagement;

namespace Universe
{
    public class SceneOperationHandle : OperationHandleBase
    {
        private System.Action<SceneOperationHandle> m_Callback;
        internal string PackageName { set; get; }

        internal SceneOperationHandle(ProviderBase provider) : base(provider)
        {
        }
        internal override void InvokeCallback()
        {
            m_Callback?.Invoke(this);
        }

        /// <summary>
        /// 完成委托
        /// </summary>
        public event System.Action<SceneOperationHandle> Completed
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
                    throw new($"{nameof(SceneOperationHandle)} is invalid");
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
                    throw new($"{nameof(SceneOperationHandle)} is invalid");
                }
            }
        }

        /// <summary>
        /// 场景对象
        /// </summary>
        public Scene SceneObject => IsValidWithWarning ? Provider.SceneObject : new();

        /// <summary>
        /// 激活场景
        /// </summary>
        public bool ActivateScene()
        {
            if (IsValidWithWarning)
            {
                if (SceneObject.IsValid() && SceneObject.isLoaded)
                {
                    return SceneManager.SetActiveScene(SceneObject);
                }

                Log.Warning($"Scene is invalid or not loaded : {SceneObject.name}");
                return false;
            }

            return false;
        }

        /// <summary>
        /// 是否为主场景
        /// </summary>
        public bool IsMainScene()
        {
            if (IsValidWithWarning)
            {
                return Provider switch
                {
                    DatabaseSceneProvider provider => provider.SceneMode == LoadSceneMode.Single,
                    BundledSceneProvider provider => provider.SceneMode == LoadSceneMode.Single,
                    _ => false
                };
            }

            return false;
        }

        /// <summary>
        /// 异步卸载子场景
        /// </summary>
        public UnloadSceneOperation UnloadAsync()
        {
            // 如果句柄无效
            if (IsValidWithWarning == false)
            {
                string error = $"{nameof(SceneOperationHandle)} is invalid.";
                UnloadSceneOperation operation = new(error);
                Engine.StartAsyncOperation(operation);
                return operation;
            }

            // 如果是主场景
            if (IsMainScene())
            {
                string error = $"Cannot unload main scene. Use {nameof(AssetSystem.LoadSceneAsync)} method to change the main scene !";
                Log.Error(error);
                UnloadSceneOperation operation = new(error);
                Engine.StartAsyncOperation(operation);
                return operation;
            }

            // 卸载子场景
            Scene sceneObject = SceneObject;
            Provider.Proxy.UnloadSubScene(Provider);
            {
                UnloadSceneOperation operation = new(sceneObject);
                Engine.StartAsyncOperation(operation);
                return operation;
            }
        }
    }
}