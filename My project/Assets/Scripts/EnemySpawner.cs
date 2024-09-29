using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [Header("Spawner Settings")]
    public GameObject enemyPrefab; // enemy to spawn
    public int enemiesToSpawn = 5;
    public float spawnDelay = 0.5f;
    public List<Transform> spawnPoints;
    public bool isSpawning = false;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    public virtual void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnEnemies());
        }
    }

    private IEnumerator<WaitForSeconds> SpawnEnemies()
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnDelay);
        }

        isSpawning = false;
    }

    private void SpawnEnemy()
    {
        // Choose a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        // Instantiate the enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        // **Assign the player reference to the enemy**
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            if (enemyController.player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    enemyController.player = playerObject.transform;
                }
                else
                {
                    Debug.LogError("Player not found! Make sure your player GameObject has the 'Player' tag assigned.");
                }
            }
        }

        // Add to the list of spawned enemies
        spawnedEnemies.Add(enemy);
    }


    public List<GameObject> GetSpawnedEnemies()
    {
        // remove any null entries
        spawnedEnemies.RemoveAll(enemy => enemy == null);
        return spawnedEnemies;
    }

    public void ResetSpawner()
    {
        spawnedEnemies.Clear();
    }
}