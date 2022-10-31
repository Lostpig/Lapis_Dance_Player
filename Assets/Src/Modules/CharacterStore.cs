using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System;

namespace LapisPlayer
{
    public class ActorSetting
    {
        public string Name { get; set; }
        public string Body { get; set; }
        public string[] Equips { get; set; }
    }
    public class CharacterSetting
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Label { get; set; }
    }
    public class CharactersStore
    {
        static private CharactersStore _instance;
        static public CharactersStore Instance
        {
            get
            {
                if (_instance == null) _instance = new CharactersStore();
                return _instance;
            }
        }

        Dictionary<string, CharacterSetting> _charaDict = new();
        Dictionary<string, ActorSetting> _actorDict = new();

        private CharactersStore()
        {
            string configPath = Path.Combine(System.Environment.CurrentDirectory, "Config", "character.json");
            string configText = File.ReadAllText(configPath, Encoding.UTF8);
            JObject charaJson = JObject.Parse(configText);

            var charas = charaJson["characters"].Children().ToList();
            foreach (var chara in charas)
            {
                CreateCharacter(chara);
            }
        }

        private void CreateCharacter(JToken charaToken)
        {
            CharacterSetting characterSetting = new();
            characterSetting.Name = charaToken.Value<string>("name");
            characterSetting.ShortName = charaToken.Value<string>("shortName");
            characterSetting.Label = charaToken.Value<string>("label");
            _charaDict.Add(characterSetting.Name, characterSetting);

            var actors = charaToken["actors"].Children().ToList();
            foreach (var actor in actors)
            {
                ActorSetting actorSetting = new();
                actorSetting.Name = actor.Value<string>("name");
                actorSetting.Body = actor.Value<string>("body");
                actorSetting.Equips = actor["eqiups"].ToObject<string[]>();

                string key = characterSetting.Name + "/" + actorSetting.Name;
                _actorDict.Add(key.ToLower(), actorSetting);
            }
        }

        public CharacterActor LoadActor(string characterName, string actorName)
        {
            var charaSetting = _charaDict[characterName];

            string key = characterName + "/" + actorName;
            var actorSetting = _actorDict[key.ToLower()];

            if (charaSetting == null || actorSetting == null)
            {
                throw new ArgumentException(nameof(actorName), $"Actor not found: {characterName}/{actorName}");
            }

            var actor = new CharacterActor(charaSetting, actorSetting);
            return actor;
        }
    }
}
