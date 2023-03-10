using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Universe
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public static partial class UniverseEditor
    {
        private static MethodInfo s_ClearConsoleMethod;
        private static MethodInfo ClearConsoleMethod
        {
            get
            {
                if (s_ClearConsoleMethod == null)
                {
                    Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
                    Type logEntries = assembly.GetType("UnityEditor.LogEntries");
                    s_ClearConsoleMethod = logEntries.GetMethod("Clear");
                }
                return s_ClearConsoleMethod;
            }
        }

        /// <summary>
        /// 清空控制台
        /// </summary>
        public static void ClearUnityConsole()
        {
            ClearConsoleMethod.Invoke(new(), null);
        }

        /// <summary>
        /// 打开搜索面板
        /// </summary>
        /// <param name="title">标题名称</param>
        /// <param name="defaultPath">默认搜索路径</param>
        /// <returns>返回选择的文件夹绝对路径，如果无效返回NULL</returns>
        public static string OpenFolderPanel(string title, string defaultPath, string defaultName = "")
        {
            string openPath = EditorUtility.OpenFolderPanel(title, defaultPath, defaultName);
            if (string.IsNullOrEmpty(openPath))
                return null;

            if (openPath.Contains("/Assets") == false)
            {
                Debug.LogWarning("Please select unity assets folder.");
                return null;
            }
            return openPath;
        }

        /// <summary>
        /// 打开搜索面板
        /// </summary>
        /// <param name="title">标题名称</param>
        /// <param name="defaultPath">默认搜索路径</param>
        /// <param name="extension"></param>
        /// <returns>返回选择的文件绝对路径，如果无效返回NULL</returns>
        public static string OpenFilePath(string title, string defaultPath, string extension = "")
        {
            string openPath = EditorUtility.OpenFilePanel(title, defaultPath, extension);
            if (string.IsNullOrEmpty(openPath))
                return null;

            if (openPath.Contains("/Assets") == false)
            {
                Debug.LogWarning("Please select unity assets file.");
                return null;
            }
            return openPath;
        }

        /// <summary>
        /// 显示进度框
        /// </summary>
        public static void DisplayProgressBar(string tips, int progressValue, int totalValue)
        {
            EditorUtility.DisplayProgressBar("进度", $"{tips} : {progressValue}/{totalValue}", (float)progressValue / totalValue);
        }

        /// <summary>
        /// 隐藏进度框
        /// </summary>
        public static void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}