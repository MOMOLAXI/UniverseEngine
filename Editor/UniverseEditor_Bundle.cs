namespace Universe
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public static partial class UniverseEditor
    {
        /// <summary>
        /// 检测AssetBundle文件是否合法
        /// </summary>
        public static bool CheckBundleFileValid(byte[] fileData)
        {
            string signature = ReadStringToNull(fileData, 20);
            if (signature is "UnityFS" or "UnityRaw" or "UnityWeb" or "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA")
            {
                return true;
            }

            return false;
        }
    }
}