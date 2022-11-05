using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LapisPlayer
{
    public class PoseStore
    {
        static private PoseStore _instance;
        static public PoseStore Instance
        {
            get
            {
                if (_instance == null) _instance = new PoseStore();
                return _instance;
            }
        }


        Dictionary<string, string> _poseDict = new();

        public PoseStore ()
        {
            CreatePoses();
        }

        private void CreatePoses ()
        {
            _poseDict.Add("Stand", "Actors/Animations/Actor/Favor/Clothes/Ani_Nor_Ash_Idle002");

            string arBundle = "Actors/Animations/Actor/Favor/AR_Poss";
            for (int i = 1; i <= 22; i++)
            {
                string name = $"Ani_VRphoto_" + (i < 10 ? ("0" + i) : i);
                _poseDict.Add(name, arBundle + "/" + name);
            }
            for (int i = 1; i <= 3; i++)
            {
                string name = $"Ani_ARselfie_0" + i;
                _poseDict.Add(name, arBundle + "/" + name);
            }
        }

        public string[] GetAllPose()
        {
            return _poseDict.Keys.ToArray();
        }
        public AnimationClip LoadPoseClip (string key)
        {
            string val = _poseDict[key];
            return AssetBundleLoader.Instance.LoadAsset<AnimationClip>(val);
        }
    }
}
