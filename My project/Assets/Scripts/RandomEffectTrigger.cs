using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEffectTrigger : MonoBehaviour
{
    public int numberOfEffects = 3; // Number of random effects to apply
    public bool effectEndFlag = false;

    private List<EnemyController> activeEnemies = new List<EnemyController>();
    private List<PlayerEffect> appliedEffects = new List<PlayerEffect>(); // Store applied effects

    private Collider triggerCollider; // Reference to the Collider component

    // reference to the EnemySpawner
    public EnemySpawner enemySpawner;

    private void Awake()
    {
        // Get the Collider component attached to this GameObject
        triggerCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player has entered the trigger
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            // Apply the random effects to the player
            appliedEffects = ApplyRandomEffects(player);

            // Display the chosen effects on the screen
            UIManager.Instance.DisplayEffects(appliedEffects);

            // Start spawning enemies
            if (enemySpawner != null)
            {
                enemySpawner.ResetSpawner(); // Clear previous enemies
                enemySpawner.StartSpawning();
            }

            // Disable the collider after activation
            triggerCollider.enabled = false;

            // Start coroutine to monitor enemies
            StartCoroutine(WaitForEnemiesToBeDefeated(player));
        }
    }

    private IEnumerator WaitForEnemiesToBeDefeated(PlayerController player)
    {
        // Wait until all enemies are destroyed
        while (true)
        {
            // Get the list of spawned enemies from the spawner
            List<GameObject> spawnedEnemies = enemySpawner.GetSpawnedEnemies();

            // If all enemies are destroyed, break the loop
            if (spawnedEnemies.Count == 0 && !enemySpawner.isSpawning)
            {
                break;
            }

            // Wait for a short time before checking again
            yield return new WaitForSeconds(0.5f);
        }

        // Remove all effects from the player
        foreach (PlayerEffect effect in appliedEffects)
        {
            player.RemoveEffect(effect);
        }

        // Clear the applied effects list
        appliedEffects.Clear();

        // Re-enable the collider to reactivate the trigger box
        triggerCollider.enabled = true;
    }

    private List<PlayerEffect> ApplyRandomEffects(PlayerController player)
    {
        // Get all possible effects
        List<PlayerEffect> allEffects = new List<PlayerEffect>((PlayerEffect[])System.Enum.GetValues(typeof(PlayerEffect)));
        List<PlayerEffect> chosenEffects = new List<PlayerEffect>();

        // Shuffle the list of effects
        ShuffleList(allEffects);

        // Select the desired number of effects
        int effectsToApply = Mathf.Min(numberOfEffects, allEffects.Count);
        for (int i = 0; i < effectsToApply; i++)
        {
            PlayerEffect effect = allEffects[i];
            chosenEffects.Add(effect);

            // Apply the effect to the player
            player.ApplyEffect(effect);
        }
        return chosenEffects;
    }

    // Fisher-Yates shuffling algorithm
    public void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
