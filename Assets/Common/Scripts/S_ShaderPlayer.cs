using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

[System.Serializable]
public class MaterialRevealEntry
{
    [Tooltip("The material using the special shader with a '_Progress' property.")]
    public Material material;

    [Tooltip("Animation curve defining progress over normalized time (0 to 1).")]
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

    [Tooltip("Duration in seconds for the progress to go from 0 to 1.")]
    public float duration = 1f;
}

/// <summary>
/// Controls reveal animations on a list of materials by updating their '_Progress' shader value according to specified curves.
/// Attach this component to any GameObject in your scene.
/// </summary>
public class S_ShaderPlayer : MonoBehaviour
{
    [Tooltip("List of material entries, each with its own curve and duration.")]
    public List<MaterialRevealEntry> revealEntries = new List<MaterialRevealEntry>();

    /// <summary>
    /// Starts reveal animations for all configured material entries.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayAll();
        }
    }

    public void PlayAll()
    {
        foreach (var entry in revealEntries)
        {
            if (entry.material != null)
                StartCoroutine(AnimateMaterialProgress(entry));
        }
    }

    /// <summary>
    /// Plays reveal animation for a specific entry by its index in the list.
    /// </summary>
    /// <param name="index">Index of the entry to play.</param>
    public void PlayAtIndex(int index)
    {
        if (index >= 0 && index < revealEntries.Count)
        {
            var entry = revealEntries[index];
            if (entry.material != null)
                StartCoroutine(AnimateMaterialProgress(entry));
        }
        else
        {
            Debug.LogWarning($"PlayAtIndex: Index {index} is out of range (0 to {revealEntries.Count - 1}).");
        }
    }

    private IEnumerator AnimateMaterialProgress(MaterialRevealEntry entry)
    {
        float elapsed = 0f;
        // Reset initial value
        entry.material.SetFloat("_Progress", entry.curve.Evaluate(0f));
        while (elapsed < entry.duration)
        {
            float normalizedTime = elapsed / entry.duration;
            float progressValue = entry.curve.Evaluate(normalizedTime);
            entry.material.SetFloat("_Progress", progressValue);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // Ensure final value at end of duration
        entry.material.SetFloat("_Progress", entry.curve.Evaluate(1f));
    }
}
