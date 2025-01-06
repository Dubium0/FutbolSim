using UnityEngine;
using BT_Implementation;
using BT_Implementation.Leaf;

public class GoToTheGoalPost : ActionNode
{
    public GoToTheGoalPost(string name, Blackboard blackBoard) : base(name, blackBoard) { }

    public override BTResult Execute()
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var zPosition = agent.TeamFlag == TeamFlag.Red ? -20 : 20;
        var targetPosition = new Vector3(0, 0, zPosition);
        var direction = targetPosition - agent.Transform.position;

        direction.y = 0;
        agent.Rigidbody.linearVelocity = Vector3.ClampMagnitude(direction, agent.AgentInfo.MaxRunSpeed);

        if (agent.IsDebugMode)
            // Debug.Log($"Team: {agent.TeamFlag}, Moving to {targetPosition}, Current: {agent.Transform.position}, Direction: {direction}");

        if (direction.magnitude < 0.5f)
        {
            agent.Rigidbody.linearVelocity = Vector3.zero;
            if (agent.IsDebugMode)
                // Debug.Log($"Team: {agent.TeamFlag}, Reached {targetPosition}.");
            return BTResult.Success;
        }

        return BTResult.Running;
    }
}