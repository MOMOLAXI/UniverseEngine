#if UNITY_2019_4_OR_NEWER
using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Universe
{
	public class AssetBundleReporterWindow : EditorWindow
	{
		[MenuItem("UniverseStudio/AssetBundle Reporter", false, 103)]
		public static void ShowExample()
		{
			AssetBundleReporterWindow window = GetWindow<AssetBundleReporterWindow>("资源包报告工具", true, EditorDefine.DockedWindowTypes);
			window.minSize = new(800, 600);
		}

		/// <summary>
		/// 视图模式
		/// </summary>
		private enum EViewMode
		{
			/// <summary>
			/// 概览视图
			/// </summary>
			Summary,

			/// <summary>
			/// 资源对象视图
			/// </summary>
			AssetView,

			/// <summary>
			/// 资源包视图
			/// </summary>
			BundleView,
		}

		private ToolbarMenu m_ViewModeMenu;
		private ReporterSummaryViewer m_SummaryViewer;
		private ReporterAssetListViewer m_AssetListViewer;
		private ReporterBundleListViewer m_BundleListViewer;

		private EViewMode m_ViewMode;
		private BuildReport m_BuildReport;
		private string m_ReportFilePath;
		private string m_SearchKeyWord;


		public void CreateGUI()
		{
			try
			{
				VisualElement root = rootVisualElement;

				// 加载布局文件
				VisualTreeAsset visualAsset =  UniverseEditor.LoadWindowUxml<AssetBundleReporterWindow>();
				if (visualAsset == null)
					return;
				
				visualAsset.CloneTree(root);

				// 导入按钮
				Button importBtn = root.Q<Button>("ImportButton");
				importBtn.clicked += ImportBtn_onClick;

				// 视图模式菜单
				m_ViewModeMenu = root.Q<ToolbarMenu>("ViewModeMenu");
				m_ViewModeMenu.menu.AppendAction(EViewMode.Summary.ToString(), ViewModeMenuAction0, ViewModeMenuFun0);
				m_ViewModeMenu.menu.AppendAction(EViewMode.AssetView.ToString(), ViewModeMenuAction1, ViewModeMenuFun1);
				m_ViewModeMenu.menu.AppendAction(EViewMode.BundleView.ToString(), ViewModeMenuAction2, ViewModeMenuFun2);

				// 搜索栏
				ToolbarSearchField searchField = root.Q<ToolbarSearchField>("SearchField");
				searchField.RegisterValueChangedCallback(OnSearchKeyWordChange);

				// 加载视图
				m_SummaryViewer = new();
				m_SummaryViewer.InitViewer();

				// 加载视图
				m_AssetListViewer = new();
				m_AssetListViewer.InitViewer();

				// 加载视图
				m_BundleListViewer = new();
				m_BundleListViewer.InitViewer();

				// 显示视图
				m_ViewMode = EViewMode.Summary;
				m_ViewModeMenu.text = EViewMode.Summary.ToString();
				m_SummaryViewer.AttachParent(root);
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
			}
		}
		public void OnDestroy()
		{
			AssetBundleRecorder.UnloadAll();
		}

		private void ImportBtn_onClick()
		{
			string selectFilePath = EditorUtility.OpenFilePanel("导入报告", FileUtility.GetProjectPath(), "json");
			if (string.IsNullOrEmpty(selectFilePath))
				return;

			m_ReportFilePath = selectFilePath;
			string jsonData = FileUtility.ReadAllText(m_ReportFilePath);
			m_BuildReport = BuildReport.Deserialize(jsonData);
			m_AssetListViewer.FillViewData(m_BuildReport, m_SearchKeyWord);
			m_BundleListViewer.FillViewData(m_BuildReport, m_ReportFilePath, m_SearchKeyWord);
			m_SummaryViewer.FillViewData(m_BuildReport);
		}
		private void OnSearchKeyWordChange(ChangeEvent<string> e)
		{
			m_SearchKeyWord = e.newValue;
			if (m_BuildReport != null)
			{
				m_AssetListViewer.FillViewData(m_BuildReport, m_SearchKeyWord);
				m_BundleListViewer.FillViewData(m_BuildReport, m_ReportFilePath, m_SearchKeyWord);
			}
		}
		private void ViewModeMenuAction0(DropdownMenuAction action)
		{
			if (m_ViewMode != EViewMode.Summary)
			{
				m_ViewMode = EViewMode.Summary;
				VisualElement root = rootVisualElement;
				m_ViewModeMenu.text = EViewMode.Summary.ToString();
				m_SummaryViewer.AttachParent(root);
				m_AssetListViewer.DetachParent();
				m_BundleListViewer.DetachParent();
			}
		}
		private void ViewModeMenuAction1(DropdownMenuAction action)
		{
			if (m_ViewMode != EViewMode.AssetView)
			{
				m_ViewMode = EViewMode.AssetView;
				VisualElement root = rootVisualElement;
				m_ViewModeMenu.text = EViewMode.AssetView.ToString();
				m_SummaryViewer.DetachParent();
				m_AssetListViewer.AttachParent(root);
				m_BundleListViewer.DetachParent();
			}
		}
		private void ViewModeMenuAction2(DropdownMenuAction action)
		{
			if (m_ViewMode != EViewMode.BundleView)
			{
				m_ViewMode = EViewMode.BundleView;
				VisualElement root = rootVisualElement;
				m_ViewModeMenu.text = EViewMode.BundleView.ToString();
				m_SummaryViewer.DetachParent();
				m_AssetListViewer.DetachParent();
				m_BundleListViewer.AttachParent(root);
			}
		}
		private DropdownMenuAction.Status ViewModeMenuFun0(DropdownMenuAction action)
		{
			if (m_ViewMode == EViewMode.Summary)
				return DropdownMenuAction.Status.Checked;
			else
				return DropdownMenuAction.Status.Normal;
		}
		private DropdownMenuAction.Status ViewModeMenuFun1(DropdownMenuAction action)
		{
			if (m_ViewMode == EViewMode.AssetView)
				return DropdownMenuAction.Status.Checked;
			else
				return DropdownMenuAction.Status.Normal;
		}
		private DropdownMenuAction.Status ViewModeMenuFun2(DropdownMenuAction action)
		{
			if (m_ViewMode == EViewMode.BundleView)
				return DropdownMenuAction.Status.Checked;
			else
				return DropdownMenuAction.Status.Normal;
		}
	}
}
#endif