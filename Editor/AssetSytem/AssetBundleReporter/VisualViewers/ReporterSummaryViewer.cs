#if UNITY_2019_4_OR_NEWER
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Universe
{
    internal class ReporterSummaryViewer
    {
        private class ItemWrapper
        {
            public string Title { get; }
            public string Value { get; }

            public ItemWrapper(string title, string value)
            {
                Title = title;
                Value = value;
            }
        }

        private VisualTreeAsset m_VisualAsset;
        private TemplateContainer m_Root;

        private ListView m_ListView;
        private readonly List<ItemWrapper> m_Items = new();


        /// <summary>
        /// 初始化页面
        /// </summary>
        public void InitViewer()
        {
            // 加载布局文件
            m_VisualAsset = UniverseEditor.LoadWindowUxml<ReporterSummaryViewer>();
            if (m_VisualAsset == null)
                return;

            m_Root = m_VisualAsset.CloneTree();
            m_Root.style.flexGrow = 1f;

            // 概述列表
            m_ListView = m_Root.Q<ListView>("ListView");
            m_ListView.makeItem = MakeListViewItem;
            m_ListView.bindItem = BindListViewItem;
        }

        /// <summary>
        /// 填充页面数据
        /// </summary>
        public void FillViewData(BuildReport buildReport)
        {
            m_Items.Clear();

            m_Items.Add(new("UniverseStudio版本", buildReport.Summary.AssetSystemVersion));
            m_Items.Add(new("引擎版本", buildReport.Summary.UnityVersion));
            m_Items.Add(new("构建时间", buildReport.Summary.BuildDate));
            m_Items.Add(new("构建耗时", ConvertTime(buildReport.Summary.BuildSeconds)));
            m_Items.Add(new("构建平台", $"{buildReport.Summary.BuildTarget}"));
            m_Items.Add(new("构建管线", $"{buildReport.Summary.BuildPipeline}"));
            m_Items.Add(new("构建模式", $"{buildReport.Summary.BuildMode}"));
            m_Items.Add(new("包裹名称", buildReport.Summary.BuildPackageName));
            m_Items.Add(new("包裹版本", buildReport.Summary.BuildPackageVersion));

            m_Items.Add(new("启用可寻址资源定位", $"{buildReport.Summary.EnableAddressable}"));
            m_Items.Add(new("资源包名唯一化", $"{buildReport.Summary.UniqueBundleName}"));
            m_Items.Add(new("加密服务类名称", $"{buildReport.Summary.EncryptionServicesClassName}"));

            m_Items.Add(new(string.Empty, string.Empty));
            m_Items.Add(new("构建参数", string.Empty));
            m_Items.Add(new("OutputNameStyle", $"{buildReport.Summary.OutputNameStyle}"));
            m_Items.Add(new("CompressOption", $"{buildReport.Summary.CompressOption}"));
            m_Items.Add(new("DisableWriteTypeTree", $"{buildReport.Summary.DisableWriteTypeTree}"));
            m_Items.Add(new("IgnoreTypeTreeChanges", $"{buildReport.Summary.IgnoreTypeTreeChanges}"));

            m_Items.Add(new(string.Empty, string.Empty));
            m_Items.Add(new("构建结果", string.Empty));
            m_Items.Add(new("构建文件总数", $"{buildReport.Summary.AssetFileTotalCount}"));
            m_Items.Add(new("主资源总数", $"{buildReport.Summary.MainAssetTotalCount}"));
            m_Items.Add(new("资源包总数", $"{buildReport.Summary.AllBundleTotalCount}"));
            m_Items.Add(new("资源包总大小", ConvertSize(buildReport.Summary.AllBundleTotalSize)));
            m_Items.Add(new("加密资源包总数", $"{buildReport.Summary.EncryptedBundleTotalCount}"));
            m_Items.Add(new("加密资源包总大小", ConvertSize(buildReport.Summary.EncryptedBundleTotalSize)));
            m_Items.Add(new("原生资源包总数", $"{buildReport.Summary.RawBundleTotalCount}"));
            m_Items.Add(new("原生资源包总大小", ConvertSize(buildReport.Summary.RawBundleTotalSize)));

            m_ListView.Clear();
            m_ListView.ClearSelection();
            m_ListView.itemsSource = m_Items;
            m_ListView.Rebuild();
        }

        /// <summary>
        /// 挂接到父类页面上
        /// </summary>
        public void AttachParent(VisualElement parent)
        {
            parent.Add(m_Root);
        }

        /// <summary>
        /// 从父类页面脱离开
        /// </summary>
        public void DetachParent()
        {
            m_Root.RemoveFromHierarchy();
        }

        // 列表相关
        static VisualElement MakeListViewItem()
        {
            VisualElement element = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };

            element.Add(new()
            {
                name = "Label1",
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    marginLeft = 3f,
                    //label.style.flexGrow = 1f;
                    width = 200
                }
            });

            element.Add(new()
            {
                name = "Label2",
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    marginLeft = 3f,
                    flexGrow = 1f,
                    width = 150
                }
            });

            return element;
        }
        void BindListViewItem(VisualElement element, int index)
        {
            ItemWrapper itemWrapper = m_Items[index];

            // Title
            Label label1 = element.Q<Label>("Label1");
            label1.text = itemWrapper.Title;

            // Value
            Label label2 = element.Q<Label>("Label2");
            label2.text = itemWrapper.Value;
        }

        static string ConvertTime(float time)
        {
            if (time <= 60)
            {
                return $"{time}秒钟";
            }

            int minute = (int)time / 60;
            return $"{minute}分钟";
        }

        static string ConvertSize(long size)
        {
            if (size == 0)
            {
                return "0";
            }

            return EditorUtility.FormatBytes(size);
        }
    }
}
#endif