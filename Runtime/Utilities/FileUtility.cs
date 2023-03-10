using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Universe
{
    /// <summary>
    /// 文件工具类
    /// </summary>
    public static class FileUtility
    {
        /// <summary>
        /// 获取规范的路径
        /// </summary>
        public static string GetRegularPath(string path)
        {
            return path.Replace('\\', '/').Replace("\\", "/"); //替换为Linux路径格式
        }

        /// <summary>
        /// 获取项目工程路径
        /// </summary>
        public static string GetProjectPath()
        {
            string projectPath = Path.GetDirectoryName(Application.dataPath);
            return GetRegularPath(projectPath);
        }

        /// <summary>
        /// 读取文件的文本数据
        /// </summary>
        public static string ReadAllText(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }

            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        /// <summary>
        /// 读取文件的字节数据
        /// </summary>
        public static byte[] ReadAllBytes(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            return File.ReadAllBytes(filePath);
        }

        /// <summary>
        /// 创建文件（如果已经存在则删除旧文件）
        /// </summary>
        public static void CreateFile(string filePath, string content)
        {
            // 删除旧文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // 创建文件夹路径
            CreateFileDirectory(filePath);

            // 创建新文件
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            using (FileStream fs = File.Create(filePath))
            {
                fs.Write(bytes, 0, bytes.Length);
                fs.Flush();
                fs.Close();
            }
        }

        /// <summary>
        /// 创建文件（如果已经存在则删除旧文件）
        /// </summary>
        public static void CreateFile(string filePath, byte[] data)
        {
            // 删除旧文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // 创建文件夹路径
            CreateFileDirectory(filePath);

            // 创建新文件
            using (FileStream fs = File.Create(filePath))
            {
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Close();
            }
        }

        /// <summary>
        /// 创建文件的文件夹路径
        /// </summary>
        public static void CreateFileDirectory(string filePath)
        {
            // 获取文件的文件夹路径
            string directory = Path.GetDirectoryName(filePath);
            CreateDirectory(directory);
        }

        /// <summary>
        /// 创建文件夹路径
        /// </summary>
        public static bool CreateDirectory(string directory)
        {
            // If the directory doesn't exist, create it.
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 删除文件夹及子目录
        /// </summary>
        public static bool DeleteDirectory(string directory, bool recrusive = true)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recrusive);
                return true;
            }

            return false;
        }

        public static bool DeleteFileOwningDirectory(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                return false;
            }

            if (fileInfo.Exists)
            {
                fileInfo.Directory.Delete(true);
            }

            return true;
        }

        /// <summary>
        /// 追溯文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <param name="rootPath"></param>
        /// <param name="result"></param>
        public static void TrackDirectories(in string path, string rootPath, Stack<string> result)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(rootPath) || result == null)
            {
                return;
            }

            result.Clear();
            string targetPath = path;
            while (true)
            {
                if (targetPath == rootPath)
                {
                    return;
                }

                string folder = Path.GetDirectoryName(targetPath);
                if (string.IsNullOrEmpty(folder))
                {
                    return;
                }
                
                if (folder != rootPath)
                {
                    result.Push(folder);
                    targetPath = folder;
                    continue;
                }

                break;
            }
        }

        public static bool DeleteFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return true;
        }

        /// <summary>
        /// 文件夹路径是否相同
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        public static bool IsSameDirectory(string d1, string d2)
        {
            string dir1 = Path.GetDirectoryName(d1);
            string dir2 = Path.GetDirectoryName(d2);
            if (string.IsNullOrEmpty(dir1) || string.IsNullOrEmpty(dir2))
            {
                return false;
            }

            return dir1 == dir2;
        }

        /// <summary>
        /// 上n级目录
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string GetParentDir(string dir, int n = 1)
        {
            string subDir = dir;

            for (int i = 0; i < n; ++i)
            {
                int last = subDir.LastIndexOf('/');
                subDir = subDir[..last];
            }

            return subDir;
        }

        /// <summary>
        /// 文件重命名
        /// </summary>
        public static void FileRename(string filePath, string newName)
        {
            string dirPath = Path.GetDirectoryName(filePath);
            string destPath;
            if (Path.HasExtension(filePath))
            {
                string extentsion = Path.GetExtension(filePath);
                destPath = $"{dirPath}/{newName}{extentsion}";
            }
            else
            {
                destPath = $"{dirPath}/{newName}";
            }

            FileInfo fileInfo = new(filePath);
            fileInfo.MoveTo(destPath);
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        public static void MoveFile(string filePath, string destPath)
        {
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }

            FileInfo fileInfo = new(filePath);
            fileInfo.MoveTo(destPath);
        }

        /// <summary>
        /// 拷贝文件夹
        /// 注意：包括所有子目录的文件
        /// </summary>
        public static void CopyDirectory(string sourcePath, string destPath)
        {
            sourcePath = GetRegularPath(sourcePath);

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            string[] fileList = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
            foreach (string file in fileList)
            {
                string temp = GetRegularPath(file);
                string savePath = temp.Replace(sourcePath, destPath);
                CopyFile(file, savePath, true);
            }
        }

        /// <summary>
        /// 拷贝文件
        /// </summary>
        public static void CopyFile(string sourcePath, string destPath, bool overwrite)
        {
            if (!File.Exists(sourcePath))
            {
                Log.Error($"File not found at path : {sourcePath}");
                return;
            }

            // 创建目录
            FileUtility.CreateFileDirectory(destPath);

            // 复制文件
            File.Copy(sourcePath, destPath, overwrite);
        }

        /// <summary>
        /// 清空文件夹
        /// </summary>
        /// <param name="directoryPath">要清理的文件夹路径</param>
        public static void ClearFolder(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            // 删除文件
            string[] allFiles = Directory.GetFiles(directoryPath);
            for (int i = 0; i < allFiles.Length; i++)
            {
                File.Delete(allFiles[i]);
            }

            // 删除文件夹
            string[] allFolders = Directory.GetDirectories(directoryPath);
            for (int i = 0; i < allFolders.Length; i++)
            {
                Directory.Delete(allFolders[i], true);
            }
        }

        /// <summary>
        /// 读取文件的所有文本内容
        /// </summary>
        public static string ReadFileAllText(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }

            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        /// <summary>
        /// 读取文本的所有文本内容
        /// </summary>
        public static string[] ReadFileAllLine(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            return File.ReadAllLines(filePath, Encoding.UTF8);
        }

        /// <summary>
        /// 递归查找目标文件夹路径
        /// </summary>
        /// <param name="root">搜索的根目录</param>
        /// <param name="folderName">目标文件夹名称</param>
        /// <returns>返回找到的文件夹路径，如果没有找到返回空字符串</returns>
        public static string FindFolder(string root, string folderName)
        {
            DirectoryInfo rootInfo = new(root);
            DirectoryInfo[] infoList = rootInfo.GetDirectories();
            for (int i = 0; i < infoList.Length; i++)
            {
                string fullPath = infoList[i].FullName;
                if (infoList[i].Name == folderName)
                {
                    return fullPath;
                }

                string result = FindFolder(fullPath, folderName);
                if (string.IsNullOrEmpty(result) == false)
                {
                    return result;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取文件大小（字节数）
        /// </summary>
        public static long GetFileSize(string filePath)
        {
            FileInfo fileInfo = new(filePath);
            return fileInfo.Length;
        }
    }
}