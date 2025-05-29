using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using Eflatun.SceneReference;
/// <summary>
/// Asynchronously loads a target scene in the background,
/// displays a timed fake loading percentage,
/// and prompts "Press Any Key" in French upon completion.
/// </summary>
public class S_LoadingBehavior : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text progressText;             // Displays loading percentage and prompts
    public string TextToShow;

    [Header("Settings")] 
    public SceneReference targetSceneName;  // Scene to load
    public float fakeLoadDuration = 2f;           // Duration in seconds for fake progress

    private AsyncOperation loadOp;

    void Start()
    {
        // Start background loading and fake progress
        StartCoroutine(FakeLoadAndActivate());
    }

    private IEnumerator FakeLoadAndActivate()
    {
        // Begin asynchronous load but don't activate yet
        loadOp = SceneManager.LoadSceneAsync(targetSceneName.BuildIndex);
        loadOp.allowSceneActivation = false;

        // Fake progress timer
        float elapsed = 0f;
        while (elapsed < fakeLoadDuration)
        {
            // Calculate percentage based on elapsed time
            float t = Mathf.Clamp01(elapsed / fakeLoadDuration);
            int percent = Mathf.RoundToInt(t * 100f);
            progressText.text = percent + "%";

            // Increment elapsed using unscaled time for consistency
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Ensure text shows 100%
        progressText.text = "100%";

        // Wait until actual load has at least reached 90% (optional, can skip)
        while (loadOp.progress < 0.9f)
            yield return null;

        // Prompt in French
        progressText.text = TextToShow + "\n \n Press any key to accept the challenge";

        // Wait for any key press
        while (!Input.anyKeyDown)
            yield return null;

        // Restore time scale and activate the loaded scene
        Time.timeScale = 1f;
        loadOp.allowSceneActivation = true;
    }
}