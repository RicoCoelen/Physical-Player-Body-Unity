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

        if (distance > chainMaxSize)
        {
            StretchKinematics(chain, goal, chainLength, chainMaxSize);
        }
        else
        {
            BackwardKineMatic(chain, goal, hint, chainLength, chainMaxSize);
            //ForwardKineMatic(chain, goal, hint);
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
            }
        }

        // add rotation offset to match
        if (Vector3.Dot(direction, transform.forward) > 0) {
            // rotate hip towards target
            Quaternion newRotation = Quaternion.AngleAxis(maxLength, Vector3.forward) * Quaternion.LookRotation(direction);
            // apply rotation with offset
            chain[0].transform.rotation = new Quaternion(newRotation.x, newRotation.y, newRotation.z, newRotation.w);
            chain[0].transform.Rotate(-90, 0, 0);
        }
        else {
            // rotate hip towards target
            Quaternion newRotation = Quaternion.AngleAxis(maxLength, Vector3.forward) * Quaternion.LookRotation(-direction);
            // apply rotation with offset
            chain[0].transform.rotation = new Quaternion(newRotation.x, newRotation.y, newRotation.z, newRotation.w);
            chain[0].transform.Rotate(90, 0, 0);
        }
        //chain[0].transform.rotation = Quaternion.Inverse(chain[0].transform.rotation);

        // put hip in the original position
        chain[0] = rootPos;
    }

    private void BackwardKineMatic(GameObject[] chain, GameObject goal, GameObject hint, float[] chainLength, float chainMaxSize)
    {
        var root = chain[0];
        chain[0].transform.position = goal.transform.position;
        for (int i = chain.Length - 1; i >= 0; i--)
        {
            if (i < 0) { 
            var velocity = (chain[i - 1].transform.position - chain[i].transform.position).normalized;
            var speed = Vector3.Distance(chain[i - 1].transform.position, chain[i].transform.position);
            chain[i].transform.position = velocity * speed;
            }
        }
        chain[0] = root;
    }

    private void ForwardKineMatic(GameObject[] chain, GameObject goal, GameObject hint, float[] chainLength, float chainMaxSize)
    {
        var root = chain[0];
        for (int i = 0; i < chain.Length - 1; i++)
        {
            if (i < 3 && chain[i] != chain[1])
            {
                var velocity = (chain[i + 1].transform.position - chain[i].transform.position).normalized;
                var speed = Vector3.Distance(chain[i + 1].transform.position, chain[i].transform.position);
                chain[i].transform.position = velocity * speed;
            }
        }
        chain[0] = root;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        
        if (isActive)
        {
            if (lLeg)
            {

            }
            if (rLeg)
            {

            }
        }

    }

    private void LateUpdate()
    {
        SolveIK(leftLegChain, lTarget, lHint, boneLengthL, maxDistanceL);
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

