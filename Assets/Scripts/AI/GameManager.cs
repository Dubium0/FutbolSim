using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
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

}

