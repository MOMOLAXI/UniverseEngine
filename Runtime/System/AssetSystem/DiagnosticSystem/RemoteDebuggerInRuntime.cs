using System;
using UnityEngine;

namespace Universe
{
    public class RemoteDebuggerInRuntime : MonoBehaviour
    {
    #if UNITY_EDITOR
        /// <summary>
        /// 编辑器下获取报告的回调
        /// </summary>
        public static Action<int, DebugReport> EditorHandleDebugReportCallback;

        /// <summary>
        /// 编辑器下请求报告数据
        /// </summary>
        public static void EditorRequestDebugReport()
        {
            if (UnityEditor.EditorApplication.isPlaying)
            {
	            var report = Engine.GetEngineSystem<AssetSystem>().GetDebugReport();
                EditorHandleDebugReportCallback?.Invoke(0, report);
            }
        }
    #else
        private void OnEnable()
        {
            PlayerConnection.instance.Register(RemoteDebuggerDefine.kMsgSendEditorToPlayer, OnHandleEditorMessage);
        }

        private void OnDisable()
        {
            PlayerConnection.instance.Unregister(RemoteDebuggerDefine.kMsgSendEditorToPlayer, OnHandleEditorMessage);
        }

        private void OnHandleEditorMessage(MessageEventArgs args)
        {
            var command = RemoteCommand.Deserialize(args.data);
            Log.Info($"On handle remote command : {command.CommandType} Param : {command.CommandParam}");
            if (command.CommandType == (int)ERemoteCommand.SampleOnce)
            {
                var debugReport = Engine.GetAssetDebugReport();
                var data = DebugReport.Serialize(debugReport);
                PlayerConnection.instance.Send(RemoteDebuggerDefine.kMsgSendPlayerToEditor, data);
            }
            else
            {
                throw new NotImplementedException(command.CommandType.ToString());
            }
        }
    #endif
    }
}