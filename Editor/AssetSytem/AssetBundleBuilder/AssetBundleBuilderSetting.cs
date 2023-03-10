using UnityEngine;

namespace Universe
{
    public class AssetBundleBuilderSetting : ScriptableObject
    {
        /// <summary>
        /// 构建版本
        /// </summary>
        public string BuildVersion = "0.0.0";

        /// <summary>
        /// 主版本号(无法向下兼容递增)
        /// </summary>
        public string MajorVersion;

        /// <summary>
        /// 次版本号(新增特性递增)
        /// </summary>
        public string MinorVersion;

        /// <summary>
        /// 修订版本号(Bug修复递增)
        /// </summary>
        public string PatchVersion;

        /// <summary>
        /// 构建管线
        /// </summary>
        public EBuildPipeline BuildPipeline = EBuildPipeline.BuiltinBuildPipeline;

        /// <summary>
        /// 构建模式
        /// </summary>
        public EBuildMode BuildMode = EBuildMode.ForceRebuild;

        /// <summary>
        /// 构建的包裹名称
        /// </summary>
        public string BuildPackage = string.Empty;

        /// <summary>
        /// 压缩方式
        /// </summary>
        public ECompressOption CompressOption = ECompressOption.LZ4;

        /// <summary>
        /// 输出文件名称样式
        /// </summary>
        public EOutputNameStyle OutputNameStyle = EOutputNameStyle.HashName;

        /// <summary>
        /// 首包资源文件的拷贝方式
        /// </summary>
        public ECopyBuildinFileOption CopyBuildinFileOption = ECopyBuildinFileOption.None;

        /// <summary>
        /// 首包资源文件的标签集合
        /// </summary>
        public string CopyBuildinFileTags = string.Empty;

        /// <summary>
        /// 加密类名称
        /// </summary>
        public string EncyptionClassName = string.Empty;
    }
}