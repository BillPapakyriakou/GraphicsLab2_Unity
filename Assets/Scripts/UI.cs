using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UI : MonoBehaviour
{
    
    PlayerController playerController;
    
    public TMP_Text scoreText;
    public TMP_Text speedText;


    // Gets and sets text values in the UI (game score and player speed)
    void Update()
    {

        playerController = FindObjectOfType<PlayerController>();

        if (playerController != null)
        {
            scoreText.text = "Score: " + playerController.GetScore().ToString();
            speedText.text = "Speed: " + playerController.GetSpeed().ToString();
        }
    }
   
}
