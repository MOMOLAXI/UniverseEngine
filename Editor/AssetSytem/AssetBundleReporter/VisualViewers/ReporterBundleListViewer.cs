#if UNITY_2019_4_OR_NEWER
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Universe
{
	internal class ReporterBundleListViewer
	{
		private enum ESortMode
		{
			BundleName,
			BundleSize,
			BundleTags
		}

		private VisualTreeAsset m_VisualAsset;
		private TemplateContainer m_Root;

		private ToolbarButton m_TopBar1;
		private ToolbarButton m_TopBar2;
		private ToolbarButton m_TopBar3;
		private ToolbarButton m_TopBar5;
		private ToolbarButton m_BottomBar1;
		private ListView m_BundleListView;
		private ListView m_IncludeListView;

		private BuildReport m_BuildReport;
		private string m_ReportFilePath;
		private string m_SearchKeyWord;
		private ESortMode m_SortMode = ESortMode.BundleName;
		private bool m_DescendingSort = false;

		/// <summary>
		/// 初始化页面
		/// </summary>
		public void InitViewer()
		{
			// 加载布局文件
			m_VisualAsset = UniverseEditor.LoadWindowUxml<ReporterBundleListViewer>();
			if (m_VisualAsset == null)
				return;

			m_Root = m_VisualAsset.CloneTree();
			m_Root.style.flexGrow = 1f;

			// 顶部按钮栏
			m_TopBar1 = m_Root.Q<ToolbarButton>("TopBar1");
			m_TopBar2 = m_Root.Q<ToolbarButton>("TopBar2");
			m_TopBar3 = m_Root.Q<ToolbarButton>("TopBar3");
			m_TopBar5 = m_Root.Q<ToolbarButton>("TopBar5");
			m_TopBar1.clicked += TopBar1_clicked;
			m_TopBar2.clicked += TopBar2_clicked;
			m_TopBar3.clicked += TopBar3_clicked;
			m_TopBar5.clicked += TopBar4_clicked;

			// 底部按钮栏
			m_BottomBar1 = m_Root.Q<ToolbarButton>("BottomBar1");

			// 资源包列表
			m_BundleListView = m_Root.Q<ListView>("TopListView");
			m_BundleListView.makeItem = MakeBundleListViewItem;
			m_BundleListView.bindItem = BindBundleListViewItem;
#if UNITY_2020_1_OR_NEWER
			m_BundleListView.onSelectionChange += BundleListView_onSelectionChange;
#else
			_bundleListView.onSelectionChanged += BundleListView_onSelectionChange;
#endif

			// 包含列表
			m_IncludeListView = m_Root.Q<ListView>("BottomListView");
			m_IncludeListView.makeItem = MakeIncludeListViewItem;
			m_IncludeListView.bindItem = BindIncludeListViewItem;
		}

		/// <summary>
		/// 填充页面数据
		/// </summary>
		public void FillViewData(BuildReport buildReport, string reprotFilePath, string searchKeyWord)
		{
			m_BuildReport = buildReport;
			m_ReportFilePath = reprotFilePath;
			m_SearchKeyWord = searchKeyWord;
			RefreshView();
		}
		private void RefreshView()
		{
			m_BundleListView.Clear();
			m_BundleListView.ClearSelection();
			m_BundleListView.itemsSource = FilterAndSortViewItems();
			m_BundleListView.Rebuild();
			RefreshSortingSymbol();
		}
		private List<ReportBundleInfo> FilterAndSortViewItems()
		{
			List<ReportBundleInfo> result = new(m_BuildReport.BundleInfos.Count);

			// 过滤列表
			foreach (ReportBundleInfo bundleInfo in m_BuildReport.BundleInfos)
			{
				if (string.IsNullOrEmpty(m_SearchKeyWord) == false)
				{
					if (bundleInfo.BundleName.Contains(m_SearchKeyWord) == false)
						continue;
				}
				result.Add(bundleInfo);
			}

			// 排序列表
			if (m_SortMode == ESortMode.BundleName)
			{
				if (m_DescendingSort)
					return result.OrderByDescending(a => a.BundleName).ToList();
				else
					return result.OrderBy(a => a.BundleName).ToList();
			}
			else if (m_SortMode == ESortMode.BundleSize)
			{
				if (m_DescendingSort)
					return result.OrderByDescending(a => a.FileSize).ToList();
				else
					return result.OrderBy(a => a.FileSize).ToList();
			}
			else if (m_SortMode == ESortMode.BundleTags)
			{
				if (m_DescendingSort)
					return result.OrderByDescending(a => a.GetTagsString()).ToList();
				else
					return result.OrderBy(a => a.GetTagsString()).ToList();
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		private void RefreshSortingSymbol()
		{
			// 刷新符号
			m_TopBar1.text = $"Bundle Name ({m_BundleListView.itemsSource.Count})";
			m_TopBar2.text = "Size";
			m_TopBar3.text = "Hash";
			m_TopBar5.text = "Tags";

			if (m_SortMode == ESortMode.BundleName)
			{
				if (m_DescendingSort)
					m_TopBar1.text = $"Bundle Name ({m_BundleListView.itemsSource.Count}) ↓";
				else
					m_TopBar1.text = $"Bundle Name ({m_BundleListView.itemsSource.Count}) ↑";
			}
			else if (m_SortMode == ESortMode.BundleSize)
			{
				if (m_DescendingSort)
					m_TopBar2.text = "Size ↓";
				else
					m_TopBar2.text = "Size ↑";
			}
			else if (m_SortMode == ESortMode.BundleTags)
			{
				if (m_DescendingSort)
					m_TopBar5.text = "Tags ↓";
				else
					m_TopBar5.text = "Tags ↑";
			}
			else
			{
				throw new NotImplementedException();
			}
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


		// 顶部列表相关
		private VisualElement MakeBundleListViewItem()
		{
			VisualElement element = new()
			{
				style =
				{
					flexDirection = FlexDirection.Row
				}
			};

			{
				Label label = new()
				{
					name = "Label1",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						flexGrow = 1f,
						width = 280
					}
				};
				element.Add(label);
			}

			{
				Label label = new()
				{
					name = "Label2",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						//label.style.flexGrow = 1f;
						width = 100
					}
				};
				element.Add(label);
			}

			{
				Label label = new()
				{
					name = "Label3",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						//label.style.flexGrow = 1f;
						width = 280
					}
				};
				element.Add(label);
			}

			{
				Label label = new()
				{
					name = "Label5",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						//label.style.flexGrow = 1f;
						width = 150
					}
				};
				element.Add(label);
			}

			{
				Label label = new()
				{
					name = "Label6",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						flexGrow = 1f,
						width = 80
					}
				};
				element.Add(label);
			}

			return element;
		}
		private void BindBundleListViewItem(VisualElement element, int index)
		{
			List<ReportBundleInfo> sourceData = m_BundleListView.itemsSource as List<ReportBundleInfo>;
			ReportBundleInfo bundleInfo = sourceData[index];

			// Bundle Name
			Label label1 = element.Q<Label>("Label1");
			label1.text = bundleInfo.BundleName;

			// Size
			Label label2 = element.Q<Label>("Label2");
			label2.text = EditorUtility.FormatBytes(bundleInfo.FileSize);

			// Hash
			Label label3 = element.Q<Label>("Label3");
			label3.text = bundleInfo.FileHash;

			// LoadMethod
			Label label5 = element.Q<Label>("Label5");
			label5.text = bundleInfo.LoadMethod.ToString();

			// Tags
			Label label6 = element.Q<Label>("Label6");
			label6.text = bundleInfo.GetTagsString();
		}
		private void BundleListView_onSelectionChange(IEnumerable<object> objs)
		{
			foreach (object item in objs)
			{
				ReportBundleInfo bundleInfo = item as ReportBundleInfo;
				FillIncludeListView(bundleInfo);
				ShowAssetBundleInspector(bundleInfo);
				break;
			}
		}
		private void ShowAssetBundleInspector(ReportBundleInfo bundleInfo)
		{
			if (bundleInfo.IsRawFile)
				return;

			string rootDirectory = Path.GetDirectoryName(m_ReportFilePath);
			string filePath = $"{rootDirectory}/{bundleInfo.FileName}";
			if (File.Exists(filePath))
				Selection.activeObject = AssetBundleRecorder.GetAssetBundle(filePath);
			else
				Selection.activeObject = null;
		}
		private void TopBar1_clicked()
		{
			if (m_SortMode != ESortMode.BundleName)
			{
				m_SortMode = ESortMode.BundleName;
				m_DescendingSort = false;
				RefreshView();
			}
			else
			{
				m_DescendingSort = !m_DescendingSort;
				RefreshView();
			}
		}
		private void TopBar2_clicked()
		{
			if (m_SortMode != ESortMode.BundleSize)
			{
				m_SortMode = ESortMode.BundleSize;
				m_DescendingSort = false;
				RefreshView();
			}
			else
			{
				m_DescendingSort = !m_DescendingSort;
				RefreshView();
			}
		}
		private void TopBar3_clicked()
		{
		}
		private void TopBar4_clicked()
		{
			if (m_SortMode != ESortMode.BundleTags)
			{
				m_SortMode = ESortMode.BundleTags;
				m_DescendingSort = false;
				RefreshView();
			}
			else
			{
				m_DescendingSort = !m_DescendingSort;
				RefreshView();
			}
		}

		// 底部列表相关
		private void FillIncludeListView(ReportBundleInfo bundleInfo)
		{
			List<ReportAssetInfo> containsList = new();
			foreach (ReportAssetInfo assetInfo in m_BuildReport.AssetInfos)
			{
				if (assetInfo.MainBundleName == bundleInfo.BundleName)
					containsList.Add(assetInfo);
			}

			m_IncludeListView.Clear();
			m_IncludeListView.ClearSelection();
			m_IncludeListView.itemsSource = containsList;
			m_IncludeListView.Rebuild();
			m_BottomBar1.text = $"Include Assets ({containsList.Count})";
		}
		private VisualElement MakeIncludeListViewItem()
		{
			VisualElement element = new()
			{
				style =
				{
					flexDirection = FlexDirection.Row
				}
			};

			{
				Label label = new()
				{
					name = "Label1",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						flexGrow = 1f,
						width = 280
					}
				};
				element.Add(label);
			}

			{
				Label label = new()
				{
					name = "Label2",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						//label.style.flexGrow = 1f;
						width = 280
					}
				};
				element.Add(label);
			}

			return element;
		}
		private void BindIncludeListViewItem(VisualElement element, int index)
		{
			List<ReportAssetInfo> containsList = m_IncludeListView.itemsSource as List<ReportAssetInfo>;
			ReportAssetInfo assetInfo = containsList[index];

			// Asset Path
			Label label1 = element.Q<Label>("Label1");
			label1.text = assetInfo.AssetPath;

			// GUID
			Label label2 = element.Q<Label>("Label2");
			label2.text = assetInfo.AssetGuid;
		}
	}
}
#endif