using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class MaterialRevealEntry
{
    // Material using the shader with a '_Progress' property
    public Material material;

    // Curve defining progress over normalized time (0 to 1)
    public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    // Duration for the progress animation (in seconds)
    public float duration = 1f;
}

public class S_ShaderPlayer : MonoBehaviour
{
    // List of material entries with their curves and durations
    public List<MaterialRevealEntry> revealEntries = new List<MaterialRevealEntry>();

    // Press Space to play all reveals
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayAll();
        }
    }

    // Play reveal animation for all materials in the list
    public void PlayAll()
    {
        foreach (var entry in revealEntries)
        {
            if (entry.material != null)
                StartCoroutine(AnimateMaterialProgress(entry));
        }
    }

    // Play reveal animation for a single entry by index
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
            Debug.LogWarning("PlayAtIndex: Index " + index + " is out of range.");
        }
    }

    // Coroutine that updates the '_Progress' property over time
    private IEnumerator AnimateMaterialProgress(MaterialRevealEntry entry)
    {
        float elapsed = 0f;
        entry.material.SetFloat("_Progress", entry.curve.Evaluate(0f));

        while (elapsed < entry.duration)
        {
            float t = elapsed / entry.duration;
            float value = entry.curve.Evaluate(t);
            entry.material.SetFloat("_Progress", value);
            elapsed += Time.deltaTime;
            yield return null;
        }

        entry.material.SetFloat("_Progress", entry.curve.Evaluate(1f));
    }
}
