using System;
using UnityEngine;

[ExecuteAlways]
public class S_CheckBansheeVar : MonoBehaviour
{
    private void Update()
    {
        if (!Application.isPlaying) {
            S_Banshee banshee = GetComponent<S_Banshee>();

            if (banshee != null) {
                if (banshee.avoidDist > banshee.range) {
                    Debug.LogError("Avoid dist must be inferior to range");
                }
            }
        }
    }
}

