using UnityEditor;
using UnityEngine;

namespace Universe
{
	public class AssetBundleInspector
	{
		[CustomEditor(typeof(AssetBundle))]
		internal class AssetBundleEditor : Editor
		{
			internal bool PathFoldout = false;
			internal bool AdvancedFoldout = false;
			public override void OnInspectorGUI()
			{
				AssetBundle bundle = target as AssetBundle;

				using (new EditorGUI.DisabledScope(true))
				{
					GUIStyle leftStyle = new(GUI.skin.GetStyle("Label"))
					{
						alignment = TextAnchor.UpperLeft
					};
					GUILayout.Label(new GUIContent("Name: " + bundle.name), leftStyle);

					string[] assetNames = bundle.GetAllAssetNames();
					PathFoldout = EditorGUILayout.Foldout(PathFoldout, "Source Asset Paths");
					if (PathFoldout)
					{
						EditorGUI.indentLevel++;
						foreach (string asset in assetNames)
							EditorGUILayout.LabelField(asset);
						EditorGUI.indentLevel--;
					}

					AdvancedFoldout = EditorGUILayout.Foldout(AdvancedFoldout, "Advanced Data");
				}

				if (AdvancedFoldout)
				{
					EditorGUI.indentLevel++;
					base.OnInspectorGUI();
					EditorGUI.indentLevel--;
				}
			}
		}
	}
}