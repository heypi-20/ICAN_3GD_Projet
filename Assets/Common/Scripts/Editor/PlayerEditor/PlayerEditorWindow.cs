using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerEditorWindow : EditorWindow
{
    [MenuItem("Tools/Player Editor Window")]
    private static void ShowWindow()
    {
        var window = GetWindow<PlayerEditorWindow>();
        window.titleContent = new GUIContent("Player Editor Window");
        window.Show();
    }

    private bool windowSwitch;
    private GameObject player;

    private MonoBehaviour[] modules;
    private bool modulesSwitch;
    
    private int countModules = 0;

    private void OnGUI()
    {
        player = FindObjectOfType<S_PlayerMultiCam>().gameObject;
        
        if (GUILayout.Button("Change Window", GUILayout.Width(120))) {
            windowSwitch = !windowSwitch;
        }
        
        if (!windowSwitch)
            ModulesWindow();
        else
            MonitorWindow();
    }

    private void ModulesWindow()
    {
        GUILayout.Label("Modules Window", "Box");

        modules = GetModules();
        
        foreach(MonoBehaviour module in modules) {
            if (module.enabled) {
                GUI.color = Color.green;
            } else {
                GUI.color = Color.red;
            }
            
            GUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField(module.GetType().Name);
            module.enabled = EditorGUILayout.Toggle(module.enabled);
            
            GUILayout.EndHorizontal();
            Repaint();
        }
    }

    private MonoBehaviour[] GetModules()
    {
        var scripts = player.GetComponents<MonoBehaviour>();
        MonoBehaviour[] getModules = new MonoBehaviour[GetNbOfModules(scripts)];
        int i = 0;
        
        foreach(var script in scripts) {
            if (script.GetType().Name.Contains("Module")) {
                getModules[i] = script;
                i++;
            }
        }
        return getModules;
    }

    private int GetNbOfModules(MonoBehaviour[] scripts)
    {
        int count = 0;
        
        foreach(var script in scripts) {
            if (script.GetType().Name.Contains("Module")) {
                count++;
            }
        }
        return count;
    }

    private void MonitorWindow()
    {
        GUILayout.Label("Monitor Window");
    }
}
