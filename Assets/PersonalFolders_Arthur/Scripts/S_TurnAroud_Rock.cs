using UnityEngine;
using DG.Tweening;

public class S_TurnAroud_Rock : MonoBehaviour
{
    [Header("Orbit Settings")]
    public GameObject[] orbitObjects; // Tableau des objets à faire tourner
    public float orbitRadius = 5f; // Distance entre le centre et les objets
    public float orbitSpeed = 10f; // Vitesse de rotation autour du centre
    public float selfRotationSpeed = 30f; // Vitesse de rotation sur eux-mêmes

    [Header("Pivot Settings")]
    public Transform pivotPoint; // Le point fixe autour duquel les objets tournent
    public Transform player; // Le joueur dont l'orientation influence la rotation

    [Header("DOTween Settings")]
    public float selfRotationDuration = 2f; // Durée de la rotation magique
    public Ease selfRotationEase = Ease.InOutSine; // Type d'effet magique

    [Header("Scale Settings")]
    public Vector3 reducedScale = Vector3.zero; // Taille réduite (désactivée)
    public Vector3 normalScale = Vector3.one; // Taille normale (activée)
    public float scaleTransitionDuration = 0.5f; // Durée de la transition de scale (en secondes)
    public Ease scaleTransitionEase = Ease.InOutQuad; // Ease pour la transition de scale
    public float scaleTransitionSpeed = 1f; // Multiplicateur pour ajuster la vitesse de transition

    private S_EnergyStorage _energystorage;

    void Start()
    {
        // Trouver l'instance de S_EnergyStorage dans la scène
        _energystorage = FindObjectOfType<S_EnergyStorage>();

        if (_energystorage == null)
        {
            Debug.LogError("Aucun script S_EnergyStorage trouvé dans la scène !");
            return;
        }

        // Répartir les objets autour du pivot
        ArrangeObjects();

        // Ajouter des rotations magiques aux objets
        ApplyMagicalRotations();
    }

    void Update()
    {
        // Vérifier si le pivot existe
        if (pivotPoint == null || player == null || _energystorage == null)
            return;

        // Mettre à jour l'état des objets en fonction de currentLevelIndex
        UpdateActiveRocks();

        // Faire tourner tous les objets, même ceux avec une petite échelle
        foreach (GameObject obj in orbitObjects)
        {
            if (obj != null)
            {
                obj.transform.RotateAround(pivotPoint.position, pivotPoint.forward, orbitSpeed * Time.deltaTime);
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
                orbitObjects[i].transform.position = pivotPoint.position + pivotPoint.rotation * position;
            }
        }
    }

    private void ApplyMagicalRotations()
    {
        foreach (GameObject obj in orbitObjects)
        {
            if (obj != null)
            {
                // Appliquer une rotation aléatoire et boucler avec DOTween
                obj.transform.DORotate(
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

    private void UpdateActiveRocks()
    {
        for (int i = 0; i < orbitObjects.Length; i++)
        {
            if (i <= _energystorage.currentLevelIndex && orbitObjects[i] != null)
            {
                // Transition de scale vers normalScale (activation)
                orbitObjects[i].transform.DOScale(normalScale, scaleTransitionDuration / scaleTransitionSpeed)
                    .SetEase(scaleTransitionEase);
            }
            else if (orbitObjects[i] != null)
            {
                // Transition de scale vers reducedScale (désactivation)
                orbitObjects[i].transform.DOScale(reducedScale, scaleTransitionDuration / scaleTransitionSpeed)
                    .SetEase(scaleTransitionEase);
            }
        }
    }
}
