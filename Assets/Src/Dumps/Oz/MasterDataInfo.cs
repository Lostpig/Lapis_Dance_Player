using Oz.GameFramework.Runtime;
using System;

namespace OzLoader
{
    [Serializable]
    public class MasterDataInfo : IPackageInfoKc, IPackageInfo
    {
        public static string MASTER_FILE_PATH; // 0x0
        public static string MASTER_LANG_PATH { get; }

        // Properties
        public int ID { get; set; }
        public string Name { get; set; }
        public ulong Size { get; set; }
        public string ShortHash { get; set; }
        public int Flags { get; set; }
        public string Version { get; set; }
        public uint Key1 { get; set; }
        public uint Key2 { get; set; }
        public uint Key3 { get; set; }
        public int[] KeyList { get; }
        public string Hash { get; set; }

    }
}
