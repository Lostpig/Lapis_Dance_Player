using UnityEngine;
using LapisPlayer;

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
        GameObject timelinePrefab = AssetBundleLoader.Instance.LoadAsset<GameObject>($"battle/song/MUSIC_0012/CM Timeline");
        var obj = GameObject.Instantiate(timelinePrefab);

        // var ctrl = AssetBundleLoader.Instance.LoadAsset<RuntimeAnimatorController>($"battle/common/animations/CM View");
        // var anis = obj.GetComponentsInChildren<Animator>();
        // foreach(var ani in anis)
        // {
        //     ani.runtimeAnimatorController = ctrl;
        // }
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
