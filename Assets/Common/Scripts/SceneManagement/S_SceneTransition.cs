using UnityEngine;
using UnityEngine.SceneManagement;
using Eflatun.SceneReference;

public class S_SceneTransition : MonoBehaviour
{
    public SceneReference sceneName;   
    public void LoadSceneByName()
    {
        SceneManager.LoadSceneAsync(sceneName.BuildIndex);

    }
}
