using System;
using System.Collections.Generic;
using UnityEngine;
using UniverseEngineEditor;

namespace Universe
{
    public class AssetBundleBuilder
    {
        readonly BuildContext m_BuildContext = new();

        /// <summary>
        /// 开始构建
        /// </summary>
        public BuildResult Run(BuildParameters buildParameters)
        {
            // 清空旧数据
            m_BuildContext.ClearAllContext();

            // 检测构建参数是否为空
            if (buildParameters == null)
            {
                throw new($"{nameof(buildParameters)} is null !");
            }

            // 检测可编程构建管线参数
            if (buildParameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
            {
                if (buildParameters.SbpParameters == null)
                {
                    throw new($"{nameof(BuildParameters.SbpParameters)} is null !");
                }

                switch (buildParameters.BuildMode)
                {
                    case EBuildMode.DryRunBuild: throw new($"{nameof(EBuildPipeline.ScriptableBuildPipeline)} not support {nameof(EBuildMode.DryRunBuild)} build mode !");
                    case EBuildMode.ForceRebuild: throw new($"{nameof(EBuildPipeline.ScriptableBuildPipeline)} not support {nameof(EBuildMode.ForceRebuild)} build mode !");
                    case EBuildMode.IncrementalBuild: break;
                    case EBuildMode.SimulateBuild: break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            // 构建参数
            BuildParametersContext buildParametersContext = new(buildParameters);
            m_BuildContext.SetContextObject(buildParametersContext);

            // 是否显示LOG
            EditorLog.EnableLog = buildParameters.BuildMode != EBuildMode.SimulateBuild;

            // 创建构建节点
            List<IBuildTask> pipeline = buildParameters.BuildPipeline switch
            {
                EBuildPipeline.BuiltinBuildPipeline => new()
                {
                    new TaskPrepare(),             //前期准备工作
                    new TaskGetBuildMap(),         //获取构建列表
                    new TaskBuilding(),            //开始执行构建
                    new TaskCopyRawFile(),         //拷贝原生文件
                    new TaskVerifyBuildResult(),   //验证构建结果
                    new TaskEncryption(),          //加密资源文件
                    new TaskUpdateBuildInfo(),     //更新构建信息
                    new TaskCreatePatchManifest(), //创建清单文件
                    new TaskCreateReport(),        //创建报告文件
                    new TaskCreatePatchPackage(),  //制作补丁包
                    new TaskCopyBuildinFiles(),    //拷贝内置文件
                },
                EBuildPipeline.ScriptableBuildPipeline => new()
                {
                    new TaskPrepare(),               //前期准备工作
                    new TaskGetBuildMap(),           //获取构建列表
                    new TaskBuildingSbp(),          //开始执行构建
                    new TaskCopyRawFile(),           //拷贝原生文件
                    new TaskVerifyBuildResultSbp(), //验证构建结果
                    new TaskEncryption(),            //加密资源文件
                    new TaskUpdateBuildInfo(),       //更新构建信息
                    new TaskCreatePatchManifest(),   //创建清单文件
                    new TaskCreateReport(),          //创建报告文件
                    new TaskCreatePatchPackage(),    //制作补丁包
                    new TaskCopyBuildinFiles(),      //拷贝内置文件
                },
                _ => new()
            };

            // 执行构建流程
            BuildResult buildResult = BuildRunner.Run(pipeline, m_BuildContext);
            if (buildResult.Success)
            {
                buildResult.OutputPackageDirectory = buildParametersContext.GetPackageOutputDirectory();
                Debug.Log($"{buildParameters.BuildMode} pipeline build succeed !");
            }
            else
            {
                Debug.LogWarning($"{buildParameters.BuildMode} pipeline build failed !");
                Debug.LogError($"Build task failed : {buildResult.FailedTask}");
                Debug.LogError($"Build task error : {buildResult.FailedInfo}");
            }
            return buildResult;
        }
    }
}