using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    public void Start()
    {

    }

    public void LoadGame()
    {
        SceneManager.LoadScene(1); // points to main game scene
    }
}
