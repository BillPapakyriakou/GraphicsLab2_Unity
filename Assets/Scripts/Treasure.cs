using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Treasure : MonoBehaviour
{

    private Vector3 currentTreasurePosition;

    MapGenerator mapGenerator;
    PlayerController playerController;
    Enemy enemy;

    public GameObject cherryTreasurePrefab;
    public GameObject lemonTreasurePrefab;
    public GameObject orangeTreasurePrefab;

    GameObject[] treasures;  // list of treasure objects

    public GameObject treasureParentObject;

    private GameObject currentTreasure;

    private GameObject player;  // Reference to the player

    public bool isShrinking;  // flag to keep track if player is shrinking


    // Awake() method is called before Start() - reason for awake here:
    // The Start() method of MapGenerator class expects treasures data and is called before the Start()
    // of Treasure (this class) which prevents treasures from not being initialized on time
    void Awake()
    {
        treasures = new GameObject[] { cherryTreasurePrefab, lemonTreasurePrefab, orangeTreasurePrefab };

    }

    void Start()
    {
        // Find the player object (make sure the player is tagged correctly)
        player = GameObject.FindWithTag("Player");
    }

    public void SpawnTreasure()
    {
            
            // Check if the player exists or if the game is not active
            if (player == null)
            {
                return;  // Skip spawning if the player is not available or the game is over
            }
            
            mapGenerator = FindObjectOfType<MapGenerator>();
        
            playerController = FindObjectOfType<PlayerController>();

            enemy = FindObjectOfType<Enemy>();

            Vector3 enemyPosition = enemy.getEnemyInstance().transform.position;

            Queue<Vector3> enemyPositionQueue = enemy.GetEnemyPositionsQueue();


            float excludeEnemyX = enemyPosition.x;
            float excludeEnemyY = enemyPosition.z;
    
            Vector3 playerPosition = playerController.transform.position;

            float excludeX = playerPosition.x;
            float excludeY = playerPosition.z;

            
            // Call FindFreePositions to get tuple of free positions in the labyrinth
            List<Tuple<int, int>> freePositions = mapGenerator.FindFreePositionsTreasure(mapGenerator.labyrinth, excludeX, excludeY, enemyPositionQueue);


            // Check if there are any free positions to spawn the treasure
            if (freePositions.Count > 0)
            {

                /*
                Debug.Log("Available Free Positions: ");
                foreach (var pos in freePositions)
                {
                    Debug.Log("Free Position: (" + pos.Item1 + ", " + pos.Item2 + ")");
                }
                */



                // Pick a random free position from the list
                int randomIndex = UnityEngine.Random.Range(0, freePositions.Count);
                Tuple<int, int> selectedPosition = freePositions[randomIndex];



                // Convert the selected position to a world position (assuming each unit corresponds to the grid cell size)
                Vector3 spawnPosition = new Vector3(selectedPosition.Item2 * mapGenerator.cubeSize - 40.0f, 0f, 45.0f - selectedPosition.Item1 * mapGenerator.cubeSize);

                currentTreasurePosition = spawnPosition;

                int randomIndexTreasure = UnityEngine.Random.Range(0, treasures.Length);


                // Instantiate the treasure at the selected position
                currentTreasure = Instantiate(treasures[randomIndexTreasure], spawnPosition, Quaternion.Euler(0, 180, 0));
                // parent floor to map object
                currentTreasure.transform.parent = treasureParentObject.transform;

            }
        
    }

   

    // Method to despawn the current treasure
    void DespawnTreasure()
    {
        if (currentTreasure != null && !isShrinking)
        {
            Destroy(currentTreasure);  // Destroy the current treasure
            currentTreasure = null;
        }
    }

    
    public IEnumerator ShrinkAndDestroy(GameObject obj, float shrinkDuration)
    {
        if (obj == null)
        {
            Debug.LogError("Cannot shrink a null object!");
            isShrinking = false;
            yield break;
        }

        if (shrinkDuration <= 0)
        {
            Debug.LogError("shrinkDuration must be greater than zero!");
            isShrinking = false;
            yield break;
        }

        isShrinking = true; // Set the flag when shrinking starts
        //Debug.Log("started shrinking");

        Vector3 initialScale = obj.transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < shrinkDuration)
        {
            // Ensure the object isnt already destroyed
            if (obj == null)
            {
                Debug.LogWarning("Object was destroyed prematurely during shrinking!");
                isShrinking = false;
                yield break;
            }

            float scale = Mathf.Lerp(1f, 0f, elapsedTime / shrinkDuration);
            obj.transform.localScale = initialScale * scale;

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Finalize shrinking
        if (obj != null)
        {
            obj.transform.localScale = Vector3.zero;
            Destroy(obj);
        }

        isShrinking = false; // Clear the flag when shrinking ends
        //Debug.Log("shrinking ended");
    }


    // Coroutine to handle treasure spawning and despawning
    public IEnumerator SpawnAndRespawnTreasure()
    {
        //while (playerController.GameIsActive)  // Infinite loop to keep spawning and despawning
        while (true)
        {
           
            // Wait until the shrinking coroutine is not active
            while (isShrinking)
            {
                //Debug.Log("spawn and respawn paused");
                yield return null;  // Pause until shrinking is complete
            }
           
            // Spawn the treasure at a random position
            SpawnTreasure();
            

            // Wait for 3 seconds
            yield return new WaitForSeconds(3.0f);

            // Despawn the current treasure
            DespawnTreasure();
        }
    }

    

    public GameObject getTreasureInstance()
    {
        return currentTreasure;
    }    

    public Vector3 getCurrentTreasurePosition() 
    {
        return currentTreasurePosition;
    }

}
