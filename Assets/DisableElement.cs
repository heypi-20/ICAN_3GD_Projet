using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableElement : MonoBehaviour
{
    public GameObject[] menuElements;

    public void DisableElements()
    {
        foreach (GameObject obj in menuElements)
        {
            obj.SetActive(false);
        }
    }
    
    public void EnableElements()
    {
        foreach (GameObject obj in menuElements)
        {
            obj.SetActive(true);
        }
    }
}
