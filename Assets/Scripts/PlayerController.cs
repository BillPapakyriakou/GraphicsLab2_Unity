using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour
{

    float[] speeds = { 30.0f, 40.0f, 50.0f, 60.0f, 70.0f };  // Define player speeds
    int speedIndex = 1;  // The current index of the speed array
    float currentSpeed;
    float raycastDistance = 5.0f;  // Ray distance for detecting collisions

    // Boundaries so that player doesnt exit the labyrinth
    float minX;  
    float maxX;
    float minZ;
    float maxZ;

    int score = 0;  // game score

    public bool GameIsActive = true;  // keeps track of game status - running or not running
    

    // Needed to read fields from MapGenerator class for computing the maze boundaries
    MapGenerator mapGenerator;

    Treasure treasure;


    // Player velocity
    Vector3 velocity;

    // Rigidbody component
    public Rigidbody myRigidbody;


    private ParticleSystem playerParticleSystem;       // Reference to the player's particle system
    private ParticleSystemRenderer particleRenderer;   // Reference to the particle renderer

    [SerializeField]                                  // shown in the inspector window
    private Material[] particleMaterials;             // Array of materials for particles

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        treasure = FindObjectOfType<Treasure>();
        myRigidbody = GetComponent<Rigidbody>();
        
        playerParticleSystem = GetComponent<ParticleSystem>();
        particleRenderer = playerParticleSystem.GetComponent<ParticleSystemRenderer>();
        playerParticleSystem.Stop();  // disable particle system in the start of the game

        GetBoundaries();  // Compute player boundaries
        SetSpeed();       // set player speed
    }

    void GetBoundaries()
    {
        // Define maze boundaries 
        minX = mapGenerator.labyrinthCenter.x - ((mapGenerator.rows - 1) * mapGenerator.cubeSize / 2);
        maxX = mapGenerator.labyrinthCenter.x + ((mapGenerator.rows - 1) * mapGenerator.cubeSize / 2);
        minZ = mapGenerator.labyrinthCenter.z - ((mapGenerator.cols - 1) * mapGenerator.cubeSize / 2);
        maxZ = mapGenerator.labyrinthCenter.z + ((mapGenerator.cols - 1) * mapGenerator.cubeSize / 2);
    }

    // Function to increase or decrease the speed index
    void IncreaseSpeedIndex(int change)
    {
        // Adjust the speed index and clamp it to stay within bounds
        speedIndex = Mathf.Clamp(speedIndex + change, 0, speeds.Length - 1);
        SetSpeed();  // Update current speed
    }

    // Function to set the players speed based on the index of the speeds array
    void SetSpeed()
    {
        currentSpeed = speeds[speedIndex];
        //Debug.Log("Current Speed: " + currentSpeed);  // Log the current speed
    }

    public int GetSpeed()
    {
        return (int)currentSpeed;
    }

    void Update()
    {
        if (GameIsActive)
        {


            // Decrease speed when the "1" key is pressed
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                IncreaseSpeedIndex(-1);
            }

            // Increase speed when the "2" key is pressed
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                IncreaseSpeedIndex(1);
            }

            Vector3 input = Vector3.zero;

            // Handles player movement
            if (Input.GetKey(KeyCode.J))
                input.x = -1;
            if (Input.GetKey(KeyCode.L))
                input.x = 1;


            if (Input.GetKey(KeyCode.I))
                input.z = 1;
            if (Input.GetKey(KeyCode.K))
                input.z = -1;

            Vector3 direction = input.normalized;
            velocity = direction * currentSpeed;


            // Ray-casting for collision detection; throw 2 rays on 2 axis to detect for objects and disable movement
            // Ignores objects with an isTrigger checkbock ticked
            RaycastHit hitX, hitZ;  
            bool hitWallX = Physics.Raycast(transform.position, new Vector3(direction.x, 0, 0), out hitX, raycastDistance, ~0, QueryTriggerInteraction.Ignore);
            bool hitWallZ = Physics.Raycast(transform.position, new Vector3(0, 0, direction.z), out hitZ, raycastDistance, ~0, QueryTriggerInteraction.Ignore);
            


            // If there is a wall in the X direction
            if (hitWallX)
            {
                // Slide along the Z-axis if blocked in the X-axis
                velocity = new Vector3(0, 0, direction.z) * currentSpeed;
            }

            // If there is a wall in the Z direction
            if (hitWallZ)
            {
                // Slide along the X-axis if blocked in the Z-axis
                velocity = new Vector3(direction.x, 0, 0) * currentSpeed;
            }

            // If both X and Z are blocked, then dont allow movement in both directions
            if (hitWallX && hitWallZ)
            {
                velocity = Vector3.zero; // Stop movement if both axes are blocked
            }

            

        }
        
    }


    void FixedUpdate()
    {
        if (GameIsActive)
        {

            Vector3 newPosition = myRigidbody.position + velocity * Time.deltaTime;

            // Axis are inverted
            newPosition.x = Mathf.Clamp(newPosition.x, minZ, maxZ); // Clamp X position to fit boundaries
            newPosition.z = Mathf.Clamp(newPosition.z, minX, maxX); // Clamp Z position to fit boundaries

            // Update players position
            myRigidbody.MovePosition(newPosition);
        }
        
    }
    // Handles collisions and other physics aspects
    void OnTriggerEnter(Collider triggerCollider)
    {

        if (!treasure.isShrinking) // Only allow shrinking if treasure isnt shrinking
        {                                          

            //print(triggerCollider.gameObject.name);
            if (triggerCollider.tag == "TreasureOrange")
            {
                StartCoroutine(treasure.ShrinkAndDestroy(triggerCollider.gameObject, 1.5f)); // shrink object
                //Debug.Log("Found orange treasure!");
                score += 2;  // increase score by 2
                //Debug.Log("Score:" + score);
                FindObjectOfType<AudioManager>().Play("orangeTreasureSound");  // play sound
                ChangeParticleMaterial(0);  // set particle material
                TriggerParticleEffect(transform.position);  // enable particle system
            }
            if (triggerCollider.tag == "TreasureCherry")
            {
                StartCoroutine(treasure.ShrinkAndDestroy(triggerCollider.gameObject, 1.5f)); // shrink object
                //Debug.Log("Found cherry treasure!");
                score += 5;  // increase score by 5
                //Debug.Log("Score:" + score);
                FindObjectOfType<AudioManager>().Play("cherryTreasureSound");  // play sound
                ChangeParticleMaterial(1);  // set particle material 
                TriggerParticleEffect(transform.position);  // enable particle system
            }
            if (triggerCollider.tag == "TreasureLemon")
            {
                StartCoroutine(treasure.ShrinkAndDestroy(triggerCollider.gameObject, 1.5f)); // shrink object 
                //Debug.Log("Found lemon treasure!");
                score += 7;  // increase score by 7
                //Debug.Log("Score:" + score);
                FindObjectOfType<AudioManager>().Play("lemonTreasureSound");  // play sound
                ChangeParticleMaterial(2);  // set particle material
                TriggerParticleEffect(transform.position);  // enable particle system
            }
        }

        if (triggerCollider.tag == "DeathSphere")
        {
            //Debug.Log("Hit sphere of death!");
            //Debug.Log("G  a  m  e   O  v  e  r  ");
            GameOver();
            FindObjectOfType<AudioManager>().Play("gameOverSound");  // play game-over sound
        }

    }
    
    public void GameOver()
    {
        GameObject playerInstance = mapGenerator.getPlayerInstance();
        GameObject treasureInstance = treasure.getTreasureInstance();
        if (playerInstance != null)
        {
            GameIsActive = false;  // Change game status 
            Destroy(playerInstance);  // Destroy player instance
            playerInstance = null;
            Destroy(treasureInstance);  // Destoy treasure instance
            treasureInstance = null;

            mapGenerator.ActivateGameOverScreen();  // enable Game over screen
        }
        
    }

    // Method to trigger the particle effect
    private void TriggerParticleEffect(Vector3 position)
    {
        if (playerParticleSystem != null)
        {
            // Set the position of the particle system to the player's position
            playerParticleSystem.transform.position = position;

            // Play the particle system immediately
            playerParticleSystem.Play();

            // Stop the particle system after its duration
            StartCoroutine(StopParticlesAfterBurst());
        }
    }

    // Coroutine to stop particles after burst
    private IEnumerator StopParticlesAfterBurst()
    {
        // Wait for the duration of the particle effect to finish (minus 0.05 so that the effect doesnt play twice) 
        yield return new WaitForSeconds(playerParticleSystem.main.duration - 0.05f);  

        // Stop the particle system after the burst
        playerParticleSystem.Stop();

    }

    // Method to change particle material by index
    private void ChangeParticleMaterial(int index)
    { 
        particleRenderer.material = particleMaterials[index];
    }

    public int GetScore()
    {
        return score;
    }

}   
