using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ModuleProfile", menuName = "", order = 0)]
public class ModuleProfile : ScriptableObject
{
    public List<string> fieldNames;
    public Dictionary<string, object> dataDictionary;
}

