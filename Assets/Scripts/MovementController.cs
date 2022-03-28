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
        controls.Player.Look.started += HandleMouseMovement;
        controls.Player.Look.performed += HandleMouseMovement;
        controls.Player.Look.canceled += HandleMouseMovement;

        // jump
        controls.Player.Jump.started += Jump;
        controls.Player.Jump.performed += Jump;
        controls.Player.Jump.canceled += Jump;

        // enable controls
        controls.Enable();
    }

    private void OnDestroy()
    {
        controls.Player.Movement.started -= HandleMovement;
        controls.Player.Movement.performed -= HandleMovement;
        controls.Player.Movement.canceled -= HandleMovement;
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

        RotateTowardsVelocity();
        VelocityWalk(leftLeg);
        VelocityWalk(rightLeg);
    }

    public void VelocityWalk(IkChain legChain)
    {
        // TODO:

        
        RaycastHit hit;
        if (Physics.Raycast(legChain.transform.position, Vector3.down, out hit, 100, layerMask))
        {
            //float distance = rb.velocity.magnitude * stepLength;
            //Vector3 direction = rb.velocity.normalized;

            legChain.Target.transform.position = hit.point;
            //Gizmos.DrawSphere(hit.point += direction * distance, 0.1f);
            //Gizmos.DrawSphere(hit.point -= direction * distance, 0.1f);
        }
        
    }

    public void RotateTowardsVelocity()
    {
        if (Vector3.Dot(rb.velocity, rb.transform.forward) > 0 && rb.velocity.magnitude > 0.1f)
        {
            hips.transform.rotation = Quaternion.Slerp(hips.transform.rotation, Quaternion.LookRotation(rb.velocity), lerpSpeed);
        }
        else
        {
            hips.transform.rotation = Quaternion.Slerp(hips.transform.rotation, Quaternion.LookRotation(-rb.velocity), lerpSpeed);
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

    public void HandleMouseMovement(InputAction.CallbackContext context)
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
