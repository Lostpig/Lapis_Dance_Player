// UNITY_SHADER_NO_UPGRADE

#ifndef _VLB_SHADER_PROPERTY_SYSTEM_INCLUDED_
#define _VLB_SHADER_PROPERTY_SYSTEM_INCLUDED_

/// ****************************************
/// PROPERTIES MACROS
/// ****************************************
#if VLB_INSTANCING_API_AVAILABLE && VLB_GPU_INSTANCING
    #if UNITY_VERSION < 201730 // https://unity3d.com/fr/unity/beta/unity2017.3.0b1
        // PRE UNITY 2017.3
        // for some reason, letting the default UNITY_MAX_INSTANCE_COUNT value generates the following error:
        // "Internal error communicating with the shader compiler process"
        #define UNITY_MAX_INSTANCE_COUNT 150
        #define VLB_DEFINE_PROP_START UNITY_INSTANCING_CBUFFER_START(Props)
        #define VLB_DEFINE_PROP_END UNITY_INSTANCING_CBUFFER_END
        #define VLB_GET_PROP(name) UNITY_ACCESS_INSTANCED_PROP(name)
    #else
        // POST UNITY 2017.3
        #define VLB_DEFINE_PROP_START UNITY_INSTANCING_BUFFER_START(Props)
        #define VLB_DEFINE_PROP_END UNITY_INSTANCING_BUFFER_END(Props)
        #define VLB_GET_PROP(name) UNITY_ACCESS_INSTANCED_PROP(Props, name)
    #endif

    #define VLB_DEFINE_PROP(type, name) UNITY_DEFINE_INSTANCED_PROP(type, name)
#elif VLB_SRP_API && VLB_SRP_BATCHER
    #define VLB_DEFINE_PROP_START CBUFFER_START(UnityPerMaterial)
    #define VLB_DEFINE_PROP_END CBUFFER_END
    #define VLB_DEFINE_PROP(type, name) type name;
    #define VLB_GET_PROP(name) name
#else
    #define VLB_DEFINE_PROP_START
    #define VLB_DEFINE_PROP_END
    #define VLB_DEFINE_PROP(type, name) uniform type name;
    #define VLB_GET_PROP(name) name
#endif
/// ****************************************

#endif
