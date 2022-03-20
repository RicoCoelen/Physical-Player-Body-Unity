using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverseLegKinematic : MonoBehaviour
{
    // if ik active
    public bool isActive = false;

    // check which leg need to do what to alternate
    public bool lLeg = false;
    public bool rLeg = false;

    // positions for the ik to go
    public GameObject lTarget;
    public GameObject rTarget;

    // hints for the bends
    public GameObject lHint;
    public GameObject rHint;

    // root bones
    [SerializeField] private GameObject mainHip;
    [SerializeField] private GameObject root;

    // bone chainess
    public GameObject[] leftLegChain;
    public GameObject[] rightLegChain;

    // every bone length
    public float[] boneLengthL;
    public float[] boneLengthR;

    // total bone length
    public float maxDistanceL;
    public float maxDistanceR;

    // delta correction distance kinematics, and iteration precision
    public float delta = 0.001f;
    public int maxIterations = 5;
    public float attractionStrength = 5f;
    public float floorOffset = 0.125f;

    private void Awake()
    {
        // create all leg chains
        leftLegChain = TwoBoneIKChainList(mainHip.transform.GetChild(0).gameObject);
        rightLegChain = TwoBoneIKChainList(mainHip.transform.GetChild(1).gameObject);

        // get their default length to maybe save processing power in the future
        boneLengthL = getBoneLengths(leftLegChain);
        boneLengthR = getBoneLengths(rightLegChain);

        // full lengths
        maxDistanceL = addLengths(boneLengthL);
        maxDistanceR = addLengths(boneLengthR);
    }

    public float[] getBoneLengths(GameObject[] chain)
    {
        List<float> boneLength = new List<float>();

        for (int i = 0; i < chain.Length - 1; i++)
        {
            var tempLength = Vector3.Distance(chain[i].transform.position, chain[i + 1].transform.position);
            boneLength.Add(tempLength);
        }
        return boneLength.ToArray();
    }

    public float addLengths(float[] boneLength)
    {
        float length = 0;
        for (int i = 0; i < boneLength.Length; i++)
        {
            length += boneLength[i];
        }
        return length;
    }

    private void SolveIK(GameObject[] chain, GameObject goal, GameObject hint, float[] chainLength, float chainMaxSize)
    {
        var distance = (chain[0].transform.position - goal.transform.position).magnitude;

        if (distance > chainMaxSize + floorOffset)
        {
            StretchKinematics(chain, goal, chainLength, chainMaxSize);
        }
        else
        {
            for (int iterations = 0; iterations < maxIterations; iterations++)
            {
                BackwardKineMatic(chain, goal, hint, chainLength);
                ForwardKineMatic(chain, goal, hint, chainLength);

                if ((chain[chain.Length - 1].transform.position - goal.transform.position).sqrMagnitude < delta * delta);
                    break;
            }
            RotateAllJoints(chain);
        }
    }

    private void RotateAllJoints(GameObject[] chain)
    {
        for (int i = 0; i < chain.Length; i++)
        {
            if (chain.Length - 1 == i)
                break;

            var direction = (chain[i+1].transform.position - chain[i].transform.position).normalized;

            Quaternion newRotation = Quaternion.LookRotation(direction, Vector3.forward);

            // apply rotation with offset
            chain[i].transform.rotation = newRotation;
            chain[i].transform.Rotate(-90, 0, 0);
        }
    }

    // stretch 
    void StretchKinematics(GameObject[] chain, GameObject target, float[] boneLength, float maxLength)
    {
        // get the direction
        var direction = (target.transform.position - chain[0].transform.position).normalized;
        // save original root position
        var rootPos = chain[0];
        
        // stretch every bone to the max bonelength towards the target 
        for (int i=0; i < chain.Length; i++) {
            if(i!=0) {
                chain[i].transform.position = chain[i - 1].transform.position + direction * boneLength[i - 1];
                chain[i].transform.rotation = Quaternion.LookRotation(direction, Vector3.forward);
                chain[i].transform.Rotate(-90, 0, 0);
            }
        }

        if (Vector3.Dot(direction, -transform.up) > 0)
        {
            // rotate hip towards target // and offset quaternion to make it viable for walking took me a while
            Quaternion newRotation = Quaternion.LookRotation(direction, Vector3.forward);

            // apply rotation with offset
            chain[0].transform.rotation = newRotation;
            chain[0].transform.Rotate(-90, 0, 0);
        }
        else
        {
            // rotate hip towards target
            Quaternion newRotation = Quaternion.LookRotation(-direction, Vector3.forward);
            // apply rotation with offset
            chain[0].transform.rotation = newRotation;
            chain[0].transform.Rotate(90, 0, 0);
        }

        // put hip in the original position
        chain[0] = rootPos;
    }

    private void BackwardKineMatic(GameObject[] chain, GameObject goal, GameObject hint, float[] chainLength)
    {
        for (int i = chain.Length - 1; i > 0; i--)
        {
            if (i == chain.Length - 1) {
                chain[i].transform.position = goal.transform.position;
            }
            else
            {
                // add a little bit of hint attraction to choose which way to bend
                var newdirection = (hint.transform.position - chain[i].transform.position).normalized;
                chain[i].transform.position = chain[i].transform.position + newdirection * attractionStrength;

                var direction = (chain[i].transform.position - chain[i + 1].transform.position).normalized;
                chain[i].transform.position = chain[i + 1].transform.position + direction * chainLength[i];
              
                //chain[i].transform.position = Vector3.MoveTowards(chain[i].transform.position, hint.transform.position, chainLength[i]);
            }
        }
    }

    private void ForwardKineMatic(GameObject[] chain, GameObject goal, GameObject hint, float[] chainLength)
    {
        for (int i = 0; i < chain.Length - 1; i++)
        {
            if (i != 0)
            {
                // algorithm
                chain[i].transform.position = chain[i - 1].transform.position + (chain[i].transform.position - chain[i - 1].transform.position).normalized * chainLength[i - 1];
            }
        }
    }

    private void LateUpdate()
    {
        SolveIK(leftLegChain, lTarget, lHint, boneLengthL, maxDistanceL);
        SolveIK(rightLegChain, rTarget, rHint, boneLengthR, maxDistanceR);
    }

    // create chains to make it easy to swap models in the future
    private GameObject[] MakeIKChainList(GameObject hip)
    {
        // some temporary variables
        var boneChain = new List<GameObject>();
        var loop = true;
        var temp = hip;

        // loop trough the bones 
        while (loop == true)
        {
            boneChain.Add(temp);
            if(temp.transform.childCount != 0)
            {
                temp = temp.transform.GetChild(0).gameObject;
            }
            else
            {
                loop = false;
            }
        }
        // return to array
        return boneChain.ToArray();
    }

    // maybe simple to use for 2 bone ik
    private GameObject[] TwoBoneIKChainList(GameObject hip)
    {
        // temp list
        var legChain = new List<GameObject>();
   
        // add upper hip
        legChain.Add(hip);

        // get/add knee
        var knee = hip.transform.GetChild(0).gameObject;
        legChain.Add(knee);

        // get/add foot
        var foot = knee.transform.GetChild(0).gameObject;
        legChain.Add(foot);

        // to array
        return legChain.ToArray();
    }

    private void OnDrawGizmos()
    {
        // positions for the ik to go
        Gizmos.DrawSphere(lTarget.transform.position, 0.1f);
        Gizmos.DrawSphere(rTarget.transform.position, 0.1f);

        // hints for the bends
        Gizmos.DrawSphere(lHint.transform.position, 0.1f);
        Gizmos.DrawSphere(rHint.transform.position, 0.1f);
    }
}

