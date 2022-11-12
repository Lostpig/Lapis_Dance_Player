using Timeline;
using UnityEngine.Playables;
using UnityEngine;

public class Context
{
    // Fields
    public InternalState state; // 0x10
    public Track track; // 0x18
    public Mixer mixer; // 0x20
    public Clip clip; // 0x28
    public Behaviour behaviour; // 0x30
    public PlayableDirector director; // 0x38
    public UnityEngine.Object BindObject; // 0x40
    public int frameCount; // 0x48
    public float weight; // 0x4c
    public double localTime; // 0x50

    // public T Clip<T>() {     }
}