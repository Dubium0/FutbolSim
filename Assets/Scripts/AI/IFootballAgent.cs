using UnityEngine;

public enum PlayerType
{
    GoalKeeper,
    Defender,
    Midfielder,
    Forward
}

public interface IFootballAgent
{
    public FootballAgentInfo AgentInfo { get; }
    public Transform Transform { get; }
    public PlayerType PlayerType { get;}

    public Rigidbody Rigidbody { get; }

    public bool IsInitialized { get; }

    public void InitAISystems(FootballTeam team,int index);

    public void TickAISystem();



}

