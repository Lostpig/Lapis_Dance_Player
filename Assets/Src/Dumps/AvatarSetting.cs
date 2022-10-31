using UnityEngine;

public class AvatarSetting : ScriptableObject
{
    // Fields
    [SerializeField]
    private eAvatarType m_Type; // 0x18
    [SerializeField]
    private GameObject m_Prefab; // 0x20
    [SerializeField]
    private int m_Slot; // 0x28
    [SerializeField]
    private string m_Dummy; // 0x30
    [SerializeField]
    private HeelSetting m_HeelSetting; // 0x38
    [SerializeField]
    private SpringSetting m_SpringSetting; // 0x40
    [SerializeField]
    private bool m_RebindAnimator; // 0x48

    // Properties
    public eAvatarType Type { get { return m_Type; } }
    public GameObject Prefab { get { return m_Prefab; } }
    public int Slot { get { return m_Slot; } }
    public string Dummy { get { return m_Dummy; } }
    public HeelSetting HeelSetting { get { return m_HeelSetting; } }
    public SpringSetting SpringSetting { get { return m_SpringSetting; } }
    public bool RebindAnimator { get { return m_RebindAnimator; } }
}
