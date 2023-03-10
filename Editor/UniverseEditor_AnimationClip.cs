using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;

namespace Universe
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public static partial class UniverseEditor
    {
        /// <summary>
        /// 检测所有动画控制器的冗余状态
        /// </summary>
        public static void PrintRedundantAnimationState(List<string> searchDirectorys)
        {
            if (searchDirectorys.Count == 0)
            {
                throw new("路径列表不能为空！");
            }

            // 获取所有资源列表
            int checkCount = 0;
            int findCount = 0;
            string[] findAssets = FindAssets(EAssetSearchType.RuntimeAnimatorController, searchDirectorys.ToArray());
            foreach (string assetPath in findAssets)
            {
                AnimatorController animator = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
                if (PrintRedundantAnimationState(animator))
                {
                    findCount++;
                    EditorLog.Error($"发现冗余的动画控制器：{assetPath}");
                }

                DisplayProgressBar("检测冗余的动画控制器", ++checkCount, findAssets.Length);
            }

            ClearProgressBar();

            if (findCount == 0)
            {
                EditorLog.Info("没有发现冗余的动画控制器");
            }
            else
            {
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// 查找动画控制器里冗余的动画状态机
        /// </summary>
        public static bool PrintRedundantAnimationState(AnimatorController animatorController)
        {
            if (animatorController == null)
            {
                return false;
            }

            string assetPath = AssetDatabase.GetAssetPath(animatorController);

            // 查找使用的状态机名称
            List<string> usedStateNames = new();
            foreach (AnimatorControllerLayer layer in animatorController.layers)
            {
                foreach (ChildAnimatorState state in layer.stateMachine.states)
                {
                    usedStateNames.Add(state.state.name);
                }
            }

            List<string> allLines = new();
            List<int> stateIndexList = new();
            using (StreamReader reader = File.OpenText(assetPath))
            {
                while (reader.ReadLine() is { } content)
                {
                    allLines.Add(content);
                    if (content.StartsWith("AnimatorState:"))
                    {
                        stateIndexList.Add(allLines.Count - 1);
                    }
                }
            }

            List<string> allStateNames = new();
            foreach (int index in stateIndexList)
            {
                for (int i = index; i < allLines.Count; i++)
                {
                    string content = allLines[i];
                    content = content.Trim();
                    if (content.StartsWith("m_Name"))
                    {
                        string[] splits = content.Split(':');
                        string name = splits[1].TrimStart(' '); //移除前面的空格
                        allStateNames.Add(name);
                        break;
                    }
                }
            }

            bool foundRedundantState = false;
            foreach (string stateName in allStateNames)
            {
                if (usedStateNames.Contains(stateName) == false)
                {
                    EditorLog.Error($"发现冗余的动画文件:{assetPath}={stateName}");
                    foundRedundantState = true;
                }
            }
            
            return foundRedundantState;
        }
    }
}