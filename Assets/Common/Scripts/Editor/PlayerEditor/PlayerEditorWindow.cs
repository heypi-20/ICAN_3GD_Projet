using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

    private Editor moduleEditor;
    private bool showEditor;
    
    private void OnGUI()
    {
        if (FindObjectOfType<S_PlayerMultiCam>() == null) {
            EditorGUILayout.LabelField("NO PLAYER FOUND");
            return;
        }
        
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
        GUILayout.Label("Modules Window", EditorStyles.boldLabel);

        modules = GetModules();
        GUIStyle labelStyle;

        foreach (MonoBehaviour module in modules) {
            if (module.enabled)
                labelStyle = LabelTextColor(Color.green);
            else
                labelStyle = LabelTextColor(Color.red);
                        
            EditorGUILayout.BeginHorizontal();
                        
            EditorGUILayout.LabelField(module.GetType().Name, labelStyle);
            GUILayout.FlexibleSpace();
            module.enabled = GUILayout.Toggle(module.enabled, "Enable/Disable");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Show Editor")) {
                if (moduleEditor != null)
                    DestroyImmediate(moduleEditor);
                moduleEditor = Editor.CreateEditor(module);
            }
            EditorGUILayout.EndHorizontal();
            
            Repaint();
        }

        if (moduleEditor != null) {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical("Box");
            moduleEditor.OnInspectorGUI();
            EditorGUILayout.EndVertical();
        }
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal("Box");
        SaveProfile();
        LoadProfile();
        EditorGUILayout.EndHorizontal();
        
        Repaint();
    }

    #region Get Player's Modules

    private MonoBehaviour[] GetModules()
    {
        var scripts = player.GetComponents<MonoBehaviour>();
        MonoBehaviour[] getModules = new MonoBehaviour[GetNbOfModules(scripts)];
        int i = 0;
        
        foreach (var script in scripts) {
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
        
        foreach (var script in scripts) {
            if (script.GetType().Name.Contains("Module")) {
                count++;
            }
        }
        return count;
    }

    #endregion

    private GUIStyle LabelTextColor(Color color)
    {
        GUIStyle labelTextStyle = new GUIStyle(EditorStyles.textField);
        labelTextStyle.normal.textColor = color;
        
        return labelTextStyle;
    }

    private void SaveProfile()
    {
        string profileName = String.Empty;
        profileName = EditorGUILayout.TextField("Profile Name", profileName);
        
        if (GUILayout.Button("Save Profile", GUILayout.Width(100))) {
            Debug.Log("Save");
            if (profileName != String.Empty && !System.IO.File.Exists("Assets/Common/Data/PlayerProfiles" + profileName + ".asset")) {
                PlayerProfile profile = CreateInstance<PlayerProfile>();
                profile.isEnable = new List<bool>();
                foreach (MonoBehaviour module in modules) {
                    profile.isEnable.Add(module.enabled);
                }
                AssetDatabase.CreateAsset(profile, "Assets/Common/Data/PlayerProfiles" + profileName + ".asset");
            } else if (profileName == String.Empty) {
                PlayerProfile profile = CreateInstance<PlayerProfile>();
                profile.isEnable = new List<bool>();
                foreach (MonoBehaviour module in modules) {
                    profile.isEnable.Add(module.enabled);
                }
                AssetDatabase.CreateAsset(profile, "Assets/Common/Data/PlayerProfiles/PlayerProfile.asset");
            }
        }
    }

    private void LoadProfile()
    {
        if (GUILayout.Button("Load Profile", GUILayout.Width(100))) {
            Debug.Log("Load");
        }
    }
    
    private void MonitorWindow()
    {
        GUILayout.Label("Monitor Window", EditorStyles.boldLabel);
        
        
    }
}
