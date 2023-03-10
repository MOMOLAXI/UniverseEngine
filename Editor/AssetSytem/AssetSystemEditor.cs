using System.IO;
using UnityEditor;
using UnityEngine;

namespace Universe
{
    public static class AssetSystemEditor
    {
        /// <summary>
        /// 获取默认的输出根路录
        /// </summary>
        public static string GetDefaultOutputRoot()
        {
            string projectPath = FileUtility.GetProjectPath();
            return $"{projectPath}/UniverseResource";
        }

        /// <summary>
        /// 获取流文件夹路径
        /// </summary>
        public static string GetStreamingAssetsFolderPath()
        {
            return $"{Application.dataPath}/StreamingAssets/{UniverseConstant.STREAMING_ASSETS_BUILDIN_FOLDER}/";
        }

        /// <summary>
        /// 清空流文件夹
        /// </summary>
        public static void ClearStreamingAssetsFolder()
        {
            string streamingFolderPath = GetStreamingAssetsFolderPath();
            FileUtility.ClearFolder(streamingFolderPath);
        }

        /// <summary>
        /// 删除流文件夹内无关的文件
        /// 删除.manifest文件和.meta文件
        /// </summary>
        public static void DeleteStreamingAssetsIgnoreFiles()
        {
            string streamingFolderPath = GetStreamingAssetsFolderPath();
            if (Directory.Exists(streamingFolderPath))
            {
                string[] files = Directory.GetFiles(streamingFolderPath, "*.manifest", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    FileInfo info = new(file);
                    info.Delete();
                }

                files = Directory.GetFiles(streamingFolderPath, "*.meta", SearchOption.AllDirectories);
                foreach (string item in files)
                {
                    FileInfo info = new(item);
                    info.Delete();
                }
            }
        }

        /// <summary>
        /// 获取构建管线的输出目录
        /// </summary>
        public static string MakePipelineOutputDirectory(string outputRoot, string buildPackage, BuildTarget buildTarget, EBuildMode buildMode)
        {
            string outputDirectory = $"{outputRoot}/{buildPackage}/{buildTarget}/{UniverseConstant.OUTPUT_FOLDER_NAME}";
            return outputDirectory;
        }
    }
}