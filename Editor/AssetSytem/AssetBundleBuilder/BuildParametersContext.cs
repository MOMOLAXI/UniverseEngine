using System;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using BuildParameters = UniverseEngineEditor.BuildParameters;

namespace Universe
{
    public class BuildParametersContext : IContextObject
    {
        private string m_PipelineOutputDirectory = string.Empty;
        private string m_PackageOutputDirectory = string.Empty;

        /// <summary>
        /// 构建参数
        /// </summary>
        public BuildParameters Parameters { get; }


        public BuildParametersContext(BuildParameters parameters)
        {
            Parameters = parameters;
        }

        /// <summary>
        /// 获取构建管线的输出目录
        /// </summary>
        /// <returns></returns>
        public string GetPipelineOutputDirectory()
        {
            if (string.IsNullOrEmpty(m_PipelineOutputDirectory))
            {
                m_PipelineOutputDirectory = AssetSystemEditor.MakePipelineOutputDirectory(Parameters.OutputRoot, Parameters.PackageName, Parameters.BuildTarget, Parameters.BuildMode);
            }
            return m_PipelineOutputDirectory;
        }

        /// <summary>
        /// 获取本次构建的补丁目录
        /// </summary>
        public string GetPackageOutputDirectory()
        {
            if (string.IsNullOrEmpty(m_PackageOutputDirectory))
            {
                m_PackageOutputDirectory = $"{Parameters.OutputRoot}/{Parameters.PackageName}/{Parameters.BuildTarget}/{Parameters.PackageVersion}";
            }
            return m_PackageOutputDirectory;
        }

        /// <summary>
        /// 获取内置构建管线的构建选项
        /// </summary>
        public BuildAssetBundleOptions GetPipelineBuildOptions()
        {
            // For the new build system, unity always need BuildAssetBundleOptions.CollectDependencies and BuildAssetBundleOptions.DeterministicAssetBundle
            // 除非设置ForceRebuildAssetBundle标记，否则会进行增量打包

            if (Parameters.BuildMode == EBuildMode.SimulateBuild)
                throw new("Should never get here !");

            BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
            opt |= BuildAssetBundleOptions.StrictMode; //Do not allow the build to succeed if any errors are reporting during it.

            if (Parameters.BuildMode == EBuildMode.DryRunBuild)
            {
                opt |= BuildAssetBundleOptions.DryRunBuild;
                return opt;
            }

            if (Parameters.CompressOption == ECompressOption.Uncompressed)
                opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
            else if (Parameters.CompressOption == ECompressOption.LZ4)
                opt |= BuildAssetBundleOptions.ChunkBasedCompression;

            if (Parameters.BuildMode == EBuildMode.ForceRebuild)
                opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle; //Force rebuild the asset bundles
            if (Parameters.DisableWriteTypeTree)
                opt |= BuildAssetBundleOptions.DisableWriteTypeTree; //Do not include type information within the asset bundle (don't write type tree).
            if (Parameters.IgnoreTypeTreeChanges)
                opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges; //Ignore the type tree changes when doing the incremental build check.

            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName;              //Disables Asset Bundle LoadAsset by file name.
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension; //Disables Asset Bundle LoadAsset by file name with extension.			

            return opt;
        }

        /// <summary>
        /// 获取可编程构建管线的构建参数
        /// </summary>
        public BundleBuildParameters GetSbpBuildParameters()
        {
            if (Parameters.BuildMode == EBuildMode.SimulateBuild)
            {
                throw new("Should never get here !");
            }

            BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(Parameters.BuildTarget);
            string pipelineOutputDirectory = GetPipelineOutputDirectory();
            BundleBuildParameters buildParams = new(Parameters.BuildTarget, targetGroup, pipelineOutputDirectory)
            {
                BundleCompression = Parameters.CompressOption switch
                {
                    ECompressOption.Uncompressed => UnityEngine.BuildCompression.Uncompressed,
                    ECompressOption.LZMA => UnityEngine.BuildCompression.LZMA,
                    ECompressOption.LZ4 => UnityEngine.BuildCompression.LZ4,
                    _ => throw new NotImplementedException(Parameters.CompressOption.ToString())
                }
            };

            if (Parameters.DisableWriteTypeTree)
            {
                buildParams.ContentBuildFlags |= UnityEditor.Build.Content.ContentBuildFlags.DisableWriteTypeTree;
            }

            buildParams.UseCache = true;
            buildParams.CacheServerHost = Parameters.SbpParameters.CacheServerHost;
            buildParams.CacheServerPort = Parameters.SbpParameters.CacheServerPort;
            buildParams.WriteLinkXML = Parameters.SbpParameters.WriteLinkXML;

            return buildParams;
        }
    }
}