using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VLB
{
    public class PlatformHelper
    {
#if UNITY_EDITOR
        static BuildTarget ms_BuildTargetOverride = BuildTarget.NoTarget;

        public static void SetBuildTargetOverride(BuildTarget target)
        {
            ms_BuildTargetOverride = target;
            Config.Instance.RefreshShader(Config.RefreshShaderFlags.All);
        }

        static BuildTarget GetCurrentBuildTarget()
        {
            if (BuildPipeline.isBuildingPlayer && ms_BuildTargetOverride != BuildTarget.NoTarget) return ms_BuildTargetOverride;
            return EditorUserBuildSettings.activeBuildTarget;
        }

        static RuntimePlatform BuildTargetToRuntimePlatform(BuildTarget buildTarget)
        {
#pragma warning disable 0618 // obsolete BuildTargets
            switch (buildTarget)
            {
                case BuildTarget.Android:               return RuntimePlatform.Android;
                case BuildTarget.PS4:                   return RuntimePlatform.PS4;
                case BuildTarget.StandaloneLinux64:     return RuntimePlatform.LinuxPlayer;
                case BuildTarget.StandaloneWindows:     return RuntimePlatform.WindowsPlayer;
                case BuildTarget.StandaloneWindows64:   return RuntimePlatform.WindowsPlayer;
                case BuildTarget.WSAPlayer:             return RuntimePlatform.WSAPlayerARM;
                case BuildTarget.XboxOne:               return RuntimePlatform.XboxOne;
                case BuildTarget.iOS:                   return RuntimePlatform.IPhonePlayer;
                case BuildTarget.tvOS:                  return RuntimePlatform.tvOS;
                case BuildTarget.WebGL:                 return RuntimePlatform.WebGLPlayer;

#if UNITY_2017_1_OR_NEWER
                case BuildTarget.Switch:                return RuntimePlatform.Switch;
#endif

#if UNITY_2017_3_OR_NEWER
                case BuildTarget.StandaloneOSX:         return RuntimePlatform.OSXPlayer;
#else
                case BuildTarget.StandaloneOSXUniversal:return RuntimePlatform.OSXPlayer;
#endif

#if UNITY_2019_3_OR_NEWER
                case BuildTarget.Stadia:                return RuntimePlatform.Stadia;
#endif

#if UNITY_2020_2_OR_NEWER
                case BuildTarget.CloudRendering:        return RuntimePlatform.CloudRendering;
#endif

#if UNITY_2021_1_OR_NEWER
                case BuildTarget.GameCoreScarlett:      return RuntimePlatform.GameCoreScarlett;
                case BuildTarget.GameCoreXboxOne:       return RuntimePlatform.GameCoreXboxOne;
                case BuildTarget.PS5:                   return RuntimePlatform.PS5;
#endif

                // obsolete
                case BuildTarget.StandaloneOSXIntel:    return RuntimePlatform.OSXPlayer;
                case BuildTarget.StandaloneOSXIntel64:  return RuntimePlatform.OSXPlayer;
                case BuildTarget.StandaloneLinuxUniversal:return RuntimePlatform.LinuxPlayer;
                case BuildTarget.WiiU:                  return RuntimePlatform.WiiU;
                case BuildTarget.PSP2:                  return RuntimePlatform.PSP2;
                case BuildTarget.PS3:                   return RuntimePlatform.PS3;
                case BuildTarget.XBOX360:               return RuntimePlatform.XBOX360;

                default:                                return (RuntimePlatform)(-1);
            }
#pragma warning restore 0618
        }

        public static bool IsValidPlatformSuffix(string suffix)
        {
            if (string.IsNullOrEmpty(suffix))
                return true;

            foreach (var platform in System.Enum.GetNames(typeof(RuntimePlatform)))
            {
                if (suffix == platform)
                    return true;
            }
            return false;
        }
#endif // UNITY_EDITOR

        public static string GetCurrentPlatformSuffix()
        {
#if UNITY_EDITOR
            return GetPlatformSuffix(BuildTargetToRuntimePlatform(GetCurrentBuildTarget()));
#else
            return GetPlatformSuffix(Application.platform);
#endif
        }

        static string GetPlatformSuffix(RuntimePlatform platform)
        {
            return platform.ToString();
        }
    }
}