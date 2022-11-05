#if UNITY_EDITOR && UNITY_2018_1_OR_NEWER
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

class OnBuildPreprocess : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        VLB.PlatformHelper.SetBuildTargetOverride(report.summary.platform);
    }
}
#endif
