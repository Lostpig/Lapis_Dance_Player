// The following comment prevents Unity from auto upgrading the shader. Please keep it to keep backward compatibility.
// UNITY_SHADER_NO_UPGRADE

#ifndef _VLB_SHADER_SPECIFIC_INCLUDED_
#define _VLB_SHADER_SPECIFIC_INCLUDED_

// POSITION TRANSFORM
#if VLB_CUSTOM_INSTANCED_OBJECT_MATRICES
    #define __VLBMatrixWorldToObject  UNITY_ACCESS_INSTANCED_PROP(Props, _WorldToLocalMatrix)
    #define __VLBMatrixObjectToWorld  UNITY_ACCESS_INSTANCED_PROP(Props, _LocalToWorldMatrix)
    #define __VLBMatrixV              unity_MatrixV
    inline float4 VLBObjectToClipPos(in float3 pos) { return mul(mul(unity_MatrixVP, __VLBMatrixObjectToWorld), float4(pos, 1.0)); }
#else
    #define __VLBMatrixWorldToObject    unity_WorldToObject
    #define __VLBMatrixObjectToWorld    unity_ObjectToWorld
    #define __VLBMatrixV                UNITY_MATRIX_V
    inline float4 VLBObjectToClipPos(in float3 pos) { return mul(UNITY_MATRIX_VP, mul(UNITY_MATRIX_M, float4(pos.xyz, 1.0))); }
#endif

inline float4 VLBObjectToWorldPos(in float4 pos)    { return mul(__VLBMatrixObjectToWorld, pos); }
#define VLBWorldToViewPos(pos)                      (mul(__VLBMatrixV, float4(pos.xyz, 1.0)).xyz)

// FRUSTUM PLANES
#define VLBFrustumPlanes unity_CameraWorldClipPlanes

// CAMERA
inline float3 __VLBWorldToObjectPos(in float3 pos) { return mul(__VLBMatrixWorldToObject, float4(pos, 1.0)).xyz; }
inline float3 VLBGetCameraPositionObjectSpace(float3 scaleObjectSpace)
{
    return __VLBWorldToObjectPos(_WorldSpaceCameraPos).xyz * scaleObjectSpace;
}

// DEPTH
#define VLBSampleDepthTexture(/*float4*/uv) (SampleSceneDepth((uv.xy) / (uv.w)))
#define VLBLinearEyeDepth(depth) LinearEyeDepth((depth), _ZBufferParams)

// FOG
#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
    #define VLB_FOG_MIX(color, fogColor, posClipSpace)  color.rgb = MixFogColor(color.rgb, fogColor.rgb, ComputeFogFactor(posClipSpace.z * posClipSpace.w))

    #if VLB_ALPHA_AS_BLACK
        #define VLB_FOG_APPLY(color) \
                float4 fogColor = unity_FogColor; \
                fogColor.rgb *= color.a;  \
                VLB_FOG_MIX(color, fogColor, i.posClipSpace);
    #else
        #define VLB_FOG_APPLY(color) VLB_FOG_MIX(color, unity_FogColor, i.posClipSpace);
    #endif
#endif

#endif // _VLB_SHADER_SPECIFIC_INCLUDED_
