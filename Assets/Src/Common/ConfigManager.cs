using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;

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
        string _soundExt;
        int _physicalType;
        int _qualityLevel;
        bool _fullscreen;

        public string AssetBundles => _assetBundles;
        public string SoundBanks => _soundBanks;
        public string Manifest => _manifest;
        public string SoundExtension => _soundExt;
        public int PhysicalType => _physicalType;
        public int QualityLevel => _qualityLevel;
        public bool FullScreen => _fullscreen;

        private ConfigManager()
        {
            Initialize();
        }
        private void Initialize()
        {
            string configPath = Path.Combine(System.Environment.CurrentDirectory, "Config", "config.json");
            string configText = File.ReadAllText(configPath, Encoding.UTF8);
            JObject configJson = JObject.Parse(configText);

            _assetBundles = GetPath(configJson.Value<string>("assetbundles"));
            _soundBanks = GetPath(configJson.Value<string>("soundbanks"));
            _manifest = GetPath(configJson.Value<string>("manifest"));
            _soundExt = configJson.Value<string>("soundExt");
            _physicalType = configJson.Value<int>("physicalType");
            _qualityLevel = configJson.Value<int>("qualityLevel");
            _fullscreen = configJson.Value<bool>("fullscreen");
        }

        private string GetPath (string p)
        {
            if (p.StartsWith("."))
            {
                string combPath = Path.Combine(System.Environment.CurrentDirectory, "Config", p);
                return Path.GetFullPath(combPath);
            }

            return p;
        }
    }
}
