using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace Universe
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public static partial class UniverseEditor
    {
        static readonly Dictionary<Type, string> s_Uxmls = new();

        /// <summary>
        /// 加载窗口的布局文件
        /// </summary>
        public static VisualTreeAsset LoadWindowUxml<TWindow>() where TWindow : class
        {
            Type windowType = typeof(TWindow);

            // 缓存里查询并加载
            if (s_Uxmls.TryGetValue(windowType, out string uxmlGuid))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(uxmlGuid);
                if (string.IsNullOrEmpty(assetPath))
                {
                    s_Uxmls.Clear();
                    throw new($"Invalid UXML GUID : {uxmlGuid} ! Please close the window and open it again !");
                }
                
                VisualTreeAsset treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
                return treeAsset;
            }

            // 全局搜索并加载
            string[] guids = AssetDatabase.FindAssets(windowType.Name);
            if (guids.Length == 0)
            {
                throw new($"Not found any assets : {windowType.Name}");
            }

            foreach (string assetGuid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                if (assetType == typeof(VisualTreeAsset))
                {
                    s_Uxmls.Add(windowType, assetGuid);
                    VisualTreeAsset treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
                    return treeAsset;
                }
            }
            
            throw new($"Not found UXML file : {windowType.Name}");
        }

        public static void FocusUnitySceneWindow()
        {
            EditorWindow.FocusWindowIfItsOpen<SceneView>();
        }
        public static void CloseUnityGameWindow()
        {
            Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.GameView");
            EditorWindow.GetWindow(T, false, "GameView", true).Close();
        }
        public static void FocusUnityGameWindow()
        {
            Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.GameView");
            EditorWindow.GetWindow(T, false, "GameView", true);
        }
        public static void FocueUnityProjectWindow()
        {
            Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.ProjectBrowser");
            EditorWindow.GetWindow(T, false, "Project", true);
        }
        public static void FocusUnityHierarchyWindow()
        {
            Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.SceneHierarchyWindow");
            EditorWindow.GetWindow(T, false, "Hierarchy", true);
        }
        public static void FocusUnityInspectorWindow()
        {
            Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.InspectorWindow");
            EditorWindow.GetWindow(T, false, "Inspector", true);
        }
        public static void FocusUnityConsoleWindow()
        {
            Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.ConsoleWindow");
            EditorWindow.GetWindow(T, false, "Console", true);
        }
    }
}