
using UnityEngine;

public class _GameState : MonoBehaviour
{
    public int PlayerLevel = 1;
    // public float GlobalSpawnRateModifier = 1f;
    
    private static _GameState _instance;
    public static _GameState Instance => _instance;

    private void Awake()
    {
        _instance = this;
    }
}
