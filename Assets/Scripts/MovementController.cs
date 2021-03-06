using System;
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
    [SerializeField] private float movementSpeed = 1;
    [SerializeField] private float jumpHeight = 1;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float maxFootDistance = 1f;
    [SerializeField] private float stepHeight = 0.1f;

    [Header("Ground variables")]
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private float groundDistanceCheck = 0.1f;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 sLeftOffset;
    [SerializeField] private Vector3 sRightOffset;
    [SerializeField] private Vector3 leftOffset;
    [SerializeField] private Vector3 rightOffset;
    private int layerMask = 1 << 6;  // Bit shift the index of the layer (6) to get a bit mask
    private Vector3 surfaceNormal; // current surface normal
    private Vector3 myNormal; // character normal

    [Header("Bones")]
    [SerializeField] private GameObject upperSpine;
    [SerializeField] private GameObject middleSpine;
    [SerializeField] private GameObject lowerSpine;
    [SerializeField] private GameObject hips;

    [Header("Limbs")]
    [SerializeField] private IkChain leftLeg;
    [SerializeField] private IkChain rightLeg;

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

        myNormal = transform.up; // normal starts as character up direction 
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

        //RotateTowardsVelocity(leftLeg, rightLeg);
       
        if (rb.velocity.magnitude > 0.1f)
        {
            var direction = Vector3.Dot(transform.root.right, rb.velocity.normalized);
            Debug.Log(direction);

            if (direction > 0.5f || direction < -0.5f)    {
                VelocityWalk(leftLeg, sLeftOffset);
                VelocityWalk(rightLeg, sRightOffset);
            }
            else
            {
                VelocityWalk(leftLeg, Vector3.zero);
                VelocityWalk(rightLeg, Vector3.zero);
            }
        }
        else
        {
            ResetWalk(leftLeg, leftOffset);
            ResetWalk(rightLeg, rightOffset);
        }

        ApplyFootIK(leftLeg);
        ApplyFootIK(rightLeg);
    }

    public void ResetWalk(IkChain leg, Vector3 legOffset)
    {
        RaycastHit hit;

        if (Physics.Raycast(leg.root.transform.position + legOffset, Vector3.down, out hit, 100, layerMask))
        {
            leg.newpos = hit.point + leg.floorOffset;
        }
        leg.Target.transform.position = Vector3.Slerp(leg.Target.transform.position, leg.newpos, lerpSpeed * Time.deltaTime);
    }

    public void VelocityWalk(IkChain leg, Vector3 legOffset)
    {
        RaycastHit hit;

        if (Physics.Raycast(leg.root.transform.position, Vector3.down, out hit, 100, layerMask))
        {
            Vector3 direction = rb.velocity.normalized;

            var maxForwardStep = hit.point + legOffset + direction * maxFootDistance;

            var dir2 = (maxForwardStep - leg.transform.position).normalized;

            // if outside area
            if (Vector3.Distance(transform.root.transform.position + legOffset, leg.Target.transform.position) > maxFootDistance)
            {
                // raycast on object if higher
                if (Physics.Raycast(leg.root.transform.position, dir2, out hit, 100, layerMask))
                {
                    leg.newpos = hit.point + leg.floorOffset;
                }
            }
        }
        leg.Target.transform.position = Vector3.Slerp(leg.Target.transform.position, leg.newpos, lerpSpeed * Time.deltaTime);
    }

    public void ApplyFootIK(IkChain leg)
    {
        RaycastHit hit;

        if (Physics.Raycast(leg.chain[2].transform.position, Vector3.down, out hit, 100, layerMask))
        {
            if (isGrounded)
            {
                surfaceNormal = hit.normal;
            }
            else
            {
                surfaceNormal = Vector3.up;
            }
        }

        myNormal = Vector3.Lerp(myNormal, surfaceNormal, lerpSpeed * Time.deltaTime);

        // find forward direction with new myNormal:
        var myForward = Vector3.Cross(transform.right, myNormal);

        // align character to the new myNormal while keeping the forward direction:
        var targetRot = Quaternion.LookRotation(myForward, myNormal);

        // change rotation of last joint
        leg.chain[2].transform.rotation = Quaternion.Lerp(leg.chain[2].transform.rotation, targetRot, lerpSpeed * Time.deltaTime);
    }

    public void RotateTowardsVelocity()    
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

        hips.transform.rotation = Quaternion.Slerp(hips.transform.rotation, rotation, lerpSpeed * Time.deltaTime);

        // then rotate upper body towards front capsule
        upperSpine.transform.rotation = transform.root.rotation;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (isGrounded && rb.velocity.magnitude > 0.1f)
        {
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(Vector3.up * (jumpHeight * rb.velocity.magnitude) , ForceMode.Impulse);
        }
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

    private void OnDrawGizmos()
    {
        if (rb.velocity.magnitude > 0.1f)
        {
            Gizmos.DrawWireSphere(transform.root.position + sLeftOffset, maxFootDistance);
            Gizmos.DrawWireSphere(transform.root.position + sRightOffset, maxFootDistance);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.root.position + leftOffset, maxFootDistance);
            Gizmos.DrawWireSphere(transform.root.position + rightOffset, maxFootDistance);
        }
    }
} 
