using System;
using System.Collections.Generic;
using UnityEngine;

public class _SpawnerManager : MonoBehaviour
{
    // [SerializeField] List<_Spawner> _spawnerConfigs = new List<_Spawner>();
    //
    // private void Start()
    // {
    //     foreach (_Spawner spawner in _spawnerConfigs)
    //     {
    //         spawner.OnSpawn += Spawn;
    //     }
    // }
    //
    // private void OnDisable()
    // {
    //     foreach (_Spawner spawner in _spawnerConfigs)
    //     {
    //         spawner.OnSpawn -= Spawn;
    //     }
    // }
    //
    // private void Update()
    // {
    //     foreach (_Spawner spawner in _spawnerConfigs)
    //     {
    //         spawner.Tick(this);
    //     }
    // }
    //
    // private void Spawn(_Spawner config, int quantity)
    // {
    //     //Pooling
    //     //Récuperer des zones
    // }

    public void Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        //todo pooling
    }
}