using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorsoRotation : MonoBehaviour
{
    public GameObject upperSpine;
    public GameObject middleSpine;
    public GameObject lowerSpine;

    public GameObject aimTarget;

    private void FixedUpdate()
    {
        RotateTowardsTarget();
    }

    public void RotateTowardsTarget()
    {
        var direction = (transform.position - aimTarget.transform.position).normalized;
        middleSpine.transform.rotation = Quaternion.LookRotation(direction, transform.forward);
    }
}
