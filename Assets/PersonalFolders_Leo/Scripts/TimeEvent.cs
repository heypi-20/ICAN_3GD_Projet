using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeEvent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FirstSeisme());
    }
    IEnumerator FirstSeisme()
    {
        yield return new WaitForSeconds(30);
        Debug.Log("Yoink");
    }
}
