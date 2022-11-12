using UnityEngine;


public class ReferenceObject : MonoBehaviour
{
    [SerializeField]
    public SceneObject Data; // 0x18

    public string GUID { get; set; }
    public GameObject GetGameObject ()
    {
        return gameObject;
    }
}

