using System;

namespace Oz.GameFramework.Runtime
{
    public interface IPackageManifest
    {
        public abstract string AppVersion { get; }
        public abstract string Version { get; }
        public abstract ulong ResVersion { get; }
    }

    public interface IPackageInfo
    {
        public abstract string Name { get; }
        public abstract ulong Size { get; }
        public abstract string Hash { get; }
        public abstract int Flags { get; }
    }

    public interface IPackageInfoKc : IPackageInfo
    {
        public abstract string Version { get; set; }
        public abstract int ID { get; set; }
        public abstract string ShortHash { get; set; }
    }

    public interface IAssetBundleInfo : IPackageInfo
    {
        public abstract int[] Dependencies { get; }
        public abstract int EncryptKey { get; }
    }

    public interface ISoundBankInfo : IPackageInfo { }
}
