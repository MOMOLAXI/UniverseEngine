using System.IO;
using System.Collections.Generic;

namespace Universe
{
    internal class FindCacheFilesOperation : AsyncOperationBase
    {
        enum ESteps
        {
            None,
            FindPrepare,
            FindBundleFiles,
            FindRawFiles,
            Done,
        }

        readonly string m_PackageName;
        float m_VerifyStartTime;
        IEnumerator<DirectoryInfo> m_BundleFilesEnumerator;
        IEnumerator<DirectoryInfo> m_RawFilesEnumerator;
        ESteps m_Steps = ESteps.None;

        /// <summary>
        /// 需要验证的元素
        /// </summary>
        public readonly List<VerifyCacheElement> VerifyElements = new(5000);

        public FindCacheFilesOperation(string packageName)
        {
            m_PackageName = packageName;
        }

        internal override void Start()
        {
            m_Steps = ESteps.FindPrepare;
            m_VerifyStartTime = UnityEngine.Time.realtimeSinceStartup;
        }

        internal override void Update()
        {
            switch (m_Steps)
            {
                case ESteps.None:
                case ESteps.Done:
                {
                    return;
                }
                case ESteps.FindPrepare:
                {
                    // BundleFiles
                    {
                        string rootPath = PersistentHelper.GetCachedBundleFileFolderPath(m_PackageName);
                        DirectoryInfo rootDirectory = new(rootPath);
                        if (rootDirectory.Exists)
                        {
                            IEnumerable<DirectoryInfo> directorieInfos = rootDirectory.EnumerateDirectories();
                            m_BundleFilesEnumerator = directorieInfos.GetEnumerator();
                        }
                    }

                    // RawFiles
                    {
                        string rootPath = PersistentHelper.GetCachedRawFileFolderPath(m_PackageName);
                        DirectoryInfo rootDirectory = new(rootPath);
                        if (rootDirectory.Exists)
                        {
                            IEnumerable<DirectoryInfo> directorieInfos = rootDirectory.EnumerateDirectories();
                            m_RawFilesEnumerator = directorieInfos.GetEnumerator();
                        }
                    }

                    m_Steps = ESteps.FindBundleFiles;
                    break;
                }
                case ESteps.FindBundleFiles when UpdateFindBundleFiles():
                {
                    return;
                }
                case ESteps.FindBundleFiles:
                {
                    m_Steps = ESteps.FindRawFiles;
                    break;
                }
                case ESteps.FindRawFiles when UpdateFindRawFiles():
                {
                    return;
                }
                // 注意：总是返回成功
                case ESteps.FindRawFiles:
                {
                    m_Steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                    float costTime = UnityEngine.Time.realtimeSinceStartup - m_VerifyStartTime;
                    Log.Info($"Find cache files elapsed time {costTime:f1} seconds");
                    break;
                }
            }
        }

        bool UpdateFindBundleFiles()
        {
            if (m_BundleFilesEnumerator == null)
                return false;

            bool isFindItem;
            while (true)
            {
                isFindItem = m_BundleFilesEnumerator.MoveNext();
                if (isFindItem == false)
                    break;

                DirectoryInfo rootFoder = m_BundleFilesEnumerator.Current;
                DirectoryInfo[] childDirectories = rootFoder.GetDirectories();
                foreach (DirectoryInfo chidDirectory in childDirectories)
                {
                    string cacheGuid = chidDirectory.Name;
                    if (CacheSystem.IsCached(m_PackageName, cacheGuid))
                        continue;

                    // 创建验证元素类
                    string fileRootPath = chidDirectory.FullName;
                    string dataFilePath = $"{fileRootPath}/{UniverseConstant.CACHE_BUNDLE_DATA_FILE_NAME}";
                    string infoFilePath = $"{fileRootPath}/{UniverseConstant.CACHE_BUNDLE_INFO_FILE_NAME}";
                    VerifyCacheElement element = new(m_PackageName, cacheGuid, fileRootPath, dataFilePath, infoFilePath);
                    VerifyElements.Add(element);
                }

                if (Engine.IsOperationBusy)
                    break;
            }

            return isFindItem;
        }

        bool UpdateFindRawFiles()
        {
            if (m_RawFilesEnumerator == null)
                return false;

            bool isFindItem;
            while (true)
            {
                isFindItem = m_RawFilesEnumerator.MoveNext();
                if (isFindItem == false)
                    break;

                DirectoryInfo rootFoder = m_RawFilesEnumerator.Current;
                DirectoryInfo[] childDirectories = rootFoder.GetDirectories();
                foreach (DirectoryInfo chidDirectory in childDirectories)
                {
                    string cacheGuid = chidDirectory.Name;
                    if (CacheSystem.IsCached(m_PackageName, cacheGuid))
                        continue;

                    // 获取数据文件的后缀名
                    string dataFileExtension = string.Empty;
                    FileInfo[] fileInfos = chidDirectory.GetFiles();
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        if (fileInfo.Extension == ".temp")
                            continue;
                        if (fileInfo.Name.StartsWith(UniverseConstant.CACHE_BUNDLE_DATA_FILE_NAME))
                        {
                            dataFileExtension = fileInfo.Extension;
                            break;
                        }
                    }

                    // 创建验证元素类
                    string fileRootPath = chidDirectory.FullName;
                    string dataFilePath = $"{fileRootPath}/{UniverseConstant.CACHE_BUNDLE_DATA_FILE_NAME}{dataFileExtension}";
                    string infoFilePath = $"{fileRootPath}/{UniverseConstant.CACHE_BUNDLE_INFO_FILE_NAME}";
                    VerifyCacheElement element = new(m_PackageName, cacheGuid, fileRootPath, dataFilePath, infoFilePath);
                    VerifyElements.Add(element);
                }

                if (Engine.IsOperationBusy)
                    break;
            }

            return isFindItem;
        }
    }
}