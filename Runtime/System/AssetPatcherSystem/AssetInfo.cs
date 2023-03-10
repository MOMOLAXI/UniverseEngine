using System;

namespace Universe
{
    public struct AssetInfo
    {
        readonly PatchAsset m_PatchAsset;
        string m_ProviderGuid;

        /// <summary>
        /// 唯一标识符
        /// </summary>
        internal string Guid
        {
            get
            {
                if (string.IsNullOrEmpty(m_ProviderGuid) == false)
                    return m_ProviderGuid;

                m_ProviderGuid = AssetType == null ? $"{AssetPath}[null]" : $"{AssetPath}[{AssetType.Name}]";
                return m_ProviderGuid;
            }
        }

        /// <summary>
        /// 身份是否无效
        /// </summary>
        internal bool IsInvalid => m_PatchAsset == null;

        /// <summary>
        /// 错误信息
        /// </summary>
        internal string Error { get; }

        /// <summary>
        /// 可寻址地址
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath { get; }

        /// <summary>
        /// 资源类型
        /// </summary>
        public Type AssetType { get; }
        
        internal AssetInfo(PatchAsset patchAsset, Type assetType)
        {

            m_PatchAsset = patchAsset ?? throw new("Should never got here !");
            AssetType = assetType;
            Address = patchAsset.Address;
            AssetPath = patchAsset.AssetPath;
            Error = string.Empty;
            m_ProviderGuid = string.Empty;
        }

        internal AssetInfo(PatchAsset patchAsset)
        {
            m_PatchAsset = patchAsset ?? throw new("Should never got here !");
            AssetType = null;
            Address = patchAsset.Address;
            AssetPath = patchAsset.AssetPath;
            Error = string.Empty;
            m_ProviderGuid = string.Empty;
        }

        internal AssetInfo(string error)
        {
            m_PatchAsset = null;
            AssetType = null;
            Address = string.Empty;
            AssetPath = string.Empty;
            Error = error;
            m_ProviderGuid = string.Empty;
        }
    }
}