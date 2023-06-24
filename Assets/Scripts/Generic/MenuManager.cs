using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void Singleplayer(int type)
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        switch (type)
        {
            case 0:
                SceneManager.LoadScene("Singleplayer (Easy)");
                break;

            case 1:
                SceneManager.LoadScene("Singleplayer (Hard)");
                break;

            case 2:
                SceneManager.LoadScene("Singleplayer (Expert)");
                break;

            default:
                break;
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
