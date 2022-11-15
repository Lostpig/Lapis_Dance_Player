using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace LapisPlayer
{
    public class MainSceneManager
    {
        RuntimeAnimatorController stageController;
        GameObject stage;
        Transform actorLocation;

        public void Start ()
        {
            var prefab = StageStore.Instance.LoadStage(MainSceneConfig.Instance.Stage);
            stage = GameObject.Instantiate(prefab);

            stageController = AssetBundleLoader.Instance.LoadAsset<RuntimeAnimatorController>("Actors/AnimationController/Common@MainStage");
            actorLocation = Utility.FindNodeByRecursion(stage, "ActorLocation").transform;

            AddActor(MainSceneConfig.Instance.P1, 1);
            AddActor(MainSceneConfig.Instance.P2, 2);
            AddActor(MainSceneConfig.Instance.P3, 3);

            if (MainSceneConfig.Instance.Stage.ToLower() == "bg105")
            {
                BindingBG105();
            }
        }

        public void AddActor (string actorKey, int pos)
        {
            if (string.IsNullOrEmpty(actorKey)) return;

            string posP = "P" + pos;
            string stage = MainSceneConfig.Instance.Stage;
            var actor = CharactersStore.Instance.LoadActor(actorKey);
            var location = actorLocation.Find(pos.ToString());
            actor.SetPosition(location.position);
            actor.Root.transform.eulerAngles = location.eulerAngles;
            actor.Root.transform.SetParent(location, true);
            actor.BindPhysicalBones();

            string idle = $"Actors/Animations/Actor/Favor/Theme_Scene/{stage}/Ani_{stage}{posP}_Tia_Idle";
            string rand = $"Actors/Animations/Actor/Favor/Theme_Scene/{stage}/Ani_{stage}{posP}_Tia_Ran001";

            var idleClip = AssetBundleLoader.Instance.LoadAsset<AnimationClip>(idle);
            var randClip = AssetBundleLoader.Instance.LoadAsset<AnimationClip>(rand);
            var ctrl = new AnimatorOverrideController(stageController);

            ctrl["Body@Idle"] = idleClip;
            ctrl["Body@RandomIdle"] = randClip ?? idleClip;
            actor.SetBaseAnimationController(ctrl);
            actor.PlayBaseAnimation();
        }
    
        private void BindingBG105 ()
        {
            var flower = Utility.FindNodeByRecursion(actorLocation.gameObject, "PP_Bg105_Flower_001");
            var flowerAnimator = flower.GetComponent<Animator>();
            var flowerClip = AssetBundleLoader.Instance.LoadAsset<AnimationClip>("Actors/Animations/Actor/Favor/Theme_Scene/BG105/Ani_Bg105_Flower_Idle");
            var flowerClip2 = AssetBundleLoader.Instance.LoadAsset<AnimationClip>("Actors/Animations/Actor/Favor/Theme_Scene/BG105/Ani_Bg105_Flower_Ans001");
            var flowerCtrl = new AnimatorOverrideController(stageController);
            flowerCtrl["Body@Idle"] = flowerClip;
            flowerCtrl["Body@RandomIdle"] = flowerClip2;
            flowerAnimator.runtimeAnimatorController = flowerCtrl;
            flowerAnimator.Play("Body@Idle");

            var cat = Utility.FindNodeByRecursion(actorLocation.gameObject, "PP_Bg105_Cat_002");
            var catAnimator = cat.GetComponent<Animator>();
            var catClip = AssetBundleLoader.Instance.LoadAsset<AnimationClip>("Actors/Animations/Actor/Favor/Theme_Scene/BG105/Ani_Bg105_Cat_Idle");
            var catClip2 = AssetBundleLoader.Instance.LoadAsset<AnimationClip>("Actors/Animations/Actor/Favor/Theme_Scene/BG105/Ani_Bg105_Cat_Ans001");
            var catCtrl = new AnimatorOverrideController(stageController);
            catCtrl["Body@Idle"] = catClip;
            catCtrl["Body@RandomIdle"] = catClip2;
            catAnimator.runtimeAnimatorController = catCtrl;
            catAnimator.Play("Body@Idle");

            var star = Utility.FindNodeByRecursion(actorLocation.gameObject, "PP_Bg105_Star_001");
            var starAnimator = star.GetComponent<Animator>();
            var starClip = AssetBundleLoader.Instance.LoadAsset<AnimationClip>("Actors/Animations/Actor/Favor/Theme_Scene/BG105/Ani_Bg105_Star_Idle");
            var starClip2 = AssetBundleLoader.Instance.LoadAsset<AnimationClip>("Actors/Animations/Actor/Favor/Theme_Scene/BG105/Ani_Bg105_Star_Ans001");
            var starCtrl = new AnimatorOverrideController(stageController);
            starCtrl["Body@Idle"] = starClip;
            starCtrl["Body@RandomIdle"] = starClip2;
            starAnimator.runtimeAnimatorController = starCtrl;
            starAnimator.Play("Body@Idle");
        }
    }
}
