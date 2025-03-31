using UnityEditor;
using UnityEngine;

public class Bug_Report : EditorWindow
{
    private int spaceValue = 5;
    private Vector2 scrollPos;

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
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        GUILayout.Label("Category", EditorStyles.boldLabel);
        QATool.category = EditorGUILayout.TextField(QATool.category);
        EditorGUILayout.Space(spaceValue);
        
        GUILayout.Label("Severity", EditorStyles.boldLabel);
        QATool.severity = EditorGUILayout.TextField(QATool.severity);
        EditorGUILayout.Space(spaceValue);
        
        GUILayout.Label("Reproductibility", EditorStyles.boldLabel);
        QATool.reproductibility = EditorGUILayout.TextField(QATool.reproductibility);
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
        
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Submit Report"))
        {
            //ToDo : Use SpreadsheetUtils methode to add the report to the Sheets
            QATool.SubmitBugReport();
        }
        
        this.Repaint();
    }
}