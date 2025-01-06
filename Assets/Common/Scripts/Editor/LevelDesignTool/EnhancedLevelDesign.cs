using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnhancedLevelDesign : EditorWindow
{
    #region Variables


    private float width;

    private Vector3 moveSnap;
    private float rotationSnap;
    private float scaleSnap;
    
    private bool linkMoveSnapValues = false; // Contrôle si les valeurs de Move Snap sont identiques
    private bool snapEnabled;

    #endregion


    [MenuItem("Tools/Enhanced Level Design")]
    private static void ShowWindow()
    {
        var window = GetWindow<EnhancedLevelDesign>();
        window.titleContent = new GUIContent("Enhanced Level Design");
        window.Show();
    }

    private void OnEnable()
    {
        // Charger les valeurs actuelles des Snap Settings
        LoadSnapSettings();
        
        // Récupère l'état actuel du snap de la grille au lancement de la fenêtre
        snapEnabled = EditorPrefs.GetBool("SceneView.gridSnap", false);
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
            GameObject selectedObject = Selection.activeGameObject;// Recupere le GO
            
            Vector3 currentPosition = selectedObject.transform.position; // Arrondi chaque position
            Vector3 roundedPosition = new Vector3(
                Mathf.Round(currentPosition.x),
                Mathf.Round(currentPosition.y),
                Mathf.Round(currentPosition.z)
            );
            
            Undo.RecordObject(selectedObject.transform, "Reset To Unit Position"); // Undo CTRL Z
            selectedObject.transform.position = roundedPosition; // Met l'objet a la pos Arrondi
        }
    }

    private void ActivateSnap()
    {
        // Applique le snap à toutes les vues de scène
        foreach (SceneView sceneView in SceneView.sceneViews)
        {
            // Activer/désactiver le snap de la grille
            
        }
    }
    
    private void HandleMouseWheel()
    {
        // Si un événement de molette est détecté
        if (Event.current.type == EventType.ScrollWheel)
        {
            float delta = Event.current.delta.y;  // Ajuste la vitesse de la molette si nécessaire

            // Modifier la valeur en fonction du champ actif
            if (linkMoveSnapValues)
            {
                moveSnap.x += delta; // Modifie X, Y et Z de manière liée
                moveSnap.y = moveSnap.x;
                moveSnap.z = moveSnap.x;
            }
            else
            {
                if (GUI.GetNameOfFocusedControl() == "Move X") // Vérifier si le champ X est actif
                    moveSnap.x += delta;
                else if (GUI.GetNameOfFocusedControl() == "Move Y") // Vérifier si le champ Y est actif
                    moveSnap.y += delta;
                else if (GUI.GetNameOfFocusedControl() == "Move Z") // Vérifier si le champ Z est actif
                    moveSnap.z += delta;
            }

            // Limiter les valeurs minimales pour éviter de petites valeurs indésirables
            moveSnap.x = Mathf.Max(0.1f, moveSnap.x);
            moveSnap.y = Mathf.Max(0.1f, moveSnap.y);
            moveSnap.z = Mathf.Max(0.1f, moveSnap.z);

            // Réactualiser l'interface
            Event.current.Use(); // Consommer l'événement de la molette
        }
    }
    
    private void OnGUI()
    {
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
        
        float originalLabelWidth = EditorGUIUtility.fieldWidth;
        EditorGUIUtility.fieldWidth = 0;
        moveSnap.x = EditorGUILayout.FloatField("X", moveSnap.x); // Activer l'édition pour X

        
        if (linkMoveSnapValues) // Synchroniser Y et Z dès que X est modifié et que le mode Link est activé
        {
            moveSnap.y = moveSnap.x;
            moveSnap.z = moveSnap.x;
        }

        // Désactiver l'édition pour Y et Z si linkMoveSnapValues est activé
        GUI.enabled = !linkMoveSnapValues;
        moveSnap.y = EditorGUILayout.FloatField("Y", moveSnap.y);
        moveSnap.z = EditorGUILayout.FloatField("Z", moveSnap.z);
        GUI.enabled = true;
        EditorGUIUtility.fieldWidth = originalLabelWidth;

        SaveSnapSettings();
        EditorGUILayout.EndHorizontal();

        #endregion
        
        #region Snap Préféfinie
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Snap Préféfinie", EditorStyles.boldLabel);
        
        if(GUILayout.Button("0.25", GUILayout.Width(50)))
        {
            moveSnap.x = 0.25f;
            moveSnap.y = 0.25f;
            moveSnap.z = 0.25f;
        }  
        
        if(GUILayout.Button("0.5", GUILayout.Width(50)))
        {
            moveSnap.x = 0.5f;
            moveSnap.y = 0.5f;
            moveSnap.z = 0.5f;
        }  
        
        if(GUILayout.Button("1", GUILayout.Width(50)))
        {
            moveSnap.x = 1f;
            moveSnap.y = 1f;
            moveSnap.z = 1f;
        }  
        
        if(GUILayout.Button("2", GUILayout.Width(50)))
        {
            moveSnap.x = 2f;
            moveSnap.y = 2f;
            moveSnap.z = 2f;
        }  
        
        if(GUILayout.Button("5", GUILayout.Width(50)))
        {
            moveSnap.x = 5f;
            moveSnap.y = 5f;
            moveSnap.z = 5f;
        }  
        
        GUILayout.EndHorizontal();
        
        #endregion
        
        rotationSnap = EditorGUILayout.FloatField("Rotation", rotationSnap);
        scaleSnap = EditorGUILayout.FloatField("Scale", scaleSnap);
        
        GUILayout.Label($"Current Snap Values:\nMove: {EditorSnapSettings.move}\nRotate: {EditorSnapSettings.rotate}\nScale: {EditorSnapSettings.scale}", EditorStyles.helpBox);
        
        GUILayout.Space(10);

        #region Simple GO Placement

        GameObject[] gameObjectBank;
        

        #endregion

        #region Button

                GUILayout.Label("Button", EditorStyles.boldLabel);
                
                GUILayout.Label("Shortcut");
                GUILayout.BeginHorizontal();
                
                if(GUILayout.Button("To Unit Position"))
                {
                    ResetSelectedObjectToUnitPosition();
                }       
                
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                GUILayout.Label("Manual Action");
                GUILayout.BeginHorizontal();
                
                if(GUILayout.Button("Reload Snap Settings"))
                {
                    LoadSnapSettings();
                }        
                
                if(GUILayout.Button("Save Snap Settings"))
                {
                    SaveSnapSettings();
                }
                
                GUILayout.EndHorizontal();

        #endregion

        #region Appel de Fonction

        // HandleMouseWheel();

        #endregion
        
        this.Repaint();
    }
}