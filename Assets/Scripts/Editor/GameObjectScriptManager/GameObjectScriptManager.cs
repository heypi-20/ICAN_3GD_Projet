using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
public class GameObjectScriptManager : EditorWindow
{
    #region Variables

    private GameObject _selectedGO;
    private Vector2 scrollPosition;
    private Texture2D windowIcon;
    private bool[] foldout; // Tableau pour les foldouts

    #endregion

    [MenuItem("Tools/ScriptManager")]
    private static void ShowWindow()
    {
        var window = GetWindow<GameObjectScriptManager>();
        window.titleContent = new GUIContent("Script Manager", window.windowIcon); // ToDo : Get Game Object Name by Selection
        window.Show();
    }

    private void OnEnable()
    {
        windowIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/Icon1.png");
    }

    private void OnGUI()
    {
        _selectedGO = Selection.activeGameObject; // Récupérer l'objet sélectionné
        bool hasScript = false; 

        if (_selectedGO == null)
        {
            GUILayout.Label("No GameObject selected.");
        }
        else
        {
            GUILayout.Label($"Selected: {_selectedGO.name}", EditorStyles.boldLabel); // J'affiche le nom du GO

            // Début du Scroll
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            // Récupérer tous les composants de l'objet
            Component[] components = _selectedGO.GetComponents<Component>();

            int scriptCount = 0;
            foreach (Component component in components)
            {
                if (component is MonoBehaviour)
                {
                    scriptCount++; // Compter le nombre de scripts attachés
                }
            }

            // Initialiser le tableau foldout uniquement une fois que le nombre de scripts est connu
            if (foldout == null || foldout.Length != scriptCount)
            {
                foldout = new bool[scriptCount];
            }

            int index = 0; // Index pour chaque script

            // Parcourir les composants et afficher ceux qui sont des scripts
            foreach (Component component in components)
            {
                if (component is MonoBehaviour script)
                {
                    hasScript = true;
                    EditorGUILayout.BeginVertical("Box");
                    GUILayout.Label($"Script: {script?.GetType().Name ?? "Unknown"}", EditorStyles.boldLabel);
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField("Script Reference", script, typeof(MonoBehaviour), true);
                    GUI.enabled = true;
                    // Afficher le Foldout pour chaque script
                    foldout[index] = EditorGUILayout.Foldout(foldout[index], "Show Properties");
                    EditorGUILayout.EndVertical();

                    if (foldout[index])
                    {
                        EditorGUILayout.BeginVertical("Box");

                        // Créer un SerializedObject pour afficher ses propriétés
                        SerializedObject serializedObject = new SerializedObject(script);
                        SerializedProperty property = serializedObject.GetIterator();

                        bool hasProperties = false;

                        // Parcourir les propriétés
                        property.NextVisible(true); // Sauter la propriété de base (Nom du script)
                        while (property.NextVisible(false))
                        {
                            hasProperties = true;

                            // Afficher la propriété
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(property.displayName, GUILayout.Width(200)); // Nom de la propriété
                            EditorGUILayout.PropertyField(property, GUIContent.none); // Valeur éditable
                            GUILayout.EndHorizontal();
                        }

                        if (!hasProperties)
                        {
                            GUILayout.Label("This script has no properties.", EditorStyles.helpBox);
                        }

                        serializedObject.ApplyModifiedProperties(); // Appliquer les modifications
                        EditorGUILayout.EndVertical();
                    }

                    index++; // Incrémenter l'index
                }
            }

            if (!hasScript)
            {
                EditorGUILayout.HelpBox("This Game Object has no script.", MessageType.Info);
            }

            GUILayout.EndScrollView();
        }

        this.Repaint();
    }
}
