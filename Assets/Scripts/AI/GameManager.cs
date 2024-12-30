using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public Transform RedGoalPosition;
    public Transform BlueGoalPosition;


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

}

