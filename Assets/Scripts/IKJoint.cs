using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKJoint : ScriptableObject
{
    public GameObject joint;

    public bool isRoot = false;
    public bool isEnd = false;
    public bool hasBone = false;

    public Quaternion rotation;
    public float boneLength;

    // Start is called before the first frame update
    public IKJoint(GameObject joint, Quaternion rotation)
    {
        this.joint = joint;
        this.rotation = rotation;
    }
}
