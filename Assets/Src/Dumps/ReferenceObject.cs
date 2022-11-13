using UnityEngine;
using LapisPlayer;

public class ReferenceObject : MonoBehaviour
{
    [SerializeField]
    public SceneObject Data; // 0x18

    public string GUID { get => Data.GUID; }
    public GameObject GetGameObject ()
    {
        return gameObject;
    }

    private void OnDestroy()
    {
        ReferenceStore.Instance.DeleteReference(GUID);
    }
}

