using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public enum TeamFlag
{
    Red,
    Blue
}
public class FootballTeam : MonoBehaviour
{

    public TeamFlag TeamFlag;
    public FootballAgent[] FootballAgents = new FootballAgent[10];
    public FootballAgent GoalKeeper;

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
        var midfieldCount = currentFormation.DefensePosition.Length;
        var attackCount = currentFormation.DefensePosition.Length;

        int index = 0;
        for (var i = 0; i < defCount; i++) {
            GameObject agent = Instantiate(DefenseAgentPrefab, currentFormation.DefensePosition[i].position, currentFormation.DefensePosition[i].rotation);
            var agentComponent = agent.GetComponent<FootballAgent>();
            agentComponent.PlayerType = PlayerType.Defender;
            FootballAgents[index] = agentComponent;
            index++;
        }

        for (var i = 0; i < midfieldCount; i++)
        {
            GameObject agent = Instantiate(MidfieldAgentPrefab, currentFormation.MidfieldPosition[i].position, currentFormation.MidfieldPosition[i].rotation);
            var agentComponent = agent.GetComponent<FootballAgent>();
            agentComponent.PlayerType = PlayerType.Midfielder;
            FootballAgents[index] = agentComponent;
            index++;
        }

        for (var i = 0; i < attackCount; i++)
        {
            GameObject agent = Instantiate(ForwardAgentPrefab, currentFormation.ForwardPosition[i].position, currentFormation.ForwardPosition[i].rotation);
            var agentComponent = agent.GetComponent<FootballAgent>();
            agentComponent.PlayerType = PlayerType.Forward;
            FootballAgents[index] = agentComponent;
            index++;
        }
    }
}

