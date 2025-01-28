using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HardBoot : MonoBehaviour
{
    public SceneReference mainScene;
    private void Awake()
    {
        SceneManager.LoadScene(mainScene.BuildIndex);
    }
}
