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

        public ActorSetting[] actors { get; set; }
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
        Dictionary<string, ActorSetting> _commonActorDict = new();

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

            var commonActors = charaJson["common_actors"].Children().ToList();
            foreach (var actor in commonActors)
            {
                CreateCommonActor(actor);
            }
        }

        private void CreateCharacter(JToken charaToken)
        {
            CharacterSetting characterSetting = new();
            characterSetting.Name = charaToken.Value<string>("name");
            characterSetting.ShortName = charaToken.Value<string>("shortName");
            characterSetting.Label = charaToken.Value<string>("label");

            var actors = charaToken["actors"].Children().ToList();
            characterSetting.actors = actors.Select(actor =>
            {
                ActorSetting actorSetting = new();
                actorSetting.Name = actor.Value<string>("name");
                actorSetting.Body = actor.Value<string>("body");
                actorSetting.Equips = actor["eqiups"].ToObject<string[]>();

                return actorSetting;
            }).ToArray();

            _charaDict.Add(characterSetting.Name.ToLower(), characterSetting);
        }
        private void CreateCommonActor(JToken actorToken)
        {
            ActorSetting actor = new();
            actor.Name = actorToken.Value<string>("name");
            actor.Body = actorToken.Value<string>("body");
            actor.Equips = actorToken["eqiups"].ToObject<string[]>();

            _commonActorDict.Add(actor.Name.ToLower(), actor);
        }

        public CharacterSetting[] GetCharacters()
        {
            return _charaDict.Values.ToArray();
        }
        public ActorSetting[] GetActors(string charaName)
        {
            string name = charaName.ToLower();
            return _charaDict[name].actors.Concat(_commonActorDict.Values).ToArray();
        }
        public CharacterActor LoadActor(string actorKey)
        {
            string[] p = actorKey.Split('/');
            return LoadActor(p[0], p[1]);
        }
        public CharacterActor LoadActor(string characterName, string actorName)
        {
            bool isCommonActor = false;
            var charaSetting = _charaDict[characterName.ToLower()];
            if (charaSetting == null)
            {
                throw new ArgumentException(nameof(characterName), $"Character not found: {characterName}");
            }

            var actorSetting = charaSetting?.actors.FirstOrDefault(a => a.Name == actorName);
            if (actorSetting == null)
            {
                actorSetting = _commonActorDict.GetValueOrDefault(actorName);
                isCommonActor = true;
            }
            if (actorSetting == null)
            {
                throw new ArgumentException(nameof(actorName), $"Actor not found: {characterName}/{actorName}");
            }

            var actor = new CharacterActor(charaSetting, actorSetting, isCommonActor);
            return actor;
        }
    }
}
