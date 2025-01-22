using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public class EnhancedLevelDesign : EditorWindow
{
    #region Variables
    
    private GameObject selectedGO; 
    private GameObject newGO;
    
    private Vector3 moveSnap;
    private float rotationSnap;
    private float scaleSnap;

    private bool showDropdown = false;
    
    private bool linkMoveSnapValues = false; // Contrôle si les valeurs de Move Snap sont identiques
    
    #endregion

    #region Propreté de l'editor

    private int fieldSnapSize = 40;
    private Vector2 scrollPos;

    #endregion
    
    [MenuItem("Tools/Enhanced Level Design")]
    private static void ShowWindow()
    {
        var window = GetWindow<EnhancedLevelDesign>();
        window.titleContent = new GUIContent("Enhanced Level Design", EditorGUIUtility.IconContent("d_MainStageView").image);
        window.Show();
    }

    private void OnEnable()
    {
        // Charger les valeurs actuelles des Snap Settings
        LoadSnapSettings();
    }

    private void LoadSnapSettings()
    {
        moveSnap = EditorSnapSettings.move;
        rotationSnap = EditorSnapSettings.rotate;
        scaleSnap = EditorSnapSettings.scale;
    }

    private void SaveSnapSettings()
    {
        // Appliquer les valeurs directement au système de snap Unity
        EditorSnapSettings.move = moveSnap;
        EditorSnapSettings.rotate = rotationSnap;
        EditorSnapSettings.scale = scaleSnap;
    }
    
    // Réinitialiser la position du GameObject sélectionné à l'unité
    private void ResetSelectedObjectToUnitPosition()
    {
        if (Selection.activeGameObject != null)// Vérifie si l'object est séléctionner
        {
            selectedGO = Selection.activeGameObject;// Recupere le GO
            
            Vector3 currentPosition = selectedGO.transform.position; // Arrondi chaque position
            Vector3 roundedPosition = new Vector3(
                Mathf.Round(currentPosition.x),
                Mathf.Round(currentPosition.y),
                Mathf.Round(currentPosition.z)
            );
            
            Undo.RecordObject(selectedGO.transform, "Reset To Unit Position"); // Undo CTRL Z
            selectedGO.transform.position = roundedPosition; // Met l'objet a la pos Arrondi
        }
    }
    
    // Fonction pour redimensionner le tableau 
    private void ResizeArray(ref float[] array, int newSize, int nbIncrement)
    {
        if (nbIncrement >= 1)
        {
            float[] newArray = new float[newSize];
            for (int i = 0; i < Mathf.Min(array.Length, newArray.Length); i++)
            {
                newArray[i] = array[i];
            }
            array = newArray;
        }
    }
    
    private void ReplaceSelectedGameObject()
    {
        // Instancier le nouveau GameObject à la position de l'ancien
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(newGO);
        if (instance != null)
        {
            instance.transform.position = selectedGO.transform.position;
            instance.transform.rotation = selectedGO.transform.rotation;
            instance.transform.localScale = selectedGO.transform.localScale;

            // Supprimer l'ancien GameObject
            Undo.DestroyObjectImmediate(selectedGO);
            // Enregistrer l'opération
            Undo.RegisterCreatedObjectUndo(instance, "Replace GameObject");
            // Sélectionner le nouvel objet
            Selection.activeGameObject = instance;
        }
    }
    
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        selectedGO = Selection.activeGameObject;
        if (selectedGO != null)
        {
            EditorGUILayout.LabelField("Selected GameObject : " + selectedGO.name, EditorStyles.helpBox);
        }
        else
        {
            EditorGUILayout.LabelField("No Selected GameObject.", EditorStyles.helpBox);
        }
        
        #region Snap Setting
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("Snap Settings", EditorStyles.boldLabel);
        // width = EditorGUILayout.Slider("Size", width, 0f, 100f);
        
        #region Move
        // Gestion des champs Move Snap
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel("Move", EditorStyles.boldLabel);
        GUIContent icon = linkMoveSnapValues ? EditorGUIUtility.IconContent("Link") : EditorGUIUtility.IconContent("Unlinked"); // Charger l'icône Link ou Unlink en fonction de l'état du toggle
        if (GUILayout.Button(icon, EditorStyles.iconButton)) // Créer un bouton avec l'icône appropriée
        {
            linkMoveSnapValues = !linkMoveSnapValues; // Inverser l'état de linkMoveSnapValues lors du clic
        }
        
        moveSnap.x = EditorGUILayout.FloatField(GUIContent.none, moveSnap.x, GUILayout.Width(fieldSnapSize)); // Activer l'édition pour X
        
        if (linkMoveSnapValues) // Synchroniser Y et Z dès que X est modifié et que le mode Link est activé
        {
            moveSnap.y = moveSnap.x;
            moveSnap.z = moveSnap.x;
        }

        // Désactiver l'édition pour Y et Z si linkMoveSnapValues est activé
        GUI.enabled = !linkMoveSnapValues;
        moveSnap.y = EditorGUILayout.FloatField(GUIContent.none, moveSnap.y, GUILayout.Width(fieldSnapSize));
        moveSnap.z = EditorGUILayout.FloatField(GUIContent.none, moveSnap.z, GUILayout.Width(fieldSnapSize));
        GUI.enabled = true;

        SaveSnapSettings();

        #endregion
        
        GUILayout.EndHorizontal();
       
        #region Snap Préféfinie
        
        EnhancedLDSave ELDSaves = (EnhancedLDSave)AssetDatabase.LoadAssetAtPath("Assets/Common/Data/EnhancedLDSaves/Saves.asset", typeof(EnhancedLDSave));
        
        GUILayout.BeginHorizontal();

        if (ELDSaves.nbIncrement != ELDSaves.increment.Length)
        {
            ELDSaves.nbIncrement = ELDSaves.increment.Length;
            // ResizeArray(ref ELDSaves.increment, ELDSaves.nbIncrement);
        }
        
        int id = 1; // ← DEGUEU CA, A CHANGER !!!!! !!!!!! (la valeur est remis a UN tout le temps !!!!!)
        
        GUILayout.BeginVertical("box");
        
        showDropdown = EditorGUILayout.Foldout(showDropdown, "Incrément Prédéfinie", true, EditorStyles.foldoutHeader);
        
        if (showDropdown)
        {
            for (int i = 0; i < ELDSaves.increment.Length; i++)
            {
                if (ELDSaves.nbIncrement > 0)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Increment " + id, EditorStyles.boldLabel);
                    ELDSaves.increment[i] = EditorGUILayout.FloatField(ELDSaves.increment[i], GUILayout.Width(100));
                    if (GUILayout.Button("Apply", GUILayout.Width(60)))
                    {
                        moveSnap.x = ELDSaves.increment[i];
                        moveSnap.y = ELDSaves.increment[i];
                        moveSnap.z = ELDSaves.increment[i];
                        Debug.Log("Scale is now : " + ELDSaves.increment[i]);
                    }

                    GUILayout.EndHorizontal();

                    // Le systeme d'ID pour les Prefix Label est Dégueu, A CHANGER !!!!!
                    id++;
                }
            }
        }


        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
                
        if(GUILayout.Button("Reload Snap Settings", GUILayout.Width(200)))
        {
            LoadSnapSettings();
        }        
                
        if(GUILayout.Button("Save Snap Settings", GUILayout.Width(200)))
        {
            SaveSnapSettings();
        }
        
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();

        GUI.enabled = false;
        EditorGUILayout.ObjectField(GUIContent.none, ELDSaves, typeof(ScriptableObject), false, GUILayout.Width(200));
        GUI.enabled = true;

        if (GUILayout.Button("Create New Saves", GUILayout.Width(200)))
        {
            //ToDo : Create New Save Assets & Load it
        }
        
        GUILayout.EndHorizontal();
        
        // GUI.enabled = false;
        // EditorGUILayout.IntField(ELDSaves.nbIncrement, GUILayout.Width(40));
        // GUI.enabled = true;

        // if (GUILayout.Button("Create New Increment", GUILayout.Width(150)) && ELDSaves.nbIncrement < 10)
        // {
        //     ELDSaves.nbIncrement++;
        // }        
        //
        // if (GUILayout.Button("Delete Last Increment", GUILayout.Width(150)) && ELDSaves.nbIncrement >= 1)
        // {
        //     ELDSaves.nbIncrement--;
        // }


        
        #endregion
        
        rotationSnap = EditorGUILayout.FloatField("Rotation", rotationSnap);
        scaleSnap = EditorGUILayout.FloatField("Scale", scaleSnap);
        
        GUILayout.Label("Manual Action");
        GUILayout.BeginHorizontal();
        
        GUILayout.Label($"Current Snap Values:\nMove: {EditorSnapSettings.move}\nRotate: {EditorSnapSettings.rotate}\nScale: {EditorSnapSettings.scale}", EditorStyles.helpBox);
        
                
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();

        #endregion
        
        #region Change Game Object
        GUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Replace Selected GameObject", EditorStyles.boldLabel);
        
        // Champ pour sélectionner le nouveau GameObject
        newGO = (GameObject)EditorGUILayout.ObjectField("Object to Instantiate", newGO, typeof(GameObject), false);
        
        // Bouton pour effectuer le remplacement
        if (GUILayout.Button("Replace") && selectedGO != null && newGO != null)
        {
            ReplaceSelectedGameObject();
        }

        GUILayout.EndVertical();
        #endregion

        #region Button
        GUILayout.BeginVertical("box");
                GUILayout.Label("Button", EditorStyles.boldLabel);
                
                GUILayout.Label("Shortcut");
                GUILayout.BeginHorizontal();
                
                if(GUILayout.Button("To Unit Position"))
                {
                    ResetSelectedObjectToUnitPosition();
                }       
                
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
        GUILayout.EndVertical();

        #endregion

        #region Appel de Fonction
        
        // Todo : Mettre des fonction qui ne doivent s'appeller automatiquement

        #endregion
        
        EditorGUILayout.EndScrollView();
        
        this.Repaint();
    }
}