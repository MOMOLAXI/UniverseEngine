using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace Universe
{
    public static class BuildRunner
    {
        static Stopwatch s_BuildWathcer;

        /// <summary>
        /// 总耗时
        /// </summary>
        public static float TotalSeconds;

        /// <summary>
        /// 执行构建流程
        /// </summary>
        /// <returns>如果成功返回TRUE，否则返回FALSE</returns>
        public static BuildResult Run(List<IBuildTask> pipeline, BuildContext context)
        {
            if (pipeline == null)
            {
                throw new ArgumentNullException(nameof(pipeline));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            BuildResult buildResult = new()
            {
                Success = true
            };

            TotalSeconds = 0;
            for (int i = 0; i < pipeline.Count; i++)
            {
                IBuildTask task = pipeline[i];
                try
                {
                    UniverseBuildTaskAttribute taskAttribute = task.GetType().GetCustomAttribute<UniverseBuildTaskAttribute>();
                    EditorLog.Info($"---------------------------------------->{taskAttribute.Desc}<---------------------------------------");
                    Function.Run(task.Run, context, out float seconds);

                    // 统计耗时
                    TotalSeconds += seconds;
                    EditorLog.Info($"{taskAttribute.Desc}耗时：{seconds}秒");
                }
                catch (Exception e)
                {
                    UniverseEditor.ClearProgressBar();
                    buildResult.FailedTask = task.GetType().Name;
                    buildResult.FailedInfo = e.ToString();
                    buildResult.Success = false;
                    break;
                }
            }

            // 返回运行结果
            EditorLog.Info($"构建过程总计耗时：{TotalSeconds}秒");
            return buildResult;
        }
    }
}