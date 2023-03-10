using UnityEngine;
using UnityEditor;

namespace Universe
{
	public class ShaderVariantCollectorSettingData
	{
		private static ShaderVariantCollectorSetting s_Setting = null;
		public static ShaderVariantCollectorSetting Setting
		{
			get
			{
				if (s_Setting == null)
					LoadSettingData();
				return s_Setting;
			}
		}

		/// <summary>
		/// 加载配置文件
		/// </summary>
		private static void LoadSettingData()
		{
			s_Setting = EditorHelper.LoadSettingData<ShaderVariantCollectorSetting>();
		}

		/// <summary>
		/// 存储文件
		/// </summary>
		public static void SaveFile()
		{
			if (Setting != null)
			{
				EditorUtility.SetDirty(Setting);
				AssetDatabase.SaveAssets();
				Debug.Log($"{nameof(ShaderVariantCollectorSetting)}.asset is saved!");
			}
		}
	}
}