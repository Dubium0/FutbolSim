using UnityEngine;
using BT_Implementation;
using BT_Implementation.Leaf;

public class SaveTheBall : ActionNode
{
    public SaveTheBall(string name, Blackboard blackBoard) : base(name, blackBoard)
    {
    }

    public override BTResult Execute()
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var ballPosition = Football.Instance.transform.position;
        var direction = ballPosition - agent.Transform.position;

        direction.y = 0; 
        var saveSpeed = Mathf.Clamp(agent.AgentInfo.MaxRunSpeed, 0, agent.AgentInfo.MaxRunSpeed);

        agent.Rigidbody.linearVelocity = direction.normalized * saveSpeed;

        if (Vector3.Distance(agent.Transform.position, ballPosition) < 1.0f) 
        {
            Football.Instance.HitBall((GameManager.Instance.GetGoalPositionAway(agent.TeamFlag) - ballPosition).normalized, agent.AgentInfo.MaxRunSpeed * 0.5f);
            agent.DisableAIForATime(0.5f);
            return BTResult.Success;
        }

        return BTResult.Running;
    }
}