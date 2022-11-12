using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LapisPlayer
{
    public class StageData
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Asset { get; set; }
        public float Height { get; set; }
    }
    public class StageStore
    {
        static private StageStore _instance;
        static public StageStore Instance
        {
            get
            {
                if (_instance == null) _instance = new StageStore();
                return _instance;
            }
        }

        Dictionary<string, StageData> _stageDict = new();

        public StageStore()
        {
            string configPath = Path.Combine(System.Environment.CurrentDirectory, "Config", "stage.json");
            string configText = File.ReadAllText(configPath, Encoding.UTF8);
            JObject stageJson = JObject.Parse(configText);

            var stages = stageJson["stages"].Children().ToList();
            foreach (var stage in stages)
            {
                StageData data = new();
                data.Name = stage.Value<string>("name");
                data.ID = stage.Value<string>("id");
                data.Asset = stage.Value<string>("prefab");
                data.Height = stage.Value<float>("height");

                _stageDict.Add(data.ID, data);
            }
        }

        public StageData[] GetAllStages()
        {
            return _stageDict.Values.ToArray();
        }
        public StageData GetStage(string id)
        {
            return _stageDict[id];
        }
        public GameObject LoadStage(string key)
        {
            StageData stage = _stageDict[key];
            if (stage == null)
            {
                return null;
            }

            var prefab = AssetBundleLoader.Instance.LoadAsset<GameObject>(stage.Asset);
            return prefab;
        }
    }
}
