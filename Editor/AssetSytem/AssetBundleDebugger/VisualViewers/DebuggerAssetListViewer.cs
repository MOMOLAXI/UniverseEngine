#if UNITY_2019_4_OR_NEWER
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Universe
{
	internal class DebuggerAssetListViewer
	{
		private VisualTreeAsset m_VisualAsset;
		private TemplateContainer m_Root;

		private ListView m_AssetListView;
		private ListView m_DependListView;
		private DebugReport m_DebugReport;

		/// <summary>
		/// 初始化页面
		/// </summary>
		public void InitViewer()
		{
			// 加载布局文件		
			m_VisualAsset = UniverseEditor.LoadWindowUxml<DebuggerAssetListViewer>();
			if (m_VisualAsset == null)
				return;

			m_Root = m_VisualAsset.CloneTree();
			m_Root.style.flexGrow = 1f;

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
		/// 清空页面
		/// </summary>
		public void ClearView()
		{
			m_DebugReport = null;
			m_AssetListView.Clear();
			m_AssetListView.ClearSelection();
			m_AssetListView.itemsSource.Clear();
			m_AssetListView.Rebuild();
		}

		/// <summary>
		/// 填充页面数据
		/// </summary>
		public void FillViewData(DebugReport debugReport, string searchKeyWord)
		{
			m_DebugReport = debugReport;
			m_AssetListView.Clear();
			m_AssetListView.ClearSelection();
			m_AssetListView.itemsSource = FilterViewItems(debugReport, searchKeyWord);
			m_AssetListView.Rebuild();
		}
		private List<DebugProviderInfo> FilterViewItems(DebugReport debugReport, string searchKeyWord)
		{
			List<DebugProviderInfo> result = new(1000);
			foreach (DebugPackageData packageData in debugReport.PackageDatas)
			{
				List<DebugProviderInfo> tempList = new(packageData.ProviderInfos.Count);
				foreach (DebugProviderInfo providerInfo in packageData.ProviderInfos)
				{
					if (string.IsNullOrEmpty(searchKeyWord) == false)
					{
						if (providerInfo.AssetPath.Contains(searchKeyWord) == false)
							continue;
					}

					providerInfo.PackageName = packageData.PackageName;
					tempList.Add(providerInfo);
				}

				tempList.Sort();
				result.AddRange(tempList);
			}
			return result;
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
					name = "Label0",
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
						width = 150
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
						width = 150
					}
				};
				element.Add(label);
			}

			{
				Label label = new()
				{
					name = "Label4",
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
					name = "Label5",
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
					name = "Label6",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						//label.style.flexGrow = 1f;
						width = 120
					}
				};
				element.Add(label);
			}

			return element;
		}
		private void BindAssetListViewItem(VisualElement element, int index)
		{
			List<DebugProviderInfo> sourceData = m_AssetListView.itemsSource as List<DebugProviderInfo>;
			DebugProviderInfo providerInfo = sourceData[index];

			// Package Name
			Label label0 = element.Q<Label>("Label0");
			label0.text = providerInfo.PackageName;

			// Asset Path
			Label label1 = element.Q<Label>("Label1");
			label1.text = providerInfo.AssetPath;

			// Spawn Scene
			Label label2 = element.Q<Label>("Label2");
			label2.text = providerInfo.SpawnScene;

			// Spawn Time
			Label label3 = element.Q<Label>("Label3");
			label3.text = providerInfo.SpawnTime;

			// Loading Time
			Label label4 = element.Q<Label>("Label4");
			label4.text = providerInfo.LoadingTime.ToString();

			// Ref Count
			Label label5 = element.Q<Label>("Label5");
			label5.text = providerInfo.RefCount.ToString();

			// Status
			StyleColor textColor;
			if (providerInfo.Status == ProviderBase.EStatus.Failed.ToString())
				textColor = new(Color.yellow);
			else
				textColor = label1.style.color;
			Label label6 = element.Q<Label>("Label6");
			label6.text = providerInfo.Status.ToString();
			label6.style.color = textColor;
		}
		private void AssetListView_onSelectionChange(IEnumerable<object> objs)
		{
			foreach (object item in objs)
			{
				DebugProviderInfo providerInfo = item as DebugProviderInfo;
				FillDependListView(providerInfo);
			}
		}

		// 底部列表相关
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
					name = "Label3",
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
					name = "Label4",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						//label.style.flexGrow = 1f;
						width = 120
					}
				};
				element.Add(label);
			}

			return element;
		}
		private void BindDependListViewItem(VisualElement element, int index)
		{
			List<DebugBundleInfo> bundles = m_DependListView.itemsSource as List<DebugBundleInfo>;
			DebugBundleInfo bundleInfo = bundles[index];

			// Bundle Name
			Label label1 = element.Q<Label>("Label1");
			label1.text = bundleInfo.BundleName;

			// Ref Count
			Label label3 = element.Q<Label>("Label3");
			label3.text = bundleInfo.RefCount.ToString();

			// Status
			Label label4 = element.Q<Label>("Label4");
			label4.text = bundleInfo.Status.ToString();
		}
		private void FillDependListView(DebugProviderInfo selectedProviderInfo)
		{
			m_DependListView.Clear();
			m_DependListView.ClearSelection();
			m_DependListView.itemsSource = selectedProviderInfo.DependBundleInfos;
			m_DependListView.Rebuild();
		}
	}
}
#endif