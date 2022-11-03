using UnityEngine;
using UnityEngine.UI;

namespace LapisPlayer
{
    public static class UIUtility
    {
        public static Sprite LoadCharacterIcon(string shortName)
        {
            // 直接写死了
            string icon = $"UIDyAtlas/CharacterIcon/IC_IDH_{shortName}_01";
            var sprite = AssetBundleLoader.Instance.LoadAsset<Sprite>(icon);
            return sprite;
        }
        public static GameObject CreateCharacterButton (GameObject parent, string shortName)
        {
            GameObject button = new GameObject();
            button.AddComponent<CanvasRenderer>();
            button.AddComponent<RectTransform>();
            Button mButton = button.AddComponent<Button>();
            Image mImage = button.AddComponent<Image>();
            mButton.targetGraphic = mImage;

            var icon = LoadCharacterIcon(shortName);
            mImage.sprite = icon;

            button.transform.SetParent(parent.transform);
            return button;
        }
    }
}
