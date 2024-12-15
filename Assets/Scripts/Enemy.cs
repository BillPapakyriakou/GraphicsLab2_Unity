using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{

    MapGenerator mapGenerator;
    PlayerController playerController;
    Treasure treasure;

    public GameObject deathSpherePrefab;

    public GameObject deathSphereParentObject;



    int maxEnemies = 2;  // Set to two but is really 3
    float SpawnInterval = 2.0f;  // time between enemy spawns
    float enemyLifeTime = 6.0f;  // time that enemy remains on the map

    private GameObject enemyInstance;

    // Queue to store enemy positions
    private Queue<Vector3> enemyPositionsQueue = new Queue<Vector3>();


    private GameObject player;  // Reference to the player

    private List<GameObject> activeEnemies = new List<GameObject>(); // List to track active enemies


    void Start()
    {
        // Find the player object (make sure the player is tagged correctly)
        player = GameObject.FindWithTag("Player");
    }

    public void SpawnEnemy()
    {

        // Check if the player exists or if the game is not active
        if (player == null)
        {
            return;  // Skip spawning if the player is not available or the game is over
        }

        mapGenerator = FindObjectOfType<MapGenerator>();

        playerController = FindObjectOfType<PlayerController>();

        treasure = FindObjectOfType<Treasure>();

        if (playerController.GameIsActive)
        {

            
            Vector3 treasurePosition = treasure.getCurrentTreasurePosition();

            float excludeTreasureX = treasurePosition.x;
            float excludeTreasureY = treasurePosition.z;

            //Debug.Log("Treasure: " + excludeTreasureX + " " + excludeTreasureY);

            Vector3 playerPosition = playerController.transform.position;

            float excludePlayerX = playerPosition.x;
            float excludePlayerY = playerPosition.z;


            // Call FindFreePositions to get tuple of free positions in the labyrinth
            List<Tuple<int, int>> freePositions = mapGenerator.FindFreePositionsEnemy(mapGenerator.labyrinth, excludePlayerX, excludePlayerY, excludeTreasureX, excludeTreasureY);


            // Check if there are any free positions to spawn the treasure
            if (freePositions.Count > 0)
            {


                // Pick a random free position from the list
                int randomIndex = UnityEngine.Random.Range(0, freePositions.Count);
                Tuple<int, int> selectedPosition = freePositions[randomIndex];



                // Convert the selected position to a world position (assuming each unit corresponds to the grid cell size)
                Vector3 spawnPosition = new Vector3(selectedPosition.Item2 * mapGenerator.cubeSize - 40.0f, 0f, 45.0f - selectedPosition.Item1 * mapGenerator.cubeSize);

                enemyPositionsQueue.Enqueue(spawnPosition);

                // Instantiate the enemy at the selected position
                enemyInstance = Instantiate(deathSpherePrefab, spawnPosition, Quaternion.identity);

                enemyInstance.transform.parent = deathSphereParentObject.transform;

                activeEnemies.Add(enemyInstance); // Track the spawned enemy


            }
        
        }
    }

    // Method to despawn the current treasure
    void DespawnEnemy(GameObject enemy)
    {
        if (enemy != null)
        {
            Destroy(enemy);  // Destroy the current treasure
            enemy = null;
            enemyPositionsQueue.Dequeue();
        }
    }

    public IEnumerator SpawnAndRespawnEnemy()
    {
        while (true)
        //while (playerController.GameIsActive)  // Infinite loop to keep spawning and despawning
        {

            SpawnEnemy();

            SpawnInterval = UnityEngine.Random.Range(1.0f, 3.0f);  // Randomize between 1 and 3
            enemyLifeTime = UnityEngine.Random.Range(4.0f, 7.0f);  // Randomize between 4 and 7
            //Debug.Log(SpawnInterval);
            //Debug.Log(enemyLifeTime);

            // Wait for the spawn interval
            yield return new WaitForSeconds(SpawnInterval);

            // Start the coroutine to despawn this enemy after its lifetime
            StartCoroutine(DespawnAfterLifetime(enemyInstance, enemyLifeTime));

            // Despawn the oldest enemy if maxEnemies limit is reached
            if (activeEnemies.Count > maxEnemies)
            {
                //Debug.Log("Max enemies!");
                GameObject enemyToDespawn = activeEnemies[0];
                activeEnemies.RemoveAt(0); // Remove from the list
                DespawnEnemy(enemyToDespawn);
            }

        }
    }

    private IEnumerator DespawnAfterLifetime(GameObject enemy, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);

        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy); // Remove from the list
            DespawnEnemy(enemy);
        }
    }

    public GameObject getEnemyInstance()
    {
        return enemyInstance;
    }

    // Get queue of enemy positions
    public Queue<Vector3> GetEnemyPositionsQueue()
    {
        return enemyPositionsQueue;
    }

}
