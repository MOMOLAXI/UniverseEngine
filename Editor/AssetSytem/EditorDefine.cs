using System;

namespace Universe
{
    public static class EditorDefine
    {
        /// <summary>
        /// 停靠窗口类型集合
        /// </summary>
        public static readonly Type[] DockedWindowTypes =
        {
            typeof(AssetBundleBuilderWindow),
            typeof(AssetBundleCollectorWindow),
            typeof(AssetBundleDebuggerWindow),
            typeof(AssetBundleReporterWindow),
            typeof(ShaderVariantCollectorWindow)
        };
    }
}