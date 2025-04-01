using UnityEditor;
using UnityEngine;

public class Bug_Report : EditorWindow
{
    private int spaceValue = 5;
    
    // Définition des options pour chaque dropdown
    public string[] categoryOptions = { "Assets - Art", "Level Design", "Script", "SFX", "VFX", "Performance", "Game Design", "UI", "Camera" };
    public string[] severityOptions = { "A - Critique", "B - Majeur", "C - Mineur", "D - Trivial" };
    public string[] reproductibilityOptions = { "100% - Always", " 75% - Very Often", "50% - Often", "25% Sometimes", "<25% Randomly, Once" };

    [MenuItem("Tools/QA/Bug Report")]
    private static void ShowWindow()
    {
        var window = GetWindow<Bug_Report>();
        window.titleContent = new GUIContent("Bug Report Window");
        window.minSize = new Vector2(400, 600);
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 400, 600);
        window.Show();
    }

    private void OnGUI()
    {
        
        // Sélection actuelle pour chaque dropdown
        int categoryIndex = Mathf.Max(0, System.Array.IndexOf(categoryOptions, QATool.category));
        int severityIndex = Mathf.Max(0, System.Array.IndexOf(severityOptions, QATool.severity));
        int reproductibilityIndex = Mathf.Max(0, System.Array.IndexOf(reproductibilityOptions, QATool.reproductibility));
        
        GUILayout.Label("Category", EditorStyles.boldLabel);
        categoryIndex = EditorGUILayout.Popup(categoryIndex, categoryOptions);
        QATool.category = categoryOptions[categoryIndex];
        EditorGUILayout.Space(spaceValue);
        
        GUILayout.Label("Severity", EditorStyles.boldLabel);
        severityIndex = EditorGUILayout.Popup(severityIndex, severityOptions);
        QATool.severity = severityOptions[severityIndex];
        EditorGUILayout.Space(spaceValue);
        
        GUILayout.Label("Reproductibility", EditorStyles.boldLabel);
        reproductibilityIndex = EditorGUILayout.Popup(reproductibilityIndex, reproductibilityOptions);
        QATool.reproductibility = reproductibilityOptions[reproductibilityIndex];
        EditorGUILayout.Space(spaceValue);
        
        GUILayout.Label("Summary", EditorStyles.boldLabel);
        QATool.summary = EditorGUILayout.TextArea(QATool.summary, GUILayout.Height(40));
        EditorGUILayout.Space(spaceValue);
        
        GUILayout.Label("Description", EditorStyles.boldLabel);
        QATool.description = EditorGUILayout.TextArea(QATool.description, GUILayout.Height(60));
        EditorGUILayout.Space(spaceValue);
        
        GUILayout.Label("Repro. Steps", EditorStyles.boldLabel);
        QATool.reproSteps = EditorGUILayout.TextArea(QATool.reproSteps, GUILayout.Height(120));
        EditorGUILayout.Space(spaceValue);

        if (GUILayout.Button("Submit Report"))
        {
            //ToDo : Use SpreadsheetUtils methode to add the report to the Sheets
            QATool.SubmitBugReport();
        }
        
        EditorGUILayout.HelpBox(QATool.submitStatus, MessageType.Info);
        
        Repaint();
    }
}