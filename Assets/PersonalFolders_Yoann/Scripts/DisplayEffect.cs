using UnityEngine;

public class DisplayEffect : MonoBehaviour
{
    public float floatSpeed = 1f;   // Vitesse du mouvement vertical
    public float floatHeight = 0.5f; // Amplitude du mouvement vertical
    public float rotationSpeed = 30f; // Vitesse de rotation

    public enum RotationAxis { X, Y, Z }
    public RotationAxis rotationAxis = RotationAxis.Y; // Axe de rotation sélectionnable

    private Vector3 startPosition;
    private Vector3 rotationVector;
    private Vector3 objectCenter;

    void Start()
    {
        startPosition = transform.position;

        if (GetComponent<Renderer>() != null)
        {
            // Définir le centre de l'objet pour que la rotation soit centrée
            objectCenter = GetComponent<Renderer>().bounds.center;
        }
        
    }

    void Update()
    {
        // Définit l'axe de rotation
        switch (rotationAxis)
        {
            case RotationAxis.X: rotationVector = transform.right; break;
            case RotationAxis.Y: rotationVector = transform.up; break;
            case RotationAxis.Z: rotationVector = transform.forward; break;
        }
        
        // Mouvement vertical
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotation autour du centre de l'objet
        transform.RotateAround(objectCenter, rotationVector, rotationSpeed * Time.deltaTime);
    }
}