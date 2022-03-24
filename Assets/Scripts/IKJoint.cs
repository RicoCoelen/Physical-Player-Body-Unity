using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKJoint
{
    public GameObject joint;

    public bool isRoot = false;
    public bool isEnd = false;
    public bool hasBone = false;

    public Quaternion m_rotation;
    public float length;

    // Start is called before the first frame update
    public IKJoint(GameObject joint, Quaternion rotation)
    {
        this.joint = joint;
        this.m_rotation = rotation;
    }
}
