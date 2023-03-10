namespace Universe
{
    public static class UniverseConstant
    {
        /// <summary>
        /// 清单文件名称
        /// </summary>
        public const string PATCH_MANIFEST_FILE_NAME = "UniversePatchManifest";
        
        /// <summary>
        /// 清单文件头标记
        /// </summary>
        public const uint PATCH_MANIFEST_FILE_SIGN = 0x594F4F;

        /// <summary>
        /// 清单文件极限大小（100MB）
        /// </summary>
        public const int PATCH_MANIFEST_FILE_MAX_SIZE = 104857600;

        /// <summary>
        /// 清单文件格式版本
        /// </summary>
        public const string PATCH_MANIFEST_FILE_VERSION = "1.0.0";
        
        /// <summary>
        /// 缓存的数据文件名称
        /// </summary>
        public const string CACHE_BUNDLE_DATA_FILE_NAME = "__data";

        /// <summary>
        /// 缓存的信息文件名称
        /// </summary>
        public const string CACHE_BUNDLE_INFO_FILE_NAME = "__info";
        
        /// <summary>
        /// 构建输出文件夹名称
        /// </summary>
        public const string OUTPUT_FOLDER_NAME = "OutputCache";
        
        /// <summary>
        /// 构建输出的报告文件
        /// </summary>
        public const string REPORT_FILE_NAME = "BuildReport";

        /// <summary>
        /// 内置资源目录名称
        /// </summary>
        public const string STREAMING_ASSETS_BUILDIN_FOLDER = "BuildinFiles";
    }
}