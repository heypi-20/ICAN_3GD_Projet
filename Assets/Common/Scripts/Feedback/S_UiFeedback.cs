using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class S_UiFeedback : MonoBehaviour
{
    public Image blurImage;
    public float fadeDuration = 0.3f;
    public float displayDuration = 1f;
    
    
    private void Start()
    {
        if (S_PlayerStateObserver.Instance != null)
        {
            S_PlayerStateObserver.Instance.OnGroundPoundStateEvent += ReceiceGroudPoundEvevent;
        }
    }
    
    private void ReceiceGroudPoundEvevent(Enum state,int level)
    {
        if (state.Equals(PlayerStates.GroundPoundState.StartGroundPound))
        {
            TriggerBlur();
        }
        if (state.Equals(PlayerStates.GroundPoundState.EndGroundPound))
        {
            
        }
    }
    
    public void TriggerBlur()
    {
        StopAllCoroutines();
        StartCoroutine(BlurRoutine());
    }

    IEnumerator BlurRoutine()
    {
        // Fade in
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));
        // Wait
        yield return new WaitForSeconds(displayDuration);
        // Fade out
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float alpha = Mathf.Lerp(from, to, t);
            blurImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }
}
