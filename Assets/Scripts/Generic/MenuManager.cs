using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void Singleplayer(bool isEasy)
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        if (isEasy)
        {
            SceneManager.LoadScene("Singleplayer (Hard)");
        }
        else
        {
            SceneManager.LoadScene("Singleplayer (Easy)");
        }
    }

    public void Multiplayer()
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.LoadScene("Multiplayer");
    }

    public void Restart()
    {
        Scene current = SceneManager.GetActiveScene();
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.LoadScene(current.name);

        Time.timeScale = 1.0f;
    }

    public void MainMenu()
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.LoadScene("Main Menu");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
