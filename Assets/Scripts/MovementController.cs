using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] private float jumpHeight = 1;
    [SerializeField] private float movementSpeed = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();    
    }

    public void Jump(InputAction.CallbackContext context)
    {
        rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
    }

    public void HandleMovement(InputAction.CallbackContext context)
    {
        var movementVector = context.ReadValue<Vector2>();
        rb.AddForce(new Vector3(movementVector.x, 0, movementVector.y) * movementSpeed, ForceMode.Force);
    }
}
