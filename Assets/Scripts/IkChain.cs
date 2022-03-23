using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IkChain : MonoBehaviour
{
    public IKJoint[] rootChain;

    public GameObject target;
    public GameObject hint;

    public GameObject rootIKJoint;
    public int maxJoints = 3;

    // delta correction distance kinematics, and iteration precision
    public float delta = 0.001f;
    public int maxIterations = 5;
    public float attractionStrength = 5f;
    public float floorOffset = 0.125f;

    public float maxChainLength;

    private void Awake()
    {
        // create all leg chains
       rootChain = MakeIKChainList(rootIKJoint, maxJoints);
    }

    // create chains to make it easy to swap models in the future
    private IKJoint[] MakeIKChainList(GameObject root, int joints)
    {
        // some temporary variables
        var chain = new IKJoint[joints];
        var temp = root;

        // loop trough the bones 
        for(int i = 0; i < joints; i++)
        {
            if (temp.transform.childCount != 0)
            {
                temp = temp.transform.GetChild(0).gameObject;
                chain[i] = new IKJoint(temp, temp.transform.rotation);
            }
        }
     
        // return to array
        return chain;
    }

    public float addLengths(float[] boneLength)
    {
        float length = 0;
        for (int i = 0; i < boneLength.Length; i++)
        {
            length += boneLength[i];
        }
        length += floorOffset;
        return length;
    }

    private void FixedUpdate()
    {
        SolveIK(rootChain, target, hint, maxChainLength);
    }


    private void RotateAllJoints(IKJoint[] chain)
    {
        for (int i = 0; i < chain.Length; i++)
        {
            if (chain.Length - 1 == i)
                break;

            var direction = (chain[i + 1].joint.transform.position - chain[i].joint.transform.position).normalized;
            Quaternion newRotation = Quaternion.LookRotation(direction, transform.forward);

            // apply rotation with offset
            chain[i].joint.transform.rotation = newRotation;
            chain[i].joint.transform.Rotate(-90, 0, 0);
        }
    }

    public void SolveIK(IKJoint[] chain, GameObject goal, GameObject hint, float chainMaxSize)
    {
        var distance = (chain[0].joint.transform.position - goal.transform.position).magnitude;

        if (distance > chainMaxSize + floorOffset)
        {
            StretchKinematics(chain, goal);
        }
        else
        {
            for (int iterations = 0; iterations < maxIterations; iterations++)
            {
                BackwardKineMatic(chain, goal, hint);
                ForwardKineMatic(chain);

                if ((chain[chain.Length - 1].joint.transform.position - goal.transform.position).sqrMagnitude < delta * delta)
                {
                    break;
                }
            }
            RotateAllJoints(chain);
        }
    }

    void StretchKinematics(IKJoint[] chain, GameObject target)
    {
        // get the direction
        var direction = (target.transform.position - chain[0].joint.transform.position).normalized;
        // save original root position
        var rootPos = chain[0];

        // stretch every bone to the max bonelength towards the target 
        for (int i = 0; i < chain.Length; i++)
        {
            if (i != 0)
            {
                chain[i].joint.transform.position = chain[i - 1].joint.transform.position + direction * chain[i - 1].boneLength;
                chain[i].joint.transform.rotation = Quaternion.LookRotation(direction, transform.forward);
                chain[i].joint.transform.Rotate(-90, 0, 0);
            }
        }

        if (Vector3.Dot(direction, -transform.up) > 0)
        {
            // rotate hip towards target // and offset quaternion to make it viable for walking took me a while
            Quaternion newRotation = Quaternion.LookRotation(direction, transform.forward);

            // apply rotation with offset
            chain[0].joint.transform.rotation = newRotation;
            chain[0].joint.transform.Rotate(-90, 0, 0);
        }
        else
        {
            // rotate hip towards target
            Quaternion newRotation = Quaternion.LookRotation(-direction, transform.forward);
            // apply rotation with offset
            chain[0].joint.transform.rotation = newRotation;
            chain[0].joint.transform.Rotate(90, 0, 0);
        }

        // put hip in the original position
        chain[0] = rootPos;
    }

    public void BackwardKineMatic(IKJoint[] chain, GameObject goal, GameObject hint)
    {
        for (int i = chain.Length - 1; i > 0; i--)
        {
            if (i == chain.Length - 1)
            {
                //chain[i].joint.transform.position = goal.transform.position;
                chain[i].joint.transform.position = Vector3.MoveTowards(chain[i].joint.transform.position, goal.transform.position, chain[i - 1].boneLength);
            }
            else
            {
                // add a little bit of hint attraction to choose which way to bend
                var newdirection = (hint.transform.position - chain[i].joint.transform.position).normalized;
                chain[i].joint.transform.position = chain[i].joint.transform.position + (newdirection * attractionStrength) * chain[i].boneLength;

                var direction = (chain[i].joint.transform.position - chain[i + 1].joint.transform.position).normalized;
                chain[i].joint.transform.position = chain[i + 1].joint.transform.position + direction * chain[i].boneLength;
            }
        }
    }

    public void ForwardKineMatic(IKJoint[] chain)
    {
        for (int i = 0; i < chain.Length - 1; i++)
        {
            if (i != 0)
            {
                // algorithm
                chain[i].joint.transform.position = chain[i - 1].joint.transform.position + (chain[i].joint.transform.position - chain[i - 1].joint.transform.position).normalized * chain[i - 1].boneLength;
            }
        }
    }
}

