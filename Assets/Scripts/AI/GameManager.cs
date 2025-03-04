using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct GameStartConfig
{
    public int homePlayerCount;
    public int awayPlayerCount;
    public bool isOnline;
    public ulong clientId;
}

public enum EGameState
{
    NotStarted,Running,Freeze, Finished
}
public class GameManager : NetworkBehaviour
{
    public Transform RedGoalPosition;
    public Bounds RedGoalBounds;

    public Goal BlueGoal;
    public Goal RedGoal;


    public Transform BlueGoalPosition;
    public Bounds BlueGoalBounds;


    public FootballTeam homeFootballTeam;
    public FootballTeam awayFootballTeam;

    public static GameManager Instance;

    private EGameState state_;
    public EGameState GameState {  get { return state_; } }

    private void OnValidate()
    {
        BlueGoalBounds.center = BlueGoalPosition.position;

        RedGoalBounds.center = RedGoalPosition.position;
    }
    private void Awake()
    {
        if(IsClient) Destroy(gameObject);

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

        }
        else
        {
            Instance = this;
            state_ = EGameState.NotStarted;
        }
    }
    private void Start()
    {
        //GameStartConfig config = new GameStartConfig();
        //config.awayPlayerCount = 0;
        //config.homePlayerCount = 1;
        //
        //
        //StartGame(config);
    }
    
    public void StartGame(GameStartConfig config)
    {
        if (config.isOnline)
        {
            awayFootballTeam.init(true, false, config.clientId);
            homeFootballTeam.init(true, true, NetworkManager.ServerClientId);
            state_ = EGameState.Running;
            return;
        }

        if (config.homePlayerCount > 0)
        {

            homeFootballTeam.init(true, true);
        }
        else
        {
            homeFootballTeam.init(false, true);
        }
        if (config.awayPlayerCount > 0)
        {

            awayFootballTeam.init(true, false);
        }
        else
        {
            awayFootballTeam.init(false, false);
        }
        state_ = EGameState.Running;
    }
    // probably temporary
    public void PauseGame()
    {
        state_ = EGameState.Freeze;
        Time.timeScale = 0;
    }
    public void ResumeGame()
    {
        state_ = EGameState.Running;
        Time.timeScale = 1;
    }

    public Vector3 GetGoalPositionHome(TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Blue:
                return BlueGoalPosition.position;
            case TeamFlag.Red:
                return RedGoalPosition.position;
            default:
                return BlueGoalPosition.position;

        }
    }

    public Bounds GetBoundsHome(TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Blue:
                BlueGoalBounds.center = BlueGoalPosition.position;  
                return BlueGoalBounds ;
            case TeamFlag.Red:
                RedGoalBounds.center = RedGoalPosition.position;
                return RedGoalBounds;
            default:
                return BlueGoalBounds;

        }
    }

    public Bounds GetBoundsAway(TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Blue:
                RedGoalBounds.center = RedGoalPosition.position;
                return RedGoalBounds;
              
            case TeamFlag.Red:
                BlueGoalBounds.center = BlueGoalPosition.position;
                return BlueGoalBounds;
            default:
                return BlueGoalBounds;

        }
    }
    public Vector3 GetGoalPositionAway(TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Blue:
                return RedGoalPosition.position;
                
            case TeamFlag.Red:
                return BlueGoalPosition.position;
            default:
                return BlueGoalPosition.position;

        }
    }

    public Goal GetEnemyGoalInstance( TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Blue:
                return RedGoal;

            case TeamFlag.Red:
                return BlueGoal;
            default:
                return BlueGoal;

        }

    }

    [SerializeField]
    private LayerMask blueTeamLayerMask;

    [SerializeField]
    private LayerMask redTeamLayerMask;

    public LayerMask GetLayerMaskOfEnemy(TeamFlag teamFlag)
    {
        switch(teamFlag)
        {
            case TeamFlag.Red:
                return blueTeamLayerMask;
            case TeamFlag.Blue:
                return redTeamLayerMask;
            default:
            return blueTeamLayerMask;


        }

    }

    public LayerMask GetLayerMaskOfTeamMate(TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Red:
                return redTeamLayerMask;
              
            case TeamFlag.Blue:
                return blueTeamLayerMask;
            default:
                return blueTeamLayerMask;


        }

    }

    public List<GameObject> GetPlayersInGoalArea(TeamFlag agentTeamFlag)
    {
        List<GameObject> playersInGoalArea = new List<GameObject>();
        Bounds goalBounds = GetBoundsHome(agentTeamFlag);
        Collider[] colliders = Physics.OverlapBox(goalBounds.center, goalBounds.extents, Quaternion.identity);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
                playersInGoalArea.Add(collider.gameObject);
        }

        return playersInGoalArea;
    }
}

