using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    private Rigidbody rb;

    private PlayerInputActions controls;

    private Vector2 movementVector;
    private bool isMoving = false;

    private Vector2 lookVector;
    private bool isLooking = false;

    [Header("Movement variables")]
    [SerializeField] private float lerpSpeed = 0.1f;
    [SerializeField] private float stepLength = 1f;
    [SerializeField] private float movementSpeed = 1;
    [SerializeField] private float jumpHeight = 1;
    [SerializeField] private float maxSpeed = 10f;
    private bool canWalk = true;
    [SerializeField] private float delay = 0.5f;
    private float timer;
    [SerializeField]  private float delta;

    [Header("Ground variables")]
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private float groundDistanceCheck = 0.1f;
    [SerializeField] private Vector3 offset;
    private int layerMask = 1 << 6;  // Bit shift the index of the layer (6) to get a bit mask
    
    [Header("Bones")]
    [SerializeField] private GameObject upperSpine;
    [SerializeField] private GameObject middleSpine;
    [SerializeField] private GameObject lowerSpine;
    [SerializeField] private GameObject hips;

    [Header("Limbs")]
    [SerializeField] private IkChain leftLeg;
    [SerializeField] private IkChain rightLeg;
    [SerializeField] private IkChain leftArm;
    [SerializeField] private IkChain rightArm;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        controls = new PlayerInputActions();

        // AWDS - movement
        controls.Player.Movement.started += HandleMovement;
        controls.Player.Movement.performed += HandleMovement;
        controls.Player.Movement.canceled += HandleMovement;

        // look rotation
        controls.Player.Look.started += HandleLookMovement;
        controls.Player.Look.performed += HandleLookMovement;
        controls.Player.Look.canceled += HandleLookMovement;

        // jump
        controls.Player.Jump.started += Jump;
        controls.Player.Jump.performed += Jump;
        controls.Player.Jump.canceled += Jump;

        // enable controls
        controls.Enable();
    }

    private void OnDestroy()
    {
        // AWDS - movement
        controls.Player.Movement.started -= HandleMovement;
        controls.Player.Movement.performed -= HandleMovement;
        controls.Player.Movement.canceled -= HandleMovement;

        // look rotation
        controls.Player.Look.started -= HandleLookMovement;
        controls.Player.Look.performed -= HandleLookMovement;
        controls.Player.Look.canceled -= HandleLookMovement;

        // jump
        controls.Player.Jump.started -= Jump;
        controls.Player.Jump.performed -= Jump;
        controls.Player.Jump.canceled -= Jump;
    }

    private void FixedUpdate()
    {
        isGrounded = Grounded();

        if (isMoving)
        {
            rb.AddForce(new Vector3(movementVector.x, 0, movementVector.y) * movementSpeed, ForceMode.Force);

            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }

        if (isLooking)
        {
            // Debug.Log(lookVector); // TODO: fix looking around using delta
        }

        RotateTowardsVelocity(leftLeg, rightLeg);
        VelocityWalk(leftLeg);
        VelocityWalk(rightLeg);
    }

    public void VelocityWalk(IkChain legChain)
    {
        // TODO:
        RaycastHit hit;

        if (Physics.Raycast(legChain.root.transform.position, Vector3.down, out hit, 100, layerMask))
        {
            float distance = rb.velocity.magnitude * stepLength;
            Vector3 direction = rb.velocity.normalized;

            var minForwardStep = hit.point += direction * distance;
            var maxForwardStep = hit.point -= direction * distance;

            var dir1 = (minForwardStep - legChain.transform.position).normalized;
            var dir2 = (maxForwardStep - legChain.transform.position).normalized;

            if (Physics.Raycast(legChain.root.transform.position, dir1, out hit, 100, layerMask))
            {
                legChain.minFeetDistance.transform.position = hit.point;
            }

            if (Physics.Raycast(legChain.root.transform.position, dir2, out hit, 100, layerMask))
            {
                legChain.maxFeetDistance.transform.position = hit.point;
            }

            if(Random.Range(0,1000) > 500)
            {
                legChain.Target.transform.position = Vector3.Lerp(legChain.maxFeetDistance.transform.position, legChain.Target.transform.position, lerpSpeed);
                 
            }
            else
            {
                legChain.Target.transform.position = Vector3.Lerp(legChain.minFeetDistance.transform.position, legChain.Target.transform.position, lerpSpeed);
            }
        }
    }

    public void RotateTowardsVelocity(IkChain leftChain, IkChain rightChain)
    {
        Quaternion rotation;

        if (Vector3.Dot(rb.velocity, rb.transform.forward) > 0 && rb.velocity.magnitude > 0.1f)
        {
            rotation = Quaternion.LookRotation(rb.velocity);
        }
        else
        {
            rotation = Quaternion.LookRotation(-rb.velocity);
        }

        hips.transform.rotation = Quaternion.Slerp(hips.transform.rotation, rotation, lerpSpeed);
     

        for (int i = 0; i < leftChain.chain.Length; i++)
        {
            if (i != 0)
            {
                //var temp = Quaternion.Slerp(leftChain.chain[i].transform.rotation, rotation, lerpSpeed);
            }
        }

        // deadzone fix for weird movements

        // then rotate upper body towards front capsule
        upperSpine.transform.rotation = transform.root.rotation;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if(isGrounded)
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
    }

    public void HandleMovement(InputAction.CallbackContext context)
    {
        if (!gameObject.activeSelf)
            return;

        if(context.canceled)
        {
            isMoving = false;
            movementVector = Vector3.zero;
        }
        else
        {
            isMoving = true;
            movementVector = context.ReadValue<Vector2>();
        }

    }

    public void HandleLookMovement(InputAction.CallbackContext context)
    {
        if (!gameObject.activeSelf)
            return;

        if (context.canceled)
        {
            isLooking = false;
            lookVector = Vector2.zero;
        }
        else
        {
            isLooking = true;
            lookVector = context.ReadValue<Vector2>();
        }
    }

    public bool Grounded()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + offset, transform.TransformDirection(Vector3.down), out hit, groundDistanceCheck, layerMask))
        {
            return true;
        }
        else { 
            return false;
        }
    }
} 
