using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_EnergyAbsorption_Module : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRadius = 5f; // Rayon de détection de la sphère
    public float pullSpeed = 5f; // Vitesse à laquelle les objets sont attirés vers le joueur

    // Transform du joueur et ensemble pour suivre les objets en cours d'absorption
    private Transform _playerTransform;
    private HashSet<GameObject> _pullingObjects = new HashSet<GameObject>();
    private S_EnergyStorage _energyStorage;
    private S_ComboSystem _comboSystem;

    
    private void Start()
    {
        // Initialisation des composants nécessaires
        _playerTransform = GetComponent<Transform>();
        _energyStorage = GetComponent<S_EnergyStorage>();
        _comboSystem = GetComponent<S_ComboSystem>();
    }
    
    private void Update()
    {
        DetectAndPullObjects();
    }

    
    private void DetectAndPullObjects()
    {
        // Si le joueur a déjà atteint sa capacité maximale d'énergie, on arrête la détection
        if (_energyStorage.currentEnergy >= _energyStorage.maxEnergy)
        {
            return;
        }

        // Utiliser une sphère de détection pour trouver tous les objets dans le rayon défini
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        
        // Parcourir tous les objets détectés
        foreach (Collider collider in hitColliders)
        {
            // Ignorer les objets qui sont déjà en train d'être attirés
            if (_pullingObjects.Contains(collider.gameObject))
            {
                continue;
            }

            // Vérifier si l'objet détecté contient un composant EnergyType
            EnergyType energyType = collider.GetComponent<EnergyType>();

            // Si c'est un objet d'énergie valide, commencer à l'attirer vers le joueur
            if (energyType is not null)
            {
                _pullingObjects.Add(collider.gameObject);
                
                // play sound
                SoundManager.Instance.Meth_Gain_Energy();
                
                StartCoroutine(PullAndDestroyObject(collider.gameObject, energyType.energyGivenUseForType));
            }
        }
    }

    // Coroutine pour attirer un objet vers le joueur puis le détruire une fois absorbé
    private IEnumerator PullAndDestroyObject(GameObject obj, float givenPoint)
    {
        while (obj != null)
        {
            if (Vector3.Distance(obj.transform.position, _playerTransform.position) <= 2f)
            {
                break;
            }

            obj.transform.position = Vector3.MoveTowards(obj.transform.position, _playerTransform.position, pullSpeed * Time.deltaTime);
            yield return null;
        }

        if (obj != null)
        {
            _pullingObjects.Remove(obj);
            _energyStorage.AddEnergy(givenPoint * _comboSystem.currentComboMultiplier);
            if (obj.TryGetComponent(out S_AddEnergyTypeWithDelay energySetting))
            {
                S_EnergyPointPoolManager.Instance.ReturnToPool(obj, energySetting.selfPrefab);
            }
            else
            {
                Destroy(obj); // fallback
            }
        }
    }

    // Dessiner une sphère Gizmos dans l'éditeur pour visualiser la zone de détection
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
