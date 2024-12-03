using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interruptor : MonoBehaviour
{
    public GameObject _Target;
    private void OnTriggerEnter(Collider other)
    {
        Destroy(_Target);
    }
}
