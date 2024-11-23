using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        collision.collider.GetComponent<Rigidbody>().AddForce(Vector3.up * 20, ForceMode.Impulse);
    }
}
