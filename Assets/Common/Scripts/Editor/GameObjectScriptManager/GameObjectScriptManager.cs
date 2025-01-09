using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ScriptInspectorWindow : EditorWindow
{
    private GameObject selectedGameObject; // GameObject sélectionné
    private Vector2 scrollPosition;       // Pour la barre de défilement
    private Editor[] scriptEditors;       // Éditeurs pour chaque script

    [MenuItem("Tools/Script Inspector")]
    public static void ShowWindow()
    {
        var window = GetWindow<ScriptInspectorWindow>();
        window.titleContent = new GUIContent("Script Inspector", EditorGUIUtility.IconContent("Handle").image);
        window.Show();
    }

    private void OnSelectionChange()
    {
        // Mettre à jour lorsque la sélection change
        selectedGameObject = Selection.activeGameObject;
        UpdateScriptEditors();
    }

    private void UpdateScriptEditors()
    {
        // Nettoyer les éditeurs précédents
        if (scriptEditors != null)
        {
            foreach (var editor in scriptEditors)
            {
                DestroyImmediate(editor);
            }
        }

        if (selectedGameObject != null)
        {
            // Filtrer pour ne garder que les scripts (MonoBehaviour)
            Component[] components = selectedGameObject.GetComponents<Component>();
            var scripts = System.Array.FindAll(components, c => c is MonoBehaviour);

            // Créer un éditeur pour chaque script
            scriptEditors = new Editor[scripts.Length];
            for (int i = 0; i < scripts.Length; i++)
            {
                scriptEditors[i] = Editor.CreateEditor(scripts[i]);
            }
        }
        else
        {
            scriptEditors = null;
        }
    }

    private void OnGUI()
    {
        if (selectedGameObject == null)
        {
            GUILayout.Label("No GameObject selected.", EditorStyles.boldLabel);
            return;
        }
        
        EditorGUILayout.LabelField("Parametre d'affichage", EditorStyles.boldLabel);

        // Zone défilable
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        if (scriptEditors != null)
        {
            foreach (var editor in scriptEditors)
            {
                if (editor == null) continue;

                EditorGUILayout.BeginVertical("box");
                editor.OnInspectorGUI(); // Afficher le script avec le même affichage que l'inspecteur
                EditorGUILayout.EndVertical();
            }
        }

        GUILayout.EndScrollView();
        
        this.Repaint();
    }
    
}


