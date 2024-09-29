using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHandler : MonoBehaviour
{
    public float damage = 10f;       // Damage dealt by the projectile
    public float lifeTime = 5f;      // Time before the projectile is destroyed
    public float knockbackForce = 5f; // Knockback force applied to the player

    void Start()
    {
        // Destroy the projectile after its lifetime expires
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the projectile hits the player
        if (other.CompareTag("Player"))
        {
            // Get the player's PlayerController script
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
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

                // Destroy the projectile
                Destroy(gameObject);
            }
        }
        else if (other.CompareTag("Environment") || other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            // Destroy the projectile if it hits the environment
            Destroy(gameObject);
        }
    }
}
