using System.Collections.Generic;

namespace Universe
{
    /// <summary>
    /// 持久化目录帮助类
    /// </summary>
    internal static class PersistentHelper
    {
        const string CACHE_FOLDER_NAME = "CacheFiles";
        const string CACHED_BUNDLE_FILE_FOLDER = "BundleFiles";
        const string CACHED_RAW_FILE_FOLDER = "RawFiles";
        const string MANIFEST_FOLDER_NAME = "ManifestFiles";
        const string APP_FOOT_PRINT_FILE_NAME = "ApplicationFootPrint.bytes";

        /// <summary>
        /// 获取缓存的BundleFile文件夹路径
        /// </summary>
        static readonly Dictionary<string, string> s_CachedBundleFileFolder = new(100);

        /// <summary>
        /// 获取缓存的RawFile文件夹路径
        /// </summary>
        static readonly Dictionary<string, string> s_CachedRawFileFolder = new(100);

        /// <summary>
        /// 删除沙盒总目录
        /// </summary>
        public static void DeleteSandbox()
        {
            string directoryPath = AssetPath.MakePersistentLoadPath(string.Empty);
            FileUtility.DeleteDirectory(directoryPath);
        }

        /// <summary>
        /// 删除沙盒内的缓存文件夹
        /// </summary>
        public static void DeleteCacheFolder()
        {
            string root = AssetPath.MakePersistentLoadPath(CACHE_FOLDER_NAME);
            FileUtility.DeleteDirectory(root);
        }

        /// <summary>
        /// 删除沙盒内的清单文件夹
        /// </summary>
        public static void DeleteManifestFolder()
        {
            string root = AssetPath.MakePersistentLoadPath(MANIFEST_FOLDER_NAME);
            FileUtility.DeleteDirectory(root);
        }

        public static string GetCachedBundleFileFolderPath(string packageName)
        {
            if (!s_CachedBundleFileFolder.TryGetValue(packageName, out string value))
            {
                string root = AssetPath.MakePersistentLoadPath(CACHE_FOLDER_NAME);
                value = $"{root}/{packageName}/{CACHED_BUNDLE_FILE_FOLDER}";
                s_CachedBundleFileFolder.Add(packageName, value);
            }

            return value;
        }
        
        public static string GetCachedRawFileFolderPath(string packageName)
        {
            if (s_CachedRawFileFolder.TryGetValue(packageName, out string value) == false)
            {
                string root = AssetPath.MakePersistentLoadPath(CACHE_FOLDER_NAME);
                value = $"{root}/{packageName}/{CACHED_RAW_FILE_FOLDER}";
                s_CachedRawFileFolder.Add(packageName, value);
            }
            return value;
        }

        /// <summary>
        /// 获取应用程序的水印文件路径
        /// </summary>
        public static string GetAppFootPrintFilePath()
        {
            return AssetPath.MakePersistentLoadPath(APP_FOOT_PRINT_FILE_NAME);
        }

        /// <summary>
        /// 获取沙盒内清单文件的路径
        /// </summary>
        public static string GetCacheManifestFilePath(string packageName, string packageVersion)
        {
            string fileName = AssetSystemNameGetter.GetManifestBinaryFileName(packageName, packageVersion);
            return AssetPath.MakePersistentLoadPath($"{MANIFEST_FOLDER_NAME}/{fileName}");
        }

        /// <summary>
        /// 获取沙盒内包裹的哈希文件的路径
        /// </summary>
        public static string GetCachePackageHashFilePath(string packageName, string packageVersion)
        {
            string fileName = AssetSystemNameGetter.GetPackageHashFileName(packageName, packageVersion);
            return AssetPath.MakePersistentLoadPath($"{MANIFEST_FOLDER_NAME}/{fileName}");
        }

        /// <summary>
        /// 获取沙盒内包裹的版本文件的路径
        /// </summary>
        public static string GetCachePackageVersionFilePath(string packageName)
        {
            string fileName = AssetSystemNameGetter.GetPackageVersionFileName(packageName);
            return AssetPath.MakePersistentLoadPath($"{MANIFEST_FOLDER_NAME}/{fileName}");
        }

        /// <summary>
        /// 保存默认的包裹版本
        /// </summary>
        public static void SaveCachePackageVersionFile(string packageName, string version)
        {
            string filePath = GetCachePackageVersionFilePath(packageName);
            FileUtility.CreateFile(filePath, version);
        }
    }
}