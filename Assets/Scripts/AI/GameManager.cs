using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Start,
    Playing,
    RedWin,
    BlueWin,
    Draw
}

public class GameManager : MonoBehaviour
{
    public GameState state = GameState.Start;
    
    public Transform RedGoalPosition;
    public Bounds RedGoalBounds;

    public Goal BlueGoal;
    public Goal RedGoal;
    
    public Transform BlueGoalPosition;
    public Bounds BlueGoalBounds;
    
    public FootballTeam RedFootballTeam;
    public FootballTeam BlueFootballTeam;

    public static GameManager Instance;

    private void OnValidate()
    {
        BlueGoalBounds.center = BlueGoalPosition.position;

        RedGoalBounds.center = RedGoalPosition.position;
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {

            Destroy(this);

        }
        else
        {
            Instance = this;
        }
    }
    
    private void Start()
    {
        state = GameState.Start;
        RedFootballTeam.enabled = false;
        BlueFootballTeam.enabled = false;
    }

    public void StartGame(bool isBothTeamsControlled, TeamFlag teamFlag = TeamFlag.Blue)
    {
        bool isRedControlled = teamFlag == TeamFlag.Red;
        RedFootballTeam.enabled = true;
        BlueFootballTeam.enabled = true;
        state = GameState.Playing;
    
        if (!isBothTeamsControlled)
        {   
            RedFootballTeam.isHumanControllable = isRedControlled;
            BlueFootballTeam.isHumanControllable = !isRedControlled;
        }
        else
        {
            RedFootballTeam.isHumanControllable = true;
            BlueFootballTeam.isHumanControllable = true;
        }
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

