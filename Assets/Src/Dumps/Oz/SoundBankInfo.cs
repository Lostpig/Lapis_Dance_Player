using System;
using Oz.GameFramework.Runtime;

namespace Oz.GameKit.Version
{
    [Serializable]
    public class SoundBankInfo : IPackageInfoKc, IPackageInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ulong Size { get; set; }
        public string ShortHash { get; set; }
        public int Flags { get; set; }
        public string Version { get; set; }
        public string Hash { get; set; }
    }
}
