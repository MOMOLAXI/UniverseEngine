#if UNITY_2019_4_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Universe
{
    public class ShaderVariantCollectorWindow : EditorWindow
    {
        [MenuItem("UniverseStudio/ShaderVariant Collector", false, 201)]
        public static void ShowExample()
        {
            ShaderVariantCollectorWindow window = GetWindow<ShaderVariantCollectorWindow>("着色器变种收集工具", true, EditorDefine.DockedWindowTypes);
            window.minSize = new(800, 600);
        }

        private List<string> m_PackageNames;

        private Button m_CollectButton;
        private TextField m_CollectOutputField;
        private Label m_CurrentShaderCountField;
        private Label m_CurrentVariantCountField;
        private SliderInt m_ProcessCapacitySlider;
        private PopupField<string> m_PackageField;

        public void CreateGUI()
        {
            try
            {
                VisualElement root = rootVisualElement;

                // 加载布局文件
                VisualTreeAsset visualAsset = UniverseEditor.LoadWindowUxml<ShaderVariantCollectorWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 包裹名称列表
                m_PackageNames = GetBuildPackageNames();

                // 文件输出目录
                m_CollectOutputField = root.Q<TextField>("CollectOutput");
                m_CollectOutputField.SetValueWithoutNotify(ShaderVariantCollectorSettingData.Setting.SavePath);
                m_CollectOutputField.RegisterValueChangedCallback(_ =>
                {
                    ShaderVariantCollectorSettingData.Setting.SavePath = m_CollectOutputField.value;
                });

                // 收集的包裹
                VisualElement packageContainer = root.Q("PackageContainer");
                if (m_PackageNames.Count > 0)
                {
                    int defaultIndex = GetDefaultPackageIndex(ShaderVariantCollectorSettingData.Setting.CollectPackage);
                    m_PackageField = new(m_PackageNames, defaultIndex)
                    {
                        label = "Package",
                        style =
                        {
                            width = 350
                        }
                    };
                    m_PackageField.RegisterValueChangedCallback(evt =>
                    {
                        ShaderVariantCollectorSettingData.Setting.CollectPackage = m_PackageField.value;
                    });
                    packageContainer.Add(m_PackageField);
                }
                else
                {
                    m_PackageField = new()
                    {
                        label = "Package",
                        style =
                        {
                            width = 350
                        }
                    };
                    packageContainer.Add(m_PackageField);
                }

                // 容器值
                m_ProcessCapacitySlider = root.Q<SliderInt>("ProcessCapacity");
                m_ProcessCapacitySlider.SetValueWithoutNotify(ShaderVariantCollectorSettingData.Setting.ProcessCapacity);
            #if !UNITY_2020_3_OR_NEWER
				_processCapacitySlider.label = $"Capacity ({_processCapacitySlider.value})";
				_processCapacitySlider.RegisterValueChangedCallback(evt =>
				{
					ShaderVariantCollectorSettingData.Setting.ProcessCapacity = _processCapacitySlider.value;
					_processCapacitySlider.label = $"Capacity ({_processCapacitySlider.value})";
				});
            #else
                m_ProcessCapacitySlider.RegisterValueChangedCallback(evt =>
                {
                    ShaderVariantCollectorSettingData.Setting.ProcessCapacity = m_ProcessCapacitySlider.value;
                });
            #endif

                m_CurrentShaderCountField = root.Q<Label>("CurrentShaderCount");
                m_CurrentVariantCountField = root.Q<Label>("CurrentVariantCount");

                // 变种收集按钮
                m_CollectButton = root.Q<Button>("CollectButton");
                m_CollectButton.clicked += CollectButton_clicked;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
        private void Update()
        {
            if (m_CurrentShaderCountField != null)
            {
                int currentShaderCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionShaderCount();
                m_CurrentShaderCountField.text = $"Current Shader Count : {currentShaderCount}";
            }

            if (m_CurrentVariantCountField != null)
            {
                int currentVariantCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionVariantCount();
                m_CurrentVariantCountField.text = $"Current Variant Count : {currentVariantCount}";
            }
        }

        private void CollectButton_clicked()
        {
            string savePath = ShaderVariantCollectorSettingData.Setting.SavePath;
            string packageName = ShaderVariantCollectorSettingData.Setting.CollectPackage;
            int processCapacity = m_ProcessCapacitySlider.value;
            ShaderVariantCollector.Run(savePath, packageName, processCapacity, null);
        }

        // 构建包裹相关
        private int GetDefaultPackageIndex(string packageName)
        {
            for (int index = 0; index < m_PackageNames.Count; index++)
            {
                if (m_PackageNames[index] == packageName)
                {
                    return index;
                }
            }

            ShaderVariantCollectorSettingData.Setting.CollectPackage = m_PackageNames[0];
            return 0;
        }
        private List<string> GetBuildPackageNames()
        {
            List<string> result = new();
            foreach (AssetBundleCollectorPackage package in AssetBundleCollectorSettingData.Setting.Packages)
            {
                result.Add(package.PackageName);
            }
            return result;
        }
    }
}
#endif