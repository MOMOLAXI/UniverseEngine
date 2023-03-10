using UnityEditor;

namespace Universe
{
    [UniverseBuildTask("拷贝内置文件到流目录")]
    public class TaskCopyBuildinFiles : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
            PatchManifestContext patchManifestContext = context.GetContextObject<PatchManifestContext>();
            EBuildMode buildMode = buildParametersContext.Parameters.BuildMode;
            if (buildMode is not (EBuildMode.ForceRebuild or EBuildMode.IncrementalBuild))
            {
                return;
            }

            if (buildParametersContext.Parameters.CopyBuildinFileOption != ECopyBuildinFileOption.None)
            {
                CopyBuildinFilesToStreaming(buildParametersContext, patchManifestContext);
            }
        }

        /// <summary>
        /// 拷贝首包资源文件
        /// </summary>
        static void CopyBuildinFilesToStreaming(BuildParametersContext buildParametersContext, PatchManifestContext patchManifestContext)
        {
            ECopyBuildinFileOption option = buildParametersContext.Parameters.CopyBuildinFileOption;
            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            string streamingAssetsDirectory = AssetSystemEditor.GetStreamingAssetsFolderPath();
            string buildPackageName = buildParametersContext.Parameters.PackageName;
            string buildPackageVersion = buildParametersContext.Parameters.PackageVersion;

            // 加载补丁清单
            PatchManifest patchManifest = patchManifestContext.Manifest;

            // 清空流目录
            if (option is ECopyBuildinFileOption.ClearAndCopyAll or ECopyBuildinFileOption.ClearAndCopyByTags)
            {
                AssetSystemEditor.ClearStreamingAssetsFolder();
            }

            // 拷贝补丁清单文件
            {
                string fileName = AssetSystemNameGetter.GetManifestBinaryFileName(buildPackageName, buildPackageVersion);
                string sourcePath = $"{packageOutputDirectory}/{fileName}";
                string destPath = $"{streamingAssetsDirectory}/{fileName}";
                FileUtility.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝补丁清单哈希文件
            {
                string fileName = AssetSystemNameGetter.GetPackageHashFileName(buildPackageName, buildPackageVersion);
                string sourcePath = $"{packageOutputDirectory}/{fileName}";
                string destPath = $"{streamingAssetsDirectory}/{fileName}";
                FileUtility.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝补丁清单版本文件
            {
                string fileName = AssetSystemNameGetter.GetPackageVersionFileName(buildPackageName);
                string sourcePath = $"{packageOutputDirectory}/{fileName}";
                string destPath = $"{streamingAssetsDirectory}/{fileName}";
                FileUtility.CopyFile(sourcePath, destPath, true);
            }

            switch (option)
            {
                // 拷贝文件列表（所有文件）
                case ECopyBuildinFileOption.ClearAndCopyAll:
                case ECopyBuildinFileOption.OnlyCopyAll:
                {
                    foreach (PatchBundle patchBundle in patchManifest.BundleList)
                    {
                        string sourcePath = $"{packageOutputDirectory}/{patchBundle.FileName}";
                        string destPath = $"{streamingAssetsDirectory}/{patchBundle.FileName}";
                        FileUtility.CopyFile(sourcePath, destPath, true);
                    }
                    break;
                }
                // 拷贝文件列表（带标签的文件）
                case ECopyBuildinFileOption.ClearAndCopyByTags:
                case ECopyBuildinFileOption.OnlyCopyByTags:
                {
                    string[] tags = buildParametersContext.Parameters.CopyBuildinFileTags.Split(';');
                    foreach (PatchBundle patchBundle in patchManifest.BundleList)
                    {
                        if (patchBundle.HasTag(tags) == false)
                            continue;
                        string sourcePath = $"{packageOutputDirectory}/{patchBundle.FileName}";
                        string destPath = $"{streamingAssetsDirectory}/{patchBundle.FileName}";
                        FileUtility.CopyFile(sourcePath, destPath, true);
                    }
                    break;
                }
            }

            // 刷新目录
            AssetDatabase.Refresh();
            EditorLog.Info($"内置文件拷贝完成：{streamingAssetsDirectory}");
        }
    }
}