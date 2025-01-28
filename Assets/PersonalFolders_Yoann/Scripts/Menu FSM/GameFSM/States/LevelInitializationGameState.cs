using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelInitializationGameState : GameState
{
    public GameObject playerPrefab;
    public override void Enter()
    {
        // InitPlayer();
        fsm.ChangeState(GetComponent<PlayingGameState>());
    }
    
    // private void InitPlayer()
    // {
    //     // GameObject spawnPoint1 = GameObject.FindGameObjectWithTag("SpawnPoint");
    //     PlayerStartPosition spawnPoint2 = GameObject.FindObjectOfType<PlayerStartPosition>();
    //     Instantiate(playerPrefab, spawnPoint2.transform.position, Quaternion.identity);
    // }
}