using System.IO;

namespace Universe
{
    /// <summary>
    /// 资源路径帮助类
    /// </summary>
    internal static class AssetPath
    {
        static string BuildinPath;
        static string SandboxPath;

        /// <summary>
        /// 获取基于流文件夹的加载路径
        /// </summary>
        public static string MakeStreamingLoadPath(string path)
        {
            if (string.IsNullOrEmpty(BuildinPath))
            {
                BuildinPath = StringUtility.Format("{0}/{1}",
                                                   UnityEngine.Application.streamingAssetsPath,
                                                   UniverseConstant.STREAMING_ASSETS_BUILDIN_FOLDER);
            }

            return StringUtility.Format("{0}/{1}", BuildinPath, path);
        }

        /// <summary>
        /// 获取基于沙盒文件夹的加载路径
        /// </summary>
        public static string MakePersistentLoadPath(string path)
        {
            string root = GetPersistentRootPath();
            return StringUtility.Format("{0}/{1}", root, path);
        }

        /// <summary>
        /// 获取沙盒文件夹路径
        /// </summary>
        public static string GetPersistentRootPath()
        {
        #if UNITY_EDITOR
            // 注意：为了方便调试查看，编辑器下把存储目录放到项目里
            if (string.IsNullOrEmpty(SandboxPath))
            {
                string directory = Path.GetDirectoryName(UnityEngine.Application.dataPath);
                string projectPath = GetRegularPath(directory);
                SandboxPath = StringUtility.Format("{0}/Sandbox", projectPath);
            }
            return SandboxPath;
        #else
			if (string.IsNullOrEmpty(SandboxPath))
			{
				SandboxPath = StringUtility.Format("{0}/Sandbox", UnityEngine.Application.persistentDataPath);
			}
			return SandboxPath;
        #endif
        }
        private static string GetRegularPath(string path)
        {
            return path.Replace('\\', '/').Replace("\\", "/"); //替换为Linux路径格式
        }

        /// <summary>
        /// 获取WWW加载本地资源的路径
        /// </summary>
        public static string ConvertToWWWPath(string path)
        {
        #if UNITY_EDITOR
            return StringUtility.Format("file:///{0}", path);
        #elif UNITY_IPHONE
			return StringUtility.Format("file://{0}", path);
        #elif UNITY_ANDROID
			return path;
        #elif UNITY_STANDALONE
			return StringUtility.Format("file:///{0}", path);
        #elif UNITY_WEBGL
			return path;
        #endif
        }
    }

}