using UnityEngine;

namespace LapisPlayer
{
    public class BootStrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void OnRuntimeMethodLoad()
        {
            QualitySettings.SetQualityLevel(ConfigManager.Instance.QualityLevel);

            bool isFullScreen = ConfigManager.Instance.FullScreen;
            int screenWidth = Display.main.systemWidth;
            int screenHeight = Display.main.systemHeight;

            Debug.Log($"Get screen resolution: {screenWidth}*{screenHeight}");

            if (!isFullScreen)
            {
                var resolution = screenWidth switch
                {
                    > 1920 => (1920, 1080),
                    > 1600 => (1600, 900),
                    > 1366 => (1366, 768),
                    > 1280 => (1280, 720),
                    _ => (1280, 720)
                };

                Screen.SetResolution(resolution.Item1, resolution.Item2, false);
            }
            else
            {
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                Screen.fullScreen = true;
                Screen.SetResolution(screenWidth, screenHeight, true);
            }
        }

    }
}
