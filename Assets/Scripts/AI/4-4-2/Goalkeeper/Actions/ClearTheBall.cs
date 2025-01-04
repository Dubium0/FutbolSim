using BT_Implementation;
using BT_Implementation.Leaf;

public class ClearTheBall : ActionNode
{
    public ClearTheBall(string name, Blackboard blackBoard) : base(name, blackBoard)
    {
    }

    public override BTResult Execute()
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var ballPosition = Football.Instance.transform.position;
        var awayGoalPosition = GameManager.Instance.GetGoalPositionAway(agent.TeamFlag);

        var direction = (awayGoalPosition - ballPosition).normalized;
        Football.Instance.HitBall(direction, agent.AgentInfo.MaxRunSpeed * 2f);

        agent.DisableAIForATime(0.5f);

        return BTResult.Success;
    }
}