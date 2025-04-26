using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class MaterialRevealEntry
{
    // Renderer to apply reveal effect
    public Renderer targetRenderer;

    // Curve defining progress over normalized time (0 to 1)
    public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    // Duration for the progress animation (in seconds)
    public float duration = 1f;

    // Baseline block capturing original renderer properties
    [HideInInspector]
    public MaterialPropertyBlock originalBlock;

    // Working block for runtime progress updates
    [HideInInspector]
    public MaterialPropertyBlock propBlock;

    // Reference to the running coroutine
    [HideInInspector]
    public Coroutine currentCoroutine;
}

public class S_ShaderPlayer : MonoBehaviour
{
    // List of entries with their target renderers, curves, and durations
    public List<MaterialRevealEntry> revealEntries = new List<MaterialRevealEntry>();

    private void Awake()
    {
        // Cache initial property block for each renderer
        foreach (var entry in revealEntries)
        {
            if (entry.targetRenderer == null) continue;
            entry.originalBlock = new MaterialPropertyBlock();
            entry.targetRenderer.GetPropertyBlock(entry.originalBlock);
        }
    }

    private void Update()
    {
        // Press Space to play all reveals
        if (Input.GetKeyDown(KeyCode.Space))
            PlayShaderAnimation();
    }

    // Play reveal animation for all entries
    public void PlayShaderAnimation()
    {
        foreach (var entry in revealEntries)
            PlayEntry(entry);
    }

    // Play reveal animation for a specific entry by index
    public void PlayAtIndex(int index)
    {
        if (index < 0 || index >= revealEntries.Count) return;
        PlayEntry(revealEntries[index]);
    }

    // Prepare and start animation for an entry
    private void PlayEntry(MaterialRevealEntry entry)
    {
        if (entry.targetRenderer == null) return;

        // Safety lock: ignore if already playing
        if (entry.currentCoroutine != null) return;

        // Restore original cached properties
        entry.targetRenderer.SetPropertyBlock(entry.originalBlock);

        // Clone baseline block for this play
        entry.propBlock = new MaterialPropertyBlock();
        entry.targetRenderer.GetPropertyBlock(entry.propBlock);

        // Start animating '_Progress'
        entry.currentCoroutine = StartCoroutine(AnimateReveal(entry));
    }

    // Coroutine to animate '_Progress' over time
    private IEnumerator AnimateReveal(MaterialRevealEntry entry)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(entry.duration, 0.0001f);

        // Initial progress
        entry.propBlock.SetFloat("_Progress", entry.curve.Evaluate(0f));
        entry.targetRenderer.SetPropertyBlock(entry.propBlock);

        while (elapsed < duration)
        {
            float tNorm = elapsed / duration;
            float value = entry.curve.Evaluate(tNorm);

            entry.propBlock.SetFloat("_Progress", value);
            entry.targetRenderer.SetPropertyBlock(entry.propBlock);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Leave final value intact and clear coroutine reference
        entry.currentCoroutine = null;
    }
}
