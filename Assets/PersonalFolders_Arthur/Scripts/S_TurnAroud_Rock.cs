using UnityEngine;
using DG.Tweening;

public class S_TurnAroud_Rock : MonoBehaviour
{
    [Header("Orbit Settings")]
    public Transform[] orbitObjects; // Tableau des objets à faire tourner
    public float orbitRadius = 5f; // Distance entre le centre et les objets
    public float orbitSpeed = 10f; // Vitesse de rotation autour du centre
    public float selfRotationSpeed = 30f; // Vitesse de rotation sur eux-mêmes

    [Header("Pivot Settings")]
    public Transform pivotPoint; // Le point fixe autour duquel les objets tournent
    public Transform player; // Le joueur dont l'orientation influence la rotation

    [Header("DOTween Settings")]
    public float selfRotationDuration = 2f; // Durée de la rotation magique
    public Ease selfRotationEase = Ease.InOutSine; // Type d'effet magique

    void Start()
    {
        // Répartir les objets autour du pivot
        ArrangeObjects();

        // Ajouter des rotations magiques aux objets
        ApplyMagicalRotations();
    }

    void Update()
    {
        // Vérifier si le pivot existe
        if (pivotPoint == null || player == null)
            return;

        // Faire tourner les objets autour du pivot en suivant l'orientation du joueur
        foreach (Transform obj in orbitObjects)
        {
            if (obj != null)
            {
                obj.RotateAround(pivotPoint.position, pivotPoint.forward, orbitSpeed * Time.deltaTime);
            }
        }

        // Aligner le pivot avec l'orientation du joueur
        pivotPoint.rotation = player.rotation;
    }

    private void ArrangeObjects()
    {
        int count = orbitObjects.Length;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;

            // Calculer la position sur le cercle
            Vector3 position = new Vector3(
                Mathf.Cos(angle) * orbitRadius,
                Mathf.Sin(angle) * orbitRadius,
                0f
            );

            // Placer l'objet autour du pivot
            if (orbitObjects[i] != null && pivotPoint != null)
            {
                orbitObjects[i].position = pivotPoint.position + pivotPoint.rotation * position;
            }
        }
    }

    private void ApplyMagicalRotations()
    {
        foreach (Transform obj in orbitObjects)
        {
            if (obj != null)
            {
                // Appliquer une rotation aléatoire et boucler avec DOTween
                obj.DORotate(
                    new Vector3(
                        Random.Range(0, 360),
                        Random.Range(0, 360),
                        Random.Range(0, 360)
                    ),
                    selfRotationDuration,
                    RotateMode.FastBeyond360
                )
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(selfRotationEase);
            }
        }
    }
}
