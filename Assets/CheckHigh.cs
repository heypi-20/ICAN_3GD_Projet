using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckHigh : MonoBehaviour
{
    public GameObject target;
    public GameObject CubyPunch;
    public GameObject CubySpawnPos;
    void Update()
    {
        if(gameObject.transform.position.y <50)
        {
            if (gameObject.GetComponent<CharacterController>() != null)
            {
                gameObject.GetComponent<CharacterController>().enabled = false;
                gameObject.transform.position = target.transform.position;
                gameObject.GetComponent<CharacterController>().enabled = true;
            }
            //Destroy(CubyPunch);
            GameObject PunchBag = Instantiate(CubyPunch, CubySpawnPos.transform.position,CubySpawnPos.transform.rotation);
            PunchBag.SetActive(true);
        }
    }
}
