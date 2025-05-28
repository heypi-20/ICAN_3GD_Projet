using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class S_BossPhaseActivation : MonoBehaviour
{
    public GameObject[] NormalPhaseObjs;
    public GameObject NormalPhaseSpawner;
    public GameObject[] BossPhaseObjs;
    public GameObject BossPhaseSpawner;


    public void ActivateBossPhase()
    {
        foreach (GameObject obj in NormalPhaseObjs)
        {
            obj.SetActive(false);
        }

        foreach (GameObject obj in BossPhaseObjs)
        {
            obj.SetActive(true);
        }
        Destroy(NormalPhaseSpawner);
        Instantiate(BossPhaseSpawner,transform.position,Quaternion.identity);
        
    }
}
