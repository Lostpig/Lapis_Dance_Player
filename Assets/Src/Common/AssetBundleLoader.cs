using System.Collections.Generic;
using UnityEngine;
using Oz.GameKit.Version;
using System.IO;
using System;
using Unity.VisualScripting;

namespace LapisPlayer
{
    public class AssetBundleLoader
    {
        static private AssetBundleLoader _instance;
        static public AssetBundleLoader Instance
        {
            get
            {
                if (_instance == null) _instance = new AssetBundleLoader();
                return _instance;
            }
        }

        const string resourcePrefix = "assets/products/resources";
        private PackageManifest _manifest;
        private Dictionary<string, AssetBundle> _loadedBundles;

        private AssetBundleLoader()
        {
            _manifest = ManifestReader.Read(ConfigManager.Instance.Manifest);
            _loadedBundles = new();
            ReStoreLoadedBundles();
        }

        private void ReStoreLoadedBundles()
        {
            var loaded = AssetBundle.GetAllLoadedAssetBundles();
            foreach (var bundle in loaded)
            {
                _loadedBundles.Add(bundle.name, bundle);
            }
        }

        private void LoadDependencies(string packageName)
        {
            int index = _manifest.PackageName2Index[packageName];
            var pkgInfo = _manifest.PackageInfos[index];

            if (pkgInfo is AssetBundleInfo abInfo)
            {
                foreach (int depIdx in abInfo.Dependencies)
                {
                    var dep = _manifest.PackageInfos[depIdx];
                    if (dep is AssetBundleInfo)
                    {
                        LoadAssetbundle(dep.Name);
                    }
                }
            }
        }
        public AssetBundle LoadAssetbundle(string packageName)
        {
            string bundleName = packageName + ".bdl";
            bool loaded = _loadedBundles.TryGetValue(bundleName, out var bundle);
            if (loaded && !bundle.IsDestroyed())
            {
                return bundle;
            }

            string fullPath = ConfigManager.Instance.AssetBundles + "/" + bundleName;
            bundle = AssetBundle.LoadFromFile(fullPath);
            _loadedBundles.Add(bundleName, bundle);
            LoadDependencies(packageName);

            return bundle;
        }

        public T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            int idx = assetPath.LastIndexOf("/");
            string bundleName = assetPath.Substring(0, idx);
            string assetName = assetPath.Substring(idx + 1);

            string packageName = resourcePrefix + "/" + bundleName.ToLower();
            var bundle = LoadAssetbundle(packageName);
            T asset = bundle.LoadAsset<T>(assetName.ToLower());
            return asset;
        }
    
        public void Reset ()
        {
            AssetBundle.UnloadAllAssetBundles(true);
            _loadedBundles.Clear();
        }
    }
}
