using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

public class MovementControllerAnimationRigging : MonoBehaviour
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
    [SerializeField] private Vector3 floorOffset;

    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 sLeftOffset;
    [SerializeField] private Vector3 sRightOffset;
    [SerializeField] private Vector3 leftOffset;
    [SerializeField] private Vector3 rightOffset;

    [SerializeField] private Vector3 leftNewPos;
    [SerializeField] private Vector3 rightNewPos;
    private int layerMask = 1 << 6;  // Bit shift the index of the layer (6) to get a bit mask
    private Vector3 surfaceNormal; // current surface normal
    private Vector3 myNormal; // character normal

    [Header("Limbs")]
    [SerializeField] private TwoBoneIKConstraint leftLeg;
    [SerializeField] private TwoBoneIKConstraint rightLeg;

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

            if (direction > 0.5f || direction < -0.5f)    {
                VelocityWalk(leftLeg, sLeftOffset, true);
                VelocityWalk(rightLeg, sRightOffset, false);
            }
            else
            {
                VelocityWalk(leftLeg, Vector3.zero, true);
                VelocityWalk(rightLeg, Vector3.zero, false);
            }
        }
        else
        {
            ResetWalk(leftLeg, leftOffset, true);
            ResetWalk(rightLeg, rightOffset, false);
        }

        ApplyFootIK(leftLeg);
        ApplyFootIK(rightLeg);
    }

    public void ResetWalk(TwoBoneIKConstraint leg, Vector3 legOffset, bool leftLeg)
    {
        RaycastHit hit;

        if (Physics.Raycast(leg.data.root.transform.position + legOffset, Vector3.down, out hit, 100, layerMask))
        {
            if (leftLeg)
            {
                leftNewPos = hit.point + floorOffset;
            }
            else
            {
                rightNewPos = hit.point + floorOffset;
            }
        }

        if (leftLeg)
        {
            leg.data.target.transform.position = Vector3.Slerp(leg.data.target.transform.position, leftNewPos, lerpSpeed * Time.deltaTime);
        }
        else
        {
            leg.data.target.transform.position = Vector3.Slerp(leg.data.target.transform.position, rightNewPos, lerpSpeed * Time.deltaTime);
        }
    }

    public void VelocityWalk(TwoBoneIKConstraint leg, Vector3 legOffset, bool leftLeg)
    {
        RaycastHit hit;

        if (Physics.Raycast(leg.data.root.transform.position, Vector3.down, out hit, 100, layerMask))
        {
            Vector3 direction = rb.velocity.normalized;

            var maxForwardStep = hit.point + legOffset + direction * maxFootDistance;

            var dir2 = (maxForwardStep - leg.data.root.transform.position).normalized;

            // if outside area 
            if (Vector3.Distance(transform.root.transform.position + legOffset, leg.data.target.transform.position) > maxFootDistance)
            {
                // raycast on object if higher
                if (Physics.Raycast(leg.data.root.transform.position, dir2, out hit, 100, layerMask))
                {
                    //leg.newpos = hit.point + leg.floorOffset;
                    if (leftLeg)
                    {
                        leftNewPos = hit.point + floorOffset;
                    }
                    else {
                        rightNewPos = hit.point + floorOffset;
                    }
                }
            }
        }
        if (leftLeg)
        {
            leg.data.target.transform.position = Vector3.Slerp(leg.data.target.transform.position, leftNewPos, lerpSpeed * Time.deltaTime);
        }
        else
        {
            leg.data.target.transform.position = Vector3.Slerp(leg.data.target.transform.position, rightNewPos, lerpSpeed * Time.deltaTime);
        }
    }

    public void ApplyFootIK(TwoBoneIKConstraint leg)
    {
        RaycastHit hit;

        if (Physics.Raycast(leg.data.tip.transform.position, transform.TransformDirection(Vector3.down), out hit, 100, layerMask))
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
        leg.data.tip.transform.rotation = Quaternion.Lerp(leg.data.tip.transform.rotation, targetRot, lerpSpeed * Time.deltaTime);
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
        Gizmos.DrawSphere(leftNewPos, 0.1f);
        Gizmos.DrawSphere(rightNewPos, 0.1f);
    }
} 
