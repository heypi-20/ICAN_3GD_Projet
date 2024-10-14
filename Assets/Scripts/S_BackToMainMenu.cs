using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S_BackToMainMenu : MonoBehaviour
{
    
    public string sceneName = "Main Menu";

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            
            SceneManager.LoadScene(sceneName);
            
           
        }
    }
}
