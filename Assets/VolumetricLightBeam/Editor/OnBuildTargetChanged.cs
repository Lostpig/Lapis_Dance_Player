#if UNITY_EDITOR && UNITY_2018_1_OR_NEWER
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

namespace VLB
{
    public class ActiveBuildTargetListener : IActiveBuildTargetChanged
    {
        public int callbackOrder { get { return 0; } }
        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            Config.Instance.RefreshShader(Config.RefreshShaderFlags.All);
            GlobalMesh.Destroy();
            VolumetricLightBeam._EditorSetAllMeshesDirty();
        }
    }
}
#endif
