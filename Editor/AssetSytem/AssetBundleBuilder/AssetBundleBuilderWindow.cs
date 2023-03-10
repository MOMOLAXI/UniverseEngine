#if UNITY_2019_4_OR_NEWER
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniverseEngineEditor;

namespace Universe
{
    public class AssetBundleBuilderWindow : EditorWindow
    {
        [MenuItem("UniverseStudio/AssetBundle Builder", false, 102)]
        public static void ShowExample()
        {
            AssetBundleBuilderWindow window = GetWindow<AssetBundleBuilderWindow>("资源包构建工具", true, EditorDefine.DockedWindowTypes);
            window.minSize = new(800, 600);
        }

        private BuildTarget m_BuildTarget;
        private List<Type> m_EncryptionServicesClassTypes;
        private List<string> m_EncryptionServicesClassNames;
        private List<string> m_BuildPackageNames;

        private Button m_SaveButton;
        private TextField m_BuildOutputField;
        private EnumField m_BuildPipelineField;
        private EnumField m_BuildModeField;
        private TextField m_BuildVersionField;
        private TextField m_BuildMajorVersionField;
        private TextField m_BuildMinorVersionField;
        private TextField m_BuildPatchVersionField;
        private PopupField<string> m_BuildPackageField;
        private PopupField<string> m_EncryptionField;
        private EnumField m_CompressionField;
        private EnumField m_OutputNameStyleField;
        private EnumField m_CopyBuildinFileOptionField;
        private TextField m_CopyBuildinFileTagsField;

        public void CreateGUI()
        {
            try
            {
                VisualElement root = rootVisualElement;

                // 加载布局文件
                VisualTreeAsset visualAsset = UniverseEditor.LoadWindowUxml<AssetBundleBuilderWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 配置保存按钮
                m_SaveButton = root.Q<Button>("SaveButton");
                m_SaveButton.clicked += SaveBtn_clicked;

                // 构建平台
                m_BuildTarget = EditorUserBuildSettings.activeBuildTarget;

                // 包裹名称列表
                m_BuildPackageNames = GetBuildPackageNames();

                // 加密服务类
                m_EncryptionServicesClassTypes = UniverseEditor.GetAssignableTypes<IEncryptionServices>();
                m_EncryptionServicesClassNames = m_EncryptionServicesClassTypes.Select(t => t.Name).ToList();

                // 输出目录
                string defaultOutputRoot = AssetSystemEditor.GetDefaultOutputRoot();
                m_BuildOutputField = root.Q<TextField>("BuildOutput");
                m_BuildOutputField.SetValueWithoutNotify(defaultOutputRoot);
                m_BuildOutputField.SetEnabled(false);

                // 构建管线
                m_BuildPipelineField = root.Q<EnumField>("BuildPipeline");
                m_BuildPipelineField.Init(AssetBundleBuilderSettingData.Setting.BuildPipeline);
                m_BuildPipelineField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.BuildPipeline);
                m_BuildPipelineField.style.width = 350;
                m_BuildPipelineField.RegisterValueChangedCallback(_ =>
                {
                    AssetBundleBuilderSettingData.IsDirty = true;
                    AssetBundleBuilderSettingData.Setting.BuildPipeline = (EBuildPipeline)m_BuildPipelineField.value;
                    RefreshWindow();
                });

                // 构建模式
                m_BuildModeField = root.Q<EnumField>("BuildMode");
                m_BuildModeField.Init(AssetBundleBuilderSettingData.Setting.BuildMode);
                m_BuildModeField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.BuildMode);
                m_BuildModeField.style.width = 350;
                m_BuildModeField.RegisterValueChangedCallback(_ =>
                {
                    AssetBundleBuilderSettingData.IsDirty = true;
                    AssetBundleBuilderSettingData.Setting.BuildMode = (EBuildMode)m_BuildModeField.value;
                    RefreshWindow();
                });

                // 构建版本
                m_BuildVersionField = root.Q<TextField>("BuildVersion");
                m_BuildVersionField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.BuildVersion);
                m_BuildVersionField.SetEnabled(false);
                m_BuildVersionField.RegisterValueChangedCallback(_ =>
                {
                    AssetBundleBuilderSettingData.IsDirty = true;
                    AssetBundleBuilderSettingData.Setting.BuildVersion = m_BuildVersionField.value;
                    RefreshWindow();
                });
                
                m_BuildMajorVersionField = root.Q<TextField>("MajorVersion");
                m_BuildMajorVersionField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.MajorVersion);
                
                m_BuildMinorVersionField = root.Q<TextField>("MinorVersion");
                m_BuildMinorVersionField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.MinorVersion);
                
                m_BuildPatchVersionField = root.Q<TextField>("PatchVersion");
                m_BuildPatchVersionField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.PatchVersion);
                
                m_BuildMajorVersionField.RegisterValueChangedCallback(_ =>
                {
                    string version = $"{m_BuildMajorVersionField.value}.{m_BuildMinorVersionField.value}.{m_BuildPatchVersionField.value}";
                    m_BuildVersionField.value = version;
                    AssetBundleBuilderSettingData.IsDirty = true;
                    AssetBundleBuilderSettingData.Setting.MajorVersion = m_BuildMajorVersionField.value;
                    RefreshWindow();
                });
                
                m_BuildMinorVersionField.RegisterValueChangedCallback(_ =>
                {
                    string version = $"{m_BuildMajorVersionField.value}.{m_BuildMinorVersionField.value}.{m_BuildPatchVersionField.value}";
                    m_BuildVersionField.value = version;
                    AssetBundleBuilderSettingData.IsDirty = true;
                    AssetBundleBuilderSettingData.Setting.MinorVersion = m_BuildMinorVersionField.value;
                    RefreshWindow();
                });
                
                m_BuildPatchVersionField.RegisterValueChangedCallback(_ =>
                {
                    string version = $"{m_BuildMajorVersionField.value}.{m_BuildMinorVersionField.value}.{m_BuildPatchVersionField.value}";
                    m_BuildVersionField.value = version;
                    AssetBundleBuilderSettingData.IsDirty = true;
                    AssetBundleBuilderSettingData.Setting.PatchVersion = m_BuildPatchVersionField.value;
                    RefreshWindow();
                });

                // 构建包裹
                VisualElement buildPackageContainer = root.Q("BuildPackageContainer");
                if (m_BuildPackageNames.Count > 0)
                {
                    int defaultIndex = GetDefaultPackageIndex(AssetBundleBuilderSettingData.Setting.BuildPackage);
                    m_BuildPackageField = new(m_BuildPackageNames, defaultIndex)
                    {
                        label = "Build Package",
                        style =
                        {
                            width = 350
                        }
                    };
                    m_BuildPackageField.RegisterValueChangedCallback(_ =>
                    {
                        AssetBundleBuilderSettingData.IsDirty = true;
                        AssetBundleBuilderSettingData.Setting.BuildPackage = m_BuildPackageField.value;
                    });
                    buildPackageContainer.Add(m_BuildPackageField);
                }
                else
                {
                    m_BuildPackageField = new()
                    {
                        label = "Build Package",
                        style =
                        {
                            width = 350
                        }
                    };
                    buildPackageContainer.Add(m_BuildPackageField);
                }

                // 加密方法
                VisualElement encryptionContainer = root.Q("EncryptionContainer");
                if (m_EncryptionServicesClassNames.Count > 0)
                {
                    int defaultIndex = GetDefaultEncryptionIndex(AssetBundleBuilderSettingData.Setting.EncyptionClassName);
                    m_EncryptionField = new(m_EncryptionServicesClassNames, defaultIndex)
                    {
                        label = "Encryption",
                        style =
                        {
                            width = 350
                        }
                    };
                    m_EncryptionField.RegisterValueChangedCallback(_ =>
                    {
                        AssetBundleBuilderSettingData.IsDirty = true;
                        AssetBundleBuilderSettingData.Setting.EncyptionClassName = m_EncryptionField.value;
                    });
                    encryptionContainer.Add(m_EncryptionField);
                }
                else
                {
                    m_EncryptionField = new()
                    {
                        label = "Encryption",
                        style =
                        {
                            width = 350
                        }
                    };
                    encryptionContainer.Add(m_EncryptionField);
                }

                // 压缩方式选项
                m_CompressionField = root.Q<EnumField>("Compression");
                m_CompressionField.Init(AssetBundleBuilderSettingData.Setting.CompressOption);
                m_CompressionField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.CompressOption);
                m_CompressionField.style.width = 350;
                m_CompressionField.RegisterValueChangedCallback(_ =>
                {
                    AssetBundleBuilderSettingData.IsDirty = true;
                    AssetBundleBuilderSettingData.Setting.CompressOption = (ECompressOption)m_CompressionField.value;
                });

                // 输出文件名称样式
                m_OutputNameStyleField = root.Q<EnumField>("OutputNameStyle");
                m_OutputNameStyleField.Init(AssetBundleBuilderSettingData.Setting.OutputNameStyle);
                m_OutputNameStyleField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.OutputNameStyle);
                m_OutputNameStyleField.style.width = 350;
                m_OutputNameStyleField.RegisterValueChangedCallback(_ =>
                {
                    AssetBundleBuilderSettingData.IsDirty = true;
                    AssetBundleBuilderSettingData.Setting.OutputNameStyle = (EOutputNameStyle)m_OutputNameStyleField.value;
                });

                // 首包文件拷贝选项
                m_CopyBuildinFileOptionField = root.Q<EnumField>("CopyBuildinFileOption");
                m_CopyBuildinFileOptionField.Init(AssetBundleBuilderSettingData.Setting.CopyBuildinFileOption);
                m_CopyBuildinFileOptionField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.CopyBuildinFileOption);
                m_CopyBuildinFileOptionField.style.width = 350;
                m_CopyBuildinFileOptionField.RegisterValueChangedCallback(_ =>
                {
                    AssetBundleBuilderSettingData.IsDirty = true;
                    AssetBundleBuilderSettingData.Setting.CopyBuildinFileOption = (ECopyBuildinFileOption)m_CopyBuildinFileOptionField.value;
                    RefreshWindow();
                });

                // 首包文件的资源标签
                m_CopyBuildinFileTagsField = root.Q<TextField>("CopyBuildinFileTags");
                m_CopyBuildinFileTagsField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.CopyBuildinFileTags);
                m_CopyBuildinFileTagsField.RegisterValueChangedCallback(_ =>
                {
                    AssetBundleBuilderSettingData.IsDirty = true;
                    AssetBundleBuilderSettingData.Setting.CopyBuildinFileTags = m_CopyBuildinFileTagsField.value;
                });

                // 构建按钮
                Button buildButton = root.Q<Button>("Build");
                buildButton.clicked += BuildButton_clicked;

                RefreshWindow();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
        public void OnDestroy()
        {
            if (AssetBundleBuilderSettingData.IsDirty)
                AssetBundleBuilderSettingData.SaveFile();
        }
        
        public void Update()
        {
            if (m_SaveButton != null)
            {
                if (AssetBundleBuilderSettingData.IsDirty)
                {
                    if (m_SaveButton.enabledSelf == false)
                        m_SaveButton.SetEnabled(true);
                }
                else
                {
                    if (m_SaveButton.enabledSelf)
                        m_SaveButton.SetEnabled(false);
                }
            }
        }

        private void RefreshWindow()
        {
            EBuildPipeline buildPipeline = AssetBundleBuilderSettingData.Setting.BuildPipeline;
            EBuildMode buildMode = AssetBundleBuilderSettingData.Setting.BuildMode;
            ECopyBuildinFileOption copyOption = AssetBundleBuilderSettingData.Setting.CopyBuildinFileOption;
            bool enableElement = buildMode == EBuildMode.ForceRebuild;
            bool tagsFiledVisible = copyOption == ECopyBuildinFileOption.ClearAndCopyByTags || copyOption == ECopyBuildinFileOption.OnlyCopyByTags;

            if (buildPipeline == EBuildPipeline.BuiltinBuildPipeline)
            {
                m_CompressionField.SetEnabled(enableElement);
                m_OutputNameStyleField.SetEnabled(enableElement);
                m_CopyBuildinFileOptionField.SetEnabled(enableElement);
                m_CopyBuildinFileTagsField.SetEnabled(enableElement);
            }
            else
            {
                m_CompressionField.SetEnabled(true);
                m_OutputNameStyleField.SetEnabled(true);
                m_CopyBuildinFileOptionField.SetEnabled(true);
                m_CopyBuildinFileTagsField.SetEnabled(true);
            }

            m_CopyBuildinFileTagsField.visible = tagsFiledVisible;
        }
        private void SaveBtn_clicked()
        {
            AssetBundleBuilderSettingData.SaveFile();
        }
        private void BuildButton_clicked()
        {
            EBuildMode buildMode = AssetBundleBuilderSettingData.Setting.BuildMode;
            if (EditorUtility.DisplayDialog("提示", $"通过构建模式【{buildMode}】来构建！", "Yes", "No"))
            {
                UniverseEditor.ClearUnityConsole();
                EditorApplication.delayCall += ExecuteBuild;
            }
            else
            {
                Debug.LogWarning("[Build] 打包已经取消");
            }
        }

        /// <summary>
        /// 执行构建
        /// </summary>
        private void ExecuteBuild()
        {
            string defaultOutputRoot = AssetSystemEditor.GetDefaultOutputRoot();
            BuildParameters buildParameters = new()
            {
                OutputRoot = defaultOutputRoot,
                BuildTarget = m_BuildTarget,
                BuildPipeline = AssetBundleBuilderSettingData.Setting.BuildPipeline,
                BuildMode = AssetBundleBuilderSettingData.Setting.BuildMode,
                PackageName = AssetBundleBuilderSettingData.Setting.BuildPackage,
                PackageVersion = m_BuildVersionField.value,
                VerifyBuildingResult = true,
                EncryptionServices = CreateEncryptionServicesInstance(),
                CompressOption = AssetBundleBuilderSettingData.Setting.CompressOption,
                OutputNameStyle = AssetBundleBuilderSettingData.Setting.OutputNameStyle,
                CopyBuildinFileOption = AssetBundleBuilderSettingData.Setting.CopyBuildinFileOption,
                CopyBuildinFileTags = AssetBundleBuilderSettingData.Setting.CopyBuildinFileTags
            };

            if (AssetBundleBuilderSettingData.Setting.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
            {
                buildParameters.SbpParameters = new()
                {
                    WriteLinkXML = true
                };
            }

            AssetBundleBuilder builder = new();
            BuildResult buildResult = builder.Run(buildParameters);
            if (buildResult.Success)
            {
                EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
            }
        }
        

        // 构建包裹相关
        private int GetDefaultPackageIndex(string packageName)
        {
            for (int index = 0; index < m_BuildPackageNames.Count; index++)
            {
                if (m_BuildPackageNames[index] == packageName)
                {
                    return index;
                }
            }

            AssetBundleBuilderSettingData.IsDirty = true;
            AssetBundleBuilderSettingData.Setting.BuildPackage = m_BuildPackageNames[0];
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

        // 加密类相关
        private int GetDefaultEncryptionIndex(string className)
        {
            for (int index = 0; index < m_EncryptionServicesClassNames.Count; index++)
            {
                if (m_EncryptionServicesClassNames[index] == className)
                {
                    return index;
                }
            }

            AssetBundleBuilderSettingData.IsDirty = true;
            AssetBundleBuilderSettingData.Setting.EncyptionClassName = m_EncryptionServicesClassNames[0];
            return 0;
        }

        IEncryptionServices CreateEncryptionServicesInstance()
        {
            if (m_EncryptionField.index < 0)
            {
                return null;
            }

            Type classType = m_EncryptionServicesClassTypes[m_EncryptionField.index];
            return (IEncryptionServices)Activator.CreateInstance(classType);
        }
    }
}
#endif