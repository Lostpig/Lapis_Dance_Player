using System;
using UnityEngine;

[Serializable]
public class SpringSetting : ScriptableObject
{
    public SpringBoneParameter[] boneParameters;
    public SpringColliderParameter[] colliderParameters;
}

[Serializable]
public class SpringBoneParameter
{
    public string nodeName;
    public string childPath;
    public string sibilingPath;
    public Vector3 boneAxis;
    public float radius;
    public float stiffnessForce;
    public float dragForce;
    public Vector3 externalForce;
    public Vector3 lowRotationLimit;
    public Vector3 highRotationLimit;
    public bool enableRotationLimit;
    public int[] colliderIDs;
}

[Serializable]
public class SpringColliderParameter
{
    public string nodeName;
    public float radius;
    public Vector3 offset;
    public string sibilingPath;
}
