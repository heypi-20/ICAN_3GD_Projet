using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Name")]
    public string scene1;
    public string scene2;
    public string scene3;

    [Header("Menu")]
    public GameObject settingWindow;

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

    public void OpenSettingButton()
    {
        settingWindow.SetActive(true);
    }

    public void CloseSettingButton()
    {
        settingWindow.SetActive(false);
    }

    public void Quit()
    {
        Debug.Log("Closing Game");
        Application.Quit();
    }
}
