#if UNITY_2019_4_OR_NEWER
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Universe
{
    public class AssetBundleCollectorWindow : EditorWindow
    {
        [MenuItem("UniverseStudio/AssetBundle Collector", false, 101)]
        public static void ShowExample()
        {
            AssetBundleCollectorWindow window = GetWindow<AssetBundleCollectorWindow>("资源包收集工具", true, EditorDefine.DockedWindowTypes);
            window.minSize = new(800, 600);
        }

        private Button m_SaveButton;
        private List<string> m_CollectorTypeList;
        private List<RuleDisplayName> m_ActiveRuleList;
        private List<RuleDisplayName> m_AddressRuleList;
        private List<RuleDisplayName> m_PackRuleList;
        private List<RuleDisplayName> m_FilterRuleList;

        private Toggle m_ShowPackageToogle;
        private Toggle m_EnableAddressableToogle;
        private Toggle m_UniqueBundleNameToogle;
        private Toggle m_ShowEditorAliasToggle;

        private VisualElement m_PackageContainer;
        private ListView m_PackageListView;
        private TextField m_PackageNameTxt;
        private TextField m_PackageDescTxt;

        private VisualElement m_GroupContainer;
        private ListView m_GroupListView;
        private TextField m_GroupNameTxt;
        private TextField m_GroupDescTxt;
        private TextField m_GroupAssetTagsTxt;

        private VisualElement m_CollectorContainer;
        private ScrollView m_CollectorScrollView;
        private PopupField<RuleDisplayName> m_ActiveRulePopupField;

        private int m_LastModifyPackageIndex = 0;
        private int m_LastModifyGroupIndex = 0;


        public void CreateGUI()
        {
            Undo.undoRedoPerformed -= RefreshWindow;
            Undo.undoRedoPerformed += RefreshWindow;

            try
            {
                m_CollectorTypeList = new()
                {
                    $"{nameof(ECollectorType.MainAssetCollector)}",
                    $"{nameof(ECollectorType.StaticAssetCollector)}",
                    $"{nameof(ECollectorType.DependAssetCollector)}"
                };
                m_ActiveRuleList = AssetBundleCollectorSettingData.GetActiveRuleNames();
                m_AddressRuleList = AssetBundleCollectorSettingData.GetAddressRuleNames();
                m_PackRuleList = AssetBundleCollectorSettingData.GetPackRuleNames();
                m_FilterRuleList = AssetBundleCollectorSettingData.GetFilterRuleNames();

                VisualElement root = rootVisualElement;

                // 加载布局文件
                VisualTreeAsset visualAsset = UniverseEditor.LoadWindowUxml<AssetBundleCollectorWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 公共设置相关
                m_ShowPackageToogle = root.Q<Toggle>("ShowPackages");
                m_ShowPackageToogle.RegisterValueChangedCallback(evt =>
                {
                    AssetBundleCollectorSettingData.ModifyPackageView(evt.newValue);
                    RefreshWindow();
                });
                m_EnableAddressableToogle = root.Q<Toggle>("EnableAddressable");
                m_EnableAddressableToogle.RegisterValueChangedCallback(evt =>
                {
                    AssetBundleCollectorSettingData.ModifyAddressable(evt.newValue);
                    RefreshWindow();
                });
                m_UniqueBundleNameToogle = root.Q<Toggle>("UniqueBundleName");
                m_UniqueBundleNameToogle.RegisterValueChangedCallback(evt =>
                {
                    AssetBundleCollectorSettingData.ModifyUniqueBundleName(evt.newValue);
                    RefreshWindow();
                });

                m_ShowEditorAliasToggle = root.Q<Toggle>("ShowEditorAlias");
                m_ShowEditorAliasToggle.RegisterValueChangedCallback(evt =>
                {
                    AssetBundleCollectorSettingData.ModifyShowEditorAlias(evt.newValue);
                    RefreshWindow();
                });

                // 配置修复按钮
                Button fixBtn = root.Q<Button>("FixButton");
                fixBtn.clicked += FixBtn_clicked;

                // 导入导出按钮
                Button exportBtn = root.Q<Button>("ExportButton");
                exportBtn.clicked += ExportBtn_clicked;
                Button importBtn = root.Q<Button>("ImportButton");
                importBtn.clicked += ImportBtn_clicked;

                // 配置保存按钮
                m_SaveButton = root.Q<Button>("SaveButton");
                m_SaveButton.clicked += SaveBtn_clicked;

                // 包裹容器
                m_PackageContainer = root.Q("PackageContainer");

                // 包裹列表相关
                m_PackageListView = root.Q<ListView>("PackageListView");
                m_PackageListView.makeItem = MakePackageListViewItem;
                m_PackageListView.bindItem = BindPackageListViewItem;
                m_PackageListView.onSelectionChange += PackageListView_onSelectionChange;

                // 包裹添加删除按钮
                VisualElement packageAddContainer = root.Q("PackageAddContainer");
                {
                    Button addBtn = packageAddContainer.Q<Button>("AddBtn");
                    addBtn.clicked += AddPackageBtn_clicked;
                    Button removeBtn = packageAddContainer.Q<Button>("RemoveBtn");
                    removeBtn.clicked += RemovePackageBtn_clicked;
                }

                // 包裹名称
                m_PackageNameTxt = root.Q<TextField>("PackageName");
                m_PackageNameTxt.RegisterValueChangedCallback(evt =>
                {
                    AssetBundleCollectorPackage selectPackage = m_PackageListView.selectedItem as AssetBundleCollectorPackage;
                    if (selectPackage != null)
                    {
                        selectPackage.PackageName = evt.newValue;
                        AssetBundleCollectorSettingData.ModifyPackage(selectPackage);
                    }
                });

                // 包裹备注
                m_PackageDescTxt = root.Q<TextField>("PackageDesc");
                m_PackageDescTxt.RegisterValueChangedCallback(evt =>
                {
                    AssetBundleCollectorPackage selectPackage = m_PackageListView.selectedItem as AssetBundleCollectorPackage;
                    if (selectPackage != null)
                    {
                        selectPackage.PackageDesc = evt.newValue;
                        AssetBundleCollectorSettingData.ModifyPackage(selectPackage);
                    }
                });

                // 分组列表相关
                m_GroupListView = root.Q<ListView>("GroupListView");
                m_GroupListView.makeItem = MakeGroupListViewItem;
                m_GroupListView.bindItem = BindGroupListViewItem;
            #if UNITY_2020_1_OR_NEWER
                m_GroupListView.onSelectionChange += GroupListView_onSelectionChange;
            #else
				m_GroupListView.onSelectionChanged += GroupListView_onSelectionChange;
            #endif

                // 分组添加删除按钮
                VisualElement groupAddContainer = root.Q("GroupAddContainer");
                {
                    Button addBtn = groupAddContainer.Q<Button>("AddBtn");
                    addBtn.clicked += AddGroupBtn_clicked;
                    Button removeBtn = groupAddContainer.Q<Button>("RemoveBtn");
                    removeBtn.clicked += RemoveGroupBtn_clicked;
                }

                // 分组容器
                m_GroupContainer = root.Q("GroupContainer");

                // 分组名称
                m_GroupNameTxt = root.Q<TextField>("GroupName");
                m_GroupNameTxt.RegisterValueChangedCallback(evt =>
                {
                    AssetBundleCollectorPackage selectPackage = m_PackageListView.selectedItem as AssetBundleCollectorPackage;
                    AssetBundleCollectorGroup selectGroup = m_GroupListView.selectedItem as AssetBundleCollectorGroup;
                    if (selectPackage != null && selectGroup != null)
                    {
                        selectGroup.GroupName = evt.newValue;
                        AssetBundleCollectorSettingData.ModifyGroup(selectPackage, selectGroup);
                        FillGroupViewData();
                    }
                });

                // 分组备注
                m_GroupDescTxt = root.Q<TextField>("GroupDesc");
                m_GroupDescTxt.RegisterValueChangedCallback(evt =>
                {
                    AssetBundleCollectorPackage selectPackage = m_PackageListView.selectedItem as AssetBundleCollectorPackage;
                    AssetBundleCollectorGroup selectGroup = m_GroupListView.selectedItem as AssetBundleCollectorGroup;
                    if (selectPackage != null && selectGroup != null)
                    {
                        selectGroup.GroupDesc = evt.newValue;
                        AssetBundleCollectorSettingData.ModifyGroup(selectPackage, selectGroup);
                        FillGroupViewData();
                    }
                });

                // 分组的资源标签
                m_GroupAssetTagsTxt = root.Q<TextField>("GroupAssetTags");
                m_GroupAssetTagsTxt.RegisterValueChangedCallback(evt =>
                {
                    AssetBundleCollectorPackage selectPackage = m_PackageListView.selectedItem as AssetBundleCollectorPackage;
                    AssetBundleCollectorGroup selectGroup = m_GroupListView.selectedItem as AssetBundleCollectorGroup;
                    if (selectPackage != null && selectGroup != null)
                    {
                        selectGroup.AssetTags = evt.newValue;
                        AssetBundleCollectorSettingData.ModifyGroup(selectPackage, selectGroup);
                    }
                });

                // 收集列表容器
                m_CollectorContainer = root.Q("CollectorContainer");

                // 收集列表相关
                m_CollectorScrollView = root.Q<ScrollView>("CollectorScrollView");
                m_CollectorScrollView.style.height = new Length(100, LengthUnit.Percent);
                m_CollectorScrollView.viewDataKey = "scrollView";

                // 收集器创建按钮
                VisualElement collectorAddContainer = root.Q("CollectorAddContainer");
                {
                    Button addBtn = collectorAddContainer.Q<Button>("AddBtn");
                    addBtn.clicked += AddCollectorBtn_clicked;
                }

                // 分组激活规则
                VisualElement activeRuleContainer = root.Q("ActiveRuleContainer");
                {
                    m_ActiveRulePopupField = new("Active Rule", m_ActiveRuleList, 0)
                    {
                        name = "ActiveRuleMaskField",
                        style =
                        {
                            unityTextAlign = TextAnchor.MiddleLeft
                        },
                        formatListItemCallback = FormatListItemCallback,
                        formatSelectedValueCallback = FormatSelectedValueCallback
                    };
                    m_ActiveRulePopupField.RegisterValueChangedCallback(evt =>
                    {
                        AssetBundleCollectorPackage selectPackage = m_PackageListView.selectedItem as AssetBundleCollectorPackage;
                        AssetBundleCollectorGroup selectGroup = m_GroupListView.selectedItem as AssetBundleCollectorGroup;
                        if (selectPackage != null && selectGroup != null)
                        {
                            selectGroup.ActiveRuleName = evt.newValue.ClassName;
                            AssetBundleCollectorSettingData.ModifyGroup(selectPackage, selectGroup);
                            FillGroupViewData();
                        }
                    });
                    activeRuleContainer.Add(m_ActiveRulePopupField);
                }

                // 刷新窗体
                RefreshWindow();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public void OnDestroy()
        {
            // 注意：清空所有撤销操作
            Undo.ClearAll();

            if (AssetBundleCollectorSettingData.IsDirty)
            {
                AssetBundleCollectorSettingData.SaveFile();
            }
        }
        public void Update()
        {
            if (m_SaveButton != null)
            {
                if (AssetBundleCollectorSettingData.IsDirty)
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

        void RefreshWindow()
        {
            m_ShowPackageToogle.SetValueWithoutNotify(AssetBundleCollectorSettingData.Setting.ShowPackageView);
            m_EnableAddressableToogle.SetValueWithoutNotify(AssetBundleCollectorSettingData.Setting.EnableAddressable);
            m_UniqueBundleNameToogle.SetValueWithoutNotify(AssetBundleCollectorSettingData.Setting.UniqueBundleName);
            m_ShowEditorAliasToggle.SetValueWithoutNotify(AssetBundleCollectorSettingData.Setting.ShowEditorAlias);

            m_GroupContainer.visible = false;
            m_CollectorContainer.visible = false;

            FillPackageViewData();
        }

        void FixBtn_clicked()
        {
            AssetBundleCollectorSettingData.FixFile();
            RefreshWindow();
        }

        void ExportBtn_clicked()
        {
            string resultPath = UniverseEditor.OpenFolderPanel("Export XML", "Assets/");
            if (resultPath != null)
            {
                AssetBundleCollectorConfig.ExportXmlConfig($"{resultPath}/{nameof(AssetBundleCollectorConfig)}.xml");
            }
        }

        void ImportBtn_clicked()
        {
            string resultPath = UniverseEditor.OpenFilePath("Import XML", "Assets/", "xml");
            if (resultPath != null)
            {
                AssetBundleCollectorConfig.ImportXmlConfig(resultPath);
                RefreshWindow();
            }
        }

        void SaveBtn_clicked()
        {
            AssetBundleCollectorSettingData.SaveFile();
        }

        private string FormatListItemCallback(RuleDisplayName ruleDisplayName)
        {
            if (m_ShowEditorAliasToggle.value)
                return ruleDisplayName.DisplayName;
            else
                return ruleDisplayName.ClassName;
        }
        private string FormatSelectedValueCallback(RuleDisplayName ruleDisplayName)
        {
            if (m_ShowEditorAliasToggle.value)
                return ruleDisplayName.DisplayName;
            else
                return ruleDisplayName.ClassName;
        }

        // 包裹列表相关
        void FillPackageViewData()
        {
            m_PackageListView.Clear();
            m_PackageListView.ClearSelection();
            m_PackageListView.itemsSource = AssetBundleCollectorSettingData.Setting.Packages;
            m_PackageListView.Rebuild();

            if (m_LastModifyPackageIndex >= 0 && m_LastModifyPackageIndex < m_PackageListView.itemsSource.Count)
            {
                m_PackageListView.selectedIndex = m_LastModifyPackageIndex;
            }

            if (m_ShowPackageToogle.value)
                m_PackageContainer.style.display = DisplayStyle.Flex;
            else
                m_PackageContainer.style.display = DisplayStyle.None;
        }
        private VisualElement MakePackageListViewItem()
        {
            VisualElement element = new();

            {
                Label label = new()
                {
                    name = "Label1",
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleLeft,
                        flexGrow = 1f,
                        height = 20f
                    }
                };
                element.Add(label);
            }

            return element;
        }
        void BindPackageListViewItem(VisualElement element, int index)
        {
            AssetBundleCollectorPackage package = AssetBundleCollectorSettingData.Setting.Packages[index];

            Label textField1 = element.Q<Label>("Label1");
            if (string.IsNullOrEmpty(package.PackageDesc))
                textField1.text = package.PackageName;
            else
                textField1.text = $"{package.PackageName} ({package.PackageDesc})";
        }
        void PackageListView_onSelectionChange(IEnumerable<object> objs)
        {
            AssetBundleCollectorPackage selectPackage = m_PackageListView.selectedItem as AssetBundleCollectorPackage;
            if (selectPackage == null)
            {
                m_GroupContainer.visible = false;
                m_CollectorContainer.visible = false;
                return;
            }

            m_GroupContainer.visible = true;
            m_LastModifyPackageIndex = m_PackageListView.selectedIndex;
            m_PackageNameTxt.SetValueWithoutNotify(selectPackage.PackageName);
            m_PackageDescTxt.SetValueWithoutNotify(selectPackage.PackageDesc);
            FillGroupViewData();
        }
        void AddPackageBtn_clicked()
        {
            Undo.RecordObject(AssetBundleCollectorSettingData.Setting, "UniverseStudio.AssetBundleCollectorWindow AddPackage");
            AssetBundleCollectorSettingData.CreatePackage("DefaultPackage");
            FillPackageViewData();
        }
        void RemovePackageBtn_clicked()
        {
            AssetBundleCollectorPackage selectPackage = m_PackageListView.selectedItem as AssetBundleCollectorPackage;
            if (selectPackage == null)
                return;

            Undo.RecordObject(AssetBundleCollectorSettingData.Setting, "UniverseStudio.AssetBundleCollectorWindow RemovePackage");
            AssetBundleCollectorSettingData.RemovePackage(selectPackage);
            FillPackageViewData();
        }

        // 分组列表相关
        void FillGroupViewData()
        {
            AssetBundleCollectorPackage selectPackage = m_PackageListView.selectedItem as AssetBundleCollectorPackage;
            if (selectPackage == null)
                return;

            m_GroupListView.Clear();
            m_GroupListView.ClearSelection();
            m_GroupListView.itemsSource = selectPackage.Groups;
            m_GroupListView.Rebuild();

            if (m_LastModifyGroupIndex >= 0 && m_LastModifyGroupIndex < m_GroupListView.itemsSource.Count)
            {
                m_GroupListView.selectedIndex = m_LastModifyGroupIndex;
            }
        }
        private VisualElement MakeGroupListViewItem()
        {
            VisualElement element = new();

            {
                Label label = new()
                {
                    name = "Label1",
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleLeft,
                        flexGrow = 1f,
                        height = 20f
                    }
                };
                element.Add(label);
            }

            return element;
        }
        void BindGroupListViewItem(VisualElement element, int index)
        {
            AssetBundleCollectorPackage selectPackage = m_PackageListView.selectedItem as AssetBundleCollectorPackage;
            if (selectPackage == null)
                return;

            AssetBundleCollectorGroup group = selectPackage.Groups[index];

            Label textField1 = element.Q<Label>("Label1");
            if (string.IsNullOrEmpty(group.GroupDesc))
                textField1.text = group.GroupName;
            else
                textField1.text = $"{group.GroupName} ({group.GroupDesc})";

            // 激活状态
            IActiveRule activeRule = AssetBundleCollectorSettingData.GetActiveRuleInstance(group.ActiveRuleName);
            bool isActive = activeRule.IsActiveGroup();
            textField1.SetEnabled(isActive);
        }
        void GroupListView_onSelectionChange(IEnumerable<object> objs)
        {
            AssetBundleCollectorGroup selectGroup = m_GroupListView.selectedItem as AssetBundleCollectorGroup;
            if (selectGroup == null)
            {
                m_CollectorContainer.visible = false;
                return;
            }

            m_CollectorContainer.visible = true;
            m_LastModifyGroupIndex = m_GroupListView.selectedIndex;
            m_ActiveRulePopupField.SetValueWithoutNotify(GetActiveRuleIndex(selectGroup.ActiveRuleName));
            m_GroupNameTxt.SetValueWithoutNotify(selectGroup.GroupName);
            m_GroupDescTxt.SetValueWithoutNotify(selectGroup.GroupDesc);
            m_GroupAssetTagsTxt.SetValueWithoutNotify(selectGroup.AssetTags);

            FillCollectorViewData();
        }
        void AddGroupBtn_clicked()
        {
            AssetBundleCollectorPackage selectPackage = m_PackageListView.selectedItem as AssetBundleCollectorPackage;
            if (selectPackage == null)
                return;

            Undo.RecordObject(AssetBundleCollectorSettingData.Setting, "UniverseStudio.AssetBundleCollectorWindow AddGroup");
            AssetBundleCollectorSettingData.CreateGroup(selectPackage, "Default Group");
            FillGroupViewData();
        }
        void RemoveGroupBtn_clicked()
        {
            AssetBundleCollectorPackage selectPackage = m_PackageListView.selectedItem as AssetBundleCollectorPackage;
            if (selectPackage == null)
                return;

            AssetBundleCollectorGroup selectGroup = m_GroupListView.selectedItem as AssetBundleCollectorGroup;
            if (selectGroup == null)
                return;

            Undo.RecordObject(AssetBundleCollectorSettingData.Setting, "UniverseStudio.AssetBundleCollectorWindow RemoveGroup");
            AssetBundleCollectorSettingData.RemoveGroup(selectPackage, selectGroup);
            FillGroupViewData();
        }

        // 收集列表相关
        void FillCollectorViewData()
        {
            AssetBundleCollectorGroup selectGroup = m_GroupListView.selectedItem as AssetBundleCollectorGroup;
            if (selectGroup == null)
                return;

            // 填充数据
            m_CollectorScrollView.Clear();
            for (int i = 0; i < selectGroup.Collectors.Count; i++)
            {
                VisualElement element = MakeCollectorListViewItem();
                BindCollectorListViewItem(element, i);
                m_CollectorScrollView.Add(element);
            }
        }
        private VisualElement MakeCollectorListViewItem()
        {
            VisualElement element = new();

            VisualElement elementTop = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };
            element.Add(elementTop);

            VisualElement elementBottom = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };
            element.Add(elementBottom);

            VisualElement elementFoldout = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };
            element.Add(elementFoldout);

            VisualElement elementSpace = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Column
                }
            };
            element.Add(elementSpace);

            // Top VisualElement
            {
                Button button = new()
                {
                    name = "Button1",
                    text = "-",
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleCenter,
                        flexGrow = 0f
                    }
                };
                elementTop.Add(button);
            }
            {
                ObjectField objectField = new()
                {
                    name = "ObjectField1",
                    label = "Collector",
                    objectType = typeof(Object),
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleLeft,
                        flexGrow = 1f
                    }
                };
                elementTop.Add(objectField);
                Label label = objectField.Q<Label>();
                label.style.minWidth = 63;
            }

            // Bottom VisualElement
            {
                Label label = new()
                {
                    style =
                    {
                        width = 90
                    }
                };
                elementBottom.Add(label);
            }
            {
                PopupField<string> popupField = new(m_CollectorTypeList, 0)
                {
                    name = "PopupField0",
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleLeft,
                        width = 150
                    }
                };
                elementBottom.Add(popupField);
            }
            if (m_EnableAddressableToogle.value)
            {
                PopupField<RuleDisplayName> popupField = new(m_AddressRuleList, 0)
                {
                    name = "PopupField1",
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleLeft,
                        width = 220
                    }
                };
                elementBottom.Add(popupField);
            }
            {
                PopupField<RuleDisplayName> popupField = new(m_PackRuleList, 0)
                {
                    name = "PopupField2",
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleLeft,
                        width = 220
                    }
                };
                elementBottom.Add(popupField);
            }
            {
                PopupField<RuleDisplayName> popupField = new(m_FilterRuleList, 0)
                {
                    name = "PopupField3",
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleLeft,
                        width = 150
                    }
                };
                elementBottom.Add(popupField);
            }
            {
                TextField textField = new()
                {
                    name = "TextField0",
                    label = "UserData",
                    style =
                    {
                        width = 200
                    }
                };
                elementBottom.Add(textField);
                Label label = textField.Q<Label>();
                label.style.minWidth = 63;
            }
            {
                TextField textField = new()
                {
                    name = "TextField1",
                    label = "Tags",
                    style =
                    {
                        width = 100,
                        marginLeft = 20,
                        flexGrow = 1
                    }
                };
                elementBottom.Add(textField);
                Label label = textField.Q<Label>();
                label.style.minWidth = 40;
            }

            // Foldout VisualElement
            {
                Label label = new()
                {
                    style =
                    {
                        width = 90
                    }
                };
                elementFoldout.Add(label);
            }
            {
                Foldout foldout = new()
                {
                    name = "Foldout1",
                    value = false,
                    text = "Main Assets"
                };
                elementFoldout.Add(foldout);
            }

            // Space VisualElement
            {
                Label label = new()
                {
                    style =
                    {
                        height = 10
                    }
                };
                elementSpace.Add(label);
            }

            return element;
        }
        void BindCollectorListViewItem(VisualElement element, int index)
        {
            AssetBundleCollectorGroup selectGroup = m_GroupListView.selectedItem as AssetBundleCollectorGroup;
            if (selectGroup == null)
                return;

            AssetBundleCollector collector = selectGroup.Collectors[index];
            Object collectObject = AssetDatabase.LoadAssetAtPath<Object>(collector.CollectPath);
            if (collectObject != null)
                collectObject.name = collector.CollectPath;

            // Foldout
            Foldout foldout = element.Q<Foldout>("Foldout1");
            foldout.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                    RefreshFoldout(foldout, selectGroup, collector);
                else
                    foldout.Clear();
            });

            // Remove Button
            Button removeBtn = element.Q<Button>("Button1");
            removeBtn.clicked += () =>
            {
                RemoveCollectorBtn_clicked(collector);
            };

            // Collector Path
            ObjectField objectField1 = element.Q<ObjectField>("ObjectField1");
            objectField1.SetValueWithoutNotify(collectObject);
            objectField1.RegisterValueChangedCallback(evt =>
            {
                collector.CollectPath = AssetDatabase.GetAssetPath(evt.newValue);
                collector.CollectorGuid = AssetDatabase.AssetPathToGUID(collector.CollectPath);
                objectField1.value.name = collector.CollectPath;
                AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
                if (foldout.value)
                {
                    RefreshFoldout(foldout, selectGroup, collector);
                }
            });

            // Collector Type
            PopupField<string> popupField0 = element.Q<PopupField<string>>("PopupField0");
            popupField0.index = GetCollectorTypeIndex(collector.CollectorType.ToString());
            popupField0.RegisterValueChangedCallback(evt =>
            {
                collector.CollectorType = EnumUtility.NameToEnum<ECollectorType>(evt.newValue);
                AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
                if (foldout.value)
                {
                    RefreshFoldout(foldout, selectGroup, collector);
                }
            });

            // Address Rule
            PopupField<RuleDisplayName> popupField1 = element.Q<PopupField<RuleDisplayName>>("PopupField1");
            if (popupField1 != null)
            {
                popupField1.index = GetAddressRuleIndex(collector.AddressRuleName);
                popupField1.formatListItemCallback = FormatListItemCallback;
                popupField1.formatSelectedValueCallback = FormatSelectedValueCallback;
                popupField1.RegisterValueChangedCallback(evt =>
                {
                    collector.AddressRuleName = evt.newValue.ClassName;
                    AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
                    if (foldout.value)
                    {
                        RefreshFoldout(foldout, selectGroup, collector);
                    }
                });
            }

            // Pack Rule
            PopupField<RuleDisplayName> popupField2 = element.Q<PopupField<RuleDisplayName>>("PopupField2");
            popupField2.index = GetPackRuleIndex(collector.PackRuleName);
            popupField2.formatListItemCallback = FormatListItemCallback;
            popupField2.formatSelectedValueCallback = FormatSelectedValueCallback;
            popupField2.RegisterValueChangedCallback(evt =>
            {
                collector.PackRuleName = evt.newValue.ClassName;
                AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
                if (foldout.value)
                {
                    RefreshFoldout(foldout, selectGroup, collector);
                }
            });

            // Filter Rule
            PopupField<RuleDisplayName> popupField3 = element.Q<PopupField<RuleDisplayName>>("PopupField3");
            popupField3.index = GetFilterRuleIndex(collector.FilterRuleName);
            popupField3.formatListItemCallback = FormatListItemCallback;
            popupField3.formatSelectedValueCallback = FormatSelectedValueCallback;
            popupField3.RegisterValueChangedCallback(evt =>
            {
                collector.FilterRuleName = evt.newValue.ClassName;
                AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
                if (foldout.value)
                {
                    RefreshFoldout(foldout, selectGroup, collector);
                }
            });

            // UserData
            TextField textFiled0 = element.Q<TextField>("TextField0");
            textFiled0.SetValueWithoutNotify(collector.UserData);
            textFiled0.RegisterValueChangedCallback(evt =>
            {
                collector.UserData = evt.newValue;
                AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
            });

            // Tags
            TextField textFiled1 = element.Q<TextField>("TextField1");
            textFiled1.SetValueWithoutNotify(collector.AssetTags);
            textFiled1.RegisterValueChangedCallback(evt =>
            {
                collector.AssetTags = evt.newValue;
                AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
            });
        }
        void RefreshFoldout(Foldout foldout, AssetBundleCollectorGroup group, AssetBundleCollector collector)
        {
            // 清空旧元素
            foldout.Clear();

            if (collector.IsValid() == false)
            {
                Debug.LogWarning($"The collector is invalid : {collector.CollectPath} in group : {group.GroupName}");
                return;
            }

            if (collector.CollectorType == ECollectorType.MainAssetCollector || collector.CollectorType == ECollectorType.StaticAssetCollector)
            {
                List<CollectAssetInfo> collectAssetInfos = null;

                try
                {
                    CollectCommand command = new(EBuildMode.SimulateBuild, m_PackageNameTxt.value, m_EnableAddressableToogle.value, m_UniqueBundleNameToogle.value);
                    collectAssetInfos = collector.GetAllCollectAssets(command, group);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.ToString());
                }

                if (collectAssetInfos != null)
                {
                    foreach (CollectAssetInfo collectAssetInfo in collectAssetInfos)
                    {
                        VisualElement elementRow = new()
                        {
                            style =
                            {
                                flexDirection = FlexDirection.Row
                            }
                        };
                        foldout.Add(elementRow);

                        string showInfo = collectAssetInfo.AssetPath;
                        if (m_EnableAddressableToogle.value)
                            showInfo = $"[{collectAssetInfo.Address}] {collectAssetInfo.AssetPath}";

                        Label label = new()
                        {
                            text = showInfo,
                            style =
                            {
                                width = 300,
                                marginLeft = 0,
                                flexGrow = 1
                            }
                        };
                        elementRow.Add(label);
                    }
                }
            }
        }
        void AddCollectorBtn_clicked()
        {
            AssetBundleCollectorGroup selectGroup = m_GroupListView.selectedItem as AssetBundleCollectorGroup;
            if (selectGroup == null)
                return;

            Undo.RecordObject(AssetBundleCollectorSettingData.Setting, "UniverseStudio.AssetBundleCollectorWindow AddCollector");
            AssetBundleCollector collector = new();
            AssetBundleCollectorSettingData.CreateCollector(selectGroup, collector);
            FillCollectorViewData();
        }
        void RemoveCollectorBtn_clicked(AssetBundleCollector selectCollector)
        {
            AssetBundleCollectorGroup selectGroup = m_GroupListView.selectedItem as AssetBundleCollectorGroup;
            if (selectGroup == null)
                return;
            if (selectCollector == null)
                return;

            Undo.RecordObject(AssetBundleCollectorSettingData.Setting, "UniverseStudio.AssetBundleCollectorWindow RemoveCollector");
            AssetBundleCollectorSettingData.RemoveCollector(selectGroup, selectCollector);
            FillCollectorViewData();
        }

        private int GetCollectorTypeIndex(string typeName)
        {
            for (int i = 0; i < m_CollectorTypeList.Count; i++)
            {
                if (m_CollectorTypeList[i] == typeName)
                    return i;
            }
            return 0;
        }
        private int GetAddressRuleIndex(string ruleName)
        {
            for (int i = 0; i < m_AddressRuleList.Count; i++)
            {
                if (m_AddressRuleList[i].ClassName == ruleName)
                    return i;
            }
            return 0;
        }
        private int GetPackRuleIndex(string ruleName)
        {
            for (int i = 0; i < m_PackRuleList.Count; i++)
            {
                if (m_PackRuleList[i].ClassName == ruleName)
                    return i;
            }
            return 0;
        }
        private int GetFilterRuleIndex(string ruleName)
        {
            for (int i = 0; i < m_FilterRuleList.Count; i++)
            {
                if (m_FilterRuleList[i].ClassName == ruleName)
                    return i;
            }
            return 0;
        }
        private RuleDisplayName GetActiveRuleIndex(string ruleName)
        {
            for (int i = 0; i < m_ActiveRuleList.Count; i++)
            {
                if (m_ActiveRuleList[i].ClassName == ruleName)
                    return m_ActiveRuleList[i];
            }
            return m_ActiveRuleList[0];
        }
    }
}
#endif