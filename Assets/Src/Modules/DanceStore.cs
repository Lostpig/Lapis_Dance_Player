using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LapisPlayer
{
    public class DanceSetting
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string Music { get; set; }
    }
    public class DanceStore
    {
        static private DanceStore _instance;
        static public DanceStore Instance
        {
            get
            {
                if (_instance == null) _instance = new DanceStore();
                return _instance;
            }
        }

        Dictionary<string, DanceSetting> _danceDict = new();

        private DanceStore()
        {
            string configPath = Path.Combine(System.Environment.CurrentDirectory, "Config", "dance.json");
            string configText = File.ReadAllText(configPath, Encoding.UTF8);
            JObject charaJson = JObject.Parse(configText);

            var dances = charaJson["dances"].Children().ToList();
            foreach (var dance in dances)
            {
                DanceSetting setting = new();
                setting.Name = dance.Value<string>("name");
                setting.ID = dance.Value<string>("id");
                setting.Music = dance.Value<string>("music");

                _danceDict.Add(setting.ID, setting);
            }
        }

        public DanceSetting GetDance(string id)
        {
            return _danceDict[id];
        }
        public DanceSetting[] GetAllDance()
        {
            return _danceDict.Values.ToArray();
        }
    }
}
