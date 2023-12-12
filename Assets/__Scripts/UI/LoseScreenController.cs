using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseScreenController : MonoBehaviour
{

    public void LoadGameScene()
    {
        SceneManager.LoadScene("Main Scene Dungeon Delver"); 
    }

    // Function to load the main menu scene
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("TitleScreen"); 
    }
}