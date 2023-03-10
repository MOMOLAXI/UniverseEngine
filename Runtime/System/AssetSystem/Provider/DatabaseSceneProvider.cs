using UnityEngine;
using UnityEngine.SceneManagement;

namespace Universe
{
	sealed internal class DatabaseSceneProvider : ProviderBase
	{
		public readonly LoadSceneMode SceneMode;
		private readonly bool m_ActivateOnLoad;
		private readonly int m_Priority;
		private AsyncOperation m_AsyncOp;

		public DatabaseSceneProvider(AssetPackageProxy proxy, string providerGuid, AssetInfo assetInfo, LoadSceneMode sceneMode, bool activateOnLoad, int priority) : base(proxy, providerGuid, assetInfo)
		{
			SceneMode = sceneMode;
			m_ActivateOnLoad = activateOnLoad;
			m_Priority = priority;
		}
		public override void Update()
		{
#if UNITY_EDITOR
			if (IsDone)
				return;

			if (Status == EStatus.None)
			{
				Status = EStatus.Loading;
			}

			// 1. 加载资源对象
			if (Status == EStatus.Loading)
			{
				LoadSceneParameters loadSceneParameters = new()
				{
					loadSceneMode = SceneMode
				};
				m_AsyncOp = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(MainAssetInfo.AssetPath, loadSceneParameters);
				if (m_AsyncOp != null)
				{
					m_AsyncOp.allowSceneActivation = true;
					m_AsyncOp.priority = m_Priority;
					SceneObject = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
					Status = EStatus.Checking;
				}
				else
				{
					Status = EStatus.Failed;
					LastError = $"Failed to load scene : {MainAssetInfo.AssetPath}";
					Log.Error(LastError);
					InvokeCompletion();
				}
			}

			// 2. 检测加载结果
			if (Status == EStatus.Checking)
			{
				Progress = m_AsyncOp.progress;
				if (m_AsyncOp.isDone)
				{				
					if (SceneObject.IsValid() && m_ActivateOnLoad)
						SceneManager.SetActiveScene(SceneObject);

					Status = SceneObject.IsValid() ? EStatus.Succeed : EStatus.Failed;
					if (Status == EStatus.Failed)
					{
						LastError = $"The loaded scene is invalid : {MainAssetInfo.AssetPath}";
						Log.Error(LastError);
					}
					InvokeCompletion();
				}
			}
#endif
		}
	}
}