using System;
using UnityEngine;
using UnityEditor;

namespace Universe
{
    public static class EditorHelper
    {
       

        /// <summary>
        /// 加载相关的配置文件
        /// </summary>
        public static TSetting LoadSettingData<TSetting>() where TSetting : ScriptableObject
        {
            Type settingType = typeof(TSetting);
            string[] guids = AssetDatabase.FindAssets($"t:{settingType.Name}");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"Create new {settingType.Name}.asset");
                TSetting setting = ScriptableObject.CreateInstance<TSetting>();
                string filePath = $"Assets/{settingType.Name}.asset";
                AssetDatabase.CreateAsset(setting, filePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return setting;
            }
            else
            {
                if (guids.Length != 1)
                {
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        Debug.LogWarning($"Found multiple file : {path}");
                    }
                    throw new($"Found multiple {settingType.Name} files !");
                }

                string filePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                TSetting setting = AssetDatabase.LoadAssetAtPath<TSetting>(filePath);
                return setting;
            }
        }
    }
}