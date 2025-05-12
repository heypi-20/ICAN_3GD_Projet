using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class S_SkillHUDFeedBack : MonoBehaviour
{
    private void Start()
    {
        S_PlayerStateObserver.Instance.OnLevelUpStateEvent += handleLevelChangeEvent;
        S_PlayerStateObserver.Instance.OnJumpStateEvent += handleJumpStateEvent;
        S_PlayerStateObserver.Instance.OnSprintStateEvent+=handleSprintStateEvent;
        S_PlayerStateObserver.Instance.OnGroundPoundStateEvent += handleGroundPoundStateEvent;
    }
    

    private void handleLevelChangeEvent(Enum state, int level)
    {
        
    }

    private void handleJumpStateEvent(Enum state, int level)
    {
        
    }

    private void handleSprintStateEvent(Enum state, int level)
    {
        
    }

    private void handleGroundPoundStateEvent(Enum state, int level)
    {
        
    }
}
