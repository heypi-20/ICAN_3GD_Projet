using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))] // Exige un Rigidbody sur le GameObject pour fonctionner correctement
public class S_PhysicsCollisionModule : MonoBehaviour
{
    public enum OverlapType { BOX, SPHERE, CAPSULE } // Définition des types d'overlap possibles

    [Header("Collision Settings")] // Paramètres de collision
    public bool useCollision = false; // Utiliser la détection de collision
    public float collisionCooldown = 1.0f; // Temps de recharge pour les collisions
    [Space(0.5f)]
    public bool usePhysicCast = false; // Utiliser la détection PhysicCast
    public float physicCastCooldown = 1.0f; // Temps de recharge pour PhysicCast

    [Space(1)]
    [Header("OverlapType Settings")] // Paramètres du type d'overlap
    public OverlapType overlapType = OverlapType.BOX; // Type d'overlap utilisé (Box, Sphere, Capsule)
    public bool withScaleSize = true; // Utiliser l'échelle de l'objet pour déterminer la taille de l'overlap
    public bool ignoreChildren = false; // Ignorer les objets enfants lors de la détection

    [Space(1)]
    [Header("Box Settings")] // Paramètres pour le type Box
    public Vector3 customSize = Vector3.one; // Taille personnalisée de la box
    public Vector3 customOffset = Vector3.zero; // Offset personnalisé de la box

    [Space(1)]
    [Header("Sphere Settings")] // Paramètres pour le type Sphere
    public float sphereRadius = 1.0f; // Rayon de la sphère

    [Space(1)]
    [Header("Capsule Settings")] // Paramètres pour le type Capsule
    public Vector3 capsulePoint1 = Vector3.zero; // Point 1 de la capsule
    public Vector3 capsulePoint2 = Vector3.up; // Point 2 de la capsule
    public float capsuleRadius = 1.0f; // Rayon de la capsule

    [Space(5)]
    [Header("Check Condition")] // Conditions de vérification
    public string[] collisionTags; // Tags à vérifier lors des collisions
    public string[] collisionTargetComponentNames; // Noms des composants cibles à vérifier lors des collisions
    public string[] physicCastTags; // Tags à vérifier lors de PhysicCast
    public LayerMask physicCastLayers; // Couches à vérifier lors de PhysicCast
    public string[] physicTargetComponentNames; // Noms des composants cibles à vérifier lors de PhysicCast

    public event Action OnTouch; // Événement déclenché lorsqu'une collision est détectée

    private Rigidbody rb; // Référence au Rigidbody
    private float lastCollisionTime = -Mathf.Infinity; // Dernier temps de collision
    private float lastPhysicCastTime = -Mathf.Infinity; // Dernier temps de PhysicCast
    private bool hasCollided = false; // Indique si une collision a eu lieu

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // Récupérer le Rigidbody attaché à cet objet
    }

    private void Update()
    {
        if (usePhysicCast && ShouldPerformPhysicCast()) // Si PhysicCast est activé et qu'on doit effectuer une détection
        {
            hasCollided = false; // Réinitialiser l'état de collision
            PerformPhysicCast(); // Effectuer la détection PhysicCast
            lastPhysicCastTime = Time.time; // Mettre à jour le temps de la dernière détection
        }
    }
    private bool ShouldPerformPhysicCast()
    {
        return Time.time >= lastPhysicCastTime + physicCastCooldown; // Vérifier si le cooldown est écoulé
    }
    public void InvokeOnTouchEvent()
    {
        OnTouch?.Invoke(); // Déclencher l'événement OnTouch si des abonnés sont présents
    }

    private void PerformPhysicCast()
    {
        switch (overlapType) // Choisir le type d'overlap à effectuer
        {
            case OverlapType.BOX:
                PerformBoxCast(); // Effectuer un BoxCast
                break;
            case OverlapType.SPHERE:
                PerformSphereCast(); // Effectuer un SphereCast
                break;
            case OverlapType.CAPSULE:
                PerformCapsuleCast(); // Effectuer un CapsuleCast
                break;
        }
    }

    private bool CheckConditions(GameObject obj, string[] tags, string[] componentNames)
    {
        bool tagCondition = tags == null || tags.Length == 0 || CheckTags(obj, tags); // Vérifier si les tags correspondent
        bool componentCondition = componentNames == null || componentNames.Length == 0 || CheckComponentsByName(obj, componentNames); // Vérifier si les composants correspondent
        return tagCondition && componentCondition; // Retourner vrai si toutes les conditions sont remplies
    }

    private void PerformBoxCast()
    {
        Vector3 boxSize = withScaleSize ? transform.lossyScale : customSize; // Déterminer la taille de la box
        Vector3 boxCenter = withScaleSize ? transform.position : transform.TransformPoint(customOffset); // Déterminer le centre de la box
        Collider[] hits = Physics.OverlapBox(boxCenter, boxSize / 2, transform.rotation, physicCastLayers); // Effectuer le BoxCast

        foreach (Collider hit in hits) // Parcourir les objets détectés
        {
            if (hit.transform == transform) // Ignorer soi-même
            {
                continue;
            }
            if (ignoreChildren && hit.transform.IsChildOf(transform)) // Ignorer les enfants si nécessaire
            {
                continue;
            }

            if (CheckConditions(hit.gameObject, physicCastTags, physicTargetComponentNames)) // Vérifier les conditions
            {
                InvokeOnTouchEvent(); // Déclencher l'événement de collision
                hasCollided = true; // Mettre à jour l'état de collision
                break;
            }
        }
    }

    private void PerformSphereCast()
    {
        Vector3 sphereCenter = transform.TransformPoint(customOffset); // Déterminer le centre de la sphère
        float radius = withScaleSize ? Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) / 2 : sphereRadius; // Déterminer le rayon de la sphère
        Collider[] hits = Physics.OverlapSphere(sphereCenter, radius, physicCastLayers); // Effectuer le SphereCast

        foreach (Collider hit in hits) // Parcourir les objets détectés
        {
            if (hit.transform == transform) // Ignorer soi-même
            {
                continue;
            }
            if (ignoreChildren && hit.transform.IsChildOf(transform)) // Ignorer les enfants si nécessaire
            {
                continue;
            }

            if (CheckConditions(hit.gameObject, physicCastTags, physicTargetComponentNames)) // Vérifier les conditions
            {
                InvokeOnTouchEvent(); // Déclencher l'événement de collision
                hasCollided = true; // Mettre à jour l'état de collision
                break;
            }
        }
    }

    private void PerformCapsuleCast()
    {
        Vector3 point1 = transform.TransformPoint(capsulePoint1 + customOffset); // Déterminer le point 1 de la capsule
        Vector3 point2 = transform.TransformPoint(capsulePoint2 + customOffset); // Déterminer le point 2 de la capsule
        float radius = withScaleSize ? Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) / 2 : capsuleRadius; // Déterminer le rayon de la capsule
        Collider[] hits = Physics.OverlapCapsule(point1, point2, radius, physicCastLayers); // Effectuer le CapsuleCast

        foreach (Collider hit in hits) // Parcourir les objets détectés
        {
            if (hit.transform == transform) // Ignorer soi-même
            {
                continue;
            }
            if (ignoreChildren && hit.transform.IsChildOf(transform)) // Ignorer les enfants si nécessaire
            {
                continue;
            }

            if (CheckConditions(hit.gameObject, physicCastTags, physicTargetComponentNames)) // Vérifier les conditions
            {
                InvokeOnTouchEvent(); // Déclencher l'événement de collision
                hasCollided = true; // Mettre à jour l'état de collision
                break;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (useCollision && Time.time >= lastCollisionTime + collisionCooldown &&
            CheckConditions(collision.gameObject, collisionTags, collisionTargetComponentNames)) // Vérifier les conditions de collision
        {
            InvokeOnTouchEvent(); // Déclencher l'événement de collision
            lastCollisionTime = Time.time; // Mettre à jour le temps de la dernière collision
        }
    }

    private bool CheckTags(GameObject obj, string[] tags)
    {
        if (tags == null || tags.Length == 0) return true; // Si aucun tag n'est spécifié, retourner vrai

        foreach (string tag in tags) // Parcourir les tags spécifiés
        {
            if (obj.CompareTag(tag)) // Si le tag correspond, retourner vrai
            {
                return true;
            }
        }
        return false; // Aucun tag ne correspond
    }

    private bool CheckComponentsByName(GameObject obj, string[] componentNames)
    {
        if (componentNames == null || componentNames.Length == 0) return true; // Si aucun composant n'est spécifié, retourner vrai

        foreach (string componentName in componentNames) // Parcourir les composants spécifiés
        {
            if (obj.GetComponent(componentName) != null) // Si le composant est trouvé, retourner vrai
            {
                return true;
            }
        }
        return false; // Aucun composant ne correspond
    }

    private void OnDrawGizmos()
    {
        if (usePhysicCast)
        {
            Gizmos.color = hasCollided ? Color.red : Color.blue; // Définir la couleur des Gizmos en fonction de l'état de collision

            switch (overlapType) // Choisir le type de Gizmo à dessiner
            {
                case OverlapType.BOX:
                    Vector3 boxSize = withScaleSize ? transform.lossyScale : customSize; // Déterminer la taille de la box
                    Vector3 boxCenter = withScaleSize ? transform.position : transform.TransformPoint(customOffset); // Déterminer le centre de la box
                    Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, boxSize); // Définir la matrice de transformation des Gizmos
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one); // Dessiner la box
                    break;
                case OverlapType.SPHERE:
                    Vector3 sphereCenter = transform.TransformPoint(customOffset); // Déterminer le centre de la sphère
                    float radius = withScaleSize ? Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) / 2 : sphereRadius; // Déterminer le rayon de la sphère
                    Gizmos.DrawWireSphere(sphereCenter, radius); // Dessiner la sphère
                    break;
                case OverlapType.CAPSULE:
                    Vector3 point1 = transform.TransformPoint(capsulePoint1 + customOffset); // Déterminer le point 1 de la capsule
                    Vector3 point2 = transform.TransformPoint(capsulePoint2 + customOffset); // Déterminer le point 2 de la capsule
                    float capsuleRadius = withScaleSize ? Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) / 2 : this.capsuleRadius; // Déterminer le rayon de la capsule
                    Gizmos.DrawWireSphere(point1, capsuleRadius); // Dessiner la sphère au point 1
                    Gizmos.DrawWireSphere(point2, capsuleRadius); // Dessiner la sphère au point 2
                    break;
            }
        }
    }
}
