using System;
using UnityEngine;

public class S_TutoCheck3 : MonoBehaviour
{
    public int gpCount;
    public int geyserCount;
    
    public GameObject portal;

    private int groundPoundCount;
    [HideInInspector] public int gCount;
    private bool gpComplete;
    private bool gComplete;
    
    private bool once = false;

    private void OnEnable()
    {
        FindObjectOfType<S_GroundPound_Module>().OnGroundPoundStateChange += CheckForGP;
    }

    private void Start()
    {
        if (portal == null) {
            Debug.LogWarning("No Portal found");
            return;
        }
        portal.SetActive(false);
    }

    private void Update()
    {
        if (gpComplete && gComplete && !once) {
            portal.SetActive(true);
            once = true;
        }
        
        if (gCount >= geyserCount)
            gComplete = true;
    }

    private void CheckForGP(Enum playerState, int level)
    {
        if (playerState.Equals(PlayerStates.GroundPoundState.EndGroundPound))
            groundPoundCount++;
        
        if (groundPoundCount == gpCount)
            gpComplete = true;
    }
}

