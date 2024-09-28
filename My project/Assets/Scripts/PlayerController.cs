using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float sprintMultiplier = 1.5f; // Multiplier for sprinting speed
    private float originalSpeed; // To store the initial move speed

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift; // Key for sprinting

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Camera Settings")]
    public Camera playerCamera; // Holds the camera in the inspector
    public float normalFOV = 80f;
    public float sprintFOV = 100f;
    public float fovChangeSpeed = 10f;

    public float shakeDuration = 0.1f;
    public float shakeMagnitude = 0.1f;
    private float shakeTime;

    [Header("Camera Sway Settings")]
    public float verticalShakeMagnitude = 0.05f; // Magnitude of vertical shake
    public float horizontalShakeMagnitude = 0.05f; // magnitude of horizontal shake

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Vector3 originalCameraPosition;

    Rigidbody rb;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        originalSpeed = moveSpeed; // Store the original speed at the start

        // Init the camera's FOV
        if(playerCamera != null)
        {
            playerCamera.fieldOfView = normalFOV;
            originalCameraPosition = playerCamera.transform.localPosition;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
        SpeedControl();
    }

    // Update is called once per frame
    private void Update()
    {
        // Ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        HandleSprint();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

        HandleCameraFOV();
        HandleCameraSway();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void HandleSprint()
    {
        // check if the sprint key is held down
        if (Input.GetKey(sprintKey) && grounded)
        {
            moveSpeed = originalSpeed * sprintMultiplier; // Increase speed by multiplier
        }
        else
        {
            moveSpeed = originalSpeed; // Reset speed to original when not sprinting
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in the air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void HandleCameraFOV()
    {
        // attempt to smoothly adjust camera FOV based on whether the player's sprinting or not
        if (Input.GetKey(sprintKey))
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, fovChangeSpeed * Time.deltaTime);
        }
        else
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, normalFOV, fovChangeSpeed * Time.deltaTime);
        }
    }

    private float swayTimer = 0f; // Timer controls sway
    public float swaySpeed = 2f; // speed of sway

    private void HandleCameraSway()
    {
        // Apply camera sway if moving faster than default speed
        if (moveSpeed > originalSpeed)
        {
            swayTimer += Time.deltaTime * swaySpeed;

            // Vertical dip (dipping down and back up)
            float verticalSwayAmount = Mathf.Sin(swayTimer) * verticalShakeMagnitude;
            // Ensure the camera only dips down by taking the negative absolute value
            float verticalDip = -Mathf.Abs(verticalSwayAmount);

            // horizontal Sway (moving left and right)
            float horizontalSwayAmount = Mathf.Sin(swayTimer * 2f) * horizontalShakeMagnitude;

            // Apply the sway to the camera's local position
            playerCamera.transform.localPosition = originalCameraPosition + new Vector3(horizontalSwayAmount, verticalDip, 0f);
        }
        else
        {
            // reset the camera position when not sprinting / at greater speed
            playerCamera.transform.localPosition = originalCameraPosition;
        }
    }
}