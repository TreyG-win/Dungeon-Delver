using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{

    public TMP_Text[] sentences;
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ShowSentences()
    {
        foreach (TMP_Text sentence in sentences)
        {
            sentence.gameObject.SetActive(true); // Set the TMP_Text object to active to make it visible
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
