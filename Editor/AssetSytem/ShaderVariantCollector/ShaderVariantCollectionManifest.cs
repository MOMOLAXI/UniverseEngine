using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace Universe
{
	[Serializable]
	public class ShaderVariantCollectionManifest
	{
		[Serializable]
		public class ShaderVariantElement
		{
			/// <summary>
			///  Pass type to use in this variant.
			/// </summary>
			public PassType PassType;

			/// <summary>
			/// Array of shader keywords to use in this variant.
			/// </summary>
			public string[] Keywords;
		}

		[Serializable]
		public class ShaderVariantInfo
		{
			/// <summary>
			/// Shader asset path in editor.
			/// </summary>
			public string AssetPath;

			/// <summary>
			/// Shader name.
			/// </summary>
			public string ShaderName;

			/// <summary>
			/// Shader variants elements list.
			/// </summary>
			public List<ShaderVariantElement> ShaderVariantElements = new(1000);
		}


		/// <summary>
		/// Number of shaders in this collection
		/// </summary>
		public int ShaderTotalCount;

		/// <summary>
		/// Number of total varians in this collection
		/// </summary>
		public int VariantTotalCount;

		/// <summary>
		/// Shader variants info list.
		/// </summary>
		public List<ShaderVariantInfo> ShaderVariantInfos = new(1000);

		/// <summary>
		/// 添加着色器变种信息
		/// </summary>
		public void AddShaderVariant(string assetPath, string shaderName, PassType passType, string[] keywords)
		{
			ShaderVariantInfo info = GetOrCreateShaderVariantInfo(assetPath, shaderName);
			ShaderVariantElement element = new()
			{
				PassType = passType,
				Keywords = keywords
			};
			info.ShaderVariantElements.Add(element);
		}
		private ShaderVariantInfo GetOrCreateShaderVariantInfo(string assetPath, string shaderName)
		{
			List<ShaderVariantInfo> selectList = ShaderVariantInfos.Where(t => t.ShaderName == shaderName && t.AssetPath == assetPath).ToList();
			if (selectList.Count == 0)
			{
				ShaderVariantInfo newInfo = new()
				{
					AssetPath = assetPath,
					ShaderName = shaderName
				};
				ShaderVariantInfos.Add(newInfo);
				return newInfo;
			}

			if (selectList.Count != 1)
				throw new("Should never get here !");

			return selectList[0];
		}


		/// <summary>
		/// 解析SVC文件并将数据写入到清单
		/// </summary>
		public static ShaderVariantCollectionManifest Extract(ShaderVariantCollection svc)
		{
			ShaderVariantCollectionManifest manifest = new()
			{
				ShaderTotalCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionShaderCount(),
				VariantTotalCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionVariantCount()
			};

			using (SerializedObject so = new(svc))
			{
				SerializedProperty shaderArray = so.FindProperty("m_Shaders.Array");
				if (shaderArray != null && shaderArray.isArray)
				{
					for (int i = 0; i < shaderArray.arraySize; ++i)
					{
						SerializedProperty shaderRef = shaderArray.FindPropertyRelative($"data[{i}].first");
						SerializedProperty shaderVariantsArray = shaderArray.FindPropertyRelative($"data[{i}].second.variants");
						if (shaderRef != null && shaderRef.propertyType == SerializedPropertyType.ObjectReference && shaderVariantsArray != null && shaderVariantsArray.isArray)
						{
							Shader shader = shaderRef.objectReferenceValue as Shader;
							if (shader == null)
							{
								throw new("Invalid shader in ShaderVariantCollection file.");
							}

							string shaderAssetPath = AssetDatabase.GetAssetPath(shader);
							string shaderName = shader.name;

							// 添加变种信息
							for (int j = 0; j < shaderVariantsArray.arraySize; ++j)
							{
								SerializedProperty propKeywords = shaderVariantsArray.FindPropertyRelative($"Array.data[{j}].keywords");
								SerializedProperty propPassType = shaderVariantsArray.FindPropertyRelative($"Array.data[{j}].passType");
								if (propKeywords != null && propPassType != null && propKeywords.propertyType == SerializedPropertyType.String)
								{
									string[] keywords = propKeywords.stringValue.Split(' ');
									PassType pathType = (PassType)propPassType.intValue;
									manifest.AddShaderVariant(shaderAssetPath, shaderName, pathType, keywords);
								}
							}
						}
					}
				}
			}

			return manifest;
		}
	}
}