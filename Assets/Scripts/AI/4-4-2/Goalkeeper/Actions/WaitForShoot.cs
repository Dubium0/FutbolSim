using BT_Implementation;
using BT_Implementation.Leaf;
using UnityEngine;

public class WaitForShoot : ActionNode
{
    public WaitForShoot(string name, Blackboard blackBoard) : base(name, blackBoard)
    {
    }

    public override BTResult Execute()
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var ball = Football.Instance;

        if (ball == null)
        {
            if (agent.IsDebugMode)
                Debug.Log("Ball not found. Cannot wait for shoot.");
            return BTResult.Failure;
        }

        Vector3 ballPosition = ball.transform.position;

        // Calculate horizontal alignment
        Vector3 targetPosition = new Vector3(ballPosition.x, agent.Transform.position.y, agent.Transform.position.z);

        // Smoothly adjust position to align with the ball
        Vector3 direction = targetPosition - agent.Transform.position;
        direction.y = 0;

        agent.Rigidbody.linearVelocity = Vector3.ClampMagnitude(direction, agent.AgentInfo.MaxRunSpeed);

        // Face the ball
        agent.Transform.LookAt(new Vector3(ballPosition.x, agent.Transform.position.y, ballPosition.z));

        if (agent.IsDebugMode)
        {
            Debug.Log($"Waiting for shoot. Aligning with ball at {ballPosition}. Current Position: {agent.Transform.position}");
        }

        // Check if goalkeeper is aligned with the ball
        if (Mathf.Abs(direction.x) < 0.1f) // Close enough on X-axis
        {
            agent.Rigidbody.linearVelocity = Vector3.zero; // Stop movement
            if (agent.IsDebugMode)
            {
                Debug.Log("Aligned with the ball. Waiting for the shoot.");
            }
            return BTResult.Running;
        }

        return BTResult.Running;
    }
}