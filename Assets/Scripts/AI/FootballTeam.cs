using System.Collections.Generic;
using UnityEngine;

public enum TeamFlag
{
    Red,
    Blue
}
public class FootballTeam : ScriptableObject
{

    public TeamFlag TeamFlag;
    public FootballAgent[] FootballAgents = new FootballAgent[10]; 
    public FootballAgent GoalKeeper;
    public FootballFormation Formation;


}

