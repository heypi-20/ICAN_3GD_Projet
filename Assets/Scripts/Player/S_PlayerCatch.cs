using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerCatch : MonoBehaviour
{
    public Transform catchPoint;
    public float checkCubeRadius = 2f;
    public string addTag = "Player";
    [Space(20)]
    [Header("Gameplay Settings")]
    public float throwForce = 10f;
    

    private Collider cube = null;
    private Rigidbody cubeRb = null;  // R�f�rence au Rigidbody de l'objet attrap�
    private Collider cubeCollider = null;  // R�f�rence au Collider de l'objet attrap�
    private bool isCatching = false;

    // Update is called once per frame
    void Update()
    {
        // V�rifier les objets dans la zone de capture
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, checkCubeRadius);

        foreach (var hitCollider in hitColliders)
        {
            // Si c'est un cube ou un objet avec le tag sp�cifique et qu'on appuie sur la touche 'E'
            if ((hitCollider.tag == "Cube" || hitCollider.tag == addTag || hitCollider.tag == "Cultivable") && Input.GetKeyDown(KeyCode.E) && cube == null)
            {
                cube = hitCollider;

                // R�cup�rer le Rigidbody (soit sur l'objet lui-m�me, soit sur le parent)
                cubeRb = cube.GetComponent<Rigidbody>();
                if (cubeRb == null)
                {
                    cubeRb = cube.GetComponentInParent<Rigidbody>();
                }

                // R�cup�rer le Collider de l'objet attrap�
                cubeCollider = cube.GetComponent<Collider>();
                if (cubeCollider == null)
                {
                    cubeCollider = cube.GetComponentInParent<Collider>();
                }

                // D�sactiver le Collider pour emp�cher des interactions physiques pendant qu'on attrape l'objet
                if (cubeCollider != null)
                {
                    cubeCollider.enabled = false;
                }
            }

            // Si l'objet a �t� attrap�, mais qu'on l�che la touche 'E'
            if (cube != null && Input.GetKeyUp(KeyCode.E))
            {
                // Ne pas changer le tag si l'objet est "Cultivable"
                if (cube.tag != "Cultivable")
                {
                    cube.gameObject.tag = "Cube";
                }
                // Activer la capture si un Rigidbody est trouv�
                if (cubeRb != null)
                {
                    isCatching = true;
                }
            }
        }

        // Si un objet est attrap�, le suivre et pouvoir le lancer
        if (isCatching && cubeRb != null)
        {
            cubeRb.useGravity = false;
            cubeRb.velocity = Vector3.zero; // Stopper tout mouvement
            cube.transform.position = catchPoint.position;

            // Si on appuie sur la touche 'E' pour lancer l'objet
            if (Input.GetKeyDown(KeyCode.E))
            {
                cubeRb.useGravity = true;
                cubeRb.AddForce(transform.forward * throwForce, ForceMode.Impulse);

                // R�activer le Collider lorsqu'on lance l'objet
                if (cubeCollider != null)
                {
                    cubeCollider.enabled = true;
                }

                // Si ce n'est pas un objet "Cultivable", restaurer le tag d'origine
                if (cube.tag != "Cultivable")
                {
                    cube.gameObject.tag = addTag;
                }

                // R�initialiser les variables apr�s avoir lanc� l'objet
                cube = null;
                cubeRb = null;
                cubeCollider = null;
                isCatching = false;
            }
        }
    }
}
