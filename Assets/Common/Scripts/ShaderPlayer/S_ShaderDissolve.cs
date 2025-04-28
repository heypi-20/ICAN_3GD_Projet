using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class DissolveEntry
{
    // Renderer to apply dissolve effect
    public Renderer targetRenderer;

    // Noise texture for dissolve pattern
    public Texture2D noiseTexture;

    // Curve defining progress over normalized time (0 to 1)
    public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    // Duration for the dissolve animation (in seconds)
    public float duration = 1f;

    // Working block for runtime updates
    [HideInInspector]
    public MaterialPropertyBlock propBlock;

    // Reference to the running coroutine
    [HideInInspector]
    public Coroutine currentCoroutine;
}

public class S_ShaderDissolve : MonoBehaviour
{
    // List of entries with their target renderers, noise textures, curves, and durations
    public List<DissolveEntry> dissolveEntries = new List<DissolveEntry>();

    private void Update()
    {
        // Press Q to trigger all dissolves
        if (Input.GetKeyDown(KeyCode.Q))
            PlayShaderDissolveAnimation();
    }

    // Play dissolve for all entries
    public void PlayShaderDissolveAnimation()
    {
        foreach (var entry in dissolveEntries)
            PlayEntry(entry);
    }

    // Play dissolve for a single entry by index
    public void PlayAtIndex(int index)
    {
        if (index < 0 || index >= dissolveEntries.Count)
            return;
        PlayEntry(dissolveEntries[index]);
    }

    // Start or restart dissolve animation for an entry
    private void PlayEntry(DissolveEntry entry)
    {
        if (entry.targetRenderer == null)
            return;

        // Stop any existing coroutine
        if (entry.currentCoroutine != null)
            return;

        // Initialize working block if needed
        if (entry.propBlock == null)
            entry.propBlock = new MaterialPropertyBlock();
        entry.targetRenderer.GetPropertyBlock(entry.propBlock);

        // Apply dissolve mode and noise once
        entry.propBlock.SetInt("_RevealMode", 2);
        entry.propBlock.SetFloat("_Invert", 0f);
        if (entry.noiseTexture != null)
            entry.propBlock.SetTexture("_NoiseTex", entry.noiseTexture);
        entry.targetRenderer.SetPropertyBlock(entry.propBlock);

        // Start animation coroutine
        entry.currentCoroutine = StartCoroutine(AnimateDissolve(entry));
    }

    // Coroutine to animate '_Progress' over time without restoring original
    private IEnumerator AnimateDissolve(DissolveEntry entry)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(entry.duration, 0.0001f);

        // Initial progress
        entry.propBlock.SetFloat("_Progress", entry.curve.Evaluate(0f));
        entry.targetRenderer.SetPropertyBlock(entry.propBlock);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float value = entry.curve.Evaluate(t);

            entry.propBlock.SetFloat("_Progress", value);
            entry.targetRenderer.SetPropertyBlock(entry.propBlock);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // End without resetting or restoring
        entry.currentCoroutine = null;
    }
}
