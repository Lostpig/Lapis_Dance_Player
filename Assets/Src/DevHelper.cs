using UnityEngine;
using LapisPlayer;
using Oz.GameKit.Version;
using Oz.GameFramework.Runtime;

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

    [ContextMenu("Read Manifest")]
    public void ReadManifest()
    {
        string manifestPath = ConfigManager.Instance.Manifest;
        var manifest = ManifestReader.Read(manifestPath);

        Debug.Log("Manifest loaded, version = " + manifest.Version);
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
