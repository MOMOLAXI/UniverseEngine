using System;
using System.IO;
using System.Collections.Generic;

namespace Universe
{
    internal static class CacheSystem
    {
        static readonly Dictionary<string, PackageCache> s_PackageCaches = new(1000);

        /// <summary>
        /// 初始化时的验证级别
        /// </summary>
        public static EVerifyLevel InitVerifyLevel { set; get; } = EVerifyLevel.Middle;

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public static void ClearAll()
        {
            s_PackageCaches.Clear();
        }

        /// <summary>
        /// 获取缓存文件总数
        /// </summary>
        public static int GetCachedFilesCount(string packageName)
        {
            PackageCache cache = GetOrCreateCache(packageName);
            return cache.GetCachedFilesCount();
        }

        /// <summary>
        /// 查询是否为验证文件
        /// </summary>
        public static bool IsCached(string packageName, string cacheGuid)
        {
            PackageCache cache = GetOrCreateCache(packageName);
            return cache.IsCached(cacheGuid);
        }

        /// <summary>
        /// 录入验证的文件
        /// </summary>
        public static void RecordFile(string packageName, string cacheGuid, PackageCache.RecordWrapper wrapper)
        {
            //Log.Info($"Record file : {packageName} = {cacheGUID}");
            PackageCache cache = GetOrCreateCache(packageName);
            cache.Record(cacheGuid, wrapper);
        }

        /// <summary>
        /// 丢弃验证的文件（同时删除文件）
        /// </summary>
        public static void DiscardFile(string packageName, string cacheGuid)
        {
            PackageCache cache = GetOrCreateCache(packageName);
            PackageCache.RecordWrapper wrapper = cache.TryGetWrapper(cacheGuid);
            if (wrapper == null)
            {
                return;
            }

            cache.Discard(cacheGuid);

            try
            {
                string dataFilePath = wrapper.DataFilePath;
                FileUtility.DeleteFileOwningDirectory(new(dataFilePath));
            }
            catch (Exception e)
            {
                Log.Error($"Failed to delete cache file ! {e.Message}");
            }
        }

        /// <summary>
        /// 验证缓存文件（子线程内操作）
        /// </summary>
        public static EVerifyResult VerifyingCacheFile(VerifyCacheElement element)
        {
            try
            {
                switch (InitVerifyLevel)
                {
                    case EVerifyLevel.Low when !File.Exists(element.InfoFilePath): return EVerifyResult.InfoFileNotExisted;
                    case EVerifyLevel.Low when !File.Exists(element.DataFilePath): return EVerifyResult.DataFileNotExisted;
                    case EVerifyLevel.Low: return EVerifyResult.Succeed;
                    case EVerifyLevel.Middle: break;
                    case EVerifyLevel.High: break;
                    default: throw new ArgumentOutOfRangeException();
                }

                if (!File.Exists(element.InfoFilePath))
                {
                    return EVerifyResult.InfoFileNotExisted;
                }

                // 解析信息文件获取验证数据
                CacheFileInfo.ReadInfoFromFile(element.InfoFilePath, out element.DataFileCRC, out element.DataFileSize);
            }
            catch (Exception)
            {
                return EVerifyResult.Exception;
            }

            return VerifyingInternal(element.DataFilePath, element.DataFileSize, element.DataFileCRC, InitVerifyLevel);
        }

        /// <summary>
        /// 验证下载文件（子线程内操作）
        /// </summary>
        public static EVerifyResult VerifyingTempFile(VerifyTempElement element)
        {
            return VerifyingInternal(element.TempDataFilePath, element.FileSize, element.FileCRC, EVerifyLevel.High);
        }

        /// <summary>
        /// 验证记录文件（主线程内操作）
        /// </summary>
        public static EVerifyResult VerifyingRecordFile(string packageName, string cacheGuid)
        {
            PackageCache cache = GetOrCreateCache(packageName);
            PackageCache.RecordWrapper wrapper = cache.TryGetWrapper(cacheGuid);
            if (wrapper == null)
            {
                return EVerifyResult.CacheNotFound;
            }

            EVerifyResult result = VerifyingInternal(wrapper.DataFilePath, wrapper.DataFileSize, wrapper.DataFileCRC, EVerifyLevel.High);
            return result;
        }

        /// <summary>
        /// 获取未被使用的缓存文件
        /// </summary>
        public static void GetUnusedCacheGUIDs(AssetsPackage package, List<string> result)
        {
            if (result == null)
            {
                return;
            }

            PackageCache cache = GetOrCreateCache(package.PackageName);
            List<string> keys = cache.GetAllKeys();
            result.Clear();
            foreach (string cacheGuid in keys)
            {
                if (package.IsIncludeBundleFile(cacheGuid) == false)
                {
                    result.Add(cacheGuid);
                }
            }
        }
        
        static EVerifyResult VerifyingInternal(string filePath, long fileSize, string fileCRC, EVerifyLevel verifyLevel)
        {
            try
            {
                if (File.Exists(filePath) == false)
                    return EVerifyResult.DataFileNotExisted;

                // 先验证文件大小
                long size = FileUtility.GetFileSize(filePath);
                if (size < fileSize)
                {
                    return EVerifyResult.FileNotComplete;
                }
                if (size > fileSize)
                {
                    return EVerifyResult.FileOverflow;
                }

                // 再验证文件CRC
                if (verifyLevel == EVerifyLevel.High)
                {
                    string crc = HashUtility.FileCRC32(filePath);
                    if (crc == fileCRC)
                    {
                        return EVerifyResult.Succeed;
                    }

                    return EVerifyResult.FileCrcError;
                }

                return EVerifyResult.Succeed;
            }
            catch (Exception)
            {
                return EVerifyResult.Exception;
            }
        }
        static PackageCache GetOrCreateCache(string packageName)
        {
            if (s_PackageCaches.TryGetValue(packageName, out PackageCache cache) == false)
            {
                cache = new(packageName);
                s_PackageCaches.Add(packageName, cache);
            }
            return cache;
        }
    }
}