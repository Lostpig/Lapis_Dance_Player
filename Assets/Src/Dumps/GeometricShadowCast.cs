using Oz.Graphics;
using System.Collections.Generic;
using UnityEngine;

public class GeometricShadowCast : MonoBehaviour
{
    // Fields
    public static List<GeometricShadowCast> geometricShadowCasts = new();
    public MeshFilter meshFilter; // 0x18
    public MeshRenderer meshRenderer; // 0x20
    private Transform staticRoot;
    private bool active = false;

    private void Awake()
    {
        geometricShadowCasts.Add(this);
    }
    private void OnDestroy()
    {
        geometricShadowCasts.Remove(this);
    }

    public void ApplyShadow (ShadowSettings setting, GameObject staticRoot)
    {
        // 需要映射到地面上，不然影子会跟着脚动
        
        // meshFilter.sharedMesh = setting.ShadowMesh;
        // meshRenderer.sharedMaterial = setting.ShadowMaterial;
        // this.staticRoot = staticRoot.transform;
        // active = true;
    }

    // TODO 阴影的映射会错位...
    /* private void LateUpdate()
    {
        if (!active) return;

        var Worldpos = transform.position;
        var Localpos = staticRoot.InverseTransformPoint(Worldpos);

        var dis = Vector3.Dot(Localpos, Vector3.up);
        var vecN = Vector3.up * dis;

        Localpos = Localpos - vecN;
        transform.position = staticRoot.TransformPoint(Localpos);
    } */
}