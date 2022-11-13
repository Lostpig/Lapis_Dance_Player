using Oz.Graphics;
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
    }
    private void OnDestroy()
    {
        geometricShadowCasts.Remove(this);
    }

    public void ApplyShadow (ShadowSettings setting)
    {
        // 需要用Ray映射到地面上，不然影子会跟着脚动
        // TODO 太麻烦了,看情况改
        // setting.RaycastOrigin

        // meshFilter.sharedMesh = setting.ShadowMesh;
        // meshRenderer.sharedMaterial = setting.ShadowMaterial;
    }

    // private void LateUpdate()
    // {
    //     // var originPos = transform.position;
    //     // transform.position = new Vector3(originPos.x, 0.005f, originPos.z);
    //     transform.eulerAngles = new Vector3(0, 90, 0);
    // }
}