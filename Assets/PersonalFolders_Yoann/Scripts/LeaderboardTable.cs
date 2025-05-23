using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardTable : MonoBehaviour
{
    [Header("Game Objects")]
    public Transform _container; // Le parent dans lequel les entrées du classement seront instanciées
    public Transform _template;  // Le prefab qui sera utilisé pour chaque entrée

    private List<LeaderboardEntry> _leaderboardEntryList;     // Liste contenant les données du classement
    private List<Transform> _leaderboardTransformList;        // Liste des objets UI instanciés pour le classement
    
    
    

    void Awake()
    {
        _template.gameObject.SetActive(false); // On désactive le template dans la scène pour qu’il ne soit pas visible

        // Création d’une liste de scores fictifs avec des noms "AAA" et des scores aléatoires
        _leaderboardEntryList = new List<LeaderboardEntry>()
        {
            new LeaderboardEntry{ _score = Random.Range(0,10000), _name = "AAA"},
            new LeaderboardEntry{ _score = Random.Range(0,10000), _name = "AAA"},
            new LeaderboardEntry{ _score = Random.Range(0,10000), _name = "AAA"},
            new LeaderboardEntry{ _score = Random.Range(0,10000), _name = "AAA"},
            new LeaderboardEntry{ _score = Random.Range(0,10000), _name = "AAA"},
        };

        _leaderboardTransformList = new List<Transform>(); // Initialisation de la liste qui contiendra les objets instanciés

        // Pour chaque entrée dans la liste de données, on crée un élément visuel
        foreach (LeaderboardEntry leaderboardEntry in _leaderboardEntryList)
        {
            AddUserEntry(leaderboardEntry, _container, _leaderboardTransformList);
        }
    }

    // Méthode responsable de créer une nouvelle ligne de classement dans l’interface
    private void AddUserEntry(LeaderboardEntry _entry, Transform container, List<Transform> leaderboardList)
    {
        // Instanciation du template dans le container parent
        Transform _newUserMark = Instantiate(_template, container);
        _newUserMark.gameObject.SetActive(true); // Activation du clone (le template de base étant désactivé)
        
        // On récupère l'objet enfant appelé "Container" qui contient les éléments de texte
        Transform _textContainer = _newUserMark.Find("Container");

        // Définition du rang (position dans la liste)
        int _rank = leaderboardList.Count + 1;
        _textContainer.Find("RankText").GetComponent<TextMeshProUGUI>().text = _rank.ToString();

        // Définition du nom du joueur
        string _name = _entry._name;
        _textContainer.Find("NameText").GetComponent<TextMeshProUGUI>().text = _name;

        // Génération aléatoire d’un temps au format HH:MM:SS
        string _time = Random.Range(0,2) + Random.Range(0,9) + ":" + 
                       Random.Range(0,5) + Random.Range(0,9) + ":" + 
                       Random.Range(0,9) + Random.Range(0,9) + Random.Range(0,9);
        _textContainer.Find("TimeText").GetComponent<TextMeshProUGUI>().text = _time;

        // Score tiré de l’entrée
        int _randomScore = _entry._score; // Ce nom pourrait être clarifié car le score n’est plus "random" ici
        _textContainer.Find("ScoreText").GetComponent<TextMeshProUGUI>().text = _randomScore.ToString();

        // On ajoute le nouvel objet à la liste des entrées visuelles
        leaderboardList.Add(_newUserMark);
    }
    
    // Classe interne représentant une entrée dans le classement
    private class LeaderboardEntry
    {
        public string _name;  // Nom du joueur
        public int _score;    // Score du joueur
    }
}



