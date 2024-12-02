using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePlant : MonoBehaviour
{
    public Collider Collider;
    public bool isActivated;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(isActivated == true)
        {
            Collider.enabled = true;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
    }
    private void OnTriggerStay(Collider other)
    {
        float timer = 2;
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            Destroy(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        other.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
    }
}
