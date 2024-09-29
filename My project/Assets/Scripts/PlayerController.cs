using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Player Attributes")]
    public float health = 50f;
    public float maxHealth = 200f;
    public float defense = 5f;
    public bool isDead = false;

    [Header("Movement")]
    public float moveSpeed;
    public float sprintMultiplier = 1.5f; // Multiplier for sprinting speed
    private float originalSpeed; // To store the initial move speed

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Attack Settings")]
    public float attackRange = 50.0f;       // Range of the ranged attack
    public float attackDamage = 20f;        // Damage dealt by the attack
    public float attackCooldown = 1.0f;     // Time between attacks (set to 1 second)
    private bool canAttack = true;          // Whether the player can attack
    public LayerMask enemyLayer;            // Layer mask to specify which layers are enemies
    public float knockbackForce = 10f;      // Force applied to knock back the enemy

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

    [Header("DOM Effects")]
    private bool isFlamesEffectActive = false;
    private Coroutine flamesCoroutine;

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
        if (isDead)
            return;

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

    // ==================== MOVEMENT AND GAMEPLAY FUNCTIONS ===================

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
        
        // Attack input
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        canAttack = false;

        // Define the raycast origin and direction
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;

        RaycastHit hit;

        // Perform the raycast
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, attackRange))
        {
            // Check if the object hit is on the enemy layer
            if (((1 << hit.collider.gameObject.layer) & enemyLayer) != 0)
            {
                // Get the EnemyController script
                EnemyController enemy = hit.collider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    // Apply damage to the enemy
                    enemy.TakeDamage(attackDamage);

                    // Apply knockback to the enemy
                    Rigidbody enemyRb = hit.collider.GetComponent<Rigidbody>();
                    if (enemyRb != null)
                    {
                        Vector3 knockbackDirection = (hit.point - rayOrigin).normalized;
                        knockbackDirection.y = 0; // Keep knockback horizontal

                        // Apply force at the center of mass to prevent torque
                        enemyRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
                    }
                }
            }
        }

        // Start attack cooldown
        Invoke(nameof(ResetAttack), attackCooldown);
    }




    private void ResetAttack()
    {
        canAttack = true;
    }

    // FOR DEBUGGING PURPOSES
    private void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            // Set the Gizmo color
            Gizmos.color = Color.red;

            // Define the raycast origin and direction
            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward * attackRange;

            // Draw the raycast line
            Gizmos.DrawRay(rayOrigin, rayDirection);

            // Optionally, draw a sphere at the end point to indicate the attack range
            Gizmos.DrawWireSphere(rayOrigin + rayDirection, 0.1f); // The sphere radius is 0.1 units
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

    // =================== CAMERA FUNCTIONS ========================

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

    // ==================== GAME INTERACTIONS ============================
    public void TakeDamage(float damageAmount)
    {
        Debug.Log("Player took damage!" + damageAmount);
        health -= damageAmount;
        health = Mathf.Clamp(health, 0, maxHealth);

        // when there's UI implemented, update it
        // UIManager.Instance.UpdateHealth(health);

        // check if player is dead
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player has died");

        isDead = true;

        // Delay the scene reload by 2 seconds
        Invoke(nameof(ReloadScene), 2f);
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ==================== PICKUP FUNCTIONS =============================

    // all of the things here can be made negative. We use this for our RNG later.

    public void IncreaseHealth(float amount)
    {
        health = health + amount;
    }

    public void IncreaseSpeed(float amount)
    {
        originalSpeed += amount;
        moveSpeed += amount;
    }

    public void IncreaseJumpForce(float amount)
    {
        jumpForce += amount;
    }

    public void IncreaseDamage(float amount)
    {
        attackDamage += amount;
    }

    public void IncreaseDefense(float amount)
    {
        defense += amount;
    }

    private HashSet<PlayerEffect> activeEffects = new HashSet<PlayerEffect>();

    public void ApplyEffect(PlayerEffect effect)
    {
        ApplyEffectLogic(effect);
    }

    /*
    Balance, // no negative effects, go all out!
    Jester, // You and enemies emit silly noises when hit
    Knight, // range way down!
    Rogue, // Enemies kill each other
    Sun, // Enemies have 2x HP
    Flames, // You take damage every 5 seconds
    Fool, // screenshake way up!
    Fates, // Everyone has 1HP
    Donjon, // You cannot move
    Comet // Jump Height x5
    */

    private void ApplyEffectLogic(PlayerEffect effect)
    {
        switch (effect)
        {
            case PlayerEffect.Balance:
                // do nothing!
                break;
            case PlayerEffect.Jester:
                // emit silly noise
                break;
            case PlayerEffect.Knight:
                // range way down
                attackRange = 10.0f;
                break;
            case PlayerEffect.Rogue:
                // enemies are mad!
                break;
            case PlayerEffect.Sun:
                // enemies have 2xHP
                break;
            case PlayerEffect.Flames:
                // take damage every 5s
                if(!isFlamesEffectActive)
                {
                    isFlamesEffectActive = true;
                    flamesCoroutine = StartCoroutine(FlamesDamageCoroutine());
                }
                break;
            case PlayerEffect.Fool:
                // screenshake way up!
                horizontalShakeMagnitude *= 20;
                break;
            case PlayerEffect.Fates:
                // you have 1HP!
                health = health - (health - 1);
                break;
            case PlayerEffect.Donjon:
                // you cannot move!
                originalSpeed = 0;
                moveSpeed = 0;
                break;
            case PlayerEffect.Comet:
                // Jump Height x5
                jumpForce *= 5;
                break;
        }
    }

    private IEnumerator FlamesDamageCoroutine()
    {
        while(isFlamesEffectActive)
        {
            yield return new WaitForSeconds(5f);
            TakeDamage(20f); // apply 20 damage to the player every 5 seconds.

            if (health <= 0)
            {
                isFlamesEffectActive = false;
            }
        }
    }

    public void RemoveEffect(PlayerEffect effect)
    {
        switch(effect)
        {
            case PlayerEffect.Balance:
                // do nothing!
                break;
            case PlayerEffect.Jester:
                // emit silly noise
                break;
            case PlayerEffect.Knight:
                // range way down
                attackRange = 50.0f;
                break;
            case PlayerEffect.Rogue:
                // enemies are mad!
                break;
            case PlayerEffect.Sun:
                // enemies have 2xHP
                break;
            case PlayerEffect.Flames:
                if (isFlamesEffectActive)
                {
                    isFlamesEffectActive = false;
                    if (flamesCoroutine != null)
                    {
                        StopCoroutine(flamesCoroutine);
                        flamesCoroutine = null;
                    }
                }
                break;
            case PlayerEffect.Fool:
                // screenshake way up!
                horizontalShakeMagnitude /= 20;
                break;
            case PlayerEffect.Fates:
                // you have 1HP!
                health = 100;
                break;
            case PlayerEffect.Donjon:
                // you cannot move!
                originalSpeed = 7;
                moveSpeed = 7;
                break;
            case PlayerEffect.Comet:
                // Jump Height x5
                jumpForce /= 5;
                break;
        }
    }
}