using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace Universe
{
	public class AssetBundleCollectorConfig
	{
		public const string CONFIG_VERSION = "2.3";

		public const string XML_VERSION = "Version";
		public const string XML_COMMON = "Common";
		public const string XML_ENABLE_ADDRESSABLE = "AutoAddressable";
		public const string XML_UNIQUE_BUNDLE_NAME = "UniqueBundleName";
		public const string XML_SHOW_PACKAGE_VIEW = "ShowPackageView";
		public const string XML_SHOW_EDITOR_ALIAS = "ShowEditorAlias";

		public const string XML_PACKAGE = "Package";
		public const string XML_PACKAGE_NAME = "PackageName";
		public const string XML_PACKAGE_DESC = "PackageDesc";

		public const string XML_GROUP = "Group";
		public const string XML_GROUP_NAME = "GroupName";
		public const string XML_GROUP_DESC = "GroupDesc";

		public const string XML_COLLECTOR = "Collector";
		public const string XML_COLLECT_PATH = "CollectPath";
		public const string XML_COLLECTOR_GUID = "CollectGUID";
		public const string XML_COLLECTOR_TYPE = "CollectType";
		public const string XML_ADDRESS_RULE = "AddressRule";
		public const string XML_PACK_RULE = "PackRule";
		public const string XML_FILTER_RULE = "FilterRule";
		public const string XML_USER_DATA = "UserData";
		public const string XML_ASSET_TAGS = "AssetTags";

		/// <summary>
		/// 导入XML配置表
		/// </summary>
		public static void ImportXmlConfig(string filePath)
		{
			if (File.Exists(filePath) == false)
				throw new FileNotFoundException(filePath);

			if (Path.GetExtension(filePath) != ".xml")
				throw new($"Only support xml : {filePath}");

			// 加载配置文件
			XmlDocument xmlDoc = new();
			xmlDoc.Load(filePath);
			XmlElement root = xmlDoc.DocumentElement;

			// 读取配置版本
			string configVersion = root.GetAttribute(XML_VERSION);
			if (configVersion != CONFIG_VERSION)
			{
				if (UpdateXmlConfig(xmlDoc) == false)
					throw new($"The config version update failed : {configVersion} -> {CONFIG_VERSION}");
				else
					Debug.Log($"The config version update succeed : {configVersion} -> {CONFIG_VERSION}");
			}

			// 读取公共配置
			bool enableAddressable = false;
			bool uniqueBundleName = false;
			bool showPackageView = false;
			bool showEditorAlias = false;
			XmlNodeList commonNodeList = root.GetElementsByTagName(XML_COMMON);
			if (commonNodeList.Count > 0)
			{
				XmlElement commonElement = commonNodeList[0] as XmlElement;
				if (commonElement.HasAttribute(XML_ENABLE_ADDRESSABLE) == false)
					throw new($"Not found attribute {XML_ENABLE_ADDRESSABLE} in {XML_COMMON}");
				if (commonElement.HasAttribute(XML_UNIQUE_BUNDLE_NAME) == false)
					throw new($"Not found attribute {XML_UNIQUE_BUNDLE_NAME} in {XML_COMMON}");
				if (commonElement.HasAttribute(XML_SHOW_PACKAGE_VIEW) == false)
					throw new($"Not found attribute {XML_SHOW_PACKAGE_VIEW} in {XML_COMMON}");
				if (commonElement.HasAttribute(XML_SHOW_EDITOR_ALIAS) == false)
					throw new($"Not found attribute {XML_SHOW_EDITOR_ALIAS} in {XML_COMMON}");

				enableAddressable = commonElement.GetAttribute(XML_ENABLE_ADDRESSABLE) == "True" ? true : false;
				uniqueBundleName = commonElement.GetAttribute(XML_UNIQUE_BUNDLE_NAME) == "True" ? true : false;
				showPackageView = commonElement.GetAttribute(XML_SHOW_PACKAGE_VIEW) == "True" ? true : false;
				showEditorAlias = commonElement.GetAttribute(XML_SHOW_EDITOR_ALIAS) == "True" ? true : false;
			}

			// 读取包裹配置
			List<AssetBundleCollectorPackage> packages = new();
			XmlNodeList packageNodeList = root.GetElementsByTagName(XML_PACKAGE);
			foreach (object packageNode in packageNodeList)
			{
				XmlElement packageElement = packageNode as XmlElement;
				if (packageElement.HasAttribute(XML_PACKAGE_NAME) == false)
					throw new($"Not found attribute {XML_PACKAGE_NAME} in {XML_PACKAGE}");
				if (packageElement.HasAttribute(XML_PACKAGE_DESC) == false)
					throw new($"Not found attribute {XML_PACKAGE_DESC} in {XML_PACKAGE}");

				AssetBundleCollectorPackage package = new()
				{
					PackageName = packageElement.GetAttribute(XML_PACKAGE_NAME),
					PackageDesc = packageElement.GetAttribute(XML_PACKAGE_DESC)
				};
				packages.Add(package);

				// 读取分组配置
				XmlNodeList groupNodeList = packageElement.GetElementsByTagName(XML_GROUP);
				foreach (object groupNode in groupNodeList)
				{
					XmlElement groupElement = groupNode as XmlElement;
					if (groupElement.HasAttribute(XML_GROUP_NAME) == false)
						throw new($"Not found attribute {XML_GROUP_NAME} in {XML_GROUP}");
					if (groupElement.HasAttribute(XML_GROUP_DESC) == false)
						throw new($"Not found attribute {XML_GROUP_DESC} in {XML_GROUP}");
					if (groupElement.HasAttribute(XML_ASSET_TAGS) == false)
						throw new($"Not found attribute {XML_ASSET_TAGS} in {XML_GROUP}");

					AssetBundleCollectorGroup group = new()
					{
						GroupName = groupElement.GetAttribute(XML_GROUP_NAME),
						GroupDesc = groupElement.GetAttribute(XML_GROUP_DESC),
						AssetTags = groupElement.GetAttribute(XML_ASSET_TAGS)
					};
					package.Groups.Add(group);

					// 读取收集器配置
					XmlNodeList collectorNodeList = groupElement.GetElementsByTagName(XML_COLLECTOR);
					foreach (object collectorNode in collectorNodeList)
					{
						XmlElement collectorElement = collectorNode as XmlElement;
						if (collectorElement.HasAttribute(XML_COLLECT_PATH) == false)
							throw new($"Not found attribute {XML_COLLECT_PATH} in {XML_COLLECTOR}");
						if (collectorElement.HasAttribute(XML_COLLECTOR_GUID) == false)
							throw new($"Not found attribute {XML_COLLECTOR_GUID} in {XML_COLLECTOR}");
						if (collectorElement.HasAttribute(XML_COLLECTOR_TYPE) == false)
							throw new($"Not found attribute {XML_COLLECTOR_TYPE} in {XML_COLLECTOR}");
						if (collectorElement.HasAttribute(XML_ADDRESS_RULE) == false)
							throw new($"Not found attribute {XML_ADDRESS_RULE} in {XML_COLLECTOR}");
						if (collectorElement.HasAttribute(XML_PACK_RULE) == false)
							throw new($"Not found attribute {XML_PACK_RULE} in {XML_COLLECTOR}");
						if (collectorElement.HasAttribute(XML_FILTER_RULE) == false)
							throw new($"Not found attribute {XML_FILTER_RULE} in {XML_COLLECTOR}");
						if (collectorElement.HasAttribute(XML_USER_DATA) == false)
							throw new($"Not found attribute {XML_USER_DATA} in {XML_COLLECTOR}");
						if (collectorElement.HasAttribute(XML_ASSET_TAGS) == false)
							throw new($"Not found attribute {XML_ASSET_TAGS} in {XML_COLLECTOR}");

						AssetBundleCollector collector = new()
						{
							CollectPath = collectorElement.GetAttribute(XML_COLLECT_PATH),
							CollectorGuid = collectorElement.GetAttribute(XML_COLLECTOR_GUID),
							CollectorType = EnumUtility.NameToEnum<ECollectorType>(collectorElement.GetAttribute(XML_COLLECTOR_TYPE)),
							AddressRuleName = collectorElement.GetAttribute(XML_ADDRESS_RULE),
							PackRuleName = collectorElement.GetAttribute(XML_PACK_RULE),
							FilterRuleName = collectorElement.GetAttribute(XML_FILTER_RULE),
							UserData = collectorElement.GetAttribute(XML_USER_DATA),
							AssetTags = collectorElement.GetAttribute(XML_ASSET_TAGS)
						};
						group.Collectors.Add(collector);
					}
				}
			}

			// 保存配置数据
			AssetBundleCollectorSettingData.ClearAll();
			AssetBundleCollectorSettingData.Setting.EnableAddressable = enableAddressable;
			AssetBundleCollectorSettingData.Setting.UniqueBundleName = uniqueBundleName;
			AssetBundleCollectorSettingData.Setting.ShowPackageView = showPackageView;
			AssetBundleCollectorSettingData.Setting.ShowEditorAlias = showEditorAlias;
			AssetBundleCollectorSettingData.Setting.Packages.AddRange(packages);
			AssetBundleCollectorSettingData.SaveFile();
			Debug.Log("导入配置完毕！");
		}

		/// <summary>
		/// 导出XML配置表
		/// </summary>
		public static void ExportXmlConfig(string savePath)
		{
			if (File.Exists(savePath))
				File.Delete(savePath);

			StringBuilder sb = new();
			sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			sb.AppendLine("<root>");
			sb.AppendLine("</root>");

			XmlDocument xmlDoc = new();
			xmlDoc.LoadXml(sb.ToString());
			XmlElement root = xmlDoc.DocumentElement;

			// 设置配置版本
			root.SetAttribute(XML_VERSION, CONFIG_VERSION);

			// 设置公共配置
			XmlElement commonElement = xmlDoc.CreateElement(XML_COMMON);
			commonElement.SetAttribute(XML_ENABLE_ADDRESSABLE, AssetBundleCollectorSettingData.Setting.EnableAddressable.ToString());
			commonElement.SetAttribute(XML_UNIQUE_BUNDLE_NAME, AssetBundleCollectorSettingData.Setting.UniqueBundleName.ToString());
			commonElement.SetAttribute(XML_SHOW_PACKAGE_VIEW, AssetBundleCollectorSettingData.Setting.ShowPackageView.ToString());
			commonElement.SetAttribute(XML_SHOW_EDITOR_ALIAS, AssetBundleCollectorSettingData.Setting.ShowEditorAlias.ToString());
			root.AppendChild(commonElement);

			// 设置Package配置
			foreach (AssetBundleCollectorPackage package in AssetBundleCollectorSettingData.Setting.Packages)
			{
				XmlElement packageElement = xmlDoc.CreateElement(XML_PACKAGE);
				packageElement.SetAttribute(XML_PACKAGE_NAME, package.PackageName);
				packageElement.SetAttribute(XML_PACKAGE_DESC, package.PackageDesc);
				root.AppendChild(packageElement);

				// 设置分组配置
				foreach (AssetBundleCollectorGroup group in package.Groups)
				{
					XmlElement groupElement = xmlDoc.CreateElement(XML_GROUP);
					groupElement.SetAttribute(XML_GROUP_NAME, group.GroupName);
					groupElement.SetAttribute(XML_GROUP_DESC, group.GroupDesc);
					groupElement.SetAttribute(XML_ASSET_TAGS, group.AssetTags);
					packageElement.AppendChild(groupElement);

					// 设置收集器配置
					foreach (AssetBundleCollector collector in group.Collectors)
					{
						XmlElement collectorElement = xmlDoc.CreateElement(XML_COLLECTOR);
						collectorElement.SetAttribute(XML_COLLECT_PATH, collector.CollectPath);
						collectorElement.SetAttribute(XML_COLLECTOR_GUID, collector.CollectorGuid);
						collectorElement.SetAttribute(XML_COLLECTOR_TYPE, collector.CollectorType.ToString());
						collectorElement.SetAttribute(XML_ADDRESS_RULE, collector.AddressRuleName);
						collectorElement.SetAttribute(XML_PACK_RULE, collector.PackRuleName);
						collectorElement.SetAttribute(XML_FILTER_RULE, collector.FilterRuleName);
						collectorElement.SetAttribute(XML_USER_DATA, collector.UserData);
						collectorElement.SetAttribute(XML_ASSET_TAGS, collector.AssetTags);
						groupElement.AppendChild(collectorElement);
					}
				}
			}

			// 生成配置文件
			xmlDoc.Save(savePath);
			Debug.Log("导出配置完毕！");
		}

		/// <summary>
		/// 升级XML配置表
		/// </summary>
		private static bool UpdateXmlConfig(XmlDocument xmlDoc)
		{
			XmlElement root = xmlDoc.DocumentElement;
			string configVersion = root.GetAttribute(XML_VERSION);
			if (configVersion == CONFIG_VERSION)
				return true;

			// 1.0 -> 2.0
			if (configVersion == "1.0")
			{
				// 添加公共元素属性
				XmlNodeList commonNodeList = root.GetElementsByTagName(XML_COMMON);
				if (commonNodeList.Count > 0)
				{
					XmlElement commonElement = commonNodeList[0] as XmlElement;
					if (commonElement.HasAttribute(XML_SHOW_PACKAGE_VIEW) == false)
						commonElement.SetAttribute(XML_SHOW_PACKAGE_VIEW, "False");
				}

				// 添加包裹元素
				XmlElement packageElement = xmlDoc.CreateElement(XML_PACKAGE);
				packageElement.SetAttribute(XML_PACKAGE_NAME, "DefaultPackage");
				packageElement.SetAttribute(XML_PACKAGE_DESC, string.Empty);
				root.AppendChild(packageElement);

				// 获取所有分组元素
				XmlNodeList groupNodeList = root.GetElementsByTagName(XML_GROUP);
				List<XmlElement> temper = new(groupNodeList.Count);
				foreach (object groupNode in groupNodeList)
				{
					XmlElement groupElement = groupNode as XmlElement;
					XmlNodeList collectorNodeList = groupElement.GetElementsByTagName(XML_COLLECTOR);
					foreach (object collectorNode in collectorNodeList)
					{
						XmlElement collectorElement = collectorNode as XmlElement;
						if (collectorElement.HasAttribute(XML_COLLECTOR_GUID) == false)
							collectorElement.SetAttribute(XML_COLLECTOR_GUID, string.Empty);
					}
					temper.Add(groupElement);
				}

				// 将分组元素转移至包裹元素下
				foreach (XmlElement groupElement in temper)
				{
					root.RemoveChild(groupElement);
					packageElement.AppendChild(groupElement);
				}

				// 更新版本
				root.SetAttribute(XML_VERSION, "2.0");
				return UpdateXmlConfig(xmlDoc);
			}

			// 2.0 -> 2.1
			if (configVersion == "2.0")
			{
				// 添加公共元素属性
				XmlNodeList commonNodeList = root.GetElementsByTagName(XML_COMMON);
				if (commonNodeList.Count > 0)
				{
					XmlElement commonElement = commonNodeList[0] as XmlElement;
					if (commonElement.HasAttribute(XML_UNIQUE_BUNDLE_NAME) == false)
						commonElement.SetAttribute(XML_UNIQUE_BUNDLE_NAME, "False");
				}

				// 更新版本
				root.SetAttribute(XML_VERSION, "2.1");
				return UpdateXmlConfig(xmlDoc);
			}

			// 2.1 -> 2.2
			if (configVersion == "2.1")
			{
				// 添加公共元素属性
				XmlNodeList commonNodeList = root.GetElementsByTagName(XML_COMMON);
				if (commonNodeList.Count > 0)
				{
					XmlElement commonElement = commonNodeList[0] as XmlElement;
					if (commonElement.HasAttribute(XML_SHOW_EDITOR_ALIAS) == false)
						commonElement.SetAttribute(XML_SHOW_EDITOR_ALIAS, "False");
				}

				// 更新版本
				root.SetAttribute(XML_VERSION, "2.2");
				return UpdateXmlConfig(xmlDoc);
			}

			// 2.2 -> 2.3
			if (configVersion == "2.2")
			{
				// 获取所有分组元素
				XmlNodeList groupNodeList = root.GetElementsByTagName(XML_GROUP);
				foreach (object groupNode in groupNodeList)
				{
					XmlElement groupElement = groupNode as XmlElement;
					XmlNodeList collectorNodeList = groupElement.GetElementsByTagName(XML_COLLECTOR);
					foreach (object collectorNode in collectorNodeList)
					{
						XmlElement collectorElement = collectorNode as XmlElement;
						if (collectorElement.HasAttribute(XML_USER_DATA) == false)
							collectorElement.SetAttribute(XML_USER_DATA, string.Empty);
					}
				}

				// 更新版本
				root.SetAttribute(XML_VERSION, "2.3");
				return UpdateXmlConfig(xmlDoc);
			}

			return false;
		}
	}
}