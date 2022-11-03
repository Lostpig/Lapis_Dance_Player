using UnityEngine;
using System.Collections.Generic;

namespace LapisPlayer
{
    public class VoewlChangeRecord
    {
        public bool active;
        public float[] perSecondChange;
        public float[] values;
        public float startTime;
        public float endTime;
        public float weight;
    }

    public class VowelManager
    {
        VowelSetting _setting;
        Dictionary<int, VowelData> _vowelMap;
        Dictionary<int, int> blendIndexToArrIndex;
        int[] _mouthBlendArr;
        SkinnedMeshRenderer faceMesh;
        SortedList<float, VoewlChangeRecord> vowelList;
        float[] updateValues;

        public VowelManager(VowelSetting setting, GameObject face)
        {
            _setting = setting;
            var faceGEO = Utility.FindNodeByRecursion(face, "face_GEO");
            faceMesh = faceGEO.GetComponent<SkinnedMeshRenderer>();

            _vowelMap = new();
            vowelList = new();

            _mouthBlendArr = CreateMouthBlends();
            blendIndexToArrIndex = new();
            for (int i = 0; i < _mouthBlendArr.Length; i++)
            {
                blendIndexToArrIndex.Add(_mouthBlendArr[i], i);
            }
            updateValues = new float[_mouthBlendArr.Length];

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

        public void AppendVowel(int vowel, float weight, float duration, float mixIn, float mixOut)
        {
            float now = Time.time;
            float contentDuration = duration - mixIn - mixOut;
            var voewlData = _vowelMap[vowel];

            if (mixIn > 0)
            {
                var targetValues = new float[_mouthBlendArr.Length];
                foreach (var bd in voewlData.BlendData)
                {
                    var arrIdx = blendIndexToArrIndex[bd.ShapeIndex];
                    targetValues[arrIdx] = bd.BlendValue;
                }

                var mixInRecord = new VoewlChangeRecord()
                {
                    active = false,
                    values = targetValues,
                    perSecondChange = new float[_mouthBlendArr.Length],
                    startTime = now,
                    endTime = now + mixIn,
                    weight = weight
                };
                if (!vowelList.ContainsKey(mixInRecord.startTime))
                {
                    vowelList.Add(mixInRecord.startTime, mixInRecord);
                }
            }
            if (mixOut > 0)
            {
                var targetValues = new float[_mouthBlendArr.Length];
                var mixOutRecord = new VoewlChangeRecord()
                {
                    active = false,
                    values = targetValues,
                    perSecondChange = new float[_mouthBlendArr.Length],
                    startTime = now + mixIn + contentDuration,
                    endTime = now + duration,
                    weight = weight
                };
                if (!vowelList.ContainsKey(mixOutRecord.startTime))
                {
                    vowelList.Add(mixOutRecord.startTime, mixOutRecord);
                }
            }

            var values = new float[_mouthBlendArr.Length];
            foreach (var bd in voewlData.BlendData)
            {
                var arrIdx = blendIndexToArrIndex[bd.ShapeIndex];
                values[arrIdx] = bd.BlendValue;
            }

            var contentRecord = new VoewlChangeRecord()
            {
                active = false,
                values = values,
                perSecondChange = new float[_mouthBlendArr.Length],
                startTime = now + mixIn,
                endTime = now + mixIn + contentDuration,
                weight = weight
            };
            if (!vowelList.ContainsKey(contentRecord.startTime))
            {
                vowelList.Add(contentRecord.startTime, contentRecord);
            }
        }
        public void AppendVowelAnimationIndex(AnimationIndex aIndex, float weight, double duration, double mixIn, double mixOut)
        {
            if (aIndex == AnimationIndex.None) return;

            var vowel = ((int)aIndex) - 1;
            AppendVowel(vowel, weight, (float)duration, (float)mixIn, (float)mixOut);
        }
        public void ActiveRecord(VoewlChangeRecord record, float now)
        {
            for (int i = 0; i < record.values.Length; i++)
            {
                var current = updateValues[i];
                var target = record.values[i];

                record.perSecondChange[i] = (target - current) / (record.endTime - now);
            }
            record.active = true;
        }

        private float LastUpdateTime = 0;
        public void Update()
        {
            var now = Time.time;
            if (LastUpdateTime == 0)
            {
                LastUpdateTime = now;
                return;
            }
            var elapsed = now - LastUpdateTime;
            LastUpdateTime = now;

            if (vowelList.Count == 0) return;

            List<float> removeItems = new();
            float sumWeight = 0;
            float[] changes = new float[updateValues.Length];

            bool hasActiveVowel = false;
            for (int i = 0; i < vowelList.Count; i++)
            {
                var curr = vowelList.Values[i];
                if (curr.startTime > now) break;
                if (curr.endTime < now)
                {
                    removeItems.Add(vowelList.Keys[i]);
                    continue;
                }

                hasActiveVowel = true;
                if (!curr.active) ActiveRecord(curr, now);
                for (int j = 0; j < changes.Length; j++)
                {
                    changes[j] += curr.perSecondChange[j] * elapsed * curr.weight;
                }
                sumWeight += curr.weight;
            }

            if (hasActiveVowel)
            {
                for (int i = 0; i < updateValues.Length; i++)
                {
                    var val = updateValues[i] + (changes[i] / sumWeight);
                    val = val > 100 ? 100f : val < 0 ? 0 : val;
                    faceMesh.SetBlendShapeWeight(_mouthBlendArr[i], val);

                    updateValues[i] = val;
                }
            }

            foreach (float key in removeItems)
            {
                vowelList.Remove(key);
            }
        }
    }
}
