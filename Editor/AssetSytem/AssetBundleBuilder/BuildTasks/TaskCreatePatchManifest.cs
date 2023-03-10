using System.Linq;
using System.Collections.Generic;
using UnityEngine.Build.Pipeline;
using UniverseEngineEditor;

namespace Universe
{
    public class PatchManifestContext : IContextObject
    {
        internal PatchManifest Manifest;
    }

    [UniverseBuildTask("创建补丁清单文件")]
    public class TaskCreatePatchManifest : IBuildTask
    {
        readonly Dictionary<string, int> m_CachedBundleID = new(10000);
        readonly Dictionary<string, string[]> m_CachedBundleDepends = new(10000);

        void IBuildTask.Run(BuildContext context)
        {
            CreatePatchManifestFile(context);
        }

        /// <summary>
        /// 创建补丁清单文件到输出目录
        /// </summary>
        void CreatePatchManifestFile(BuildContext context)
        {
            BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();
            BuildParametersContext buildParametersContext = context.GetContextObject<BuildParametersContext>();
            BuildParameters buildParameters = buildParametersContext.Parameters;
            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();

            // 创建新补丁清单
            PatchManifest patchManifest = new()
            {
                FileVersion = UniverseConstant.PATCH_MANIFEST_FILE_VERSION,
                EnableAddressable = buildMapContext.EnableAddressable,
                OutputNameStyle = (int)buildParameters.OutputNameStyle,
                PackageName = buildParameters.PackageName,
                PackageVersion = buildParameters.PackageVersion,
                BundleList = GetAllPatchBundle(context)
            };

            patchManifest.AssetList = GetAllPatchAsset(context, patchManifest);

            // 更新Unity内置资源包的引用关系
            if (buildParameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
            {
                if (buildParameters.BuildMode == EBuildMode.IncrementalBuild)
                {
                    TaskBuildingSbp.BuildResultContext buildResultContext = context.GetContextObject<TaskBuildingSbp.BuildResultContext>();
                    UpdateBuiltInBundleReference(patchManifest, buildResultContext, buildMapContext.ShadersBundleName);
                }
            }

            switch (buildParameters.BuildPipeline)
            {
                // 更新资源包之间的引用关系
                case EBuildPipeline.ScriptableBuildPipeline:
                {
                    if (buildParameters.BuildMode == EBuildMode.IncrementalBuild)
                    {
                        TaskBuildingSbp.BuildResultContext buildResultContext = context.GetContextObject<TaskBuildingSbp.BuildResultContext>();
                        UpdateScriptPipelineReference(patchManifest, buildResultContext);
                    }
                    break;
                }
                // 更新资源包之间的引用关系
                case EBuildPipeline.BuiltinBuildPipeline:
                {
                    if (buildParameters.BuildMode != EBuildMode.SimulateBuild)
                    {
                        TaskBuilding.BuildResultContext buildResultContext = context.GetContextObject<TaskBuilding.BuildResultContext>();
                        UpdateBuiltinPipelineReference(patchManifest, buildResultContext);
                    }
                    break;
                }
            }

            // 创建补丁清单文本文件
            {
                string fileName = AssetSystemNameGetter.GetManifestJsonFileName(buildParameters.PackageName, buildParameters.PackageVersion);
                string filePath = $"{packageOutputDirectory}/{fileName}";
                AssetManifestPatcher.SerializeToJson(filePath, patchManifest);
                EditorLog.Info($"创建补丁清单文件：{filePath}");
            }

            // 创建补丁清单二进制文件
            string packageHash;
            {
                string fileName = AssetSystemNameGetter.GetManifestBinaryFileName(buildParameters.PackageName, buildParameters.PackageVersion);
                string filePath = $"{packageOutputDirectory}/{fileName}";
                AssetManifestPatcher.SerializeToBinary(filePath, patchManifest);
                packageHash = HashUtility.FileMD5(filePath);
                EditorLog.Info($"创建补丁清单文件：{filePath}");

                PatchManifestContext patchManifestContext = new();
                byte[] bytesData = FileUtility.ReadAllBytes(filePath);
                patchManifestContext.Manifest = AssetManifestPatcher.DeserializeFromBinary(bytesData);
                context.SetContextObject(patchManifestContext);
            }

            // 创建补丁清单哈希文件
            {
                string fileName = AssetSystemNameGetter.GetPackageHashFileName(buildParameters.PackageName, buildParameters.PackageVersion);
                string filePath = $"{packageOutputDirectory}/{fileName}";
                FileUtility.CreateFile(filePath, packageHash);
                EditorLog.Info($"创建补丁清单哈希文件：{filePath}");
            }

            // 创建补丁清单版本文件
            {
                string fileName = AssetSystemNameGetter.GetPackageVersionFileName(buildParameters.PackageName);
                string filePath = $"{packageOutputDirectory}/{fileName}";
                FileUtility.CreateFile(filePath, buildParameters.PackageVersion);
                EditorLog.Info($"创建补丁清单版本文件：{filePath}");
            }
        }

        /// <summary>
        /// 获取资源包列表
        /// </summary>
        static List<PatchBundle> GetAllPatchBundle(BuildContext context)
        {
            BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();

            List<PatchBundle> result = new(1000);
            for (int i = 0; i < buildMapContext.BundleInfos.Count; i++)
            {
                BuildBundleInfo bundleInfo = buildMapContext.BundleInfos[i];
                PatchBundle patchBundle = bundleInfo.CreatePatchBundle();
                result.Add(patchBundle);
            }
            return result;
        }

        /// <summary>
        /// 获取资源列表
        /// </summary>
        List<PatchAsset> GetAllPatchAsset(BuildContext context, PatchManifest patchManifest)
        {
            BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();

            List<PatchAsset> result = new(1000);
            for (int i = 0; i < buildMapContext.BundleInfos.Count; i++)
            {
                BuildBundleInfo bundleInfo = buildMapContext.BundleInfos[i];
                BuildAssetInfo[] assetInfos = bundleInfo.GetAllPatchAssetInfos();
                for (int j = 0; j < assetInfos.Length; j++)
                {
                    BuildAssetInfo assetInfo = assetInfos[j];
                    PatchAsset patchAsset = new()
                    {
                        Address = buildMapContext.EnableAddressable ? assetInfo.Address : string.Empty,
                        AssetPath = assetInfo.AssetPath,
                        AssetTags = assetInfo.AssetTags.ToArray(),
                        BundleID = GetAssetBundleID(assetInfo.BundleName, patchManifest)
                    };

                    patchAsset.DependIDs = GetAssetBundleDependIDs(patchAsset.BundleID, assetInfo, patchManifest);
                    result.Add(patchAsset);
                }
            }
            return result;
        }
        static int[] GetAssetBundleDependIDs(int mainBundleID, BuildAssetInfo assetInfo, PatchManifest patchManifest)
        {
            List<int> result = new();
            foreach (BuildAssetInfo dependAssetInfo in assetInfo.AllDependAssetInfos)
            {
                if (dependAssetInfo.HasBundleName())
                {
                    int bundleID = GetAssetBundleID(dependAssetInfo.BundleName, patchManifest);
                    if (mainBundleID != bundleID)
                    {
                        if (result.Contains(bundleID) == false)
                            result.Add(bundleID);
                    }
                }
            }
            return result.ToArray();
        }

        static int GetAssetBundleID(string bundleName, PatchManifest patchManifest)
        {
            for (int index = 0; index < patchManifest.BundleList.Count; index++)
            {
                if (patchManifest.BundleList[index].BundleName == bundleName)
                    return index;
            }
            throw new($"Not found bundle name : {bundleName}");
        }

        /// <summary>
        /// 更新Unity内置资源包的引用关系
        /// </summary>
        void UpdateBuiltInBundleReference(PatchManifest patchManifest, TaskBuildingSbp.BuildResultContext buildResultContext, string shadersBunldeName)
        {
            // 获取所有依赖着色器资源包的资源包列表
            List<string> shaderBundleReferenceList = new();
            foreach (KeyValuePair<string, BundleDetails> valuePair in buildResultContext.Results.BundleInfos)
            {
                if (valuePair.Value.Dependencies.Any(t => t == shadersBunldeName))
                {
                    shaderBundleReferenceList.Add(valuePair.Key);
                }
            }

            // 注意：没有任何资源依赖着色器
            if (shaderBundleReferenceList.Count == 0)
            {
                return;
            }

            // 获取着色器资源包索引
            bool Predicate(PatchBundle s) => s.BundleName == shadersBunldeName;
            int shaderBundleId = patchManifest.BundleList.FindIndex(Predicate);
            if (shaderBundleId == -1)
            {
                throw new("没有发现着色器资源包！");
            }

            // 检测依赖交集并更新依赖ID
            foreach (PatchAsset patchAsset in patchManifest.AssetList)
            {
                IEnumerable<string> dependBundles = GetPatchAssetAllDependBundles(patchManifest, patchAsset);
                List<string> conflictAssetPathList = dependBundles.Intersect(shaderBundleReferenceList).ToList();
                if (conflictAssetPathList.Count > 0)
                {
                    List<int> newDependIDs = new(patchAsset.DependIDs);
                    if (newDependIDs.Contains(shaderBundleId) == false)
                    {
                        newDependIDs.Add(shaderBundleId);
                    }

                    patchAsset.DependIDs = newDependIDs.ToArray();
                }
            }
        }
        static IEnumerable<string> GetPatchAssetAllDependBundles(PatchManifest patchManifest, PatchAsset patchAsset)
        {
            List<string> result = new();
            string mainBundle = patchManifest.BundleList[patchAsset.BundleID].BundleName;
            result.Add(mainBundle);
            foreach (int dependID in patchAsset.DependIDs)
            {
                string dependBundle = patchManifest.BundleList[dependID].BundleName;
                result.Add(dependBundle);
            }

            return result;
        }

		#region 资源包引用关系相关

        void UpdateScriptPipelineReference(PatchManifest patchManifest, TaskBuildingSbp.BuildResultContext buildResultContext)
        {
            int totalCount = patchManifest.BundleList.Count;

            // 缓存资源包ID
            m_CachedBundleID.Clear();
            int progressValue = 0;
            for (int i = 0; i < patchManifest.BundleList.Count; i++)
            {
                PatchBundle patchBundle = patchManifest.BundleList[i];
                int bundleID = GetAssetBundleID(patchBundle.BundleName, patchManifest);
                m_CachedBundleID.Add(patchBundle.BundleName, bundleID);
                UniverseEditor.DisplayProgressBar("缓存资源包索引", ++progressValue, totalCount);
            }

            UniverseEditor.ClearProgressBar();

            // 缓存资源包依赖
            m_CachedBundleDepends.Clear();
            progressValue = 0;
            for (int i = 0; i < patchManifest.BundleList.Count; i++)
            {
                PatchBundle patchBundle = patchManifest.BundleList[i];
                if (patchBundle.IsRawFile)
                    continue;

                if (buildResultContext.Results.BundleInfos.ContainsKey(patchBundle.BundleName) == false)
                    throw new($"Not found bundle in SBP build results : {patchBundle.BundleName}");

                string[] depends = buildResultContext.Results.BundleInfos[patchBundle.BundleName].Dependencies;
                m_CachedBundleDepends.Add(patchBundle.BundleName, depends);
                UniverseEditor.DisplayProgressBar("缓存资源包依赖列表", ++progressValue, totalCount);
            }

            UniverseEditor.ClearProgressBar();

            // 计算资源包引用列表
            for (int i = 0; i < patchManifest.BundleList.Count; i++)
            {
                PatchBundle patchBundle = patchManifest.BundleList[i];
                patchBundle.ReferenceIDs = GetBundleRefrenceIDs(patchManifest, patchBundle);
                UniverseEditor.DisplayProgressBar("计算资源包引用关系", ++progressValue, totalCount);
            }

            UniverseEditor.ClearProgressBar();
        }
        void UpdateBuiltinPipelineReference(PatchManifest patchManifest, TaskBuilding.BuildResultContext buildResultContext)
        {
            int totalCount = patchManifest.BundleList.Count;

            // 缓存资源包ID
            m_CachedBundleID.Clear();
            int progressValue = 0;
            foreach (PatchBundle patchBundle in patchManifest.BundleList)
            {
                int bundleID = GetAssetBundleID(patchBundle.BundleName, patchManifest);
                m_CachedBundleID.Add(patchBundle.BundleName, bundleID);
                UniverseEditor.DisplayProgressBar("缓存资源包索引", ++progressValue, totalCount);
            }

            UniverseEditor.ClearProgressBar();

            // 缓存资源包依赖
            m_CachedBundleDepends.Clear();
            progressValue = 0;
            foreach (PatchBundle patchBundle in patchManifest.BundleList)
            {
                string[] depends = buildResultContext.UnityManifest.GetDirectDependencies(patchBundle.BundleName);
                m_CachedBundleDepends.Add(patchBundle.BundleName, depends);
                UniverseEditor.DisplayProgressBar("缓存资源包依赖列表", ++progressValue, totalCount);
            }

            UniverseEditor.ClearProgressBar();

            // 计算资源包引用列表
            progressValue = 0;
            foreach (PatchBundle patchBundle in patchManifest.BundleList)
            {
                patchBundle.ReferenceIDs = GetBundleRefrenceIDs(patchManifest, patchBundle);
                UniverseEditor.DisplayProgressBar("计算资源包引用关系", ++progressValue, totalCount);
            }

            UniverseEditor.ClearProgressBar();
        }

        private int[] GetBundleRefrenceIDs(PatchManifest patchManifest, PatchBundle targetBundle)
        {
            List<string> referenceList = new();
            foreach (PatchBundle patchBundle in patchManifest.BundleList)
            {
                string bundleName = patchBundle.BundleName;
                if (bundleName == targetBundle.BundleName)
                {
                    continue;
                }

                IEnumerable<string> dependencies = GetCachedBundleDepends(bundleName);
                if (dependencies.Contains(targetBundle.BundleName))
                {
                    referenceList.Add(bundleName);
                }
            }

            List<int> result = new();
            foreach (string bundleName in referenceList)
            {
                int bundleID = GetCachedBundleID(bundleName);
                if (!result.Contains(bundleID))
                {
                    result.Add(bundleID);
                }
            }

            return result.ToArray();
        }
        private int GetCachedBundleID(string bundleName)
        {
            if (!m_CachedBundleID.TryGetValue(bundleName, out int value))
            {
                throw new($"Not found cached bundle ID : {bundleName}");
            }

            return value;
        }
        IEnumerable<string> GetCachedBundleDepends(string bundleName)
        {
            if (!m_CachedBundleDepends.TryGetValue(bundleName, out string[] value))
            {
                throw new($"Not found cached bundle depends : {bundleName}");
            }

            return value;
        }

		#endregion

    }
}