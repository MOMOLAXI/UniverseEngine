using UnityEngine;
using UnityEditor;

namespace Universe
{
	public static class ShaderVariantCollectionHelper
	{
		public static void ClearCurrentShaderVariantCollection()
		{
			UniverseEditor.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "ClearCurrentShaderVariantCollection");
		}
		public static void SaveCurrentShaderVariantCollection(string savePath)
		{
			UniverseEditor.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "SaveCurrentShaderVariantCollection", savePath);
		}
		public static int GetCurrentShaderVariantCollectionShaderCount()
		{
			return (int)UniverseEditor.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionShaderCount");
		}
		public static int GetCurrentShaderVariantCollectionVariantCount()
		{
			return (int)UniverseEditor.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionVariantCount");
		}

		/// <summary>
		/// 获取着色器的变种总数量
		/// </summary>
		public static string GetShaderVariantCount(string assetPath)
		{
			Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);
			object variantCount = UniverseEditor.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetVariantCount", shader, true);
			return variantCount.ToString();
		}
	}
}