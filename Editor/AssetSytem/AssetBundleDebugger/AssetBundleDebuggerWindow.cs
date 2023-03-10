#if UNITY_2019_4_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine.Networking.PlayerConnection;

namespace Universe
{
	public class AssetBundleDebuggerWindow : EditorWindow
	{
		[MenuItem("UniverseStudio/AssetBundle Debugger", false, 104)]
		public static void ShowExample()
		{
			AssetBundleDebuggerWindow wnd = GetWindow<AssetBundleDebuggerWindow>("资源包调试工具", true, EditorDefine.DockedWindowTypes);
			wnd.minSize = new(800, 600);
		}

		/// <summary>
		/// 视图模式
		/// </summary>
		private enum EViewMode
		{
			/// <summary>
			/// 内存视图
			/// </summary>
			MemoryView,

			/// <summary>
			/// 资源对象视图
			/// </summary>
			AssetView,

			/// <summary>
			/// 资源包视图
			/// </summary>
			BundleView,
		}


		private readonly Dictionary<int, RemotePlayerSession> m_PlayerSessions = new();

		private Label m_PlayerName;
		private ToolbarMenu m_ViewModeMenu;
		private SliderInt m_FrameSlider;
		private DebuggerAssetListViewer m_AssetListViewer;
		private DebuggerBundleListViewer m_BundleListViewer;

		private EViewMode m_ViewMode;
		private string m_SearchKeyWord;
		private DebugReport m_CurrentReport;
		private RemotePlayerSession m_CurrentPlayerSession;
		private int m_RangeIndex = 0;


		public void CreateGUI()
		{
			try
			{
				VisualElement root = rootVisualElement;

				// 加载布局文件
				VisualTreeAsset visualAsset = UniverseEditor.LoadWindowUxml<AssetBundleDebuggerWindow>();
				if (visualAsset == null)
					return;

				visualAsset.CloneTree(root);

				// 采样按钮
				Button sampleBtn = root.Q<Button>("SampleButton");
				sampleBtn.clicked += SampleBtn_onClick;

				// 导出按钮
				Button exportBtn = root.Q<Button>("ExportButton");
				exportBtn.clicked += ExportBtn_clicked;

				// 用户列表菜单
				m_PlayerName = root.Q<Label>("PlayerName");
				m_PlayerName.text = "Editor player";

				// 视口模式菜单
				m_ViewModeMenu = root.Q<ToolbarMenu>("ViewModeMenu");
				m_ViewModeMenu.menu.AppendAction(EViewMode.AssetView.ToString(), OnViewModeMenuChange, OnViewModeMenuStatusUpdate, EViewMode.AssetView);
				m_ViewModeMenu.menu.AppendAction(EViewMode.BundleView.ToString(), OnViewModeMenuChange, OnViewModeMenuStatusUpdate, EViewMode.BundleView);
				m_ViewModeMenu.text = EViewMode.AssetView.ToString();

				// 搜索栏
				ToolbarSearchField searchField = root.Q<ToolbarSearchField>("SearchField");
				searchField.RegisterValueChangedCallback(OnSearchKeyWordChange);

				// 帧数相关
				{
					m_FrameSlider = root.Q<SliderInt>("FrameSlider");
					m_FrameSlider.label = "Frame:";
					m_FrameSlider.highValue = 0;
					m_FrameSlider.lowValue = 0;
					m_FrameSlider.value = 0;
					m_FrameSlider.RegisterValueChangedCallback(evt =>
					{
						OnFrameSliderChange(evt.newValue);
					});

					ToolbarButton frameLast = root.Q<ToolbarButton>("FrameLast");
					frameLast.clicked += OnFrameLast_clicked;

					ToolbarButton frameNext = root.Q<ToolbarButton>("FrameNext");
					frameNext.clicked += OnFrameNext_clicked;

					ToolbarButton frameClear = root.Q<ToolbarButton>("FrameClear");
					frameClear.clicked += OnFrameClear_clicked;
				}

				// 加载视图
				m_AssetListViewer = new();
				m_AssetListViewer.InitViewer();

				// 加载视图
				m_BundleListViewer = new();
				m_BundleListViewer.InitViewer();

				// 显示视图
				m_ViewMode = EViewMode.AssetView;
				m_AssetListViewer.AttachParent(root);

				// 远程调试
				EditorConnection.instance.Initialize();
				EditorConnection.instance.RegisterConnection(OnHandleConnectionEvent);
				EditorConnection.instance.RegisterDisconnection(OnHandleDisconnectionEvent);
				EditorConnection.instance.Register(RemoteDebuggerDefine.kMsgSendPlayerToEditor, OnHandlePlayerMessage);
				RemoteDebuggerInRuntime.EditorHandleDebugReportCallback = OnHandleDebugReport;
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
			}
		}
		public void OnDestroy()
		{
			// 远程调试
			EditorConnection.instance.UnregisterConnection(OnHandleConnectionEvent);
			EditorConnection.instance.UnregisterDisconnection(OnHandleDisconnectionEvent);
			EditorConnection.instance.Unregister(RemoteDebuggerDefine.kMsgSendPlayerToEditor, OnHandlePlayerMessage);
			m_PlayerSessions.Clear();
		}

		private void OnHandleConnectionEvent(int playerId)
		{
			Debug.Log($"Game player connection : {playerId}");
			m_PlayerName.text = $"Connected player : {playerId}";
		}
		private void OnHandleDisconnectionEvent(int playerId)
		{
			Debug.Log($"Game player disconnection : {playerId}");
			m_PlayerName.text = $"Disconneced player : {playerId}";
		}
		private void OnHandlePlayerMessage(MessageEventArgs args)
		{
			DebugReport debugReport = DebugReport.Deserialize(args.data);
			OnHandleDebugReport(args.playerId, debugReport);
		}
		private void OnHandleDebugReport(int playerId, DebugReport debugReport)
		{
			Debug.Log($"Handle player {playerId} debug report !");
			m_CurrentPlayerSession = GetOrCreatePlayerSession(playerId);
			m_CurrentPlayerSession.AddDebugReport(debugReport);
			m_FrameSlider.highValue = m_CurrentPlayerSession.MaxRangeValue;
			m_FrameSlider.value = m_CurrentPlayerSession.MaxRangeValue;
			UpdateFrameView(m_CurrentPlayerSession);
		}
		private void OnFrameSliderChange(int sliderValue)
		{
			if (m_CurrentPlayerSession != null)
			{
				m_RangeIndex = m_CurrentPlayerSession.ClampRangeIndex(sliderValue); ;
				UpdateFrameView(m_CurrentPlayerSession, m_RangeIndex);
			}
		}
		private void OnFrameLast_clicked()
		{
			if (m_CurrentPlayerSession != null)
			{
				m_RangeIndex = m_CurrentPlayerSession.ClampRangeIndex(m_RangeIndex - 1);
				m_FrameSlider.value = m_RangeIndex;
				UpdateFrameView(m_CurrentPlayerSession, m_RangeIndex);
			}
		}
		private void OnFrameNext_clicked()
		{
			if (m_CurrentPlayerSession != null)
			{
				m_RangeIndex = m_CurrentPlayerSession.ClampRangeIndex(m_RangeIndex + 1);
				m_FrameSlider.value = m_RangeIndex;
				UpdateFrameView(m_CurrentPlayerSession, m_RangeIndex);
			}
		}
		private void OnFrameClear_clicked()
		{
			if (m_CurrentPlayerSession != null)
			{
				m_FrameSlider.label = "Frame:";
				m_FrameSlider.value = 0;
				m_FrameSlider.lowValue = 0;
				m_FrameSlider.highValue = 0;
				m_CurrentPlayerSession.ClearDebugReport();
				m_AssetListViewer.ClearView();
				m_BundleListViewer.ClearView();
			}
		}

		private RemotePlayerSession GetOrCreatePlayerSession(int playerId)
		{
			if (m_PlayerSessions.TryGetValue(playerId, out RemotePlayerSession session))
			{
				return session;
			}
			else
			{
				RemotePlayerSession newSession = new(playerId);
				m_PlayerSessions.Add(playerId, newSession);
				return newSession;
			}
		}
		private void UpdateFrameView(RemotePlayerSession playerSession)
		{
			if (playerSession != null)
			{
				UpdateFrameView(playerSession, playerSession.MaxRangeValue);
			}
		}
		private void UpdateFrameView(RemotePlayerSession playerSession, int rangeIndex)
		{
			if (playerSession == null)
				return;

			DebugReport debugReport = playerSession.GetDebugReport(rangeIndex);
			if (debugReport != null)
			{
				m_CurrentReport = debugReport;
				m_FrameSlider.label = $"Frame: {debugReport.FrameCount}";
				m_AssetListViewer.FillViewData(debugReport, m_SearchKeyWord);
				m_BundleListViewer.FillViewData(debugReport, m_SearchKeyWord);
			}
		}

		private void SampleBtn_onClick()
		{
			// 发送采集数据的命令
			RemoteCommand command = new()
			{
				CommandType = (int)ERemoteCommand.SampleOnce,
				CommandParam = string.Empty
			};
			byte[] data = RemoteCommand.Serialize(command);
			EditorConnection.instance.Send(RemoteDebuggerDefine.kMsgSendEditorToPlayer, data);
			RemoteDebuggerInRuntime.EditorRequestDebugReport();
		}
		private void ExportBtn_clicked()
		{
			if (m_CurrentReport == null)
			{
				Debug.LogWarning("Debug report is null.");
				return;
			}

			string resultPath = UniverseEditor.OpenFolderPanel("Export JSON", "Assets/");
			if (resultPath != null)
			{
				// 注意：排序保证生成配置的稳定性
				foreach (DebugPackageData packageData in m_CurrentReport.PackageDatas)
				{
					packageData.ProviderInfos.Sort();
					foreach (DebugProviderInfo providerInfo in packageData.ProviderInfos)
					{
						providerInfo.DependBundleInfos.Sort();
					}
				}

				string filePath = $"{resultPath}/{nameof(DebugReport)}_{m_CurrentReport.FrameCount}.json";
				string fileContent = JsonUtility.ToJson(m_CurrentReport, true);
				FileUtility.CreateFile(filePath, fileContent);
			}
		}
		private void OnSearchKeyWordChange(ChangeEvent<string> e)
		{
			m_SearchKeyWord = e.newValue;
			if (m_CurrentReport != null)
			{
				m_AssetListViewer.FillViewData(m_CurrentReport, m_SearchKeyWord);
				m_BundleListViewer.FillViewData(m_CurrentReport, m_SearchKeyWord);
			}
		}
		private void OnViewModeMenuChange(DropdownMenuAction action)
		{
			EViewMode viewMode = (EViewMode)action.userData;
			if (m_ViewMode != viewMode)
			{
				m_ViewMode = viewMode;
				VisualElement root = rootVisualElement;
				m_ViewModeMenu.text = viewMode.ToString();

				if (viewMode == EViewMode.AssetView)
				{
					m_AssetListViewer.AttachParent(root);
					m_BundleListViewer.DetachParent();
				}
				else if (viewMode == EViewMode.BundleView)
				{
					m_AssetListViewer.DetachParent();
					m_BundleListViewer.AttachParent(root);
				}
				else
				{
					throw new NotImplementedException(viewMode.ToString());
				}
			}
		}
		private DropdownMenuAction.Status OnViewModeMenuStatusUpdate(DropdownMenuAction action)
		{
			EViewMode viewMode = (EViewMode)action.userData;
			if (m_ViewMode == viewMode)
				return DropdownMenuAction.Status.Checked;
			else
				return DropdownMenuAction.Status.Normal;
		}
	}
}
#endif