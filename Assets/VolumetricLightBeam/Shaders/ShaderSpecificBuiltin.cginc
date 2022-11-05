// The following comment prevents Unity from auto upgrading the shader. Please keep it to keep backward compatibility.
// UNITY_SHADER_NO_UPGRADE

#ifndef _VLB_SHADER_SPECIFIC_INCLUDED_
#define _VLB_SHADER_SPECIFIC_INCLUDED_

// POSITION TRANSFORM
#if UNITY_VERSION < 540
    #define __VLBMatrixWorldToObject    _World2Object
    #define __VLBMatrixObjectToWorld    _Object2World
    #define __VLBMatrixV                UNITY_MATRIX_V
    inline float4 VLBObjectToClipPos(in float3 pos) { return mul(UNITY_MATRIX_MVP, float4(pos, 1.0)); }
#else
    #if VLB_CUSTOM_INSTANCED_OBJECT_MATRICES
        #define __VLBMatrixWorldToObject  UNITY_ACCESS_INSTANCED_PROP(Props, _WorldToLocalMatrix)
        #define __VLBMatrixObjectToWorld  UNITY_ACCESS_INSTANCED_PROP(Props, _LocalToWorldMatrix)
        #define __VLBMatrixV              unity_MatrixV
        inline float4 VLBObjectToClipPos(in float3 pos) { return mul(mul(unity_MatrixVP, __VLBMatrixObjectToWorld), float4(pos, 1.0)); }
    #else
        #define __VLBMatrixWorldToObject    unity_WorldToObject
        #define __VLBMatrixObjectToWorld    unity_ObjectToWorld
        #define __VLBMatrixV                UNITY_MATRIX_V
        #define VLBObjectToClipPos          UnityObjectToClipPos
    #endif
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
#ifndef UNITY_DECLARE_DEPTH_TEXTURE // handle Unity pre 5.6.0
#define UNITY_DECLARE_DEPTH_TEXTURE(tex) sampler2D_float tex
#endif
UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

#define VLBSampleDepthTexture(/*float4*/uv) (SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, (uv)/(uv.w)))
#define VLBLinearEyeDepth(depth) (LinearEyeDepth(depth))

// FOG
#define VLB_FOG_UNITY_BUILTIN_COORDS

#if VLB_ALPHA_AS_BLACK
#define VLB_FOG_APPLY(color) \
        float4 fogColor = unity_FogColor; \
        fogColor.rgb *= color.a;  \
        UNITY_APPLY_FOG_COLOR(i.fogCoord, color, fogColor);
        // since we use this shader with Additive blending, fog color should be modulated by general alpha
#else
#define VLB_FOG_APPLY(color) UNITY_APPLY_FOG(i.fogCoord, color);
#endif

#endif // _VLB_SHADER_SPECIFIC_INCLUDED_
