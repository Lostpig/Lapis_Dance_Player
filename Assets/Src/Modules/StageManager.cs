using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VLB;

namespace LapisPlayer
{
    public class StageData
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        public string Asset { get; private set; }
        public float DefaultHeight { get; private set; }

        public StageData(string id, string name, string asset, float defaultHeight = 0)
        {
            ID = id;
            Name = name;
            Asset = asset;
            DefaultHeight = defaultHeight;
        }
    }
    public class StageManager
    {
        // 一共就7个舞台，直接写死了
        static readonly StageData[] stages = new StageData[]
        {
            new StageData("BG301", "supernova 舞台", "SceneAssets/Scene_stage/BG301/Prefab/BG301", 0.01f),
            new StageData("BG302", "IV KROLE 舞台", "SceneAssets/Scene_stage/BG302/Prefab/BG302_Mix", 0.015f),
            new StageData("BG2004", "LiGHTs 舞台", "SceneAssets/Scene_stage/BG2004/Prefab/BG2004", 0.025f),
            new StageData("BG304", "この花は乙女 舞台", "SceneAssets/Scene_stage/BG304/Prefab/BG304", 0.02f),
            new StageData("BG305", "Sugar Pockets 舞台", "SceneAssets/Scene_stage/BG305/Prefab/BG305", 0.01f),
            new StageData("BG306", "Sadistic★Candy 舞台", "SceneAssets/Scene_stage/BG306/Prefab/BG306", 0.02f),
            new StageData("BG307", "Ray 舞台", "SceneAssets/Scene_stage/BG307/Prefab/BG307", 0),
            new StageData("BG303", "废案(BG303)", "SceneAssets/Scene_stage/BG303/Prefab/BG303"),

            new StageData("BG105", "魔法研究室", "SceneAssets/Scene_main/BG105/Prefab/BG105"),
            new StageData("BG106", "餐厅", "SceneAssets/Scene_main/BG106/Prefab/BG106"),
            new StageData("BG107", "卧室", "SceneAssets/Scene_main/BG107/Prefab/BG107"),
            new StageData("BG108", "游戏室(未实装)", "SceneAssets/Scene_main/BG108/Prefab/BG108"),
        };
        public static StageData[] GetAllStages()
        {
            return stages;
        }

        GameObject _current;
        Dictionary<string, StageData> _stageDict = new();

        public StageManager()
        {
            foreach (StageData stage in stages)
            {
                _stageDict.Add(stage.ID, stage);
            }
        }

        public StageData[] GetStages()
        {
            return stages;
        }
        public StageData GetStage(string key)
        {
            return _stageDict[key];
        }
        public void LoadStage(string key)
        {
            DestroyScene();

            StageData stage = _stageDict[key];
            if (stage == null)
            {
                return;
            }

            var prefab = AssetBundleLoader.Instance.LoadAsset<GameObject>(stage.Asset);
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
