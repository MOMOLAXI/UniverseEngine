using UnityEditor;
using UnityEngine;
using UniverseEngineEditor;

namespace Universe
{
	public static class AssetBundleSimulateBuilder
	{
		/// <summary>
		/// 模拟构建
		/// </summary>
		public static string SimulateBuild(string packageName)
		{
			Debug.Log($"Begin to create simulate package : {packageName}");
			string defaultOutputRoot = AssetSystemEditor.GetDefaultOutputRoot();
			BuildParameters buildParameters = new()
			{
				OutputRoot = defaultOutputRoot,
				BuildTarget = EditorUserBuildSettings.activeBuildTarget,
				BuildMode = EBuildMode.SimulateBuild,
				PackageName = packageName,
				PackageVersion = "Simulate"
			};

			AssetBundleBuilder builder = new();
			BuildResult buildResult = builder.Run(buildParameters);
			if (buildResult.Success)
			{
				string manifestFileName = AssetSystemNameGetter.GetManifestBinaryFileName(buildParameters.PackageName, buildParameters.PackageVersion);
				string manifestFilePath = $"{buildResult.OutputPackageDirectory}/{manifestFileName}";
				return manifestFilePath;
			}
			else
			{
				return null;
			}
		}
	}
}