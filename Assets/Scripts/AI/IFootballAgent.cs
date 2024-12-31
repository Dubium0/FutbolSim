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

    public Transform FocusPointTransform { get; }
    public PlayerType PlayerType { get;}

    public Rigidbody Rigidbody { get; }

    public bool IsInitialized { get; }

    public TeamFlag TeamFlag { get;}

    public void InitAISystems(FootballTeam team, PlayerType playerType,int index);

    public void TickAISystem();


    public int TryToAcquireBall();

   


}

