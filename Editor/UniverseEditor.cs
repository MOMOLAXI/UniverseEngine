using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = System.Object;

namespace Universe
{

    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public static partial class UniverseEditor
    {
        static UniverseEditor()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        static void OnEditorUpdate()
        {

        }

        [InitializeOnLoadMethod]
        static void InitializeEditor()
        {
            
        }

        [DidReloadScripts]
        static void OnReloadeAssembly()
        {
            EditorLog.Info("Compile Successfully");
        }

        /// <summary>
        /// 获取带继承关系的所有类的类型
        /// </summary>
        public static List<Type> GetAssignableTypes<T>()
        {
            TypeCache.TypeCollection collection = TypeCache.GetTypesDerivedFrom(typeof(T));
            return collection.ToList();
        }


        /// <summary>
        /// 调用私有的静态方法
        /// </summary>
        /// <param name="type">类的类型</param>
        /// <param name="method">类里要调用的方法名</param>
        /// <param name="parameters">调用方法传入的参数</param>
        public static object InvokeNonPublicStaticMethod(Type type, string method, params object[] parameters)
        {
            MethodInfo methodInfo = type.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static);
            if (methodInfo == null)
            {
                EditorLog.Error($"{type.FullName} not found method : {method}");
                return null;
            }
            return methodInfo.Invoke(null, parameters);
        }

        /// <summary>
        /// 调用公开的静态方法
        /// </summary>
        /// <param name="type">类的类型</param>
        /// <param name="method">类里要调用的方法名</param>
        /// <param name="parameters">调用方法传入的参数</param>
        public static object InvokePublicStaticMethod(Type type, string method, params object[] parameters)
        {
            MethodInfo methodInfo = type.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
            {
                EditorLog.Error($"{type.FullName} not found method : {method}");
                return null;
            }
            return methodInfo.Invoke(null, parameters);
        }

        public static List<T> FindInterfaces<T>()
        {
            List<T> interfaces = new();

            GameObject[] rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (var rootGameObject in rootGameObjects)
            {
                T[] childrenInterfaces = rootGameObject.GetComponentsInChildren<T>();
                foreach (T childInterface in childrenInterfaces)
                {
                    interfaces.Add(childInterface);
                }
            }

            return interfaces;
        }


        public static MethodInfo[] GetMethods(this MonoBehaviour monoBehaviour)
        {
            MonoScript monoScript = MonoScript.FromMonoBehaviour(monoBehaviour);
            MethodInfo[] methods = monoScript.GetClass().GetMethods();

            return methods;
        }

        public static MethodInfo[] GetMethods(this ScriptableObject scriptableObject)
        {
            MonoScript monoScript = MonoScript.FromScriptableObject(scriptableObject);
            MethodInfo[] methods = monoScript.GetClass().GetMethods();

            return methods;
        }

        public static Type[] GetAllDerivedObjects<T>(bool allowAbstract = true) where T : Component
        {
            List<Type> result = new();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsSubclassOf(typeof(T)))
                    {
                        if (type.IsAbstract && allowAbstract)
                        {
                            result.Add(type);
                        }
                        else
                        {
                            result.Add(type);
                        }
                    }
                }
            }

            return result.ToArray();
        }

        public static T GetInterface<T>(this MonoBehaviour monoBehaviour)
        {
            T[] walkInterfaces = monoBehaviour.GetComponents<T>();
            for (int i = 0; i < walkInterfaces.Length; i++)
            {
                T inter = walkInterfaces[i];
                object interfaceObject = inter;
                if (ReferenceEquals(interfaceObject, monoBehaviour))
                {
                    return inter;
                }
            }

            return default;
        }

        public static Type[] GetAllDerivedComponents(Type componentType, bool allowAbstract = true)
        {
            if (!componentType.IsSubclassOf(typeof(Component)))
            {
                return null;
            }

            List<Type> result = new();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsSubclassOf(componentType))
                    {
                        if (type.IsAbstract)
                        {
                            if (allowAbstract)
                                result.Add(type);
                        }
                        else
                        {
                            result.Add(type);
                        }
                    }
                }
            }

            return result.ToArray();
        }

        public static Type[] GetAllDerivedObjectsClass<T>(bool allowAbstract = true) where T : class
        {
            List<Type> result = new();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsSubclassOf(typeof(T)))
                    {
                        if (type.IsAbstract)
                        {
                            if (allowAbstract)
                                result.Add(type);
                        }
                        else
                        {
                            result.Add(type);
                        }
                    }
                }
            }

            return result.ToArray();
        }

        public static bool HasDirtyScenes()
        {
            int sceneCount = SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                {
                    return true;
                }
            }

            return false;
        }

        public static string ReadStringToNull(IReadOnlyList<byte> data, int maxLength)
        {
            List<byte> bytes = new();
            for (int i = 0; i < data.Count; i++)
            {
                if (i >= maxLength)
                {
                    break;
                }

                byte b = data[i];
                if (b == 0)
                {
                    break;
                }

                bytes.Add(b);
            }

            if (bytes.Count == 0)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
        }
    }
}