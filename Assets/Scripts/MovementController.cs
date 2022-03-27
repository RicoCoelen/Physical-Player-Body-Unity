using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private float movementSpeed = 1;
    [SerializeField] private float jumpHeight = 1;
    [SerializeField] private float groundDistanceCheck = 0.1f;
    [SerializeField] private float lerpSpeed = 0.1f;
    [SerializeField] private float stepLength = 1f;
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private Vector3 offset;

    public GameObject upperSpine;
    public GameObject middleSpine;
    public GameObject lowerSpine;

    public IkChain leftLegTarget;
    public IkChain rightLegTarget;

    public InputAction fire;
    [SerializeField] private InputActionAsset controls;
    private InputActionMap _inputActionMap;

    private PlayerInput m_playerInput;
    private Vector2 m_lookAxis;
    private Vector2 m_moveAxis;

    private void Awake()
    {
        m_playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        m_playerInput.actions["Movement"].performed += HandleMovement;
        m_playerInput.actions["Movement"].started += HandleMovement;
        m_playerInput.actions["Movement"].canceled += HandleMovement;

    }

    private void OnDisable()
    {
        m_playerInput.actions["Movement"].performed -= HandleMovement;
        m_playerInput.actions["Movement"].started -= HandleMovement;
        m_playerInput.actions["Movement"].canceled -= HandleMovement;
    }

    private void FixedUpdate()
    {
        isGrounded = Grounded();
        Vector3 velocity = new Vector3(rb.velocity.x, 0, rb.velocity.y);
        RotateTowardsVelocity(velocity);
        //VelocityWalk(velocity);

    }

    public void VelocityWalk(Vector3 velocity, IkChain legChain)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.localPosition, Vector3.down, out hit))
        {
            float distance = rb.velocity.magnitude * stepLength;
            Vector3 direction = rb.velocity.normalized;

            Gizmos.DrawSphere(hit.point, 0.1f);
            Gizmos.DrawSphere(hit.point += direction * distance, 0.1f);
            Gizmos.DrawSphere(hit.point -= direction * distance, 0.1f);
        }


    }

    public void RotateTowardsVelocity(Vector3 velocity)
    {
        // transform.root.forward
        Quaternion temp = Quaternion.LookRotation(new Vector3(velocity.x, 0, velocity.z), transform.root.forward);
        
        middleSpine.transform.rotation = Quaternion.Lerp(middleSpine.transform.rotation, temp, Time.time * lerpSpeed);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if(isGrounded)
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
    }

    public void HandleMovement(InputAction.CallbackContext context)
    {
        var movementVector = context.ReadValue<Vector2>();

        // do controls here
        //controls.Player.LMB.performed += _ => LMBisPressed = true;
        //controls.Player.LMB.canceled += _ => LMBisPressed = false;
        rb.AddForce(new Vector3(movementVector.x, 0, movementVector.y) * movementSpeed, ForceMode.Force);
    }

    public bool Grounded()
    {
        // Bit shift the index of the layer (6) to get a bit mask
        int layerMask = 1 << 6;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        //layerMask = ~layerMask;

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
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

    }
} 
