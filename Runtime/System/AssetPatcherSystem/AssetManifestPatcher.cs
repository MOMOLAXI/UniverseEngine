using System;
using System.IO;
using UnityEngine;

namespace Universe
{
    public static class AssetManifestPatcher
    {

    #if UNITY_EDITOR
        /// <summary>
        /// 序列化（JSON文件）
        /// </summary>
        public static void SerializeToJson(string savePath, PatchManifest manifest)
        {
            string json = JsonUtility.ToJson(manifest, true);
            FileUtility.CreateFile(savePath, json);
        }

        /// <summary>
        /// 序列化（二进制文件）
        /// </summary>
        public static void SerializeToBinary(string savePath, PatchManifest patchManifest)
        {
            using (FileStream fs = new(savePath, FileMode.Create))
            {
                // 创建缓存器
                BufferWriter buffer = new(UniverseConstant.PATCH_MANIFEST_FILE_MAX_SIZE);

                // 写入文件标记
                buffer.WriteUInt32(UniverseConstant.PATCH_MANIFEST_FILE_SIGN);

                // 写入文件版本
                buffer.WriteUTF8(patchManifest.FileVersion);

                // 写入文件头信息
                buffer.WriteBool(patchManifest.EnableAddressable);
                buffer.WriteInt32(patchManifest.OutputNameStyle);
                buffer.WriteUTF8(patchManifest.PackageName);
                buffer.WriteUTF8(patchManifest.PackageVersion);

                // 写入资源列表
                buffer.WriteInt32(patchManifest.AssetList.Count);
                for (int i = 0; i < patchManifest.AssetList.Count; i++)
                {
                    PatchAsset patchAsset = patchManifest.AssetList[i];
                    buffer.WriteUTF8(patchAsset.Address);
                    buffer.WriteUTF8(patchAsset.AssetPath);
                    buffer.WriteUTF8Array(patchAsset.AssetTags);
                    buffer.WriteInt32(patchAsset.BundleID);
                    buffer.WriteInt32Array(patchAsset.DependIDs);
                }

                // 写入资源包列表
                buffer.WriteInt32(patchManifest.BundleList.Count);
                for (int i = 0; i < patchManifest.BundleList.Count; i++)
                {
                    PatchBundle patchBundle = patchManifest.BundleList[i];
                    buffer.WriteUTF8(patchBundle.BundleName);
                    buffer.WriteUTF8(patchBundle.FileHash);
                    buffer.WriteUTF8(patchBundle.FileCRC);
                    buffer.WriteInt64(patchBundle.FileSize);
                    buffer.WriteBool(patchBundle.IsRawFile);
                    buffer.WriteByte(patchBundle.LoadMethod);
                    buffer.WriteUTF8Array(patchBundle.Tags);
                    buffer.WriteInt32Array(patchBundle.ReferenceIDs);
                }

                // 写入文件流
                buffer.WriteToStream(fs);
                fs.Flush();
            }
        }

        /// <summary>
        /// 反序列化（二进制文件）
        /// </summary>
        public static PatchManifest DeserializeFromBinary(byte[] binaryData)
        {
            // 创建缓存器
            BufferReader buffer = new(binaryData);

            // 读取文件标记
            uint fileSign = buffer.ReadUInt32();
            if (fileSign != UniverseConstant.PATCH_MANIFEST_FILE_SIGN)
            {
                throw new("Invalid manifest file !");
            }

            // 读取文件版本
            string fileVersion = buffer.ReadUTF8();
            if (fileVersion != UniverseConstant.PATCH_MANIFEST_FILE_VERSION)
            {
                throw new($"The manifest file version are not compatible : {fileVersion} != {UniverseConstant.PATCH_MANIFEST_FILE_VERSION}");
            }

            PatchManifest manifest = new();
            {
                // 读取文件头信息
                manifest.FileVersion = fileVersion;
                manifest.EnableAddressable = buffer.ReadBool();
                manifest.OutputNameStyle = buffer.ReadInt32();
                manifest.PackageName = buffer.ReadUTF8();
                manifest.PackageVersion = buffer.ReadUTF8();

                // 读取资源列表
                int patchAssetCount = buffer.ReadInt32();
                manifest.AssetList = new(patchAssetCount);
                for (int i = 0; i < patchAssetCount; i++)
                {
                    PatchAsset patchAsset = new()
                    {
                        Address = buffer.ReadUTF8(),
                        AssetPath = buffer.ReadUTF8(),
                        AssetTags = buffer.ReadUTF8Array(),
                        BundleID = buffer.ReadInt32(),
                        DependIDs = buffer.ReadInt32Array()
                    };
                    manifest.AssetList.Add(patchAsset);
                }

                // 读取资源包列表
                int patchBundleCount = buffer.ReadInt32();
                manifest.BundleList = new(patchBundleCount);
                for (int i = 0; i < patchBundleCount; i++)
                {
                    PatchBundle patchBundle = new()
                    {
                        BundleName = buffer.ReadUTF8(),
                        FileHash = buffer.ReadUTF8(),
                        FileCRC = buffer.ReadUTF8(),
                        FileSize = buffer.ReadInt64(),
                        IsRawFile = buffer.ReadBool(),
                        LoadMethod = buffer.ReadByte(),
                        Tags = buffer.ReadUTF8Array(),
                        ReferenceIDs = buffer.ReadInt32Array()
                    };
                    manifest.BundleList.Add(patchBundle);
                }
            }

            // BundleDic
            manifest.BundleDic = new(manifest.BundleList.Count);
            foreach (PatchBundle patchBundle in manifest.BundleList)
            {
                patchBundle.ParseBundle(manifest.PackageName, manifest.OutputNameStyle);
                manifest.BundleDic.Add(patchBundle.BundleName, patchBundle);
            }

            // AssetDic
            manifest.AssetDic = new(manifest.AssetList.Count);
            foreach (PatchAsset patchAsset in manifest.AssetList)
            {
                // 注意：我们不允许原始路径存在重名
                string assetPath = patchAsset.AssetPath;
                if (manifest.AssetDic.ContainsKey(assetPath))
                    throw new($"AssetPath have existed : {assetPath}");
                manifest.AssetDic.Add(assetPath, patchAsset);
            }

            return manifest;
        }
    #endif

        public static string GetRemoteBundleFileExtension(string bundleName)
        {
            string fileExtension = Path.GetExtension(bundleName);
            return fileExtension;
        }
        public static string GetRemoteBundleFileName(int nameStyle, string bundleName, string fileExtension, string fileHash)
        {
            switch (nameStyle)
            {
                //HashName
                case 1:
                {
                    return StringUtility.Format("{0}{1}", fileHash, fileExtension);
                }
                //BundleName_HashName
                case 4:
                {
                    string fileName = bundleName.Remove(bundleName.LastIndexOf('.'));
                    return StringUtility.Format("{0}_{1}{2}", fileName, fileHash, fileExtension);
                }
                default: throw new NotImplementedException($"Invalid name style : {nameStyle}");
            }
        }

        /// <summary>
        /// 获取解压BundleInfo
        /// </summary>
        public static BundleInfo GetUnpackInfo(PatchBundle patchBundle)
        {
            // 注意：我们把流加载路径指定为远端下载地址
            string streamingPath = AssetPath.ConvertToWWWPath(patchBundle.StreamingFilePath);
            BundleInfo bundleInfo = new(patchBundle, BundleInfo.ELoadMode.LoadFromStreaming, streamingPath, streamingPath);
            return bundleInfo;
        }
    }
}