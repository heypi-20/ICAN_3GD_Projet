using UnityEngine;

public class MainMenuGameState : GameState
{
    public GameObject menuGO;
    
    public override void Enter()
    {
        menuGO.SetActive(true);
    }
    
    //Used by button - single responsibility
    public void TransitionToLevel1()
    {
        fsm.selectedLevel = fsm.level1;
        fsm.ChangeState(GetComponent<LoadingLevelGameState>());
    }

    public void TransitionToLevel2()
    {
        fsm.selectedLevel = fsm.level2;
        fsm.ChangeState(GetComponent<LoadingLevelGameState>());
    }

    public override void Exit()
    {
        menuGO.SetActive(false);
    }
}