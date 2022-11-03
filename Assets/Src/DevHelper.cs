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

    [ContextMenu("Load Scene")]
    public void LoadScene()
    {
        var scene = AssetBundleLoader.Instance.LoadAsset<GameObject>("SceneAssets/Scene_stage/BG301/Prefab/BG301");
        Instantiate(scene);
    }

    [ContextMenu("Print dataPath")]
    public void PrintDataPath ()
    {
        Debug.Log("Unity DataPath:" + Application.dataPath);
        Debug.Log("Application Dir:" + System.Environment.CurrentDirectory);
    }

    [ContextMenu("Compute WWISE")]
    public void ComputeWwise()
    {
        uint num = fnv(2166136261, "Play_MUSIC_0025");
        Debug.Log(num);
    }
    private uint fnv (uint h, string s)
    {
        foreach(char c in s)
        {
            h = (h * 16777619) ^ c;
        }
        return h;
    }
}
