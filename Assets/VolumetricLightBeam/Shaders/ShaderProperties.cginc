// UNITY_SHADER_NO_UPGRADE

#ifndef _VLB_SHADER_PROPERTIES_INCLUDED_
#define _VLB_SHADER_PROPERTIES_INCLUDED_

#include "ShaderPropertySystem.cginc"

/// ****************************************
/// PROPERTIES DECLARATION
/// ****************************************
VLB_DEFINE_PROP_START

#if VLB_CUSTOM_INSTANCED_OBJECT_MATRICES
    VLB_DEFINE_PROP(float4x4, _LocalToWorldMatrix)
    VLB_DEFINE_PROP(float4x4, _WorldToLocalMatrix)
#endif

    // if VLB_COLOR_GRADIENT_MATRIX_HIGH || VLB_COLOR_GRADIENT_MATRIX_LOW
    VLB_DEFINE_PROP(float4x4, _ColorGradientMatrix)
    // else
    VLB_DEFINE_PROP(float4, _ColorFlat)
    // endif

    VLB_DEFINE_PROP(half, _AlphaInside)
    VLB_DEFINE_PROP(half, _AlphaOutside)
    VLB_DEFINE_PROP(float2, _ConeSlopeCosSin)   // between -1 and +1
    VLB_DEFINE_PROP(float2, _ConeRadius)        // x = start radius ; y = end radius
    VLB_DEFINE_PROP(float, _ConeApexOffsetZ)    // > 0
    VLB_DEFINE_PROP(float, _AttenuationLerpLinearQuad)
    VLB_DEFINE_PROP(float3, _DistanceFallOff)   // fallOffStart, fallOffEnd, maxGeometryDistance
    VLB_DEFINE_PROP(float, _DistanceCamClipping)
    VLB_DEFINE_PROP(float, _FadeOutFactor)
    VLB_DEFINE_PROP(float, _FresnelPow)             // must be != 0 to avoid infinite fresnel
    VLB_DEFINE_PROP(float, _GlareFrontal)
    VLB_DEFINE_PROP(float, _GlareBehind)
    VLB_DEFINE_PROP(float, _DrawCap)
    VLB_DEFINE_PROP(float4, _CameraParams)          // xyz: object space forward vector ; w: cameraIsInsideBeamFactor (-1 : +1)

    // if VLB_OCCLUSION_CLIPPING_PLANE
    VLB_DEFINE_PROP(float4, _DynamicOcclusionClippingPlaneWS)
    VLB_DEFINE_PROP(float,  _DynamicOcclusionClippingPlaneProps)
    // elif VLB_OCCLUSION_DEPTH_TEXTURE
    VLB_DEFINE_PROP(float,     _DynamicOcclusionDepthProps)
    // endif

    // if VLB_DEPTH_BLEND
    VLB_DEFINE_PROP(float, _DepthBlendDistance)
    // endif

    // if VLB_NOISE_3D
    VLB_DEFINE_PROP(float4, _NoiseVelocityAndScale)
    VLB_DEFINE_PROP(float2, _NoiseParam)
    // endif

    // if VLB_MESH_SKEWING
    VLB_DEFINE_PROP(float3, _LocalForwardDirection)
    // endif

    VLB_DEFINE_PROP(float2, _TiltVector)
    VLB_DEFINE_PROP(float4, _AdditionalClippingPlaneWS)

VLB_DEFINE_PROP_END

// UNITY_REVERSED_Z define is broken for WebGL and URP
uniform float _VLB_UsesReversedZBuffer; // not reversed in OpenGL on WebGL

#if VLB_OCCLUSION_DEPTH_TEXTURE
// Setting a Texture property to a GPU instanced material is not supported, so keep it as regular property
uniform sampler2D _DynamicOcclusionDepthTexture;
#endif

#if VLB_DITHERING
uniform float _VLB_DitheringFactor;
uniform sampler2D _VLB_DitheringNoiseTex;
uniform float4 _VLB_DitheringNoiseTex_TexelSize;
#endif

/// ****************************************

#endif
