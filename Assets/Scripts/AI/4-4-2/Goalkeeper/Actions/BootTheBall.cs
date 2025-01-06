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
        
        // get if agent red or blue. based on that set direction + or -
        var direction = agent.TeamFlag == TeamFlag.Red ? Vector3.right : Vector3.left;
        var power = Random.Range(1, 3);
        
        ball.HitBall(direction, power);
        agent.DisableAIForATime(0.5f);
        
        return BTResult.Success;
    }
}