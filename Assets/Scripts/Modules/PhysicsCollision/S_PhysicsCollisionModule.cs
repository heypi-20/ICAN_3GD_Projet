using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))] // Exige un Rigidbody sur le GameObject pour fonctionner correctement
public class S_PhysicsCollisionModule : MonoBehaviour
{
    public enum OverlapType { BOX, SPHERE, CAPSULE } // D�finition des types d'overlap possibles

    [Header("Collision Settings")] // Param�tres de collision
    public bool useCollision = false; // Utiliser la d�tection de collision
    public float collisionCooldown = 1.0f; // Temps de recharge pour les collisions
    [Space(0.5f)]
    public bool usePhysicCast = false; // Utiliser la d�tection PhysicCast
    public float physicCastCooldown = 1.0f; // Temps de recharge pour PhysicCast

    [Space(1)]
    [Header("OverlapType Settings")] // Param�tres du type d'overlap
    public OverlapType overlapType = OverlapType.BOX; // Type d'overlap utilis� (Box, Sphere, Capsule)
    public bool withScaleSize = true; // Utiliser l'�chelle de l'objet pour d�terminer la taille de l'overlap
    public bool ignoreChildren = false; // Ignorer les objets enfants lors de la d�tection

    [Space(1)]
    [Header("Box Settings")] // Param�tres pour le type Box
    public Vector3 customSize = Vector3.one; // Taille personnalis�e de la box
    public Vector3 customOffset = Vector3.zero; // Offset personnalis� de la box

    [Space(1)]
    [Header("Sphere Settings")] // Param�tres pour le type Sphere
    public float sphereRadius = 1.0f; // Rayon de la sph�re

    [Space(1)]
    [Header("Capsule Settings")] // Param�tres pour le type Capsule
    public Vector3 capsulePoint1 = Vector3.zero; // Point 1 de la capsule
    public Vector3 capsulePoint2 = Vector3.up; // Point 2 de la capsule
    public float capsuleRadius = 1.0f; // Rayon de la capsule

    [Space(5)]
    [Header("Check Condition")] // Conditions de v�rification
    public string[] collisionTags; // Tags � v�rifier lors des collisions
    public string[] collisionTargetComponentNames; // Noms des composants cibles � v�rifier lors des collisions
    public string[] physicCastTags; // Tags � v�rifier lors de PhysicCast
    public LayerMask physicCastLayers; // Couches � v�rifier lors de PhysicCast
    public string[] physicTargetComponentNames; // Noms des composants cibles � v�rifier lors de PhysicCast

    public event Action OnTouch; // �v�nement d�clench� lorsqu'une collision est d�tect�e

    private Rigidbody rb; // R�f�rence au Rigidbody
    private float lastCollisionTime = -Mathf.Infinity; // Dernier temps de collision
    private float lastPhysicCastTime = -Mathf.Infinity; // Dernier temps de PhysicCast
    private bool hasCollided = false; // Indique si une collision a eu lieu

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // R�cup�rer le Rigidbody attach� � cet objet
    }

    private void Update()
    {
        if (usePhysicCast && ShouldPerformPhysicCast()) // Si PhysicCast est activ� et qu'on doit effectuer une d�tection
        {
            hasCollided = false; // R�initialiser l'�tat de collision
            PerformPhysicCast(); // Effectuer la d�tection PhysicCast
            lastPhysicCastTime = Time.time; // Mettre � jour le temps de la derni�re d�tection
        }
    }
    private bool ShouldPerformPhysicCast()
    {
        return Time.time >= lastPhysicCastTime + physicCastCooldown; // V�rifier si le cooldown est �coul�
    }
    public void InvokeOnTouchEvent()
    {
        OnTouch?.Invoke(); // D�clencher l'�v�nement OnTouch si des abonn�s sont pr�sents
    }

    private void PerformPhysicCast()
    {
        switch (overlapType) // Choisir le type d'overlap � effectuer
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
        bool tagCondition = tags == null || tags.Length == 0 || CheckTags(obj, tags); // V�rifier si les tags correspondent
        bool componentCondition = componentNames == null || componentNames.Length == 0 || CheckComponentsByName(obj, componentNames); // V�rifier si les composants correspondent
        return tagCondition && componentCondition; // Retourner vrai si toutes les conditions sont remplies
    }

    private void PerformBoxCast()
    {
        Vector3 boxSize = withScaleSize ? transform.lossyScale : customSize; // D�terminer la taille de la box
        Vector3 boxCenter = withScaleSize ? transform.position : transform.TransformPoint(customOffset); // D�terminer le centre de la box
        Collider[] hits = Physics.OverlapBox(boxCenter, boxSize / 2, transform.rotation, physicCastLayers); // Effectuer le BoxCast

        foreach (Collider hit in hits) // Parcourir les objets d�tect�s
        {
            if (hit.transform == transform) // Ignorer soi-m�me
            {
                continue;
            }
            if (ignoreChildren && hit.transform.IsChildOf(transform)) // Ignorer les enfants si n�cessaire
            {
                continue;
            }

            if (CheckConditions(hit.gameObject, physicCastTags, physicTargetComponentNames)) // V�rifier les conditions
            {
                InvokeOnTouchEvent(); // D�clencher l'�v�nement de collision
                hasCollided = true; // Mettre � jour l'�tat de collision
                break;
            }
        }
    }

    private void PerformSphereCast()
    {
        Vector3 sphereCenter = transform.TransformPoint(customOffset); // D�terminer le centre de la sph�re
        float radius = withScaleSize ? Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) / 2 : sphereRadius; // D�terminer le rayon de la sph�re
        Collider[] hits = Physics.OverlapSphere(sphereCenter, radius, physicCastLayers); // Effectuer le SphereCast

        foreach (Collider hit in hits) // Parcourir les objets d�tect�s
        {
            if (hit.transform == transform) // Ignorer soi-m�me
            {
                continue;
            }
            if (ignoreChildren && hit.transform.IsChildOf(transform)) // Ignorer les enfants si n�cessaire
            {
                continue;
            }

            if (CheckConditions(hit.gameObject, physicCastTags, physicTargetComponentNames)) // V�rifier les conditions
            {
                InvokeOnTouchEvent(); // D�clencher l'�v�nement de collision
                hasCollided = true; // Mettre � jour l'�tat de collision
                break;
            }
        }
    }

    private void PerformCapsuleCast()
    {
        Vector3 point1 = transform.TransformPoint(capsulePoint1 + customOffset); // D�terminer le point 1 de la capsule
        Vector3 point2 = transform.TransformPoint(capsulePoint2 + customOffset); // D�terminer le point 2 de la capsule
        float radius = withScaleSize ? Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) / 2 : capsuleRadius; // D�terminer le rayon de la capsule
        Collider[] hits = Physics.OverlapCapsule(point1, point2, radius, physicCastLayers); // Effectuer le CapsuleCast

        foreach (Collider hit in hits) // Parcourir les objets d�tect�s
        {
            if (hit.transform == transform) // Ignorer soi-m�me
            {
                continue;
            }
            if (ignoreChildren && hit.transform.IsChildOf(transform)) // Ignorer les enfants si n�cessaire
            {
                continue;
            }

            if (CheckConditions(hit.gameObject, physicCastTags, physicTargetComponentNames)) // V�rifier les conditions
            {
                InvokeOnTouchEvent(); // D�clencher l'�v�nement de collision
                hasCollided = true; // Mettre � jour l'�tat de collision
                break;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (useCollision && Time.time >= lastCollisionTime + collisionCooldown &&
            CheckConditions(collision.gameObject, collisionTags, collisionTargetComponentNames)) // V�rifier les conditions de collision
        {
            InvokeOnTouchEvent(); // D�clencher l'�v�nement de collision
            lastCollisionTime = Time.time; // Mettre � jour le temps de la derni�re collision
        }
    }

    private bool CheckTags(GameObject obj, string[] tags)
    {
        if (tags == null || tags.Length == 0) return true; // Si aucun tag n'est sp�cifi�, retourner vrai

        foreach (string tag in tags) // Parcourir les tags sp�cifi�s
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
        if (componentNames == null || componentNames.Length == 0) return true; // Si aucun composant n'est sp�cifi�, retourner vrai

        foreach (string componentName in componentNames) // Parcourir les composants sp�cifi�s
        {
            if (obj.GetComponent(componentName) != null) // Si le composant est trouv�, retourner vrai
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
            Gizmos.color = hasCollided ? Color.red : Color.blue; // D�finir la couleur des Gizmos en fonction de l'�tat de collision

            switch (overlapType) // Choisir le type de Gizmo � dessiner
            {
                case OverlapType.BOX:
                    Vector3 boxSize = withScaleSize ? transform.lossyScale : customSize; // D�terminer la taille de la box
                    Vector3 boxCenter = withScaleSize ? transform.position : transform.TransformPoint(customOffset); // D�terminer le centre de la box
                    Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, boxSize); // D�finir la matrice de transformation des Gizmos
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one); // Dessiner la box
                    break;
                case OverlapType.SPHERE:
                    Vector3 sphereCenter = transform.TransformPoint(customOffset); // D�terminer le centre de la sph�re
                    float radius = withScaleSize ? Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) / 2 : sphereRadius; // D�terminer le rayon de la sph�re
                    Gizmos.DrawWireSphere(sphereCenter, radius); // Dessiner la sph�re
                    break;
                case OverlapType.CAPSULE:
                    Vector3 point1 = transform.TransformPoint(capsulePoint1 + customOffset); // D�terminer le point 1 de la capsule
                    Vector3 point2 = transform.TransformPoint(capsulePoint2 + customOffset); // D�terminer le point 2 de la capsule
                    float capsuleRadius = withScaleSize ? Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) / 2 : this.capsuleRadius; // D�terminer le rayon de la capsule
                    Gizmos.DrawWireSphere(point1, capsuleRadius); // Dessiner la sph�re au point 1
                    Gizmos.DrawWireSphere(point2, capsuleRadius); // Dessiner la sph�re au point 2
                    break;
            }
        }
    }
}
