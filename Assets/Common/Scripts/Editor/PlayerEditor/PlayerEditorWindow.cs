using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        profileName = EditorGUILayout.TextField("Profile Name (MUST ADD)", profileName);
        
        if (GUILayout.Button("Save Profile", GUILayout.Width(100))) {
            if (profileName != String.Empty && !File.Exists("Assets/Common/Data/PlayerProfiles/" + profileName + ".asset")) {
                PlayerProfile playerProfile = CreateInstance<PlayerProfile>();
                playerProfile.isEnable = new List<bool>();
                playerProfile.moduleProfiles = new List<ModuleProfile>();
                
                ProfileCreator(playerProfile);
                AssetDatabase.CreateAsset(playerProfile, "Assets/Common/Data/PlayerProfiles/" + profileName + ".asset");
            } else if (profileName != String.Empty && File.Exists("Assets/Common/Data/PlayerProfiles/" + profileName + ".asset")) {
                PlayerProfile playerProfile = AssetDatabase.LoadAssetAtPath<PlayerProfile>("Assets/Common/Data/PlayerProfiles/" + profileName + ".asset");
                
                ProfileHandler(playerProfile);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            } else if (profileName == String.Empty) {
                
                profileName = "MUST ADD PROFILE NAME";
                
            }
        }
    }

    private void ProfileCreator(PlayerProfile playerProfile)
    {
        foreach (MonoBehaviour module in modules) {
            ModuleProfile moduleProfile = CreateInstance<ModuleProfile>();
            moduleProfile.fieldNames = new List<string>();
            moduleProfile.dataDictionary = new Dictionary<string, object>();
                    
            FieldInfo[] fields = module.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                    
            foreach (FieldInfo field in fields) {
                moduleProfile.fieldNames.Add(field.Name);
                moduleProfile.dataDictionary.Add(field.Name, field.GetValue(module));
            }
                    
            playerProfile.isEnable.Add(module.enabled);
            playerProfile.moduleProfiles.Add(moduleProfile);
            foreach (KeyValuePair<string, object> kvp in moduleProfile.dataDictionary) {
                Debug.Log("Key = " + kvp.Key + ", Value = " + kvp.Value);
            }
            if (!Directory.Exists("Assets/Common/Data/ModuleProfiles/" + profileName)) {
                Directory.CreateDirectory("Assets/Common/Data/ModuleProfiles/" + profileName);
                AssetDatabase.Refresh();
            }
            AssetDatabase.CreateAsset(moduleProfile, "Assets/Common/Data/ModuleProfiles/" + profileName + "/" + module.GetType().Name + ".asset");
        }
    }

    private void ProfileHandler(PlayerProfile playerProfile)
    {
        LoadAllAssetsOfType<ModuleProfile>(out ModuleProfile[] moduleProfiles);
        
        for (int i = 0; i < moduleProfiles.Length; i++) {
            FieldInfo[] newFields = modules[i].GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            moduleProfiles[i] = CreateInstance<ModuleProfile>();
            moduleProfiles[i].fieldNames = new List<string>();
            moduleProfiles[i].dataDictionary = new Dictionary<string, object>();

            foreach (FieldInfo newField in newFields) {
                moduleProfiles[i].fieldNames.Add(newField.Name);
                moduleProfiles[i].dataDictionary.Add(newField.Name, newField.GetValue(modules[i]));
            }   
            
            playerProfile.isEnable[i] = modules[i].enabled;
            playerProfile.moduleProfiles[i] = moduleProfiles[i];
            foreach (KeyValuePair<string, object> kvp in moduleProfiles[i].dataDictionary) {
                Debug.Log("Key = " + kvp.Key + ", Value = " + kvp.Value);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private int index;
        
    private void LoadProfile()
    {
        index = EditorGUILayout.Popup(index, profileNames, GUILayout.Width(100));
    }
    
    private void MonitorWindow()
    {
        GUILayout.Label("Monitor Window", EditorStyles.boldLabel);
        
        
    }
}
