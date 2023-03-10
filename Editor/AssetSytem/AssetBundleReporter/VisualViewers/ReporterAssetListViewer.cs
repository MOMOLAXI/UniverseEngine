#if UNITY_2019_4_OR_NEWER
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Universe
{
	internal class ReporterAssetListViewer
	{
		private enum ESortMode
		{
			AssetPath,
			BundleName
		}

		private VisualTreeAsset m_VisualAsset;
		private TemplateContainer m_Root;

		private ToolbarButton m_TopBar1;
		private ToolbarButton m_TopBar2;
		private ToolbarButton m_BottomBar1;
		private ListView m_AssetListView;
		private ListView m_DependListView;

		private BuildReport m_BuildReport;
		private string m_SearchKeyWord;
		private ESortMode m_SortMode = ESortMode.AssetPath;
		private bool m_DescendingSort = false;


		/// <summary>
		/// 初始化页面
		/// </summary>
		public void InitViewer()
		{
			// 加载布局文件
			m_VisualAsset = UniverseEditor.LoadWindowUxml<ReporterAssetListViewer>();
			if (m_VisualAsset == null)
				return;

			m_Root = m_VisualAsset.CloneTree();
			m_Root.style.flexGrow = 1f;

			// 顶部按钮栏
			m_TopBar1 = m_Root.Q<ToolbarButton>("TopBar1");
			m_TopBar2 = m_Root.Q<ToolbarButton>("TopBar2");
			m_TopBar1.clicked += TopBar1_clicked;
			m_TopBar2.clicked += TopBar2_clicked;

			// 底部按钮栏
			m_BottomBar1 = m_Root.Q<ToolbarButton>("BottomBar1");

			// 资源列表
			m_AssetListView = m_Root.Q<ListView>("TopListView");
			m_AssetListView.makeItem = MakeAssetListViewItem;
			m_AssetListView.bindItem = BindAssetListViewItem;
#if UNITY_2020_1_OR_NEWER
			m_AssetListView.onSelectionChange += AssetListView_onSelectionChange;
#else
			_assetListView.onSelectionChanged += AssetListView_onSelectionChange;
#endif

			// 依赖列表
			m_DependListView = m_Root.Q<ListView>("BottomListView");
			m_DependListView.makeItem = MakeDependListViewItem;
			m_DependListView.bindItem = BindDependListViewItem;
		}

		/// <summary>
		/// 填充页面数据
		/// </summary>
		public void FillViewData(BuildReport buildReport, string searchKeyWord)
		{
			m_BuildReport = buildReport;
			m_SearchKeyWord = searchKeyWord;
			RefreshView();
		}
		private void RefreshView()
		{
			m_AssetListView.Clear();
			m_AssetListView.ClearSelection();
			m_AssetListView.itemsSource = FilterAndSortViewItems();
			m_AssetListView.Rebuild();
			RefreshSortingSymbol();
		}
		private List<ReportAssetInfo> FilterAndSortViewItems()
		{
			List<ReportAssetInfo> result = new(m_BuildReport.AssetInfos.Count);

			// 过滤列表
			foreach (ReportAssetInfo assetInfo in m_BuildReport.AssetInfos)
			{
				if (string.IsNullOrEmpty(m_SearchKeyWord) == false)
				{
					if (assetInfo.AssetPath.Contains(m_SearchKeyWord) == false)
						continue;
				}
				result.Add(assetInfo);
			}

			// 排序列表
			if (m_SortMode == ESortMode.AssetPath)
			{
				if (m_DescendingSort)
					return result.OrderByDescending(a => a.AssetPath).ToList();
				else
					return result.OrderBy(a => a.AssetPath).ToList();
			}
			else if (m_SortMode == ESortMode.BundleName)
			{
				if (m_DescendingSort)
					return result.OrderByDescending(a => a.MainBundleName).ToList();
				else
					return result.OrderBy(a => a.MainBundleName).ToList();
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		private void RefreshSortingSymbol()
		{
			// 刷新符号
			m_TopBar1.text = $"Asset Path ({m_AssetListView.itemsSource.Count})";
			m_TopBar2.text = "Main Bundle";

			if (m_SortMode == ESortMode.AssetPath)
			{
				if (m_DescendingSort)
					m_TopBar1.text = $"Asset Path ({m_AssetListView.itemsSource.Count}) ↓";
				else
					m_TopBar1.text = $"Asset Path ({m_AssetListView.itemsSource.Count}) ↑";
			}
			else if (m_SortMode == ESortMode.BundleName)
			{
				if (m_DescendingSort)
					m_TopBar2.text = "Main Bundle ↓";
				else
					m_TopBar2.text = "Main Bundle ↑";
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


		// 资源列表相关
		private VisualElement MakeAssetListViewItem()
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
						flexGrow = 1f,
						width = 150
					}
				};
				element.Add(label);
			}

			return element;
		}
		private void BindAssetListViewItem(VisualElement element, int index)
		{
			List<ReportAssetInfo> sourceData = m_AssetListView.itemsSource as List<ReportAssetInfo>;
			ReportAssetInfo assetInfo = sourceData[index];
			ReportBundleInfo bundleInfo = m_BuildReport.GetBundleInfo(assetInfo.MainBundleName);

			// Asset Path
			Label label1 = element.Q<Label>("Label1");
			label1.text = assetInfo.AssetPath;

			// Main Bundle
			Label label2 = element.Q<Label>("Label2");
			label2.text = bundleInfo.BundleName;
		}
		private void AssetListView_onSelectionChange(IEnumerable<object> objs)
		{
			foreach (object item in objs)
			{
				ReportAssetInfo assetInfo = item as ReportAssetInfo;
				FillDependListView(assetInfo);
			}
		}
		private void TopBar1_clicked()
		{
			if (m_SortMode != ESortMode.AssetPath)
			{
				m_SortMode = ESortMode.AssetPath;
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

		// 依赖列表相关
		private void FillDependListView(ReportAssetInfo assetInfo)
		{
			List<ReportBundleInfo> bundles = new();
			ReportBundleInfo mainBundle = m_BuildReport.GetBundleInfo(assetInfo.MainBundleName);
			bundles.Add(mainBundle);
			foreach (string dependBundleName in assetInfo.DependBundles)
			{
				ReportBundleInfo dependBundle = m_BuildReport.GetBundleInfo(dependBundleName);
				bundles.Add(dependBundle);
			}

			m_DependListView.Clear();
			m_DependListView.ClearSelection();
			m_DependListView.itemsSource = bundles;
			m_DependListView.Rebuild();
			m_BottomBar1.text = $"Depend Bundles ({bundles.Count})";
		}
		private VisualElement MakeDependListViewItem()
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

			return element;
		}
		private void BindDependListViewItem(VisualElement element, int index)
		{
			List<ReportBundleInfo> bundles = m_DependListView.itemsSource as List<ReportBundleInfo>;
			ReportBundleInfo bundleInfo = bundles[index];

			// Bundle Name
			Label label1 = element.Q<Label>("Label1");
			label1.text = bundleInfo.BundleName;

			// Size
			Label label2 = element.Q<Label>("Label2");
			label2.text = EditorUtility.FormatBytes(bundleInfo.FileSize);

			// Hash
			Label label3 = element.Q<Label>("Label3");
			label3.text = bundleInfo.FileHash;
		}
	}
}
#endif