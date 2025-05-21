

public class S_MainMenuState : S_IGameState
{
    private S_GameFlowController _fsm;
    public S_MainMenuState(S_GameFlowController fsm) { _fsm = fsm; }

    public void OnEnter(params object[] args)
    {
        //show mainmenu page
        // unload existant scene
    }

    public void OnTick() { }

    public void OnExit()
    {
        //hide mainmenu page
    }
}