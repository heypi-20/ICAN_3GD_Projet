using System;
using UnityEngine;

public class DestroyEverything : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        Destroy(other.gameObject);
    }
}
