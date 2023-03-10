using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Universe
{
	public static class ShaderVariantCollector
	{
		private enum ESteps
		{
			None,
			Prepare,
			CollectAllMaterial,
			CollectVariants,
			CollectSleeping,
			WaitingDone,
		}

		private const float WAIT_MILLISECONDS = 1000f;
		private const float SLEEP_MILLISECONDS = 100f;
		private static string s_SavePath;
		private static string s_PackageName;
		private static int s_ProcessMaxNum;
		private static Action s_CompletedCallback;

		private static ESteps s_Steps = ESteps.None;
		private static Stopwatch s_ElapsedTime;
		private static List<string> s_AllMaterials;
		private static List<GameObject> s_AllSpheres = new(1000);


		/// <summary>
		/// 开始收集
		/// </summary>
		public static void Run(string savePath, string packageName, int processMaxNum, Action completedCallback)
		{
			if (s_Steps != ESteps.None)
				return;

			if (Path.HasExtension(savePath) == false)
				savePath = $"{savePath}.shadervariants";
			if (Path.GetExtension(savePath) != ".shadervariants")
				throw new("Shader variant file extension is invalid.");
			if (string.IsNullOrEmpty(packageName))
				throw new("Package name is null or empty !");

			// 注意：先删除再保存，否则ShaderVariantCollection内容将无法及时刷新
			AssetDatabase.DeleteAsset(savePath);
			FileUtility.CreateFileDirectory(savePath);
			s_SavePath = savePath;
			s_PackageName = packageName;
			s_ProcessMaxNum = processMaxNum;
			s_CompletedCallback = completedCallback;

			// 聚焦到游戏窗口
			UniverseEditor.FocusUnityGameWindow();

			// 创建临时测试场景
			CreateTempScene();

			s_Steps = ESteps.Prepare;
			EditorApplication.update += EditorUpdate;
		}

		private static void EditorUpdate()
		{
			if (s_Steps == ESteps.None)
				return;

			if (s_Steps == ESteps.Prepare)
			{
				ShaderVariantCollectionHelper.ClearCurrentShaderVariantCollection();
				s_Steps = ESteps.CollectAllMaterial;
				return; //等待一帧
			}

			if (s_Steps == ESteps.CollectAllMaterial)
			{
				s_AllMaterials = GetAllMaterials();
				s_Steps = ESteps.CollectVariants;
				return; //等待一帧
			}
			
			if (s_Steps == ESteps.CollectVariants)
			{
				int count = Mathf.Min(s_ProcessMaxNum, s_AllMaterials.Count);
				List<string> range = s_AllMaterials.GetRange(0, count);
				s_AllMaterials.RemoveRange(0, count);
				CollectVariants(range);

				if (s_AllMaterials.Count > 0)
				{
					s_ElapsedTime = Stopwatch.StartNew();
					s_Steps = ESteps.CollectSleeping;
				}
				else
				{
					s_ElapsedTime = Stopwatch.StartNew();
					s_Steps = ESteps.WaitingDone;
				}
			}

			if (s_Steps == ESteps.CollectSleeping)
			{
				if (s_ElapsedTime.ElapsedMilliseconds > SLEEP_MILLISECONDS)
				{
					DestroyAllSpheres();
					s_ElapsedTime.Stop();
					s_Steps = ESteps.CollectVariants;
				}
			}

			if (s_Steps == ESteps.WaitingDone)
			{
				// 注意：一定要延迟保存才会起效
				if (s_ElapsedTime.ElapsedMilliseconds > WAIT_MILLISECONDS)
				{
					s_ElapsedTime.Stop();
					s_Steps = ESteps.None;

					// 保存结果并创建清单
					ShaderVariantCollectionHelper.SaveCurrentShaderVariantCollection(s_SavePath);
					CreateManifest();

					Debug.Log("搜集SVC完毕！");
					EditorApplication.update -= EditorUpdate;
					s_CompletedCallback?.Invoke();
				}
			}
		}
		private static void CreateTempScene()
		{
			EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
		}
		private static List<string> GetAllMaterials()
		{
			int progressValue = 0;
			List<string> allAssets = new(1000);

			// 获取所有打包的资源
			CollectResult collectResult = AssetBundleCollectorSettingData.Setting.GetPackageAssets(EBuildMode.DryRunBuild, s_PackageName);
			foreach (CollectAssetInfo assetInfo in collectResult.CollectAssets)
			{
				string[] depends = AssetDatabase.GetDependencies(assetInfo.AssetPath, true);
				foreach (string dependAsset in depends)
				{
					if (allAssets.Contains(dependAsset) == false)
						allAssets.Add(dependAsset);
				}
				UniverseEditor.DisplayProgressBar("获取所有打包资源", ++progressValue, collectResult.CollectAssets.Count);
			}
			UniverseEditor.ClearProgressBar();

			// 搜集所有材质球
			progressValue = 0;
			List<string> allMaterial = new(1000);
			foreach (string assetPath in allAssets)
			{
				Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(Material))
				{
					allMaterial.Add(assetPath);
				}
				UniverseEditor.DisplayProgressBar("搜集所有材质球", ++progressValue, allAssets.Count);
			}
			UniverseEditor.ClearProgressBar();

			// 返回结果
			return allMaterial;
		}
		private static void CollectVariants(List<string> materials)
		{
			Camera camera = Camera.main;
			if (camera == null)
				throw new("Not found main camera.");

			// 设置主相机
			float aspect = camera.aspect;
			int totalMaterials = materials.Count;
			float height = Mathf.Sqrt(totalMaterials / aspect) + 1;
			float width = Mathf.Sqrt(totalMaterials / aspect) * aspect + 1;
			float halfHeight = Mathf.CeilToInt(height / 2f);
			float halfWidth = Mathf.CeilToInt(width / 2f);
			camera.orthographic = true;
			camera.orthographicSize = halfHeight;
			camera.transform.position = new(0f, 0f, -10f);

			// 创建测试球体
			int xMax = (int)(width - 1);
			int x = 0, y = 0;
			int progressValue = 0;
			for (int i = 0; i < materials.Count; i++)
			{
				string material = materials[i];
				Vector3 position = new(x - halfWidth + 1f, y - halfHeight + 1f, 0f);
				GameObject go = CreateSphere(material, position, i);
				if (go != null)
					s_AllSpheres.Add(go);
				if (x == xMax)
				{
					x = 0;
					y++;
				}
				else
				{
					x++;
				}
				UniverseEditor.DisplayProgressBar("照射所有材质球", ++progressValue, materials.Count);
			}
			UniverseEditor.ClearProgressBar();
		}
		private static GameObject CreateSphere(string assetPath, Vector3 position, int index)
		{
			Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
			Shader shader = material.shader;
			if (shader == null)
				return null;

			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			go.GetComponent<Renderer>().sharedMaterial = material;
			go.transform.position = position;
			go.name = $"Sphere_{index} | {material.name}";
			return go;
		}
		private static void DestroyAllSpheres()
		{
			foreach(GameObject go in s_AllSpheres)
			{
				Object.DestroyImmediate(go);
			}
			s_AllSpheres.Clear();

			// 尝试释放编辑器加载的资源
			EditorUtility.UnloadUnusedAssetsImmediate(true);
		}
		private static void CreateManifest()
		{
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			ShaderVariantCollection svc = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(s_SavePath);
			if (svc != null)
			{
				ShaderVariantCollectionManifest wrapper = ShaderVariantCollectionManifest.Extract(svc);
				string jsonData = JsonUtility.ToJson(wrapper, true);
				string savePath = s_SavePath.Replace(".shadervariants", ".json");
				File.WriteAllText(savePath, jsonData);
			}

			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
		}
	}
}