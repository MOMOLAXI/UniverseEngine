namespace Universe
{
    [UniverseBuildTask("资源包加密")]
    public class TaskEncryption : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            BuildParametersContext buildParameters = context.GetContextObject<BuildParametersContext>();
            BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();

            EBuildMode buildMode = buildParameters.Parameters.BuildMode;
            if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
            {
                EncryptingBundleFiles(buildParameters, buildMapContext);
            }
        }

        /// <summary>
        /// 加密文件
        /// </summary>
        static void EncryptingBundleFiles(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext)
        {
            IEncryptionServices encryptionServices = buildParametersContext.Parameters.EncryptionServices;
            if (encryptionServices == null)
            {
                return;
            }

            if (encryptionServices.GetType() == typeof(EncryptionNone))
            {
                return;
            }

            int progressValue = 0;
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            for (int i = 0; i < buildMapContext.BundleInfos.Count; i++)
            {
                BuildBundleInfo bundleInfo = buildMapContext.BundleInfos[i];
                EncryptFileInfo fileInfo = new()
                {
                    BundleName = bundleInfo.BundleName,
                    FilePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}"
                };

                EncryptResult encryptResult = encryptionServices.Encrypt(fileInfo);
                if (encryptResult.LoadMethod != EBundleLoadMethod.Normal)
                {
                    // 注意：原生文件不支持加密
                    if (bundleInfo.IsRawFile)
                    {
                        EditorLog.Warning($"Encryption not support raw file : {bundleInfo.BundleName}");
                        continue;
                    }

                    string filePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}.encrypt";
                    FileUtility.CreateFile(filePath, encryptResult.EncryptedData);
                    bundleInfo.EncryptedFilePath = filePath;
                    bundleInfo.LoadMethod = encryptResult.LoadMethod;
                    EditorLog.Info($"Bundle文件加密完成：{filePath}");
                }

                // 进度条
                UniverseEditor.DisplayProgressBar("加密资源包", ++progressValue, buildMapContext.BundleInfos.Count);
            }

            UniverseEditor.ClearProgressBar();
        }
    }
}