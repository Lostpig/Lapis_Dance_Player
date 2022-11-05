using UnityEngine;

public class AutoRotation : MonoBehaviour
{
    // Fields
    public float[] speedSingle; // 0x18
    public float agent; // 0x20
    public float speedTotal; // 0x24

    private Transform[] childItems;

    private void Start()
    {
        var count = gameObject.transform.childCount;
        if (count != speedSingle.Length)
        {
            Debug.LogWarning("AutoRotation: child transform count invalid");
        }

        childItems = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            childItems[i] = gameObject.transform.GetChild(i);
        }
    }
    private void Update()
    {
        for (int i = 0; i < childItems.Length; i++)
        {
            childItems[i].Rotate(Vector3.up, speedSingle[i] / speedTotal);
        }
    }
}

