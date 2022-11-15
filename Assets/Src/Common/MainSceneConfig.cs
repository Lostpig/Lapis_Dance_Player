using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using System.Linq;

namespace LapisPlayer
{
    public class MainSceneConfig
    {
        public string Stage { get; private set; }
        public string P1 { get; private set; }
        public string P2 { get; private set; }
        public string P3 { get; private set; }

        public string[] Bgms { get; private set; }

        static private MainSceneConfig _instance;
        static public MainSceneConfig Instance
        {
            get
            {
                if (_instance == null) _instance = new MainSceneConfig();
                return _instance;
            }
        }

        private MainSceneConfig()
        {
            string configPath = Path.Combine(System.Environment.CurrentDirectory, "Config", "mainscene.json");
            string configText = File.ReadAllText(configPath, Encoding.UTF8);
            JObject configJson = JObject.Parse(configText);

            Stage = configJson.Value<string>("stage");
            P1 = configJson.Value<string>("p1");
            P2 = configJson.Value<string>("p2");
            P3 = configJson.Value<string>("p3");

            Bgms = configJson["bgms"].ToObject<string[]>();
        }
    }
}
