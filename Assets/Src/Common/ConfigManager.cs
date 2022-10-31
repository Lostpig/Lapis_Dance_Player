using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace LapisPlayer
{
    public class ConfigManager
    {
        static private ConfigManager _instance;
        static public ConfigManager Instance
        {
            get
            {
                if (_instance == null) _instance = new ConfigManager();
                return _instance;
            }
        }

        string _assetBundles;
        string _soundBanks;
        string _manifest;

        public string AssetBundles => _assetBundles;
        public string SoundBanks => _soundBanks;
        public string Manifest => _manifest;

        private ConfigManager()
        {
            Initialize();
        }
        private void Initialize()
        {
            string configPath = Path.Combine(System.Environment.CurrentDirectory, "Config", "config.json");
            string configText = File.ReadAllText(configPath, Encoding.UTF8);
            var configDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(configText);

            _assetBundles = configDict["assetbundles"];
            _soundBanks = configDict["soundbanks"];
            _manifest = configDict["manifest"];
        }
    }
}
