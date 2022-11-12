using System;
using UnityEngine.Playables;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class Rebingding
{
    // Fields
    [SerializeField] public UnityEngine.Object Key; // 0x10
    [SerializeField] public SceneReference Value; // 0x18
}

public class SceneTimelineRebinder : MonoBehaviour
{
    // Fields
    public List<Rebingding> rebinding; // 0x18
    public bool PlayAfterBind; // 0x20
    private PlayableDirector _playableDirector; // 0x28

    // Properties
    public PlayableDirector Director { get => _playableDirector; }

    private void Start()
    {
        _playableDirector = GetComponent<PlayableDirector>();
        Rebind();
    }
    public void ReKey() { }
    public void Rebind()
    {
        Dictionary<string, ReferenceObject> objDict = new();
        // var objs = gameObject.GetComponentsInChildren<ReferenceObject>();
        var objs = GameObject.FindObjectsOfType<ReferenceObject>();

        foreach (var obj in objs)
        {
            objDict.Add(obj.Data.GUID, obj);
        }

        foreach (var reb in rebinding)
        {
            if (reb.Key == null) continue;
            if (reb.Value.Dynamic)
            {
                var go = GameObject.Find(reb.Value.DyamicName);
                if (go == null) continue;

                if (string.IsNullOrEmpty(reb.Value.TypeName))
                {
                    Director.SetGenericBinding(reb.Key, go);
                }
                else
                {
                    var rebindItem = go.GetComponent(reb.Value.TypeName);
                    if (rebindItem == null)
                    {
                        Debug.LogError("Rebinder type not found:" + reb.Value.TypeName);
                        Director.SetGenericBinding(reb.Key, go);
                        continue;
                    }

                    Director.SetGenericBinding(reb.Key, rebindItem);
                }
            }
            else if (objDict.ContainsKey(reb.Value.GUID))
            {
                var refObj = objDict[reb.Value.GUID];
                var go = refObj.GetGameObject();

                if (string.IsNullOrEmpty(reb.Value.TypeName))
                {
                    Director.SetGenericBinding(reb.Key, go);
                }
                else
                {
                    var rebindItem = go.GetComponent(reb.Value.TypeName);
                    if (rebindItem == null)
                    {
                        Debug.LogError("Rebinder type not found:" + reb.Value.TypeName);
                        continue;
                    }

                    Director.SetGenericBinding(reb.Key, rebindItem);
                }
            }
            else if (reb.Value.TypeName == "AudioSource")
            {
                var audioSource = GameObject.Find("Launcher").GetComponent<AudioSource>();
                Director.SetGenericBinding(reb.Key, audioSource);
            }
        }
    }
    public void RebindManually(string trackName, UnityEngine.Object bindObj) { }
    private void OnDestroy() { }
}
