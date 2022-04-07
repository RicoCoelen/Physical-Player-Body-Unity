using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IkChain : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Bones")]
    [SerializeField] public GameObject root; // fill root bone
    [SerializeField] public GameObject[] chain;
    [SerializeField] private float[] boneLength;    
    [SerializeField] public float maxDistance; // total bone length

    [Header("Points")]
    [SerializeField] public GameObject Target;
    [SerializeField] public GameObject Hint;
    [SerializeField] public Vector3 newpos;

    [Header("Rotation Offset")]
    [SerializeField] public Vector3 offsetRotation; // offset if rotation weird
    public Vector3 floorOffset = new Vector3(0, 0.125f, 0); // offset if rotation weird

    // delta correction distance kinematics, and iteration precision
    [Header("IK Variables")]
    public float delta = 0.001f;
    public int maxIterations = 5;
    public float attractionStrength = 5f;
    public float chainLerpSpeed = 50f;
    public bool currentLeg = false;


    private void Awake()
    {
        chain = TwoBoneIKChainList(root);

        boneLength = getBoneLengths(chain);

        maxDistance = AddLengths(boneLength);

        rb = GetComponent<Rigidbody>();

        // temp for now
        Hint = new GameObject($"{ transform.name }: Hint");
        Hint.transform.parent = transform.root.transform;
        Hint.transform.position = Hint.transform.position + transform.root.forward * 1;

        Target = new GameObject($"{ transform.name }: Target");
    }

    private float[] getBoneLengths(GameObject[] chain)
    {
        List<float> boneLength = new List<float>();

        for (int i = 0; i < chain.Length - 1; i++)
        {
            var tempLength = Vector3.Distance(chain[i].transform.position, chain[i + 1].transform.position);
            boneLength.Add(tempLength);
        }
        return boneLength.ToArray();
    }

    private float AddLengths(float[] boneLength)
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

        if (distance > chainMaxSize + (floorOffset.y * 0.9f)) // minimal stretching
        {
            StretchKinematics(chain, goal, chainLength);
            RotateAllStretchedJoints(chain);
        }
        else
        {
            for (int iterations = 0; iterations < maxIterations; iterations++)
            {
                BackwardKineMatic(chain, goal, hint, chainLength);
                ForwardKineMatic(chain, chainLength);

                if ((chain[chain.Length - 1].transform.position - goal.transform.position).sqrMagnitude < delta)
                {
                    break;
                }
            }
        }
        RotateAllJoints(chain);

    }

    private void RotateAllJoints(GameObject[] chain)
    {
        for (int i = 0; i < chain.Length; i++)
        {
            if (chain.Length - 1 == i)
                break;

            var direction = chain[i + 1].transform.position - chain[i].transform.position;
            Quaternion newRotation = Quaternion.LookRotation(direction.normalized, chain[i].transform.forward);

            // apply rotation with offset
            chain[i].transform.rotation = Quaternion.identity;
            chain[i].transform.rotation = newRotation;
            chain[i].transform.Rotate(offsetRotation);
        }
    }
    
    private void RotateAllStretchedJoints(GameObject[] chain)
    {
        for (int i = 0; i < chain.Length; i++)
        {
            if (chain.Length - 1 != i)
            {
                var direction = (chain[i + 1].transform.position - chain[i].transform.position).normalized;
                Quaternion newRotation = Quaternion.LookRotation(direction, transform.root.forward);

                // apply rotation with offset
                chain[i].transform.rotation = Quaternion.identity;
                chain[i].transform.rotation = newRotation;
                chain[i].transform.Rotate(offsetRotation);
            }
        }
    }

    private void StretchKinematics(GameObject[] chain, GameObject target, float[] boneLength)
    {
        // get the direction
        var direction = (target.transform.position - chain[0].transform.position).normalized;
        // save original root position
        var rootPos = chain[0];

        // stretch every bone to the max bonelength towards the target 
        for (int i = 0; i < chain.Length; i++)
        {
            if (i == 0)
            {
                chain[i].transform.position = rootPos.transform.position;
            }
            else
            {
                chain[i].transform.position = chain[i - 1].transform.position + direction * boneLength[i - 1];
            }
        }

        // put hip in the original position
        chain[0] = rootPos;
    }

    private void BackwardKineMatic(GameObject[] chain, GameObject goal, GameObject hint, float[] chainLength)
    {
        var rootPos = chain[0];

        for (int i = chain.Length - 1; i > 0; i--)
        {
            if (i == chain.Length - 1)
            {
                //chain[i].transform.position = goal.transform.position;
                chain[i].transform.position = Vector3.MoveTowards(chain[i].transform.position, goal.transform.position, maxDistance);
            }
            else
            {
                // add a little bit of hint attraction to choose which way to bend
                var newdirection = (hint.transform.position - chain[i].transform.position).normalized;
                chain[i].transform.position = chain[i].transform.position + (newdirection * attractionStrength) * chainLength[i];

                // put current joint in the right postition
                var direction = (chain[i].transform.position - chain[i + 1].transform.position).normalized;
                chain[i].transform.position = chain[i + 1].transform.position + direction * chainLength[i];
            }
        }

        // put hip in the original position
        chain[0] = rootPos;
    }

    private void ForwardKineMatic(GameObject[] chain, float[] chainLength)
    {
        var rootPos = chain[0];

        for (int i = 0; i < chain.Length - 1; i++)
        {
            if (i != 0)
            {
                // algorithm
                chain[i].transform.position = chain[i - 1].transform.position + (chain[i].transform.position - chain[i - 1].transform.position).normalized * chainLength[i - 1];
            }
        }

        // put hip in the original position
        chain[0] = rootPos;
    }

    private void FixedUpdate()
    {
        SolveIK(chain, Target, Hint, boneLength, maxDistance);
    }

    // create chains to make it easy to swap models in the future
    private GameObject[] MakeIKChainList(GameObject hip)
    {
        // some temporary variables
        var boneChain = new List<GameObject>();
        var temp = hip;

        // loop trough the bones 
        while (true)
        {
            boneChain.Add(temp);
            if (temp.transform.childCount != 0)
            {
                temp = temp.transform.GetChild(0).gameObject;
            }
            else
            {
                break;
            }
        }

        // return to array
        return boneChain.ToArray();
    }

    // maybe simple to use for 2 bone ik
    private GameObject[] TwoBoneIKChainList(GameObject root)
    {
        // temp list
        var legChain = new List<GameObject>();

        // add upper hip
        legChain.Add(root);

        // get/add knee
        var knee = root.transform.GetChild(0).gameObject;
        legChain.Add(knee);

        // get/add foot
        var foot = knee.transform.GetChild(0).gameObject;
        legChain.Add(foot);

        // to array
        return legChain.ToArray();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(Target.transform.position, 0.01f);
        Gizmos.DrawSphere(Hint.transform.position, 0.01f);
    }
}

