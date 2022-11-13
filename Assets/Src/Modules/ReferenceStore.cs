using UnityEngine;
using System.Collections.Generic;

namespace LapisPlayer
{
    public class ReferenceStore
    {
        static private ReferenceStore _instance;
        static public ReferenceStore Instance
        {
            get
            {
                if (_instance == null) _instance = new ReferenceStore();
                return _instance;
            }
        }

        Dictionary<string, ReferenceObject> guidDict = new();

        public void AddReference(ReferenceObject ro)
        {
            if (guidDict.ContainsKey(ro.GUID))
            {
                var alive = guidDict[ro.GUID];
                if (ro != alive)
                {
                    Debug.LogError("Reference error: different object has same guid = " + ro.GUID);
                }
                return;
            }

            guidDict.Add(ro.GUID, ro);
        }
        public ReferenceObject GetReference(string guid)
        {
            if (guidDict.ContainsKey(guid))
            {
                return guidDict[guid];
            }

            Debug.Log("ReferenceObject not found: guid = " + guid);
            return null;
        }
        public void DeleteReference(string guid)
        {
            guidDict.Remove(guid);
        }
        public void Clear()
        {
            guidDict.Clear();
        }

        public bool Include(string guid)
        {
            return guidDict.ContainsKey(guid);
        }
    }
}
