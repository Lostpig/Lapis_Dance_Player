// The following comment prevents Unity from auto upgrading the shader. Please keep it to keep backward compatibility.
// UNITY_SHADER_NO_UPGRADE

#ifndef _VLB_SHADER_SPECIFIC_INCLUDED_
#define _VLB_SHADER_SPECIFIC_INCLUDED_

/////////////////////////////////////////////////////
// Redefine missing functions from legacy pipeline
/////////////////////////////////////////////////////
inline float4 ComputeNonStereoScreenPos(float4 pos)
{
    float4 o = pos * 0.5f;
    o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
    o.zw = pos.zw;
    return o;
}

#define TransformStereoScreenSpaceTex(uv, w) uv

inline float4 ComputeScreenPos(float4 pos)
{
    float4 o = ComputeNonStereoScreenPos(pos);
#if defined(UNITY_SINGLE_PASS_STEREO)
    o.xy = TransformStereoScreenSpaceTex(o.xy, pos.w);
#endif
    return o;
}
/////////////////////////////////////////////////////

// POSITION TRANSFORM
inline float4 VLBObjectToClipPos(in float3 pos)     { return mul(UNITY_MATRIX_VP, mul(UNITY_MATRIX_M, float4(pos.xyz, 1.0))); }

// Don't use UNITY_MATRIX_M directly here, because ApplyCameraTranslationToMatrix has been applied to it to substract the camera position.
// But we can't use GetRawUnityObjectToWorld neither, since it doesn't work on Unity 2018.4.19 and HDRP 4.10.0 with GPUInstancing.
// So we counter the effect of ApplyCameraTranslationToMatrix by adding the _WorldSpaceCameraPos back.
inline float4 VLBObjectToWorldPos(in float4 pos)
{
    float4x4 modelMatrix = UNITY_MATRIX_M;

#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
    modelMatrix._m03_m13_m23 += _WorldSpaceCameraPos;
#endif

    return mul(modelMatrix, pos);
}

#define VLBObjectToViewPos(pos)                     (mul(UNITY_MATRIX_V, mul(UNITY_MATRIX_M, float4(pos.xyz, 1.0))).xyz)

// FRUSTUM PLANES
#define VLBFrustumPlanes _FrustumPlanes

// CAMERA
inline float3 __VLBWorldToObjectPos(in float3 pos) { return mul(UNITY_MATRIX_I_M, float4(pos, 1.0)).xyz; }
inline float3 VLBGetCameraPositionObjectSpace(float3 scaleObjectSpace)
{
    // getting access directly to _WorldSpaceCameraPos gives wrong values
    return __VLBWorldToObjectPos(GetCurrentViewPosition()) * scaleObjectSpace;
}

// DEPTH
#define VLBSampleDepthTexture(/*float4*/uv) (SampleCameraDepth((uv.xy) / (uv.w)))
#define VLBLinearEyeDepth(depth) LinearEyeDepth((depth), _ZBufferParams)

#endif // _VLB_SHADER_SPECIFIC_INCLUDED_
 