using LapisPlayer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LapisPlayer
{
    public class StageManager
    {
        // 一共就7个舞台，直接写死了
        static readonly string[] assets = new string[]
        {
            "SceneAssets/Scene_stage/BG301/Prefab/BG301",
            "SceneAssets/Scene_stage/BG302/Prefab/BG302",
            "SceneAssets/Scene_stage/BG303/Prefab/BG303",
            "SceneAssets/Scene_stage/BG304/Prefab/BG304",
            "SceneAssets/Scene_stage/BG305/Prefab/BG305",
            "SceneAssets/Scene_stage/BG306/Prefab/BG306",
            "SceneAssets/Scene_stage/BG307/Prefab/BG307",
        };
        public static string[] GetAllStages()
        {
            return assets.Select(asset => asset.Substring(asset.LastIndexOf("/"))).ToArray();
        }

        GameObject _current;
        Dictionary<string, string> stages = new();

        public StageManager ()
        {
            foreach(string asset in assets)
            {
                string name = asset.Substring(asset.LastIndexOf("/"));
                stages.Add(name, asset);
            }
        }

        public string[] GetStages()
        {
            return stages.Keys.ToArray();
        }
        public void LoadStage(string key)
        {
            DestroyScene();

            string asset = stages[key];
            if (string.IsNullOrEmpty(asset))
            {
                return;
            }

            var prefab = AssetBundleLoader.Instance.LoadAsset<GameObject>(asset);
            _current = GameObject.Instantiate(prefab);
        }
        public void DestroyScene()
        {
            if (_current != null)
            {
                GameObject.Destroy(_current);
                _current = null;
            }
        }
    }
}
