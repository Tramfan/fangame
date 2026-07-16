using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{


    public GameObject GameOverMenuUI;


    public void LoadMenu()
        {
            SceneManager.LoadScene("Menu");
        }

    public void Restart()
        {
            SceneManager.LoadScene("Game");
        }
}
