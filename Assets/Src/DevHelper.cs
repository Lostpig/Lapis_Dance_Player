using UnityEngine;
using LapisPlayer;
using Oz.Timeline;

public class DevHelper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Load Battle Song")]
    public void LoadBattleSong()
    {
        GameObject timelinePrefab = AssetBundleLoader.Instance.LoadAsset<GameObject>($"battle/song/MUSIC_0026/CM Timeline");
        GameObject.Instantiate(timelinePrefab);

        GameObject arTtimelinePrefab = AssetBundleLoader.Instance.LoadAsset<GameObject>($"ar/song/MUSIC_0026/CM_Timeline_AR");
        GameObject.Instantiate(arTtimelinePrefab);
    }

    [ContextMenu("Load Character")]
    public void LoadCharacter()
    {
        CharactersStore.Instance.LoadActor("Alpha/r001");
    }

    [ContextMenu("Load Scene")]
    public void LoadScene()
    {
        var scene = AssetBundleLoader.Instance.LoadAsset<GameObject>("SceneAssets/Scene_stage/BG2004/Prefab/BG2004");
        Instantiate(scene);
    }

    [ContextMenu("Reset AB")]
    public void ResetAB ()
    {
        AssetBundleLoader.Instance.Reset();
    }

    [ContextMenu("Print Config")]
    public void PrintConfig()
    {
        Debug.Log(ConfigManager.Instance.Manifest);
        Debug.Log(ConfigManager.Instance.AssetBundles);
        Debug.Log(ConfigManager.Instance.SoundBanks);
        Debug.Log(ConfigManager.Instance.SoundExtension);
        Debug.Log(ConfigManager.Instance.PhysicalType);
    }
}
