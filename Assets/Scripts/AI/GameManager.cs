using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public Transform RedGoalPosition;
    public Bounds RedGoalBounds;

    public Transform BlueGoalPosition;
    public Bounds BlueGoalBounds;


    public FootballTeam RedFootballTeam;
    public FootballTeam BlueFootballTeam;

    public static GameManager Instance;

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
                return BlueGoalBounds ;
            case TeamFlag.Red:
                return RedGoalBounds;
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

}

