using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverseLegKinematic : MonoBehaviour
{
    [SerializeField] private GameObject mainHip;

    public List<GameObject> leftLegChain;
    public List<GameObject> rightLegChain;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    private void Awake()
    {
        // get all leg chains
        leftLegChain = MakeIKChainList(mainHip.transform.GetChild(0).gameObject);
        rightLegChain = MakeIKChainList(mainHip.transform.GetChild(1).gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
    }



    // create chains to make it easy to swap models in the future
    private List<GameObject> MakeIKChainList(GameObject hip)
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

        return legChain;
    }
}
