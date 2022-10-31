using System;
using System.Collections.Generic;
using UnityEngine;

public class ScaleSettings : ScriptableObject
{
    [SerializeField] private List<ScaleData> listScaleData; // 0x18
    [SerializeField] private float scaleRatio; // 0x20
    [SerializeField] private bool isLoli; // 0x24
    [SerializeField] private int breastUp; // 0x28
    [SerializeField] private int breastDown; // 0x2c
    [SerializeField] private int breastLarger; // 0x30
    [SerializeField] private int breastSmall; // 0x34

    public bool IsLoli { get => isLoli; set => isLoli = value; }
    public float ScaleRatio { get => scaleRatio; set => scaleRatio = value; }
    public List<ScaleData> ListScaleData { get => listScaleData; }
    public int BreastUp { get => breastUp; set => breastUp = value; }
    public int BreastDown { get => breastDown; set => breastDown = value; }
    public int BreastLarger { get => breastLarger; set => breastLarger = value; }
    public int BreastSmall { get => breastSmall; set => breastSmall = value; }
}

[Serializable]
public class ScaleData
{
    private string m_BoneName; // 0x10
    private Vector3 m_Scale; // 0x18

    public String BoneName { get => m_BoneName; }
    public Vector3 Scale { get => m_Scale; }
}

