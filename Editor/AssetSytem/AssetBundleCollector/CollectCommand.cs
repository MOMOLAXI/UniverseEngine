
namespace Universe
{
	public class CollectCommand
	{
		/// <summary>
		/// 构建模式
		/// </summary>
		public EBuildMode BuildMode { get; }

		/// <summary>
		/// 包裹名称
		/// </summary>
		public string PackageName { get; }

		/// <summary>
		/// 是否启用可寻址资源定位
		/// </summary>
		public bool EnableAddressable { get; }

		/// <summary>
		/// 资源包名唯一化
		/// </summary>
		public bool UniqueBundleName { get; }

		public CollectCommand(EBuildMode buildMode, string packageName, bool enableAddressable, bool uniqueBundleName)
		{
			BuildMode = buildMode;
			PackageName = packageName;
			EnableAddressable = enableAddressable;
			UniqueBundleName = uniqueBundleName;
		}
	}
}