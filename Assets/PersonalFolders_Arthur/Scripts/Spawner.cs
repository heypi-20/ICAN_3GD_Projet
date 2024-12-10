using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public float timer;
    public float TimerTarget;
    public GameObject NPC;
    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if(timer<0)
        {
            Instantiate(NPC,transform.position,transform.rotation);
            timer = TimerTarget;
        }
    }
}
