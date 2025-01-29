using UnityEngine;

[CreateAssetMenu(fileName = "Saves", menuName = "Create New Tool Saves", order = 0)]
public class EnhancedLDSave : ScriptableObject
{
    public float[] increment;
    public bool canBeChanged = false;

}