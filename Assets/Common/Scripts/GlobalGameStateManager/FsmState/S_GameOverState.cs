using UnityEngine;

public class S_GameOverState : S_IGameState
{
    private S_GameFlowController _fsm;
    public S_GameOverState(S_GameFlowController fsm) { _fsm = fsm; }

    public void OnEnter(params object[] args)
    {
        //active game over menu
        //Set timescale to 0
        //actualise game result
    }

    public void OnTick() { }

    public void OnExit()
    {
        // hide game over menu
    }
}