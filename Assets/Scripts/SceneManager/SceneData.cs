using Eflatun.SceneReference;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneData", menuName = "SceneData", order = 0)]
public class SceneData : ScriptableObject
{
    public SceneReference sceneRef;
    public bool isPersistant;
    public bool isOpen;
}

