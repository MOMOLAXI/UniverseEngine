namespace Universe
{
	internal class DeserializeManifestOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			DeserializeFileHeader,
			PrepareAssetList,
			DeserializeAssetList,
			PrepareBundleList,
			DeserializeBundleList,
			Done,
		}
	
		private readonly BufferReader m_Buffer;
		private int m_PatchAssetCount;
		private int m_PatchBundleCount;
		private int m_ProgressTotalValue;
		private ESteps m_Steps = ESteps.None;

		/// <summary>
		/// 解析的清单实例
		/// </summary>
		public PatchManifest Manifest { private set; get; }

		public DeserializeManifestOperation(byte[] binaryData)
		{
			m_Buffer = new(binaryData);
		}
		internal override void Start()
		{
			m_Steps = ESteps.DeserializeFileHeader;
		}
		internal override void Update()
		{
			if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
				return;

			try
			{
				if (m_Steps == ESteps.DeserializeFileHeader)
				{
					if (m_Buffer.IsValid == false)
					{
						m_Steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = "Buffer is invalid !";
						return;
					}

					// 读取文件标记
					uint fileSign = m_Buffer.ReadUInt32();
					if (fileSign != UniverseConstant.PATCH_MANIFEST_FILE_SIGN)
					{
						m_Steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = "The manifest file format is invalid !";
						return;
					}

					// 读取文件版本
					string fileVersion = m_Buffer.ReadUTF8();
					if (fileVersion != UniverseConstant.PATCH_MANIFEST_FILE_VERSION)
					{
						m_Steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"The manifest file version are not compatible : {fileVersion} != {UniverseConstant.PATCH_MANIFEST_FILE_VERSION}";
						return;
					}

					// 读取文件头信息
					Manifest = new()
					{
						FileVersion = fileVersion,
						EnableAddressable = m_Buffer.ReadBool(),
						OutputNameStyle = m_Buffer.ReadInt32(),
						PackageName = m_Buffer.ReadUTF8(),
						PackageVersion = m_Buffer.ReadUTF8()
					};

					m_Steps = ESteps.PrepareAssetList;
				}

				if (m_Steps == ESteps.PrepareAssetList)
				{
					m_PatchAssetCount = m_Buffer.ReadInt32();
					Manifest.AssetList = new(m_PatchAssetCount);
					Manifest.AssetDic = new(m_PatchAssetCount);
					m_ProgressTotalValue = m_PatchAssetCount;
					m_Steps = ESteps.DeserializeAssetList;
				}
				if (m_Steps == ESteps.DeserializeAssetList)
				{
					while (m_PatchAssetCount > 0)
					{
						PatchAsset patchAsset = new()
						{
							Address = m_Buffer.ReadUTF8(),
							AssetPath = m_Buffer.ReadUTF8(),
							AssetTags = m_Buffer.ReadUTF8Array(),
							BundleID = m_Buffer.ReadInt32(),
							DependIDs = m_Buffer.ReadInt32Array()
						};
						Manifest.AssetList.Add(patchAsset);

						// 注意：我们不允许原始路径存在重名
						string assetPath = patchAsset.AssetPath;
						if (Manifest.AssetDic.ContainsKey(assetPath))
							throw new($"AssetPath have existed : {assetPath}");
						Manifest.AssetDic.Add(assetPath, patchAsset);

						m_PatchAssetCount--;
						Progress = 1f - m_PatchAssetCount / m_ProgressTotalValue;
						if (Engine.IsOperationBusy)
							break;
					}

					if (m_PatchAssetCount <= 0)
					{
						m_Steps = ESteps.PrepareBundleList;
					}
				}

				if (m_Steps == ESteps.PrepareBundleList)
				{
					m_PatchBundleCount = m_Buffer.ReadInt32();
					Manifest.BundleList = new(m_PatchBundleCount);
					Manifest.BundleDic = new(m_PatchBundleCount);
					m_ProgressTotalValue = m_PatchBundleCount;
					m_Steps = ESteps.DeserializeBundleList;
				}
				if (m_Steps == ESteps.DeserializeBundleList)
				{
					while (m_PatchBundleCount > 0)
					{
						PatchBundle patchBundle = new()
						{
							BundleName = m_Buffer.ReadUTF8(),
							FileHash = m_Buffer.ReadUTF8(),
							FileCRC = m_Buffer.ReadUTF8(),
							FileSize = m_Buffer.ReadInt64(),
							IsRawFile = m_Buffer.ReadBool(),
							LoadMethod = m_Buffer.ReadByte(),
							Tags = m_Buffer.ReadUTF8Array(),
							ReferenceIDs = m_Buffer.ReadInt32Array()
						};
						Manifest.BundleList.Add(patchBundle);

						patchBundle.ParseBundle(Manifest.PackageName, Manifest.OutputNameStyle);
						Manifest.BundleDic.Add(patchBundle.BundleName, patchBundle);

						m_PatchBundleCount--;
						Progress = 1f - m_PatchBundleCount / m_ProgressTotalValue;
						if (Engine.IsOperationBusy)
							break;
					}

					if (m_PatchBundleCount <= 0)
					{
						m_Steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
					}
				}
			}
			catch (System.Exception e)
			{
				Manifest = null;
				m_Steps = ESteps.Done;
				Status = EOperationStatus.Failed;
				Error = e.Message;
			}
		}
	}
}