using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UniverseEngineEditor;

namespace Universe
{
    [UniverseBuildTask("创建构建报告文件")]
    public class TaskCreateReport : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            BuildParametersContext buildParameters = context.GetContextObject<BuildParametersContext>();
            BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();
            PatchManifestContext patchManifestContext = context.GetContextObject<PatchManifestContext>();

            EBuildMode buildMode = buildParameters.Parameters.BuildMode;
            if (buildMode != EBuildMode.SimulateBuild)
            {
                CreateReportFile(buildParameters, buildMapContext, patchManifestContext);
            }
        }

        private void CreateReportFile(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext, PatchManifestContext patchManifestContext)
        {
            BuildParameters buildParameters = buildParametersContext.Parameters;

            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            PatchManifest patchManifest = patchManifestContext.Manifest;
            BuildReport buildReport = new();

            // 概述信息
            UnityEditor.PackageManager.PackageInfo packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(BuildReport).Assembly);
            if (packageInfo != null)
            {
                buildReport.Summary.AssetSystemVersion = packageInfo.version;
            }

            buildReport.Summary.UnityVersion = UnityEngine.Application.unityVersion;
            buildReport.Summary.BuildDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            buildReport.Summary.BuildSeconds = BuildRunner.TotalSeconds;
            buildReport.Summary.BuildTarget = buildParameters.BuildTarget;
            buildReport.Summary.BuildPipeline = buildParameters.BuildPipeline;
            buildReport.Summary.BuildMode = buildParameters.BuildMode;
            buildReport.Summary.BuildPackageName = buildParameters.PackageName;
            buildReport.Summary.BuildPackageVersion = buildParameters.PackageVersion;
            buildReport.Summary.EnableAddressable = buildMapContext.EnableAddressable;
            buildReport.Summary.UniqueBundleName = buildMapContext.UniqueBundleName;
            buildReport.Summary.EncryptionServicesClassName = buildParameters.EncryptionServices == null ? "null" : buildParameters.EncryptionServices.GetType().FullName;

            // 构建参数
            buildReport.Summary.OutputNameStyle = buildParameters.OutputNameStyle;
            buildReport.Summary.CompressOption = buildParameters.CompressOption;
            buildReport.Summary.DisableWriteTypeTree = buildParameters.DisableWriteTypeTree;
            buildReport.Summary.IgnoreTypeTreeChanges = buildParameters.IgnoreTypeTreeChanges;

            // 构建结果
            buildReport.Summary.AssetFileTotalCount = buildMapContext.AssetFileCount;
            buildReport.Summary.MainAssetTotalCount = GetMainAssetCount(patchManifest);
            buildReport.Summary.AllBundleTotalCount = GetAllBundleCount(patchManifest);
            buildReport.Summary.AllBundleTotalSize = GetAllBundleSize(patchManifest);
            buildReport.Summary.EncryptedBundleTotalCount = GetEncryptedBundleCount(patchManifest);
            buildReport.Summary.EncryptedBundleTotalSize = GetEncryptedBundleSize(patchManifest);
            buildReport.Summary.RawBundleTotalCount = GetRawBundleCount(patchManifest);
            buildReport.Summary.RawBundleTotalSize = GetRawBundleSize(patchManifest);

            // 资源对象列表
            buildReport.AssetInfos = new(patchManifest.AssetList.Count);
            foreach (PatchAsset patchAsset in patchManifest.AssetList)
            {
                PatchBundle mainBundle = patchManifest.BundleList[patchAsset.BundleID];
                ReportAssetInfo reportAssetInfo = new()
                {
                    Address = patchAsset.Address,
                    AssetPath = patchAsset.AssetPath,
                    AssetTags = patchAsset.AssetTags,
                    AssetGuid = AssetDatabase.AssetPathToGUID(patchAsset.AssetPath),
                    MainBundleName = mainBundle.BundleName,
                    MainBundleSize = mainBundle.FileSize,
                    DependBundles = GetDependBundles(patchManifest, patchAsset),
                    DependAssets = GetDependAssets(buildMapContext, mainBundle.BundleName, patchAsset.AssetPath)
                };

                buildReport.AssetInfos.Add(reportAssetInfo);
            }

            // 资源包列表
            buildReport.BundleInfos = new(patchManifest.BundleList.Count);
            foreach (PatchBundle patchBundle in patchManifest.BundleList)
            {
                ReportBundleInfo reportBundleInfo = new()
                {
                    BundleName = patchBundle.BundleName,
                    FileName = patchBundle.FileName,
                    FileHash = patchBundle.FileHash,
                    FileCRC = patchBundle.FileCRC,
                    FileSize = patchBundle.FileSize,
                    Tags = patchBundle.Tags,
                    ReferenceIDs = patchBundle.ReferenceIDs,
                    IsRawFile = patchBundle.IsRawFile,
                    LoadMethod = (EBundleLoadMethod)patchBundle.LoadMethod
                };
                buildReport.BundleInfos.Add(reportBundleInfo);
            }

            // 序列化文件
            string fileName = AssetSystemNameGetter.GetReportFileName(buildParameters.PackageName, buildParameters.PackageVersion);
            string filePath = $"{packageOutputDirectory}/{fileName}";
            BuildReport.Serialize(filePath, buildReport);
            EditorLog.Info($"资源构建报告文件创建完成：{filePath}");
        }

        /// <summary>
        /// 获取资源对象依赖的所有资源包
        /// </summary>
        static List<string> GetDependBundles(PatchManifest patchManifest, PatchAsset patchAsset)
        {
            List<string> dependBundles = new(patchAsset.DependIDs.Length);
            foreach (int index in patchAsset.DependIDs)
            {
                string dependBundleName = patchManifest.BundleList[index].BundleName;
                dependBundles.Add(dependBundleName);
            }
            return dependBundles;
        }

        /// <summary>
        /// 获取资源对象依赖的其它所有资源
        /// </summary>
        static List<string> GetDependAssets(BuildMapContext buildMapContext, string bundleName, string assetPath)
        {
            List<string> result = new();
            if (buildMapContext.TryGetBundleInfo(bundleName, out BuildBundleInfo bundleInfo))
            {
                BuildAssetInfo findAssetInfo = null;
                for (int i = 0; i < bundleInfo.BuildinAssets.Count; i++)
                {
                    BuildAssetInfo buildinAsset = bundleInfo.BuildinAssets[i];
                    if (buildinAsset.AssetPath == assetPath)
                    {
                        findAssetInfo = buildinAsset;
                        break;
                    }
                }

                if (findAssetInfo == null)
                {
                    throw new($"Not found asset {assetPath} in bunlde {bundleName}");
                }

                for (int i = 0; i < findAssetInfo.AllDependAssetInfos.Count; i++)
                {
                    BuildAssetInfo dependAssetInfo = findAssetInfo.AllDependAssetInfos[i];
                    result.Add(dependAssetInfo.AssetPath);
                }
            }
            else
            {
                throw new($"Not found bundle : {bundleName}");
            }

            return result;
        }

        static int GetMainAssetCount(PatchManifest patchManifest)
        {
            return patchManifest.AssetList.Count;
        }

        static int GetAllBundleCount(PatchManifest patchManifest)
        {
            return patchManifest.BundleList.Count;
        }

        static long GetAllBundleSize(PatchManifest patchManifest)
        {
            long fileBytes = 0;
            for (int i = 0; i < patchManifest.BundleList.Count; i++)
            {
                PatchBundle patchBundle = patchManifest.BundleList[i];
                fileBytes += patchBundle.FileSize;
            }

            return fileBytes;
        }
        static int GetEncryptedBundleCount(PatchManifest patchManifest)
        {
            int fileCount = 0;
            for (int i = 0; i < patchManifest.BundleList.Count; i++)
            {
                PatchBundle patchBundle = patchManifest.BundleList[i];
                if (patchBundle.LoadMethod != (byte)EBundleLoadMethod.Normal)
                {
                    fileCount++;
                }
            }

            return fileCount;
        }
        static long GetEncryptedBundleSize(PatchManifest patchManifest)
        {
            long fileBytes = 0;
            for (int i = 0; i < patchManifest.BundleList.Count; i++)
            {
                PatchBundle patchBundle = patchManifest.BundleList[i];
                if (patchBundle.LoadMethod != (byte)EBundleLoadMethod.Normal)
                {
                    fileBytes += patchBundle.FileSize;
                }
            }

            return fileBytes;
        }
        static int GetRawBundleCount(PatchManifest patchManifest)
        {
            int fileCount = 0;
            for (int i = 0; i < patchManifest.BundleList.Count; i++)
            {
                PatchBundle patchBundle = patchManifest.BundleList[i];
                if (patchBundle.IsRawFile)
                {
                    fileCount++;
                }
            }
            return fileCount;
        }

        static long GetRawBundleSize(PatchManifest patchManifest)
        {
            long fileBytes = 0;
            for (int i = 0; i < patchManifest.BundleList.Count; i++)
            {
                PatchBundle patchBundle = patchManifest.BundleList[i];
                if (patchBundle.IsRawFile)
                {
                    fileBytes += patchBundle.FileSize;
                }
            }

            return fileBytes;
        }
    }
}