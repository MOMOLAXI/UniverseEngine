namespace Universe
{
    public static class AssetSystemNameGetter
    {
        /// <summary>
        /// 获取构建报告文件名
        /// </summary>
        public static string GetReportFileName(string packageName, string packageVersion)
        {
            return $"{UniverseConstant.REPORT_FILE_NAME}_{packageName}_{packageVersion}.json";
        }

        /// <summary>
        /// 获取清单文件完整名称
        /// </summary>
        public static string GetManifestBinaryFileName(string packageName, string packageVersion)
        {
            return $"{UniverseConstant.PATCH_MANIFEST_FILE_NAME}_{packageName}_{packageVersion}.bytes";
        }

        /// <summary>
        /// 获取清单文件完整名称
        /// </summary>
        public static string GetManifestJsonFileName(string packageName, string packageVersion)
        {
            return $"{UniverseConstant.PATCH_MANIFEST_FILE_NAME}_{packageName}_{packageVersion}.json";
        }

        /// <summary>
        /// 获取包裹的哈希文件完整名称
        /// </summary>
        public static string GetPackageHashFileName(string packageName, string packageVersion)
        {
            return $"{UniverseConstant.PATCH_MANIFEST_FILE_NAME}_{packageName}_{packageVersion}.hash";
        }

        /// <summary>
        /// 获取包裹的版本文件完整名称
        /// </summary>
        public static string GetPackageVersionFileName(string packageName)
        {
            return $"{UniverseConstant.PATCH_MANIFEST_FILE_NAME}_{packageName}.version";
        }
    }
}