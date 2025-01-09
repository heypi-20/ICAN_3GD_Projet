using UnityEditor;
using UnityEngine;

public class EnhancedLevelDesign : EditorWindow
{
    #region Variables
    
    public float[] personnalIncrement;

    private Vector3 moveSnap;
    private float rotationSnap;
    private float scaleSnap;
    
    private bool linkMoveSnapValues = false; // Contrôle si les valeurs de Move Snap sont identiques

    #endregion

    #region Propreté de l'editor

    private int fieldSnapSize = 40;

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
        
        #region Snap Préféfinie
        
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
        
        for (int i = 0; i < personnalIncrement.Length; i++)
        {
            
            EditorGUILayout.FloatField("Personnal Increment", personnalIncrement[i]);
            if (GUILayout.Button("increment"))
            {
                // Action lorsque le bouton est cliqué
                Debug.Log(personnalIncrement[i] + " clicked!");
            }
        }

        #region Appel de Fonction

        #endregion
        
        this.Repaint();
    }
}