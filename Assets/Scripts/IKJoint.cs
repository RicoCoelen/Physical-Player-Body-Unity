using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKJoint
{
    public GameObject joint;

    public bool isRoot = false;
    public bool isEnd = false;
    public bool hasBone = false;

    public Quaternion start_rotation;
    public Vector3 start_position;
    public float length;

    // Start is called before the first frame update
    public IKJoint(GameObject joint, Quaternion rotation, Vector3 position)
    {
        this.joint = joint;
        this.start_rotation = rotation;
        this.start_position = position;
    }
}
