using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Oz.Timeline;

namespace LapisPlayer
{
    public class VowelClipItem
    {
        public int Vowel;
        public float Weight;
        public float Duration;
        public float MixIn;
        public float MixOut;
        public float Start;
    }

    public class VowelManager
    {
        const int CutPerSecond = 60;
        const float CutTime = 1f / CutPerSecond;

        VowelSetting _setting;
        Dictionary<int, VowelData> _vowelMap;
        Dictionary<int, int> blendIndexToArrIndex;
        int[] _mouthBlendArr;
        SkinnedMeshRenderer faceMesh;

        public VowelManager(VowelSetting setting, GameObject face)
        {
            _setting = setting;
            var faceGEO = Utility.FindNodeByRecursion(face, "face_GEO");
            faceMesh = faceGEO.GetComponent<SkinnedMeshRenderer>();

            _vowelMap = new();

            _mouthBlendArr = CreateMouthBlends();
            blendIndexToArrIndex = new();
            for (int i = 0; i < _mouthBlendArr.Length; i++)
            {
                blendIndexToArrIndex.Add(_mouthBlendArr[i], i);
            }
            CreateVowelMap(setting);
        }
        public int[] CreateMouthBlends()
        {
            var count = faceMesh.sharedMesh.blendShapeCount;
            List<int> mouthBlendList = new();

            for (var i = 0; i < count; i++)
            {
                var name = faceMesh.sharedMesh.GetBlendShapeName(i);
                if (name.Contains("mouth"))
                {
                    mouthBlendList.Add(i);
                }
            }
            return mouthBlendList.ToArray();
        }
        public void CreateVowelMap(VowelSetting setting)
        {
            setting.VowelData.ForEach((data) =>
            {
                _vowelMap.Add(data.Vowel, data);
            });
        }

        float[][] clipTimeCuts;
        float trackDuration = 0;
        public void BindWithVowelCilipInfo(VowelClipInfo[] clips)
        {
            float end = 0;

            VowelClipItem[] items = clips.Select(c =>
            {
                var t = (float)(c.Start + c.Duration);
                if (t > end) end = t;

                return new VowelClipItem()
                {
                    Start = (float)c.Start,
                    Duration = (float)c.Duration,
                    MixIn = (float)c.EaseInDuration,
                    MixOut = (float)c.EaseOutDuration,
                    Vowel = (int)c.Index - 1,
                    Weight = 1
                };
            }).ToArray();

            BindClips(items, end);
        }
        public void BindWithLipSyncAsset(LipsyncAsset[] assets)
        {
            float end = 0;

            VowelClipItem[] items = assets.Select(ass =>
            {
                var t = (float)(ass.Clip.start + ass.Clip.duration);
                if (t > end) end = t;

                return new VowelClipItem()
                {
                    Start = (float)ass.Clip.start,
                    Duration = (float)ass.Clip.duration,
                    MixIn = (float)ass.Clip.mixInDuration,
                    MixOut = (float)ass.Clip.mixOutDuration,
                    Vowel = (int)ass.Index - 1,
                    Weight = ass.Weight
                };
            }).ToArray();

            BindClips(items, end);
        }

        public void BindClips(VowelClipItem[] items, float trackTime)
        {
            int cutCount = (int)Math.Ceiling(trackTime) * CutPerSecond;
            float[][] timeCuts = new float[cutCount][];
            float[] weights = new float[cutCount];

            for (int i = 0; i < cutCount; i++)
            {
                timeCuts[i] = new float[_mouthBlendArr.Length];
            }

            foreach (var item in items)
            {
                float contentDuration = item.Duration - item.MixIn - item.MixOut;
                var voewlData = _vowelMap[item.Vowel];

                if (item.MixIn >= 0.01)
                {
                    SetMixIn(item, ref timeCuts, ref weights);
                }
                SetCutContent(item, ref timeCuts, ref weights);
                if (item.MixOut >= 0.01)
                {
                    SetMixOut(item, ref timeCuts, ref weights);
                }
            }

            clipTimeCuts = timeCuts;
            trackDuration = trackTime;
        }
        private void SetMixIn(VowelClipItem item, ref float[][] timeCuts, ref float[] weights)
        {
            float start = item.Start;
            float end = start + item.MixIn;
            float duration = end - start;

            int cutStart = (int)Math.Ceiling(start * CutPerSecond);
            int cutEnd = (int)Math.Ceiling(end * CutPerSecond);

            var voewlData = _vowelMap[item.Vowel];
            for (int i = cutStart; i < cutEnd; i++)
            {
                if (i >= weights.Length)
                {
                    Debug.Log("Now Overflow");
                }

                var cutPrec = (i * CutTime - start) / duration;
                var originWeight = weights[i];

                foreach (var bd in voewlData.BlendData)
                {
                    var arrIdx = blendIndexToArrIndex[bd.ShapeIndex];
                    var appendValue = bd.BlendValue * cutPrec;
                    var originValue = timeCuts[i][arrIdx];

                    var mergeValue = ((originValue * originWeight) + (appendValue * item.Weight)) / (originWeight + item.Weight);
                    timeCuts[i][arrIdx] = mergeValue;
                }
                weights[i] += item.Weight;
            }
        }
        private void SetCutContent(VowelClipItem item, ref float[][] timeCuts, ref float[] weights)
        {
            float start = item.Start + item.MixIn;
            float end = item.Start + item.Duration - item.MixOut;

            int cutStart = (int)Math.Ceiling(start * CutPerSecond);
            int cutEnd = (int)Math.Ceiling(end * CutPerSecond);

            var voewlData = _vowelMap[item.Vowel];
            for (int i = cutStart; i < cutEnd; i++)
            {
                var originWeight = weights[i];

                foreach (var bd in voewlData.BlendData)
                {
                    var arrIdx = blendIndexToArrIndex[bd.ShapeIndex];
                    var appendValue = bd.BlendValue;
                    var originValue = timeCuts[i][arrIdx];

                    var mergeValue = ((originValue * originWeight) + (appendValue * item.Weight)) / (originWeight + item.Weight);
                    timeCuts[i][arrIdx] = mergeValue;
                }
                weights[i] += item.Weight;
            }
        }
        private void SetMixOut(VowelClipItem item, ref float[][] timeCuts, ref float[] weights)
        {
            float start = item.Start + item.Duration - item.MixOut;
            float end = item.Start + item.Duration;
            float duration = end - start;

            int cutStart = (int)Math.Ceiling(start * CutPerSecond);
            int cutEnd = (int)Math.Ceiling(end * CutPerSecond);

            var voewlData = _vowelMap[item.Vowel];
            for (int i = cutStart; i < cutEnd; i++)
            {
                var cutPrec = (i * CutTime - start) / duration;
                var originWeight = weights[i];

                foreach (var bd in voewlData.BlendData)
                {
                    var arrIdx = blendIndexToArrIndex[bd.ShapeIndex];
                    var appendValue = bd.BlendValue * (1f - cutPrec);
                    var originValue = timeCuts[i][arrIdx];

                    var mergeValue = ((originValue * originWeight) + (appendValue * item.Weight)) / (originWeight + item.Weight);
                    timeCuts[i][arrIdx] = mergeValue;
                }
                weights[i] += item.Weight;
            }
        }

        public void Update(float now)
        {
            if (now >= trackDuration)
            {
                for (int i = 0; i < _mouthBlendArr.Length; i++)
                {
                    faceMesh.SetBlendShapeWeight(_mouthBlendArr[i], 0);
                }
                return;
            }

            int prevCut = (int)(now * CutPerSecond - 0.5f);
            int nextCut = (int)(now * CutPerSecond + 0.5f);
            float ran = now - (prevCut * CutTime);

            for (int i = 0; i < _mouthBlendArr.Length; i++)
            {
                var prevVal = clipTimeCuts[prevCut][i];
                var nextVal = clipTimeCuts[nextCut][i];
                var val = prevVal + (nextVal - prevVal) * ran;

                faceMesh.SetBlendShapeWeight(_mouthBlendArr[i], val);
            }
        }
    }
}
