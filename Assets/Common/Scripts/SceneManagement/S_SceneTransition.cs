using UnityEngine;
using UnityEngine.SceneManagement;

public class S_SceneTransition : MonoBehaviour
{
    public string sceneName;   
    public void LoadSceneByName()
    {
        SceneManager.LoadSceneAsync(sceneName);

    }
}
