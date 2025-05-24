using UnityEngine;
using BT_Implementation;
using BT_Implementation.Leaf;

public class GoToTheGoalPost : ActionNode
{
    public GoToTheGoalPost(string name, Blackboard blackBoard) : base(name, blackBoard) { }

    public override BTResult Execute()
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var ballPosition = Football.Instance.transform.position;

        var zPosition = agent.TeamFlag == TeamFlag.Home ? -21 : 21;
        var adjustedX = Mathf.Clamp(ballPosition.x, -5f, 5f);
        var targetPosition = new Vector3(adjustedX, 0, zPosition);

        var direction = targetPosition - agent.Transform.position;
        direction.y = 0;

        agent.Rigidbody.linearVelocity = Vector3.ClampMagnitude(direction, agent.AgentInfo.MaxRunSpeed);

        if (direction.magnitude < 0.5f)
        {
            agent.Rigidbody.linearVelocity = Vector3.zero;
            return BTResult.Success;
        }

        return BTResult.Running;
    }
}