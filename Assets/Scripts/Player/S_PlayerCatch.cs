using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerCatch : MonoBehaviour
{
    public Transform catchPoint;
    public float checkCubeRadius = 2f;
    public float throwForce = 10f;

    private Collider cube = null;
    private bool isCatching = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, checkCubeRadius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.tag == "Player" && Input.GetKeyDown(KeyCode.E) && cube == null)
            {
                cube = hitCollider;
            }

            if (cube != null && Input.GetKeyUp(KeyCode.E))
            {
                isCatching = true;
            }
        }

        if (isCatching)
        {
            cube.gameObject.GetComponent<Rigidbody>().useGravity = false;
            cube.transform.position = catchPoint.position;

            if (Input.GetKeyDown(KeyCode.E))
            {
                cube.gameObject.GetComponent<Rigidbody>().AddForce(transform.forward * throwForce, ForceMode.Impulse);
                cube.gameObject.GetComponent<Rigidbody>().useGravity = true;
                cube = null;
                isCatching = false;
            }
        }
    }
}
