using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Eflatun.SceneReference;

public class SceneManagerWindow : EditorWindow
{
    [MenuItem("Tools/SceneManagerWindow")]
    private static void ShowWindow()
    {
        var window = GetWindow<SceneManagerWindow>();
        window.titleContent = new GUIContent("SceneManagerWindow");
        window.Show();
    }
    
    [MenuItem("Tools/Reopen Project")]
    public static void ReopenProject()
    {
        EditorApplication.OpenProject(System.IO.Directory.GetCurrentDirectory());
    }

    [HideInInspector] public SceneData[] scenes;
    
    private string[] scenesGUIDs;
    private string missingScene;
    
    private bool checkSceneData = false;

    private void OnGUI()
    {
        LoadAllAssetsOfType<SceneData>(out scenes);
        foreach(var scene in scenes) {
            SerializedObject so = new SerializedObject(scene);
            so.Update();
            
            EditorGUILayout.BeginHorizontal("Box");
            
            EditorGUILayout.LabelField(scene.sceneRef.Name);

            scene.isPersistant = GUILayout.Toggle(scene.isPersistant, "Persistant");
            so.FindProperty("isPersistant").boolValue = scene.isPersistant;
            
            GUI.enabled = false;
            scene.isOpen = GUILayout.Toggle(SceneManager.GetSceneByName(scene.sceneRef.Name).isLoaded, "isOpen ?");
            GUI.enabled = true;
            so.FindProperty("isOpen").boolValue = scene.isOpen;
            
            OpenScene(scene.sceneRef.Path);
            CloseScene(scene);
            EditorGUILayout.EndHorizontal();
            
            so.ApplyModifiedProperties();
        }
        
        // checkSceneData = GUILayout.Toggle(checkSceneData, "Check If SceneData ?");
        //
        // // if (Event.current.type == EventType.Repaint && checkSceneData) {
        // //     CheckIfSceneDataExist();
        // // }
    }
    private static void LoadAllAssetsOfType<T>(out T[] assets) where T : Object
    {
        string[] guids = AssetDatabase.FindAssets("t:"+typeof(T));
        assets = new T[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
    }

    private void CheckIfSceneDataExist()
    {
        scenesGUIDs = AssetDatabase.FindAssets("t:Scene");

        for (int i = 0; i < scenesGUIDs.Length; i++) {
            var scenePath = AssetDatabase.GUIDToAssetPath(scenesGUIDs[i]);
            var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (!System.IO.File.Exists( "Assets/Data/SceneDatas" + sceneName + ".asset")) {
                SceneData newScene = ScriptableObject.CreateInstance<SceneData>();
                newScene.sceneRef = SceneReference.FromScenePath(scenePath);
                AssetDatabase.CreateAsset(newScene, "Assets/Data/SceneDatas" + sceneName + ".asset");
            }
        }
    }

    private void OpenScene(string scenePath)
    {
        if (GUILayout.Button("Open")) {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        }
    }

    private void CloseScene(SceneData scene)
    {
        if (GUILayout.Button("Close")) {
            if (scene.isPersistant) {
                EditorSceneManager.CloseScene(SceneManager.GetSceneByName(scene.sceneRef.Name), false);
            } else {
                EditorSceneManager.CloseScene(SceneManager.GetSceneByName(scene.sceneRef.Name), true);
            }
        }
    }
}

