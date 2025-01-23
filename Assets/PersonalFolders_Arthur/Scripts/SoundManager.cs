using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SoundManager : MonoBehaviour
{

    #region Check singleton
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SoundManager>();
                
                if (_instance == null)
                {
                    GameObject singleton = new GameObject(typeof(SoundManager).ToString());
                    _instance = singleton.AddComponent<SoundManager>();
                    DontDestroyOnLoad(singleton);
                }
            }
            return _instance;
        }
    }
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    #endregion
    
    [Header("Shoot_No_Hit")]
    public string Shoot_No_Hit1;
    public string Shoot_No_Hit2;
    public string Shoot_No_Hit3;
    public string Shoot_No_Hit4;


    public void Meth_Shoot_No_Hit(int currentLevel)
    {
        if (currentLevel == 1)
        {
            FMODUnity.RuntimeManager.PlayOneShot(Shoot_No_Hit1);
        }

        if (currentLevel == 2)
        {
            FMODUnity.RuntimeManager.PlayOneShot(Shoot_No_Hit2);
        }
        if (currentLevel == 3)
        {
            FMODUnity.RuntimeManager.PlayOneShot(Shoot_No_Hit3);
        }

        if (currentLevel == 4)
        {
            FMODUnity.RuntimeManager.PlayOneShot(Shoot_No_Hit4);
        }
        
    }
    
}
