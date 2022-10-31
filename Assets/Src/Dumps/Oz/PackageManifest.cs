using System;
using System.Collections.Generic;
using Oz.GameFramework.Runtime;

namespace Oz.GameKit.Version
{
    [Serializable]
    public class PackageManifest : IPackageManifest
    {
        // Fields
        private string m_Version; // 0x20
        private Dictionary<string, int> m_PackageName2Index; // 0x28
        private List<IPackageInfo> m_PackageInfos; // 0x30
        private Queue<int> m_EmptyIndexQueue; // 0x38

        public string AppVersion { get; set; }
        public ulong ResVersion { get; set; }
        public string Version { get; set; }

        public Dictionary<string, int> PackageName2Index
        {
            get => m_PackageName2Index;
            set => m_PackageName2Index = value;
        }
        public List<IPackageInfo> PackageInfos
        {
            get => m_PackageInfos;
            set => m_PackageInfos = value;
        }
        public Queue<int> EmptyIndexQueue
        {
            get => m_EmptyIndexQueue;
            set => m_EmptyIndexQueue = value;
        }
    }
}
