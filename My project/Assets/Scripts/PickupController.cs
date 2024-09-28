using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupController : MonoBehaviour
{
    // list potential pickup types
    public enum PickupType { Health, Speed, JumpForce, Damage, Defense }
    public PickupType pickupType;

    public float amount = 10f; // amount to adjust attribute
    public bool isTemporary = false; // choose whether the effect is temporary
    public float duration = 5f; // duration of the effect if temporary

    public ParticleSystem pickupEffect; // optional unless we want to add them later.
    public AudioClip pickupSound; // optional unless we want to add sound later

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // get the player controller from the player object
            Debug.Log("Found Player in PickupController");
            PlayerController player = other.GetComponent<PlayerController>();
            Debug.Log(player);

            // if something returns 
            if (player != null)
            {
                Debug.Log("Player  doesn't equal null in Pickupcontroller");
                ApplyPickup(player);
                Debug.Log("Applied Pickup in pickupcontroller");

                // play through the effects
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                // and... destroy the pickup object
                Destroy(gameObject);
            }
        }
    }

    private void ApplyPickup(PlayerController player)
    {
        if (isTemporary)
        {
            // apply temporary effect for desired time
            switch (pickupType)
            {
                case PickupType.Speed:
                    // player.StartCoroutine(player.ChangeSpeedTemporarily(amount, duration));
                    break;
                case PickupType.JumpForce:
                    // player.StartCoroutine(player.ChangeJumpForceTemporarily(amount, duration));
                    break;
                // Add cases for other temporary attributes if needed

                default:
                    Debug.LogWarning("Temporary effect not implemented for this pickup type.");
                    break;
            }
        }
        else
        {
            // Apply the perminent effect
            switch (pickupType)
            {
                case PickupType.Health: // Health
                    player.IncreaseHealth(amount);
                    break;
                case PickupType.Speed: // Speed
                    player.IncreaseSpeed(amount);
                    break;
                case PickupType.JumpForce: // Jumpheight
                    player.IncreaseJumpForce(amount);
                    break;
                case PickupType.Damage: // increased damage
                    player.IncreaseDamage(amount);
                    break;
                case PickupType.Defense: // Defense
                    player.IncreaseDefense(amount);
                    break;
            }
        }
    }
}
