using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Functions setted in Editor on button click behaviour.

    // When clicked on Play Button, Start the game.
    public void StartGame()                     
    {
        SceneManager.LoadScene("GameScene");
    }


    // Quit
    public void QuitGame()                     
    {
        Application.Quit();
    }
}
