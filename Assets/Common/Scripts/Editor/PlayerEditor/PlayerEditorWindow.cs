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

    private string profileName = String.Empty;
    private PlayerProfile[] profiles;
    private string[] profileNames;
    
    private Vector2 scrollPosition;

    private void OnGUI()
    {
        if (FindObjectOfType<S_CustomCharacterController>() == null) {
            EditorGUILayout.LabelField("NO PLAYER FOUND");
            return;
        }
        
        player = FindObjectOfType<S_CustomCharacterController>().gameObject;
        
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
        
        LoadAllAssetsOfType<PlayerProfile>(out profiles);
        profileNames = new string[profiles.Length];

        for (int i = 0; i < profiles.Length; i++) {
            profileNames[i] = profiles[i].name;
        }

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
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.BeginVertical("Box");
            moduleEditor.OnInspectorGUI();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndScrollView();
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

    #region Utility Functions

    private GUIStyle LabelTextColor(Color color)
    {
        GUIStyle labelTextStyle = new GUIStyle(EditorStyles.textField);
        labelTextStyle.normal.textColor = color;
        
        return labelTextStyle;
    }
    
    private static void LoadAllAssetsOfType<T>(out T[] assets) where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets("t:"+typeof(T));
        assets = new T[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
    }

    #endregion
    
    private void SaveProfile()
    {
        profileName = EditorGUILayout.TextField("Profile Name", profileName);
        
        if (GUILayout.Button("Save Profile", GUILayout.Width(100))) {
            if (profileName != String.Empty && !System.IO.File.Exists("Assets/Common/Data/PlayerProfiles/" + profileName + ".asset")) {
                PlayerProfile profile = CreateInstance<PlayerProfile>();
                profile.isEnable = new List<bool>();
                foreach (MonoBehaviour module in modules) {
                    profile.isEnable.Add(module.enabled);
                }
                AssetDatabase.CreateAsset(profile, "Assets/Common/Data/PlayerProfiles/" + profileName + ".asset");
                profileName = String.Empty;
                Debug.Log("Save");
            } else if (profileName == String.Empty) {
                PlayerProfile profile = CreateInstance<PlayerProfile>();
                profile.isEnable = new List<bool>();
                foreach (MonoBehaviour module in modules) {
                    profile.isEnable.Add(module.enabled);
                }
                profile.editor = moduleEditor;

                AssetDatabase.CreateAsset(profile, "Assets/Common/Data/PlayerProfiles/PlayerProfile.asset");
            }
        }
    }
    
    private int index = 0;
        
    private void LoadProfile()
    {
        index = EditorGUILayout.Popup(index, profileNames, GUILayout.Width(100));
    }
    
    private void MonitorWindow()
    {
        GUILayout.Label("Monitor Window", EditorStyles.boldLabel);
        
        
    }
}
