using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Universe
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public static partial class UniverseEditor
    {
        /// <summary>
        /// 搜集资源
        /// </summary>
        /// <param name="searchType">搜集的资源类型</param>
        /// <param name="searchInFolders">指定搜索的文件夹列表</param>
        /// <returns>返回搜集到的资源路径列表</returns>
        public static string[] FindAssets(EAssetSearchType searchType, string[] searchInFolders)
        {
            // 注意：AssetDatabase.FindAssets()不支持末尾带分隔符的文件夹路径
            for (int i = 0; i < searchInFolders.Length; i++)
            {
                string folderPath = searchInFolders[i];
                searchInFolders[i] = folderPath.TrimEnd('/');
            }

            // 注意：获取指定目录下的所有资源对象（包括子文件夹）
            string[] guids = AssetDatabase.FindAssets(searchType == EAssetSearchType.All ? string.Empty : $"t:{searchType}", searchInFolders);

            // 注意：AssetDatabase.FindAssets()可能会获取到重复的资源
            List<string> result = new();
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (result.Contains(assetPath) == false)
                {
                    result.Add(assetPath);
                }
            }

            // 返回结果
            return result.ToArray();
        }

        /// <summary>
        /// 搜集资源
        /// </summary>
        /// <param name="searchType">搜集的资源类型</param>
        /// <param name="searchInFolder">指定搜索的文件夹</param>
        /// <returns>返回搜集到的资源路径列表</returns>
        public static string[] FindAssets(EAssetSearchType searchType, string searchInFolder)
        {
            return FindAssets(searchType, new[] { searchInFolder });
        }

        /// <summary>
        /// 转换文件的绝对路径为Unity资源路径
        /// 例如 D:\\YourPorject\\Assets\\Works\\file.txt 替换为 Assets/Works/file.txt
        /// </summary>
        public static string AbsolutePathToAssetPath(string absolutePath)
        {
            string content = FileUtility.GetRegularPath(absolutePath);
            return StringUtility.Substring(content, "Assets/", true);
        }

        /// <summary>
        /// 转换Unity资源路径为文件的绝对路径
        /// 例如：Assets/Works/file.txt 替换为 D:\\YourPorject/Assets/Works/file.txt
        /// </summary>
        public static string AssetPathToAbsolutePath(string assetPath)
        {
            string projectPath = FileUtility.GetProjectPath();
            return $"{projectPath}/{assetPath}";
        }

        /// <summary>
        /// 检测所有损坏的预制体文件
        /// </summary>
        public static void CheckCorruptionPrefab(List<string> searchDirectorys)
        {
            if (searchDirectorys.Count == 0)
            {
                throw new("路径列表不能为空！");
            }

            // 获取所有资源列表
            int checkCount = 0;
            int invalidCount = 0;
            string[] findAssets = FindAssets(EAssetSearchType.Prefab, searchDirectorys.ToArray());
            foreach (string assetPath in findAssets)
            {
                Object prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
                if (prefab == null)
                {
                    invalidCount++;
                    Debug.LogError($"发现损坏预制件：{assetPath}");
                }

                DisplayProgressBar("检测预制件文件是否损坏", ++checkCount, findAssets.Length);
            }

            ClearProgressBar();

            if (invalidCount == 0)
            {
                EditorLog.Info("没有发现损坏预制件");
            }
        }
    }
}