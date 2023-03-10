using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Universe
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public static partial class UniverseEditor
    {
        /// <summary>
        /// 清理所有材质球的冗余属性
        /// </summary>
        public static void ClearMaterialUnusedProperty(List<string> searchDirectorys)
        {
            if (searchDirectorys.Count == 0)
            {
                throw new("路径列表不能为空！");
            }

            // 获取所有资源列表
            int checkCount = 0;
            int removedCount = 0;
            string[] findAssets = FindAssets(EAssetSearchType.Material, searchDirectorys.ToArray());
            foreach (string assetPath in findAssets)
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if (ClearMaterialUnusedProperty(mat))
                {
                    removedCount++;
                    EditorLog.Warning($"材质球已被处理：{assetPath}");
                }

                DisplayProgressBar("清理冗余的材质球", ++checkCount, findAssets.Length);
            }
            ClearProgressBar();

            if (removedCount == 0)
            {
                EditorLog.Info("没有发现冗余的材质球");
            }
            else
            {
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// 清理无用的材质球属性
        /// </summary>
        public static bool ClearMaterialUnusedProperty(Material mat)
        {
            bool removeUnused = false;
            SerializedObject so = new(mat);
            SerializedProperty sp = so.FindProperty("m_SavedProperties");

            sp.Next(true);
            do
            {
                if (sp.isArray == false)
                    continue;

                for (int i = sp.arraySize - 1; i >= 0; --i)
                {
                    SerializedProperty serializedProperty = sp.GetArrayElementAtIndex(i);
                    if (serializedProperty.isArray)
                    {
                        for (int ii = serializedProperty.arraySize - 1; ii >= 0; --ii)
                        {
                            SerializedProperty p2 = serializedProperty.GetArrayElementAtIndex(ii);
                            SerializedProperty val = p2.FindPropertyRelative("first");
                            if (mat.HasProperty(val.stringValue) == false)
                            {
                                EditorLog.Info($"Material {mat.name} remove unused property : {val.stringValue}");
                                serializedProperty.DeleteArrayElementAtIndex(ii);
                                removeUnused = true;
                            }
                        }
                    }
                    else
                    {
                        SerializedProperty val = serializedProperty.FindPropertyRelative("first");
                        if (mat.HasProperty(val.stringValue) == false)
                        {
                            EditorLog.Info($"Material {mat.name} remove unused property : {val.stringValue}");
                            sp.DeleteArrayElementAtIndex(i);
                            removeUnused = true;
                        }
                    }
                }
            } while (sp.Next(false));

            so.ApplyModifiedProperties();
            return removeUnused;
        }
    }
}