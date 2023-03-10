#if UNITY_EDITOR
using System;
using System.Reflection;

namespace Universe
{
    public static class EditorSimulateModeHelper
    {
        static Type ClassType;

        /// <summary>
        /// 编辑器下模拟构建补丁清单
        /// </summary>
        public static string SimulateBuild(string packageName)
        {
            ClassType ??= Assembly.Load("UniverseStudio.Editor").GetType("UniverseStudio.Editor.AssetBundleSimulateBuilder");
            string manifestFilePath = (string)InvokePublicStaticMethod(ClassType, "SimulateBuild", packageName);
            return manifestFilePath;
        }

        static object InvokePublicStaticMethod(Type type, string method, params object[] parameters)
        {
            MethodInfo methodInfo = type.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
            {
                UnityEngine.Debug.LogError($"{type.FullName} not found method : {method}");
                return null;
            }
            return methodInfo.Invoke(null, parameters);
        }
    }
}
#else
namespace Universe
{ 
	public static class EditorSimulateModeHelper
	{
		/// <summary>
		/// 编辑器下模拟构建补丁清单
		/// </summary>
		public static string SimulateBuild(string packageName) { throw new System.Exception("Only support in unity editor !"); }
	}
}
#endif