using BT_Implementation;
using BT_Implementation.Leaf;
using UnityEngine;

public class BootTheBall : ActionNode
{
    public BootTheBall(string name, Blackboard blackBoard) : base(name, blackBoard) { }

    public override BTResult Execute()
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var ball = Football.Instance;

        if (ball == null || agent != ball.CurrentOwnerPlayer)
        {
            if (agent.IsDebugMode) Debug.Log("Cannot boot the ball. Either no ball or no possession.");
            return BTResult.Failure;
        }

        Vector3 bootDirection = (GameManager.Instance.GetGoalPositionAway(agent.TeamFlag) - agent.Transform.position).normalized;
        ball.HitBall(bootDirection, agent.AgentInfo.MaximumShootPower);

        if (agent.IsDebugMode) Debug.Log("Ball booted away.");
        return BTResult.Success;
    }
}