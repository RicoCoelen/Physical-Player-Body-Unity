using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private float movementSpeed = 1;

    [SerializeField] private float jumpHeight = 1;
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float groundDistanceCheck = 0.1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();    
    }
    private void FixedUpdate()
    {
        isGrounded = Grounded();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if(isGrounded)
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
    }

    public void HandleMovement(InputAction.CallbackContext context)
    {
        var movementVector = context.ReadValue<Vector2>();
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
} 
