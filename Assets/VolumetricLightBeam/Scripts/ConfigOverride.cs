using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VLB
{
    [HelpURL(Consts.HelpUrlConfig)]
    public class ConfigOverride : Config // useless override, only useful for backward compatibility
    {
        public const string kAssetName = "VLBConfigOverride";

#if UNITY_EDITOR
        public static void CreateAsset(Object obj, string folderParent, string folderResources, string assetName)
        {
            if (!AssetDatabase.IsValidFolder(string.Format("{0}/{1}", folderParent, folderResources)))
                AssetDatabase.CreateFolder(folderParent, folderResources);

            AssetDatabase.CreateAsset(obj, string.Format("{0}/{1}/{2}", folderParent, folderResources, assetName));
            AssetDatabase.SaveAssets();
        }

        public static ConfigOverride CreateInstanceOverride()
        {
            var asset = CreateInstance<ConfigOverride>();
            CreateAsset(asset, "Assets", "Resources", kAssetName + ".asset");
            return asset;
        }
#endif
    }
}
