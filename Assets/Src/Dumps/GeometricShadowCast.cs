using System.Collections.Generic;
using UnityEngine;

public class GeometricShadowCast : MonoBehaviour
{
    // Fields
    public static List<GeometricShadowCast> geometricShadowCasts = new();
    public MeshFilter meshFilter; // 0x18
    public MeshRenderer meshRenderer; // 0x20

    private void Awake()
    {
        geometricShadowCasts.Add(this);

        GetComponent<MeshFilter>().mesh = meshFilter.mesh;
    }
    private void OnDestroy()
    {
        geometricShadowCasts.Remove(this);
    }

}