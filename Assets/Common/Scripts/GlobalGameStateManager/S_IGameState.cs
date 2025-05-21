

//Game state interface
public interface S_IGameState
{
    void OnEnter(params object[] args);
    void OnTick();
    void OnExit();
}