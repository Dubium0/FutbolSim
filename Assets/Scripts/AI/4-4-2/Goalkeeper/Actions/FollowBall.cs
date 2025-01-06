using BT_Implementation;
using BT_Implementation.Leaf;
using UnityEngine;

public class FollowBall : ActionNode
{
    private const float RADIUS = 5f; // Radius within which the goalkeeper can move along the X-axis

    public FollowBall(string name, Blackboard blackBoard) : base(name, blackBoard) { }

    public override BTResult Execute()
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var ball = Football.Instance;

        if (ball == null)
        {
            if (agent.IsDebugMode)
                Debug.Log("Ball not found. Cannot follow.");
            return BTResult.Failure;
        }

        var ballX = ball.transform.position.x;
        var homePositionX = GameManager.Instance.GetGoalPositionHome(agent.TeamFlag).x;

        // Clamp the X position to keep the goalkeeper within its radius
        var targetX = Mathf.Clamp(ballX, homePositionX - RADIUS, homePositionX + RADIUS);
        var targetPosition = new Vector3(targetX, agent.Transform.position.y, agent.Transform.position.z);

        // Move towards the target position
        var direction = targetPosition - agent.Transform.position;
        agent.Rigidbody.linearVelocity = Vector3.ClampMagnitude(direction, agent.AgentInfo.MaxRunSpeed);

        if (agent.IsDebugMode)
        {
            Debug.Log($"Following Ball: Target X = {targetX}, Ball X = {ballX}, Current Position = {agent.Transform.position}");
        }

        // Stop moving if the target is reached
        if (Mathf.Abs(direction.x) < 0.1f)
        {
            agent.Rigidbody.linearVelocity = Vector3.zero;
            return BTResult.Success;
        }

        return BTResult.Running;
    }
}