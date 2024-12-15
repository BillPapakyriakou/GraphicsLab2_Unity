using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    Treasure treasure;
    Enemy enemy;


    public int rows = 10;
	public int cols = 10;

	public int cubeSize = 10;


	public Vector3 labyrinthCenter = new Vector3(0, 0, 5);

    Vector3 startPlayerPos = new Vector3(-40f, 0, 25f);



    // 2D Array for storing the coords of the cubes for maze generation

    public int[,] labyrinth = {
	{1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
	{1, 0, 0, 0, 0, 0, 0, 0, 0, 1},
	{0, 0, 1, 1, 1, 1, 0, 1, 0, 1},
	{1, 0, 1, 0, 0, 0, 0, 1, 0, 1},
	{1, 0, 1, 0, 1, 1, 0, 1, 0, 1},
	{1, 0, 0, 0, 0, 1, 0, 0, 0, 1},
	{1, 0, 1, 1, 0, 1, 1, 1, 0, 1},
	{1, 0, 0, 0, 0, 0, 0, 1, 0, 0},
	{1, 0, 1, 0, 1, 1, 0, 0, 0, 1},
	{1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
	};


	public GameObject mazeCubePrefab;
    public GameObject mazeFloorPrefab;
    public GameObject playerPrefab;

    private GameObject playerInstance;  

    public GameObject playerParentObject;

    public Rigidbody myRigidbody;

    public Canvas gameCanvas;

    public Canvas gameOverCanvas;

    
    // Method to create labyrinth on Start()
    public void GenerateLabyrinth()
	{
        for (int i = 0; i < rows; ++i)
		{
			for (int j = 0; j < cols; ++j)
			{

				if (labyrinth[i, j] == 1)
				{
					// set position for cube to spawn
					Vector3 spawnPosition = new Vector3(i * cubeSize, 0, j * cubeSize);

					// move cube to position so that the maze center is (0, 0, 5)
                    spawnPosition -= new Vector3((rows - 1) * cubeSize / 2, 0, (cols - 1) * cubeSize / 2);
                    spawnPosition += labyrinthCenter;

					// spawn cube
                    GameObject mazeCube = (GameObject)Instantiate(mazeCubePrefab, spawnPosition, Quaternion.identity);
                    
					// parent cube to map object
					mazeCube.transform.parent = transform;
                }
			}
        }

		// spawn floor
        GameObject mazeFloor = (GameObject)Instantiate(mazeFloorPrefab, labyrinthCenter, Quaternion.identity);
        
		// move floor by 5 on the y axis
		mazeFloor.transform.position -= new Vector3(0, 5, 0);
		
		// parent floor to map object
        mazeFloor.transform.parent = transform;

        transform.rotation = Quaternion.Euler(0, 90, 0); // Rotate the maze parent by 90 degrees
    }


	void SpawnPlayer()
	{
		// Spawn player prefab
        playerInstance = Instantiate(playerPrefab, startPlayerPos, Quaternion.identity);
        playerInstance.transform.parent = playerParentObject.transform; // Parent to empty object

        myRigidbody = playerInstance.GetComponent<Rigidbody>();
    }

    


    // Method to find free positions in the maze for the enemy - same as FindFreePositions but also checks for treasure coords
    public List<Tuple<int, int>> FindFreePositionsEnemy(int[,] labyrinth, float excludePlayerX, float excludePlayerY, float excludeTreasureX, float excludeTreasureY)
    {
        List<Tuple<int, int>> freePositions = new List<Tuple<int, int>>();  // free positions list (x,y)

        // Initial position in labyrinth is (0,2)
        int labyrinthPosPlayerX = (int)((50.0f - excludePlayerY) / 10.0f); // Reverse the y-coordinate conversion
        int labyrinthPosPlayerY = (int)((excludePlayerX + 45.0f) / 10.0f); // Reverse the x-coordinate conversion
        //Debug.Log(labyrinthPosX + " " + labyrinthPosY);

        int labyrinthPosTreasureX = (int)((50.0f - excludeTreasureY) / 10.0f); // Reverse the y-coordinate conversion
        int labyrinthPosTreasureY = (int)((excludeTreasureX + 45.0f) / 10.0f); // Reverse the x-coordinate conversion

        // Iterate through the labyrinth
        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < cols; ++j)
            {
                // Check if the position is free (labyrinth[i][j] == 0) and not the excluded position
                if (labyrinth[i, j] == 0 && !(j == labyrinthPosPlayerY && i == labyrinthPosPlayerX) && !(j == labyrinthPosTreasureY && i == labyrinthPosTreasureX))
                {
                    // Add the free position to the list
                    freePositions.Add(new Tuple<int, int>(i, j));
                }
            }
        }

        return freePositions;
    }
    

    public List<Tuple<int, int>> FindFreePositionsTreasure(int[,] labyrinth, float excludePlayerX, float excludePlayerY, Queue<Vector3> enemyPositionsQueue)
    {
        List<Tuple<int, int>> freePositions = new List<Tuple<int, int>>();  // Free positions list (x, y)

        // Convert player position to labyrinth coordinates
        int labyrinthPosPlayerX = (int)((50.0f - excludePlayerY) / 10.0f); // Reverse the z-coordinate conversion
        int labyrinthPosPlayerY = (int)((excludePlayerX + 45.0f) / 10.0f); // Reverse the x-coordinate conversion

        // Create a hash set to track all the enemy positions in the queue (avoiding duplicates)
        HashSet<Tuple<int, int>> enemyPositionsSet = new HashSet<Tuple<int, int>>();

        // Iterate through the enemy positions queue and add them to the set
        foreach (var enemyPos in enemyPositionsQueue)
        {
            int enemyPosX = (int)((50.0f - enemyPos.z) / 10.0f); // Reverse the z-coordinate conversion
            int enemyPosY = (int)((enemyPos.x + 45.0f) / 10.0f); // Reverse the x-coordinate conversion
            enemyPositionsSet.Add(new Tuple<int, int>(enemyPosX, enemyPosY));
        }

        // Iterate through the labyrinth to find free positions
        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < cols; ++j)
            {
                // Check if the position is free (labyrinth[i, j] == 0)
                // and it's not occupied by the player or any enemy in the queue
                if (labyrinth[i, j] == 0 &&
                    !(j == labyrinthPosPlayerY && i == labyrinthPosPlayerX) && // Exclude player
                    !enemyPositionsSet.Contains(new Tuple<int, int>(i, j)))  // Exclude enemy positions in the queue
                {
                    // Add the free position to the list
                    freePositions.Add(new Tuple<int, int>(i, j));
                }
            }
        }

        return freePositions;
    }



    void Start()
    {

        gameCanvas.gameObject.SetActive(true);  // enable game canvas (to display game score and player speed)
        
        treasure = FindObjectOfType<Treasure>();
        enemy = FindObjectOfType<Enemy>();

        GenerateLabyrinth();  // create labyrinth
		SpawnPlayer();  // spawn player

        
        
        StartCoroutine(enemy.SpawnAndRespawnEnemy());  // start spawning and despawning enemies
        StartCoroutine(treasure.SpawnAndRespawnTreasure());  // start spawning and despawning treasure

    }
    
    public GameObject getPlayerInstance()
    {
        return playerInstance;
    }

    public void ActivateGameOverScreen()
    {
        gameOverCanvas.gameObject.SetActive(true);
    }
    
}
