
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class PlayModeSceneLoader
{
    private static SceneData[] _scenes;

    static PlayModeSceneLoader()
    {
        EditorApplication.playModeStateChanged += ChangeState;
    }

    private static void ChangeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode) {
            LoadPersistantScenes();
        }
    }

    private static void LoadPersistantScenes()
    {
        LoadAllAssetsOfType<SceneData>(out _scenes);
        foreach(var scene in _scenes) {
            if (scene.isPersistant && !scene.isOpen) {
                SceneManager.LoadSceneAsync(scene.sceneRef.Name, LoadSceneMode.Additive);
            }
        }
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
}

