using UnityEngine;

public class AutoRotation : MonoBehaviour
{
    // Fields
    public float[] speedSingle; // 0x18
    public float agent; // 0x20
    public float speedTotal; // 0x24

    private Transform[] childItems;
    private Quaternion[] originRotations;

    private void Start()
    {
        var count = gameObject.transform.childCount;
        if (count != speedSingle.Length)
        {
            Debug.LogWarning("AutoRotation: child transform count invalid");
        }

        childItems = new Transform[count];
        originRotations = new Quaternion[count];
        for (int i = 0; i < count; i++)
        {
            childItems[i] = gameObject.transform.GetChild(i);
            originRotations[i] = gameObject.transform.localRotation;
        }
    }
    private void Update()
    {
        var t = (Time.time * speedTotal) % 360;

        for (int i = 0; i < childItems.Length; i++)
        {
            childItems[i].localRotation = Quaternion.AngleAxis(t * speedSingle[i], Vector3.up);
        }
    }
}

