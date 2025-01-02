

using BT_Implementation;
using BT_Implementation.Leaf;
using UnityEngine;

public class SendBallToEnemySideFromTheAir : ActionNode
{
    public SendBallToEnemySideFromTheAir(string name, Blackboard blackBoard) : base(name, blackBoard)
    {
    }

    public override BTResult Execute()
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var directionToSend = GameManager.Instance.GetGoalPositionAway(agent.TeamFlag);

        var zDirection = (directionToSend - agent.Transform.position).z;

        zDirection = Mathf.Clamp(zDirection, -1.0f, 1.0f);

        var xDirection = Random.Range(-1.0f, 1.0f);

        var finalDir = new Vector3(xDirection, Mathf.Sin(45 * Mathf.Deg2Rad), zDirection);

        Football.Instance.HitBall(finalDir, 15);

        return BTResult.Success;

    }
}