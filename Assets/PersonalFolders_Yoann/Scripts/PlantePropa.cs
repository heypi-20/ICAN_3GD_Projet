using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantePropa : MonoBehaviour
{
    public float _chrono = 5f;
    public float _cTarget = 3f;
    public int Growth;
    public bool CanGrow = true;
    // Update is called once per frame
    void Update()
    {
        _chrono -= Time.deltaTime;
        if(_chrono < 0 && CanGrow)
        {
            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * 1.5f, 0.4f , gameObject.transform.localScale.z * 1.5f);
            Growth += 1;
            _chrono = _cTarget;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Seed"))
        {
            CanGrow = false;
            for (int i = Growth; i > 0; i--)
            {
                Growth -= 1;
                Instantiate(collision.gameObject);
                gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x / 1.5f, 0.4f, gameObject.transform.localScale.z / 1.5f);
            }
        }
    }
}
