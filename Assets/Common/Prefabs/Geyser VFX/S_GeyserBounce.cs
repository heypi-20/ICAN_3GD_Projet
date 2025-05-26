using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_GeyserBounce : MonoBehaviour
{
    [Tooltip("Vitesse vers le haut pendant le bounce")]
    public float bounceForce = 10f;

    [Tooltip("Dur�e du bounce (secondes)")]
    public float bounceDuration = 0.3f;

    public bool GoUp = false;
    public GameObject Bumper;
    public float targetY = 2f;              // Position Y cible modifiable dans l�inspecteur
    public float lerpSpeed = 5f;

    private void Start()
    {
        GetComponentInChildren<MeshRenderer>().enabled = false;
        GoUp = true;
        Bumper.transform.position = new Vector3(transform.position.x, transform.position.y-4, transform.position.z);
    }

    private void Update()
    {
        if (GoUp && Bumper != null)
        {
            Vector3 currentPosition = Bumper.transform.position;
            Vector3 targetPosition = new Vector3(currentPosition.x, targetY, currentPosition.z);

            Bumper.transform.position = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * lerpSpeed);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        CharacterController controller = other.GetComponent<CharacterController>();
        if (controller != null)
        {
            // D�marre le bounce via coroutine
            StartCoroutine(ApplyBounce(controller));
            S_TutoCheck3 tutoCheck = FindObjectOfType<S_TutoCheck3>();

            if (tutoCheck != null) {
                tutoCheck.gCount++;
            }
        }
    }

    private IEnumerator ApplyBounce(CharacterController controller)
    {
        float elapsed = 0f;

        while (elapsed < bounceDuration)
        {
            Debug.Log("bounce");
            controller.Move(Vector3.up * bounceForce * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
