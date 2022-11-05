#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace VLB
{
    [CustomEditor(typeof(ConfigOverride))]
    public class ConfigOverrideEditor : ConfigEditor // useless override, only useful for backward compatibility
    {
    }
}
#endif
