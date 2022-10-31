
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Oz.GameFramework.Runtime;
using Oz.GameKit.Version;
using OzLoader;

namespace LapisPlayer
{
    public class ManifestReader
    {
        public static PackageManifest Read(string file)
        {
            string json = File.ReadAllText(file, Encoding.UTF8);

            JObject manifestJson = JObject.Parse(json);
            
            PackageManifest manifest = new();
            manifest.AppVersion = manifestJson.Value<string>("AppVersion_k__BackingField");
            manifest.ResVersion = manifestJson.Value<ulong>("ResVersion_k__BackingField");
            manifest.Version = manifestJson.Value<string>("m_Version");

            manifest.PackageName2Index = manifestJson["m_PackageName2Index"].ToObject<Dictionary<string, int>>();

            var pkgs = manifestJson["m_PackageInfos"].Children().ToList();
            List<IPackageInfo> pkgList = new();
            foreach (var pkg in pkgs)
            {
                if (pkg.Type == JTokenType.Null)
                {
                    pkgList.Add(null);
                    continue;
                }

                var type = pkg.Value<string>("_class_name");
                if (type == "Oz_GameKit_Version_AssetBundleInfo")
                {
                    AssetBundleInfo abi = new();
                    abi.ID = pkg.Value<int>("ID_k__BackingField");
                    abi.Name = pkg.Value<string>("Name_k__BackingField");
                    abi.Size = pkg.Value<ulong>("Size_k__BackingField");
                    abi.ShortHash = pkg.Value<string>("ShortHash_k__BackingField");
                    abi.Flags = pkg.Value<int>("Flags_k__BackingField");
                    abi.Version = pkg.Value<string>("Version_k__BackingField");
                    abi.EncryptKey = pkg.Value<int>("EncryptKey_k__BackingField");
                    abi.IsActivityAsset = pkg.Value<bool>("IsActivityAsset_k__BackingField");
                    abi.Hash = pkg.Value<string>("Hash_k__BackingField");

                    abi.Dependencies = pkg["Dependencies_k__BackingField"].ToObject<int[]>();

                    pkgList.Add(abi);
                }
                else if (type == "Oz_GameKit_Version_SoundBankInfo")
                {
                    SoundBankInfo sbi = new();
                    sbi.ID = pkg.Value<int>("ID_k__BackingField");
                    sbi.Name = pkg.Value<string>("Name_k__BackingField");
                    sbi.Size = pkg.Value<ulong>("Size_k__BackingField");
                    sbi.ShortHash = pkg.Value<string>("ShortHash_k__BackingField");
                    sbi.Flags = pkg.Value<int>("Flags_k__BackingField");
                    sbi.Version = pkg.Value<string>("Version_k__BackingField");
                    sbi.Hash = pkg.Value<string>("Hash_k__BackingField");

                    pkgList.Add(sbi);
                }
                else if (type == "Oz_GameKit_Version_MasterDataInfo")
                {
                    MasterDataInfo mdi = new();
                    mdi.ID = pkg.Value<int>("ID_k__BackingField");
                    mdi.Name = pkg.Value<string>("Name_k__BackingField");
                    mdi.Size = pkg.Value<ulong>("Size_k__BackingField");
                    mdi.ShortHash = pkg.Value<string>("ShortHash_k__BackingField");
                    mdi.Flags = pkg.Value<int>("Flags_k__BackingField");
                    mdi.Version = pkg.Value<string>("Version_k__BackingField");
                    mdi.Hash = pkg.Value<string>("Hash_k__BackingField");
                    mdi.Key1 = pkg.Value<uint>("Key1_k__BackingField");
                    mdi.Key2 = pkg.Value<uint>("Key2_k__BackingField");
                    mdi.Key3 = pkg.Value<uint>("Key3_k__BackingField");

                    pkgList.Add(mdi);
                }
                else
                {
                    throw new InvalidDataException($"Unknown package type: {type}");
                }
            }
            manifest.PackageInfos = pkgList;

            var emptyIndexQueueArr = manifestJson["m_EmptyIndexQueue"]["array"].ToObject<int[]>();
            Queue<int> emptyIndexQueue = new(emptyIndexQueueArr);
            manifest.EmptyIndexQueue = emptyIndexQueue;

            return manifest;
        }
    }
}
