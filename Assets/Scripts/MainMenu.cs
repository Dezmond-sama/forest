using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string levelScene;
    public void StartButton()
    {
        SceneManager.LoadScene(levelScene);
    }

    public void AboutButton()
    {

    }

    public void ExitButton()
    {
        Application.Quit();
    }
}
