using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocoMotionMovement : MonoBehaviour
{
    [SerializeField] private GameObject lFoot;
    [SerializeField] private GameObject rFoot;

    [SerializeField] private GameObject lFootHint;
    [SerializeField] private GameObject rFootHint;

    public Rigidbody rb;

    // sine wave test
    public float amplitudeX = 10.0f;
    public float amplitudeY = 5.0f;
    public float omegaX = 1.0f;
    public float omegaY = 5.0f;
    public float index;
    public float offSet = 1f;
    public float stepOffset = 1f;

    public Vector3 velocity;

    private void Awake()
    {
        rb = transform.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        this.velocity = rb.velocity;
        SineWave();
    }

    public void SineWave()
    {
        index += Time.deltaTime;
        float x = amplitudeX * Mathf.Cos(omegaX * index);
        float y = Mathf.Abs(amplitudeY * Mathf.Sin(omegaY * index));

        lFoot.transform.localPosition = new Vector3(0 + offSet, y, x);
        rFoot.transform.localPosition = new Vector3(0 - offSet, y, -x);
    }

    public void LocoMotion()
    {

    }
}
