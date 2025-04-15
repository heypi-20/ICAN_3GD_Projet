using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(S_BasicSprint_Module))]
public class S_BasicSprint_ModuleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        S_BasicSprint_Module module = (S_BasicSprint_Module)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Already Damaged Enemies", EditorStyles.boldLabel);

        // Display the HashSet content
        if(module.alreadyDamagedEnemies != null)
        {
            // Convert HashSet to List for display purposes
            List<EnemyBase> enemyList = new List<EnemyBase>(module.alreadyDamagedEnemies);
            foreach(EnemyBase enemy in enemyList)
            {
                EditorGUILayout.ObjectField(enemy, typeof(EnemyBase), true);
            }
        }
        else
        {
            EditorGUILayout.LabelField("No enemies have been damaged.");
        }
    }
}