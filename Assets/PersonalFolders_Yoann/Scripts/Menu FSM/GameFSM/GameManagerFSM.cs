using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManagerFSM : MonoBehaviour
{
    
    private static GameManagerFSM instance;

    public static GameManagerFSM Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManagerFSM>();
            }

            return instance;
        }
    }

    public void DoStuff(){}
    
    //Dans un vrai système, cette variable serait chargée depuis une sauvegarde, ou choisie depuis un menu de
    //sélection de niveau : 
    public SceneReference level1;
    public SceneReference level2;
    public SceneReference selectedLevel;
    
    private GameState currentState;
    
    private void Start()
    {
        GameState[] states = GetComponents<GameState>();
        foreach (GameState state in states)
        {
            state.Initialize(this);
        }
        
        //Make persistent (EZ version)
        // DontDestroyOnLoad(gameObject);
       
        currentState = GetComponent<MainMenuGameState>();
        currentState.Enter();
    }

    private void Update()
    {
        currentState?.Tick();
    }

    public void ChangeState(GameState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
}