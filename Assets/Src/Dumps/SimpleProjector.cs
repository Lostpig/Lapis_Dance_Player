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

    private void OnEnable()
    {
        // TODO 凭感觉填的值，具体值的获得方式不清楚
        Debug.Log($"VLB values [{VolumBeam.intensityInside}, {VolumBeam.intensityOutside}, {VolumBeam.fallOffStart}, {VolumBeam.fallOffEnd}]");
        // VolumBeam.intensityGlobal = 0.85f;
    }

    // private void OnDestroy() { }

    // private void OnDisable() { }

    private void LateUpdate()
    {
        // var originEuler = prefabInstance.transform.eulerAngles;
        var originPos = prefabInstance.transform.position;

        prefabInstance.transform.position = new Vector3(originPos.x, 0.025f, originPos.z);
        prefabInstance.transform.eulerAngles = new Vector3(0, 90, 0);
    }

    // private void OnDrawGizmosSelected() { }
}

