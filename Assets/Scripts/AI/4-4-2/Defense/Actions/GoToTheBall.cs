using UnityEngine;
using BT_Implementation;
using BT_Implementation.Leaf;
using Player.Controller;
using Unity.VisualScripting;
using Mono.Cecil.Cil;

public class GoToTheBall : ActionNode
{


    public GoToTheBall(string name, Blackboard blackBoard) : base(name, blackBoard)
    {
    }

    public override BTResult Execute()
    {

        var futureBallPosition = Football.Instance.GetDropPointAfterTSeconds(1);

        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");


        var distanceToBall = Vector3.Distance(agent.Transform.position, futureBallPosition);

        if(distanceToBall > 1)
        {
            var direction = (futureBallPosition - agent.Transform.position);
            direction.y = 0;
            var prevY = agent.Rigidbody.linearVelocity.y;
            agent.Rigidbody.linearVelocity = (direction.normalized * agent.AgentInfo.MaxSpeed) + Vector3.up * prevY;
            return BTResult.Running;
        }

        return BTResult.Success;
    }
}
