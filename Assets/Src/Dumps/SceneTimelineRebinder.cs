using System;
using UnityEngine.Playables;
using UnityEngine;
using System.Collections.Generic;
using LapisPlayer;

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
                        Debug.LogError("Rebinder type not found:" + reb.Value.TypeName + " on " + go.name);

                        var type = Type.GetType(reb.Value.TypeName);
                        type = type ?? Type.GetType($"UnityEngine.{reb.Value.TypeName}, UnityEngine");
                        var component = go.AddComponent(type);

                        Director.SetGenericBinding(reb.Key, component == null ? go : component);
                        continue;
                    }

                    Director.SetGenericBinding(reb.Key, rebindItem);
                }
            }
            else if (ReferenceStore.Instance.Include(reb.Value.GUID))
            {
                var refObj = ReferenceStore.Instance.GetReference(reb.Value.GUID);
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
