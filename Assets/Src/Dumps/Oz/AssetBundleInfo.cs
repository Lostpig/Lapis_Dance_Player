using System;
using Oz.GameFramework.Runtime;

namespace Oz.GameKit.Version
{
    [Serializable]
    public class AssetBundleInfo : IPackageInfoKc, IPackageInfo, IAssetBundleInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ulong Size { get; set; }
        public string ShortHash { get; set; }
        public int Flags { get; set; }
        public string Version { get; set; }
        public int[] Dependencies { get; set; }
        public int EncryptKey { get; set; }
        public bool IsActivityAsset { get; set; }
        public string Hash { get; set; }
        private string ActivityPathName { get; }
    }
}
