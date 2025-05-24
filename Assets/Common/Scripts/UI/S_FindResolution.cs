using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_FindResolution : MonoBehaviour
{
    
    void Start()
    {
        ApplyDefaultResolutionIfFirstTime();
    }
    private void ApplyDefaultResolutionIfFirstTime()
    {
        if (!PlayerPrefs.HasKey("ResolutionIndex"))
        {
            Resolution current = Screen.currentResolution;
            var resolutions = Screen.resolutions;
            int closestIndex = 0;
            int minDiff = int.MaxValue;

            for (int i = 0; i < resolutions.Length; i++)
            {
                int diff = Mathf.Abs(resolutions[i].width - current.width) +
                           Mathf.Abs(resolutions[i].height - current.height);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    closestIndex = i;
                }
            }

            // 应用
            var chosen = resolutions[closestIndex];
            Screen.SetResolution(chosen.width, chosen.height, true);
            PlayerPrefs.SetInt("ResolutionIndex", closestIndex);
            PlayerPrefs.Save();
        }
    }

}
