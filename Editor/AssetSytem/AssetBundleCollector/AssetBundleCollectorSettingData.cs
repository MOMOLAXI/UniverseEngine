using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Universe
{
	public class AssetBundleCollectorSettingData
	{
		private static readonly Dictionary<string, Type> s_CacheActiveRuleTypes = new();
		private static readonly Dictionary<string, IActiveRule> s_CacheActiveRuleInstance = new();

		private static readonly Dictionary<string, Type> s_CacheAddressRuleTypes = new();
		private static readonly Dictionary<string, IAddressRule> s_CacheAddressRuleInstance = new();

		private static readonly Dictionary<string, Type> s_CachePackRuleTypes = new();
		private static readonly Dictionary<string, IPackRule> s_CachePackRuleInstance = new();

		private static readonly Dictionary<string, Type> s_CacheFilterRuleTypes = new();
		private static readonly Dictionary<string, IFilterRule> s_CacheFilterRuleInstance = new();

		/// <summary>
		/// 配置数据是否被修改
		/// </summary>
		public static bool IsDirty { private set; get; } = false;


		private static AssetBundleCollectorSetting s_Setting = null;
		public static AssetBundleCollectorSetting Setting
		{
			get
			{
				if (s_Setting == null)
					LoadSettingData();
				return s_Setting;
			}
		}

		/// <summary>
		/// 加载配置文件
		/// </summary>
		private static void LoadSettingData()
		{
			s_Setting = EditorHelper.LoadSettingData<AssetBundleCollectorSetting>();

			// IPackRule
			{
				// 清空缓存集合
				s_CachePackRuleTypes.Clear();
				s_CachePackRuleInstance.Clear();

				// 获取所有类型
				List<Type> types = new(100)
				{
					typeof(PackSeparately),
					typeof(PackDirectory),
					typeof(PackTopDirectory),
					typeof(PackCollector),
					typeof(PackGroup),
					typeof(PackRawFile),
					typeof(PackShaderVariants)
				};

				List<Type> customTypes = UniverseEditor.GetAssignableTypes<IPackRule>();
				types.AddRange(customTypes);
				for (int i = 0; i < types.Count; i++)
				{
					Type type = types[i];
					if (s_CachePackRuleTypes.ContainsKey(type.Name) == false)
						s_CachePackRuleTypes.Add(type.Name, type);
				}
			}

			// IFilterRule
			{
				// 清空缓存集合
				s_CacheFilterRuleTypes.Clear();
				s_CacheFilterRuleInstance.Clear();

				// 获取所有类型
				List<Type> types = new(100)
				{
					typeof(CollectAll),
					typeof(CollectScene),
					typeof(CollectPrefab),
					typeof(CollectSprite)
				};

				List<Type> customTypes = UniverseEditor.GetAssignableTypes<IFilterRule>();
				types.AddRange(customTypes);
				for (int i = 0; i < types.Count; i++)
				{
					Type type = types[i];
					if (s_CacheFilterRuleTypes.ContainsKey(type.Name) == false)
						s_CacheFilterRuleTypes.Add(type.Name, type);
				}
			}

			// IAddressRule
			{
				// 清空缓存集合
				s_CacheAddressRuleTypes.Clear();
				s_CacheAddressRuleInstance.Clear();

				// 获取所有类型
				List<Type> types = new(100)
				{
					typeof(AddressByFileName),
					typeof(AddressByFolderAndFileName),
					typeof(AddressByGroupAndFileName)
				};

				List<Type> customTypes = UniverseEditor.GetAssignableTypes<IAddressRule>();
				types.AddRange(customTypes);
				for (int i = 0; i < types.Count; i++)
				{
					Type type = types[i];
					if (s_CacheAddressRuleTypes.ContainsKey(type.Name) == false)
						s_CacheAddressRuleTypes.Add(type.Name, type);
				}
			}

			// IActiveRule
			{
				// 清空缓存集合
				s_CacheActiveRuleTypes.Clear();
				s_CacheActiveRuleInstance.Clear();

				// 获取所有类型
				List<Type> types = new(100)
				{
					typeof(EnableGroup),
					typeof(DisableGroup),
				};

				List<Type> customTypes = UniverseEditor.GetAssignableTypes<IActiveRule>();
				types.AddRange(customTypes);
				for (int i = 0; i < types.Count; i++)
				{
					Type type = types[i];
					if (s_CacheActiveRuleTypes.ContainsKey(type.Name) == false)
						s_CacheActiveRuleTypes.Add(type.Name, type);
				}
			}
		}

		/// <summary>
		/// 存储配置文件
		/// </summary>
		public static void SaveFile()
		{
			if (Setting != null)
			{
				IsDirty = false;
				EditorUtility.SetDirty(Setting);
				AssetDatabase.SaveAssets();
				Debug.Log($"{nameof(AssetBundleCollectorSetting)}.asset is saved!");
			}
		}

		/// <summary>
		/// 修复配置文件
		/// </summary>
		public static void FixFile()
		{
			bool isFixed = Setting.FixConfigError();
			if (isFixed)
			{
				IsDirty = true;
			}
		}

		/// <summary>
		/// 清空所有数据
		/// </summary>
		public static void ClearAll()
		{
			Setting.ClearAll();
			SaveFile();
		}

		public static List<RuleDisplayName> GetActiveRuleNames()
		{
			if (s_Setting == null)
				LoadSettingData();

			List<RuleDisplayName> names = new();
			foreach (KeyValuePair<string, Type> pair in s_CacheActiveRuleTypes)
			{
				RuleDisplayName ruleName = new()
				{
					ClassName = pair.Key,
					DisplayName = GetRuleDisplayName(pair.Key, pair.Value)
				};
				names.Add(ruleName);
			}
			return names;
		}
		public static List<RuleDisplayName> GetAddressRuleNames()
		{
			if (s_Setting == null)
				LoadSettingData();

			List<RuleDisplayName> names = new();
			foreach (KeyValuePair<string, Type> pair in s_CacheAddressRuleTypes)
			{
				RuleDisplayName ruleName = new()
				{
					ClassName = pair.Key,
					DisplayName = GetRuleDisplayName(pair.Key, pair.Value)
				};
				names.Add(ruleName);
			}
			return names;
		}
		public static List<RuleDisplayName> GetPackRuleNames()
		{
			if (s_Setting == null)
				LoadSettingData();

			List<RuleDisplayName> names = new();
			foreach (KeyValuePair<string, Type> pair in s_CachePackRuleTypes)
			{
				RuleDisplayName ruleName = new()
				{
					ClassName = pair.Key,
					DisplayName = GetRuleDisplayName(pair.Key, pair.Value)
				};
				names.Add(ruleName);
			}
			return names;
		}
		public static List<RuleDisplayName> GetFilterRuleNames()
		{
			if (s_Setting == null)
				LoadSettingData();

			List<RuleDisplayName> names = new();
			foreach (KeyValuePair<string, Type> pair in s_CacheFilterRuleTypes)
			{
				RuleDisplayName ruleName = new()
				{
					ClassName = pair.Key,
					DisplayName = GetRuleDisplayName(pair.Key, pair.Value)
				};
				names.Add(ruleName);
			}
			return names;
		}
		private static string GetRuleDisplayName(string name, Type type)
		{
			DisplayNameAttribute attribute = EditorAttribute.GetAttribute<DisplayNameAttribute>(type);
			if (attribute != null && string.IsNullOrEmpty(attribute.DisplayName) == false)
				return attribute.DisplayName;
			else
				return name;
		}

		public static bool HasActiveRuleName(string ruleName)
		{
			return s_CacheActiveRuleTypes.Keys.Contains(ruleName);
		}
		public static bool HasAddressRuleName(string ruleName)
		{
			return s_CacheAddressRuleTypes.Keys.Contains(ruleName);
		}
		public static bool HasPackRuleName(string ruleName)
		{
			return s_CachePackRuleTypes.Keys.Contains(ruleName);
		}
		public static bool HasFilterRuleName(string ruleName)
		{
			return s_CacheFilterRuleTypes.Keys.Contains(ruleName);
		}

		public static IActiveRule GetActiveRuleInstance(string ruleName)
		{
			if (s_CacheActiveRuleInstance.TryGetValue(ruleName, out IActiveRule instance))
				return instance;

			// 如果不存在创建类的实例
			if (s_CacheActiveRuleTypes.TryGetValue(ruleName, out Type type))
			{
				instance = (IActiveRule)Activator.CreateInstance(type);
				s_CacheActiveRuleInstance.Add(ruleName, instance);
				return instance;
			}
			else
			{
				throw new($"{nameof(IActiveRule)}类型无效：{ruleName}");
			}
		}
		public static IAddressRule GetAddressRuleInstance(string ruleName)
		{
			if (s_CacheAddressRuleInstance.TryGetValue(ruleName, out IAddressRule instance))
				return instance;

			// 如果不存在创建类的实例
			if (s_CacheAddressRuleTypes.TryGetValue(ruleName, out Type type))
			{
				instance = (IAddressRule)Activator.CreateInstance(type);
				s_CacheAddressRuleInstance.Add(ruleName, instance);
				return instance;
			}
			else
			{
				throw new($"{nameof(IAddressRule)}类型无效：{ruleName}");
			}
		}
		public static IPackRule GetPackRuleInstance(string ruleName)
		{
			if (s_CachePackRuleInstance.TryGetValue(ruleName, out IPackRule instance))
				return instance;

			// 如果不存在创建类的实例
			if (s_CachePackRuleTypes.TryGetValue(ruleName, out Type type))
			{
				instance = (IPackRule)Activator.CreateInstance(type);
				s_CachePackRuleInstance.Add(ruleName, instance);
				return instance;
			}
			else
			{
				throw new($"{nameof(IPackRule)}类型无效：{ruleName}");
			}
		}
		public static IFilterRule GetFilterRuleInstance(string ruleName)
		{
			if (s_CacheFilterRuleInstance.TryGetValue(ruleName, out IFilterRule instance))
				return instance;

			// 如果不存在创建类的实例
			if (s_CacheFilterRuleTypes.TryGetValue(ruleName, out Type type))
			{
				instance = (IFilterRule)Activator.CreateInstance(type);
				s_CacheFilterRuleInstance.Add(ruleName, instance);
				return instance;
			}
			else
			{
				throw new($"{nameof(IFilterRule)}类型无效：{ruleName}");
			}
		}

		// 公共参数编辑相关
		public static void ModifyPackageView(bool showPackageView)
		{
			Setting.ShowPackageView = showPackageView;
			IsDirty = true;
		}
		public static void ModifyAddressable(bool enableAddressable)
		{
			Setting.EnableAddressable = enableAddressable;
			IsDirty = true;
		}
		public static void ModifyUniqueBundleName(bool uniqueBundleName)
		{
			Setting.UniqueBundleName = uniqueBundleName;
			IsDirty = true;
		}
		public static void ModifyShowEditorAlias(bool showAlias)
		{
			Setting.ShowEditorAlias = showAlias;
			IsDirty = true;
		}

		// 资源包裹编辑相关
		public static AssetBundleCollectorPackage CreatePackage(string packageName)
		{
			AssetBundleCollectorPackage package = new()
			{
				PackageName = packageName
			};
			Setting.Packages.Add(package);
			IsDirty = true;
			return package;
		}
		public static void RemovePackage(AssetBundleCollectorPackage package)
		{
			if (Setting.Packages.Remove(package))
			{
				IsDirty = true;
			}
			else
			{
				Debug.LogWarning($"Failed remove package : {package.PackageName}");
			}
		}
		public static void ModifyPackage(AssetBundleCollectorPackage package)
		{
			if (package != null)
			{
				IsDirty = true;
			}
		}

		// 资源分组编辑相关
		public static AssetBundleCollectorGroup CreateGroup(AssetBundleCollectorPackage package, string groupName)
		{
			AssetBundleCollectorGroup group = new()
			{
				GroupName = groupName
			};
			package.Groups.Add(group);
			IsDirty = true;
			return group;
		}
		public static void RemoveGroup(AssetBundleCollectorPackage package, AssetBundleCollectorGroup group)
		{
			if (package.Groups.Remove(group))
			{
				IsDirty = true;
			}
			else
			{
				Debug.LogWarning($"Failed remove group : {group.GroupName}");
			}
		}
		public static void ModifyGroup(AssetBundleCollectorPackage package, AssetBundleCollectorGroup group)
		{
			if (package != null && group != null)
			{
				IsDirty = true;
			}
		}

		// 资源收集器编辑相关
		public static void CreateCollector(AssetBundleCollectorGroup group, AssetBundleCollector collector)
		{
			group.Collectors.Add(collector);
			IsDirty = true;
		}
		public static void RemoveCollector(AssetBundleCollectorGroup group, AssetBundleCollector collector)
		{
			if (group.Collectors.Remove(collector))
			{
				IsDirty = true;
			}
			else
			{
				Debug.LogWarning($"Failed remove collector : {collector.CollectPath}");
			}
		}
		public static void ModifyCollector(AssetBundleCollectorGroup group, AssetBundleCollector collector)
		{
			if (group != null && collector != null)
			{
				IsDirty = true;
			}
		}

		/// <summary>
		/// 获取所有的资源标签
		/// </summary>
		public static string GetPackageAllTags(string packageName)
		{
			List<string> allTags = Setting.GetPackageAllTags(packageName);
			return string.Join(";", allTags);
		}
	}
}