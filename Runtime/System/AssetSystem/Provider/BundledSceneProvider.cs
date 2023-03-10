using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Universe
{
	sealed internal class BundledSceneProvider : BundledProvider
	{
		public readonly LoadSceneMode SceneMode;
		private readonly string m_SceneName;
		private readonly bool m_ActivateOnLoad;
		private readonly int m_Priority;
		private AsyncOperation m_AsyncOp;

		public BundledSceneProvider(AssetPackageProxy proxy, string providerGuid, AssetInfo assetInfo, LoadSceneMode sceneMode, bool activateOnLoad, int priority) : base(proxy, providerGuid, assetInfo)
		{
			SceneMode = sceneMode;
			m_SceneName = Path.GetFileNameWithoutExtension(assetInfo.AssetPath);
			m_ActivateOnLoad = activateOnLoad;
			m_Priority = priority;
		}
		public override void Update()
		{
			DebugBeginRecording();

			if (IsDone)
				return;

			if (Status == EStatus.None)
			{
				Status = EStatus.CheckBundle;
			}

			// 1. 检测资源包
			if (Status == EStatus.CheckBundle)
			{
				if (DependBundleGroup.IsDone() == false)
					return;
				if (OwnerBundle.IsDone() == false)
					return;

				if (DependBundleGroup.IsSucceed() == false)
				{
					Status = EStatus.Failed;
					LastError = DependBundleGroup.GetLastError();
					InvokeCompletion();
					return;
				}

				if (OwnerBundle.Status != BundleLoaderBase.EStatus.Succeed)
				{
					Status = EStatus.Failed;
					LastError = OwnerBundle.LastError;
					InvokeCompletion();
					return;
				}

				Status = EStatus.Loading;
			}

			// 2. 加载场景
			if (Status == EStatus.Loading)
			{
				// 注意：如果场景不存在则返回NULL
				m_AsyncOp = SceneManager.LoadSceneAsync(MainAssetInfo.AssetPath, SceneMode);
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
					LastError = $"Failed to load scene : {m_SceneName}";
					Log.Error(LastError);
					InvokeCompletion();
				}
			}

			// 3. 检测加载结果
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
						LastError = $"The load scene is invalid : {MainAssetInfo.AssetPath}";
						Log.Error(LastError);
					}
					InvokeCompletion();
				}
			}
		}
	}
}