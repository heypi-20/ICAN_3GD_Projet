using UnityEngine;

public class S_CameraController : MonoBehaviour
{
    // Sensibilité de la souris ajustable dans l'inspecteur
    [Header("Mouse Sensitivity")]
    public float mouseSensitivityX = 100f;

    private float rotationX = 0f;

    void Start()
    {
        // Verrouille le curseur au centre de l'écran
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Obtenir la position horizontale de la souris (axe X)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX * Time.deltaTime;

        // Ajouter la rotation en X basée sur l'entrée de la souris
        rotationX += mouseX;

        // Appliquer la rotation sur l'axe Y de la caméra (rotation horizontale)
        transform.localRotation = Quaternion.Euler(0f, rotationX, 0f);
    }
}