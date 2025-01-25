using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSnow : MonoBehaviour
{
    public GameObject Snow;
    public int numberOfObjects = 50;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            // Instancier l'objet prefab
            Instantiate(Snow,gameObject.transform.position,gameObject.transform.rotation);
        }
    }

}
