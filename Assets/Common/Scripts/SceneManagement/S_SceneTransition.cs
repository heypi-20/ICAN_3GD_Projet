using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Eflatun.SceneReference;

public class S_SceneTransition : MonoBehaviour
{
    public SceneReference sceneName;   
    public bool WaitForSceneLoad = false;
    public void LoadSceneByName()
    {
        if (WaitForSceneLoad)
        {
            StartCoroutine(DelayedLoadSceneByName());
        }
        else
        {
            SceneManager.LoadSceneAsync(sceneName.BuildIndex);
        }
    }

    private IEnumerator DelayedLoadSceneByName()
    {
        new WaitForSeconds(2.5f);
        SceneManager.LoadSceneAsync(sceneName.BuildIndex);
        yield break;
    }
}
