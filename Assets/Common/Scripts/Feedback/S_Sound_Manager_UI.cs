using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class S_Sound_Manager_UI : MonoBehaviour
{
    public EventReference Music_Menu;
    private EventInstance Instance_MusicMenu;

    private void Start()
    {
        Instance_MusicMenu = RuntimeManager.CreateInstance(Music_Menu);
        Instance_MusicMenu.start();
    }

    private void OnDestroy()
    {
        Instance_MusicMenu.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
