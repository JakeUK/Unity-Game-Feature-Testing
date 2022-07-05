using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;

    #region External

    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;

    public bool playerCanJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;

    #endregion

    #region Internal

    private bool isWalking = false;
    private bool isGrounded = false;


    #endregion

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        CheckGrounded();

        if(playerCanJump && Input.GetKeyDown(jumpKey) && isGrounded) {
            Jump();
        }
    }

    private void FixedUpdate() {

        if (playerCanMove) {
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            if(targetVelocity.magnitude != 0 && isGrounded) {
                isWalking = true;
            } else {
                isWalking = false;
            }

            targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;

            Vector3 currentVelocity = rb.velocity;
            Vector3 velocityChange = (targetVelocity - currentVelocity);

            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);

            rb.AddForce(velocityChange, ForceMode.VelocityChange);

        }

    }

    private void CheckGrounded() {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * 0.5f), transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distanceToFloor = 0.75f;

        if(Physics.Raycast(origin, direction, out RaycastHit hit, distanceToFloor)) {
            Debug.DrawRay(origin, direction * distanceToFloor, Color.red);
            isGrounded = true;
        } else {
            isGrounded = false;
        }
    }

    private void Jump() {
        if (isGrounded) {
            rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
            isGrounded = false;
        }
    }
}
