using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum EnemyType { Melee, Ranged }
    [Header("Enemy Settings")]
    public EnemyType enemyType = EnemyType.Melee;  // Select the enemy type in the Inspector

    public Transform player;  // Assign the player in the Inspector
    public float health = 100f; // enemy health

    [Header("Movement Settings")]
    public float meleeSpeed = 7.0f;
    public float rangedSpeed = 5.0f;
    private float speed;  // Current speed based on enemy type

    public float attackRange = 1.5f;       // Distance within which the melee enemy can attack
    public float rangedAttackRange = 5.0f; // Desired distance to maintain from the player for ranged enemies
    public float retreatRange = 3.0f;      // Distance at which ranged enemies start to move away

    [Header("Attack Settings")]
    public float damage = 10f;         // Damage dealt to the player
    public float knockbackForce = 5f;  // Force applied to knock back the player
    private bool canAttack = true;
    public float attackCooldown = 1.0f; // Time between attacks

    [Header("Ranged Attack Settings")]
    public GameObject projectilePrefab;  // Assign the projectile prefab in the Inspector
    public float projectileSpeed = 10f;
    public float fireRate = 1.5f;
    private float nextFireTime = 0f;

    private Animator animator;

    private float groundY;     // Store the Y position where the enemy should stay (ground level)

    void Start()
    {
        groundY = transform.position.y;

        animator = GetComponent<Animator>();


        // Set the speed based on enemy type
        speed = (enemyType == EnemyType.Melee) ? meleeSpeed : rangedSpeed;

        // Adjust the collider size to match the attack range
        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        if (collider != null)
        {
            collider.isTrigger = true; // Ensure it's set as a trigger

            if (enemyType == EnemyType.Melee)
            {
                collider.radius = attackRange;
            }
            else if (enemyType == EnemyType.Ranged)
            {
                collider.radius = rangedAttackRange;
            }
        }
    }

    void Update()
    {

        // Calculate the direction to the player
        Vector3 direction = player.position - transform.position;
        direction.y = 0; // Lock Y-axis
        transform.LookAt(player.transform);

        if (enemyType == EnemyType.Melee)
        {
            // Move toward the player
            MoveTowardsPlayer(direction.normalized);
            animator.SetBool("IsMoving", true);
        }
        else if (enemyType == EnemyType.Ranged)
        {
            float distanceToPlayer = direction.magnitude;

            if (distanceToPlayer < retreatRange)
            {
                // Player is too close; move away
                Vector3 retreatDirection = (transform.position - player.position).normalized;
                Move(retreatDirection);
                animator.SetBool("IsMoving", true);
            }
            else if (distanceToPlayer > rangedAttackRange)
            {
                // Player is too far; move closer
                MoveTowardsPlayer(direction.normalized);
                animator.SetBool("IsMoving", true);
            }
            else
            {
                // Within desired range; stop moving and attack
                if (Time.time >= nextFireTime)
                {
                    FireProjectile();
                    nextFireTime = Time.time + fireRate;
                    animator.SetBool("IsMoving", false);
                }
            }
        }

        // Ensure the enemy stays on the ground
        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
    }

    private void MoveTowardsPlayer(Vector3 direction)
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void Move(Vector3 direction)
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void FireProjectile()
    {
        animator.SetTrigger("AttackTrigger");

        // Instantiate the projectile
        GameObject projectile = Instantiate(projectilePrefab, transform.position + Vector3.up * 1.0f, Quaternion.identity);

        // Calculate direction towards the player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // Set the projectile's velocity
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = directionToPlayer * projectileSpeed;
        }

        // Optionally, set the projectile to face the direction it's moving
        projectile.transform.forward = directionToPlayer;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the enemy collided with the player
        if (other.CompareTag("Player") && canAttack)
        {
            animator.SetTrigger("AttackTrigger");

            // Get the player's PlayerController script
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                animator.SetBool("IsMoving", false);
                // Apply damage to the player
                playerController.TakeDamage(damage);

                // Apply knockback to the player
                Rigidbody playerRb = other.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                    knockbackDirection.y = 0; // Keep knockback horizontal
                    playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
                }

                // Start attack cooldown
                canAttack = false;
                Invoke(nameof(ResetAttack), attackCooldown);
            }
            else
            {
                Debug.LogWarning("PlayerController not found on collided object.");
            }
        }
    }

    private void ResetAttack()
    {
        canAttack = true;
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        health = Mathf.Clamp(health, 0, health);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

}
