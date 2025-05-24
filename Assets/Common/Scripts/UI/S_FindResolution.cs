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

            Screen.SetResolution(current.width, current.height, true);

            Resolution[] all = Screen.resolutions;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].width == current.width && all[i].height == current.height)
                {
                    PlayerPrefs.SetInt("ResolutionIndex", i);
                    PlayerPrefs.Save();
                    break;
                }
            }
        }
    }

}
