﻿using System.IO;

namespace Universe
{
	[DisplayName("定位地址: 文件名")]
	public class AddressByFileName : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			return Path.GetFileNameWithoutExtension(data.AssetPath);
		}
	}

	[DisplayName("定位地址: 分组名_文件名")]
	public class AddressByGroupAndFileName : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
			return $"{data.GroupName}_{fileName}";
		}
	}

	[DisplayName("定位地址: 文件夹名_文件名")]
	public class AddressByFolderAndFileName : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
			string collectorName = Path.GetFileNameWithoutExtension(data.CollectPath);
			return $"{collectorName}_{fileName}";
		}
	}
}