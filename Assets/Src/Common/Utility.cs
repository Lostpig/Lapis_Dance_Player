using System;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;

namespace LapisPlayer
{
    public static class Utility
    {
        /*
         * 获取inactive的子对象
         */
        public static GameObject FindObject(GameObject parent, string name)
        {
            Transform[] trs = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in trs)
            {
                if (t.name == name)
                {
                    return t.gameObject;
                }
            }
            return null;
        }

        public static GameObject FindNodeByPath(GameObject rootObj, string nodepath)
        {
            string[] parts = nodepath.Split("/");

            GameObject current = rootObj;
            GameObject tempObj = null;

            foreach (string part in parts)
            {
                int count = current.transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    tempObj = current.transform.GetChild(i).gameObject;
                    if (tempObj.name == part)
                    {
                        current = tempObj;
                        break;
                    }
                    tempObj = null;
                }

                if (tempObj == null)
                {
                    return null;
                }
            }

            return current;
        }

        public static GameObject FindNodeByName(GameObject rootObj, string nodeName)
        {
            if (rootObj == null) return null;
            var result = rootObj.transform.Find(nodeName);
            return result ? result.gameObject : null;
        }

        public static GameObject FindNodeByRecursion(GameObject rootObj, string nodeName)
        {
            var transform = FindChildByRecursion(rootObj.transform, nodeName);
            return transform ? transform.gameObject : null;
        }

        private static Transform FindChildByRecursion(Transform aParent, string aName)
        {
            if (aParent == null) return null;
            var result = aParent.Find(aName);
            if (result != null)
                return result;
            foreach (Transform child in aParent)
            {
                result = FindChildByRecursion(child, aName);
                if (result != null)
                    return result;
            }
            return null;
        }

        // 计算两点间一点，返回值为相对start点的坐标
        // percent 两点间距离的百分比
        public static Vector3 GetPointFromStart(Vector3 start, Vector3 end, float percent)
        {
            var x = (end.x - start.x) * percent;
            var y = (end.y - start.y) * percent;
            var z = (end.z - start.z) * percent;

            // Vector3 normal = (end - start).normalized;
            // float distance = Vector3.Distance(start, end);
            // return normal * (distance * percent);

            return new Vector3(x, y, z);
        }

        public static Quaternion YLookRotation(Vector3 right, Vector3 up)
        {
            Quaternion rightToForward = Quaternion.Euler(90f, 0f, 0f);
            Quaternion forwardToTarget = Quaternion.LookRotation(right, up);

            return forwardToTarget * rightToForward;
        }
        public static Quaternion XLookRotation(Vector3 right, Vector3 up)
        {
            Quaternion rightToForward = Quaternion.Euler(0f, -90f, 0f);
            Quaternion forwardToTarget = Quaternion.LookRotation(right, up);

            return forwardToTarget * rightToForward;
        }

        public static bool IsParentNode(Transform self, Transform aParent)
        {
            Transform parent = self.parent;

            while (parent != null)
            {
                if (aParent == parent) return true;
                parent = parent.parent;
            }

            return false;
        }

        public static async Task<AudioClip> LoadAudioClip(string filePath)
        {
            string fullPath = "file:///" + filePath;
            AudioClip clip = null;
            float startTime = Time.time;

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.WAV);
            www.SendWebRequest();

            try
            {
                while (!www.isDone)
                {
                    await Task.Delay(5);
                    if (Time.time > startTime + 10)
                    {
                        throw new TimeoutException();
                    }
                }

                if (www.result == UnityWebRequest.Result.ProtocolError) Debug.Log($"{www.error}");
                else
                {
                    clip = DownloadHandlerAudioClip.GetContent(www);
                }
            }
            catch (Exception err)
            {
                Debug.Log($"{err.Message}, {err.StackTrace}");
            }

            return clip;
        }

        public static void ActiveToTop(GameObject go)
        {
            var curr = go;
            while (curr != null)
            {
                curr.SetActive(true);
                curr = curr.transform.parent?.gameObject;
            }
        }
    }
}
