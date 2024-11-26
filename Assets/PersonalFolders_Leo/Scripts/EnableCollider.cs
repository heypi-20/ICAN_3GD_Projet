using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ColliderOn());
    }
    
    IEnumerator ColliderOn()
    {
        yield return new WaitForSeconds(0.8f);
        gameObject.GetComponent<Collider>().enabled = true;
    }
}
