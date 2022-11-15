using LapisPlayer;
using UnityEngine;
using VLB;

public class SimpleProjector : MonoBehaviour
{
    // Fields
    public GameObject Prefab; // 0x18
    public float Distance; // 0x20
    public float Scale; // 0x24
    public float Offset; // 0x28
    private Ray ray; // 0x2c
    private RaycastHit hit; // 0x44
    public GameObject prefabInstance; // 0x70
    public VolumetricLightBeam VolumBeam; // 0x78
    public LayerMask layermask; // 0x80

    private void Awake()
    {
        prefabInstance.AddComponent<LightSpotHorizontal>();
    }
    private void OnEnable()
    {
        // prefabInstance.layer = 6;
        // Debug.Log($"VLB values [{VolumBeam.intensityInside}, {VolumBeam.intensityOutside}, {VolumBeam.fallOffStart}, {VolumBeam.fallOffEnd}]");
    }

    // private void OnDestroy() { }

    // private void OnDisable() { }

    // private void OnDrawGizmosSelected() { }
}

