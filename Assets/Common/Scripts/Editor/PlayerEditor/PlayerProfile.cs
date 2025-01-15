using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProfile", menuName = "PlayerProfile", order = 0)]
public class PlayerProfile : ScriptableObject
{
    public List<bool> isEnable;
    public Editor editor;

}
