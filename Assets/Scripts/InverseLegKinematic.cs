using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverseLegKinematic : MonoBehaviour
{
    public bool isActive = false;

    public bool lLeg = false;
    public bool rLeg = false;

    public GameObject lTarget;
    public GameObject rTarget;

    [SerializeField] private GameObject mainHip;
    [SerializeField] private GameObject root;

    public GameObject[] leftLegChain;
    public GameObject[] rightLegChain;

    public float distanceL;
    public float distanceR;

    // Start is called before the first frame update
    void Start()
    {

    }

    public float getLength(GameObject[] chain)
    {
        float length = 0;

        for (int i = 0; i < chain.Length - 1; i++)
        {
            length += Vector3.Distance(chain[i].transform.position, chain[i + 1].transform.position);
        }

        return length;
    }

    private void Awake()
    {
        // get all leg chains
        leftLegChain = twoBoneIKChainList(mainHip.transform.GetChild(0).gameObject);
        rightLegChain = twoBoneIKChainList(mainHip.transform.GetChild(1).gameObject);

        // get their default max length to maybe save processing power
        distanceL = getLength(leftLegChain);
        distanceR = getLength(rightLegChain);

        
    }

    private void solveIK(GameObject[] chain, GameObject goal, float chainLength)
    {
        var distance = (chain[0].transform.position - goal.transform.position).magnitude;  

        if (distance > chainLength)
        {

        }
        else
        {
            backwardKineMatic(leftLegChain, lTarget);
            forwardKineMatic(leftLegChain, lTarget);
        }
    }

    private void backwardKineMatic(GameObject[] chain, GameObject goal)
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

    private void forwardKineMatic(GameObject[] chain, GameObject goal)
    {
        for (int i = 0; i < chain.Length - 1; i++)
        {
            if (i < 3)
            {
                var velocity = (chain[i + 1].transform.position - chain[i].transform.position).normalized;
                var speed = Vector3.Distance(chain[i + 1].transform.position, chain[i].transform.position);
                chain[i].transform.position = velocity * speed;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        solveIK(leftLegChain, lTarget, distanceL);
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
    private GameObject[] twoBoneIKChainList(GameObject hip)
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

        return legChain.ToArray();
    }
}

