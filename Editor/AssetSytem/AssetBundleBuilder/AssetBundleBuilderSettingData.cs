using UnityEditor;

namespace Universe
{
    public static class AssetBundleBuilderSettingData
    {
        private static AssetBundleBuilderSetting s_Setting;
        public static AssetBundleBuilderSetting Setting
        {
            get
            {
                if (s_Setting == null)
                {
                    LoadSettingData();
                }
                
                return s_Setting;
            }
        }

        /// <summary>
        /// 配置数据是否被修改
        /// </summary>
        public static bool IsDirty { set; get; }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        private static void LoadSettingData()
        {
            s_Setting = EditorHelper.LoadSettingData<AssetBundleBuilderSetting>();
        }

        /// <summary>
        /// 存储文件
        /// </summary>
        public static void SaveFile()
        {
            if (Setting != null)
            {
                IsDirty = false;
                EditorUtility.SetDirty(Setting);
                AssetDatabase.SaveAssets();
                EditorLog.Info($"{nameof(AssetBundleBuilderSetting)}.asset is saved!");
            }
        }
    }
}