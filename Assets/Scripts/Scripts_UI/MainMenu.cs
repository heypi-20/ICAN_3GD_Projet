using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string scene1;
    public string scene2;
    public string scene3;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Closing Game");
            Application.Quit();
        }
    }

    public void GoToScene1()
    {
        Debug.Log("Loading " + scene1 + " scene");
        SceneManager.LoadScene(scene1);
    }

    public void GoToScene2()
    {
        Debug.Log("Loading " + scene2 + " scene");
        SceneManager.LoadScene(scene2);
    }

    public void GoToScene3()
    {
        Debug.Log("Loading " + scene3 + " scene");
        SceneManager.LoadScene(scene3);
    }

    void Quit()
    {
        Debug.Log("Closing Game");
        Application.Quit();
    }
}
