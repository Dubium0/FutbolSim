using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;


public enum TeamFlag
{
    Red,
    Blue
}

public enum FormationPhase
{
    Defense,
    Start,
    Default,
    Attack
}
public class FootballTeam : MonoBehaviour
{

    public TeamFlag TeamFlag;
    public IFootballAgent[] FootballAgents = new IFootballAgent[10];
    public IFootballAgent GoalKeeper;

    private Transform[] homePositions_ = new Transform[11];

    public FootballFormation DefenseFormation;
    public FootballFormation StartFormation;
    public FootballFormation DefaultFormation;
    public FootballFormation AttackFormation;



    public GameObject DefenseAgentPrefab;
    public GameObject MidfieldAgentPrefab;
    public GameObject ForwardAgentPrefab;
    public GameObject GoalKeeperAgentPrefab;

    private FootballFormation currentFormation;

    private void Awake()
    {
        currentFormation = StartFormation;
        CreateAgents();
    }

    private void CreateAgents()
    {
        var defCount = currentFormation.DefensePosition.Length;
        var midfieldCount = currentFormation.MidfieldPosition.Length;
        var attackCount = currentFormation.ForwardPosition.Length;

        int index = 0;
        for (var i = 0; i < defCount; i++)
        {
            GameObject agent = Instantiate(DefenseAgentPrefab, currentFormation.DefensePosition[i].position, currentFormation.DefensePosition[i].rotation);
            var agentComponent = agent.GetComponent<IFootballAgent>();
            agentComponent.InitAISystems(this,PlayerType.Defender, index);
            FootballAgents[index] = agentComponent;
            homePositions_[index] = currentFormation.DefensePosition[i];
            index++;
        }

        for (var i = 0; i < midfieldCount; i++)
        {
            GameObject agent = Instantiate(MidfieldAgentPrefab, currentFormation.MidfieldPosition[i].position, currentFormation.MidfieldPosition[i].rotation);
            var agentComponent = agent.GetComponent<IFootballAgent>();
            agentComponent.InitAISystems(this, PlayerType.Midfielder, index);
            FootballAgents[index] = agentComponent;
            homePositions_[index] = currentFormation.MidfieldPosition[i];
            index++;
        }

        for (var i = 0; i < attackCount; i++)
        {
            GameObject agent = Instantiate(ForwardAgentPrefab, currentFormation.ForwardPosition[i].position, currentFormation.ForwardPosition[i].rotation);
            var agentComponent = agent.GetComponent<IFootballAgent>();
            agentComponent.InitAISystems(this, PlayerType.Forward, index);
            FootballAgents[index] = agentComponent;
            homePositions_[index] = currentFormation.ForwardPosition[i];
            index++;
        }
    }

    private void UpdateHomePositions()
    {
        var defCount = currentFormation.DefensePosition.Length;
        var midfieldCount = currentFormation.MidfieldPosition.Length;
        var attackCount = currentFormation.ForwardPosition.Length;

        int index = 0;
        for (var i = 0; i < defCount; i++)
        {
         
            homePositions_[index] = currentFormation.DefensePosition[i];
            index++;
        }

        for (var i = 0; i < midfieldCount; i++)
        {
           
            homePositions_[index] = currentFormation.MidfieldPosition[i];
            index++;
        }

        for (var i = 0; i < attackCount; i++)
        {
            homePositions_[index] = currentFormation.ForwardPosition[i];
            index++;
        }
    }

    public Vector3 GetHomePosition(int index)
    {
        if (index < 0 || index >= FootballAgents.Length)
        {
            Debug.LogError("Index out of range");
            return Vector3.zero;
        }
        return homePositions_[index].position;

    }

    
}
