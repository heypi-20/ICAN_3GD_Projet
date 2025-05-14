using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_GeyserBounce : MonoBehaviour
{
    [Tooltip("Vitesse vers le haut pendant le bounce (unités/seconde)")]
    public float bounceForce = 10f;

    [Tooltip("Durée du bounce (secondes)")]
    public float bounceDuration = 0.3f;

    private void OnTriggerEnter(Collider other)
    {
        CharacterController controller = other.GetComponent<CharacterController>();
        if (controller != null)
        {
            // Démarre le bounce via coroutine
            StartCoroutine(ApplyBounce(controller));
        }
    }

    private IEnumerator ApplyBounce(CharacterController controller)
    {
        float elapsed = 0f;

        while (elapsed < bounceDuration)
        {
            controller.Move(Vector3.up * bounceForce * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
