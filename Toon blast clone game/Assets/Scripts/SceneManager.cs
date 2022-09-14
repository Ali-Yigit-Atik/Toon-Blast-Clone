using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;


public class SceneManager : MonoBehaviour
{
    public void UploadScene(string levelName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
    }

    public void quitGame()
    {

        Application.Quit(); 
    }
}
