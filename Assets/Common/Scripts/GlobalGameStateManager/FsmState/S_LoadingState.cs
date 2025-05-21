using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S_LoadingState : MonoBehaviour, S_IGameState
{
    [Header("UI References")]
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private GameObject _continuePromptPanel;

    [Header("Loading Settings")]
    [SerializeField] private float _minimumLoadingDuration = 2f;

    private string _sceneToLoad;
    private bool _isWaitingForKey;
    private AsyncOperation _loadingOperation;

    public void OnEnter(params object[] args)
    {
        _loadingPanel.SetActive(true);
        _continuePromptPanel.SetActive(false);
        _isWaitingForKey = false;

        _sceneToLoad = (args.Length > 0) ? args[0] as string : null;

        if (string.IsNullOrEmpty(_sceneToLoad))
        {
            Debug.LogError("No scene name provided for loading.");
            return;
        }

        StartCoroutine(HandleSceneLoading());
    }

    private IEnumerator HandleSceneLoading()
    {
        float timer = 0f;

        _loadingOperation = SceneManager.LoadSceneAsync(_sceneToLoad, LoadSceneMode.Additive);
        _loadingOperation.allowSceneActivation = false;

        while (timer < _minimumLoadingDuration || _loadingOperation.progress < 0.9f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        _continuePromptPanel.SetActive(true);
        _isWaitingForKey = true;
    }

    public void OnTick()
    {
        if (_isWaitingForKey && Input.anyKeyDown)
        {
            _isWaitingForKey = false;
            StartCoroutine(ActivateLoadedScene());
        }
    }

    private IEnumerator ActivateLoadedScene()
    {
        _loadingOperation.allowSceneActivation = true;

        while (!_loadingOperation.isDone)
            yield return null;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(_sceneToLoad));
        S_GameFlowController.Instance.FireEvent(S_GameEvent.EnterGameState);
    }

    public void OnExit()
    {
        _loadingPanel.SetActive(false);
        _continuePromptPanel.SetActive(false);
        _isWaitingForKey = false;
        _loadingOperation = null;
    }
}
