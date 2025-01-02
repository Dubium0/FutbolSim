using BT_Implementation;
using BT_Implementation.Leaf;
using System.Collections.Generic;
using UnityEngine;

public class ShootTheBall : ActionNode
{
    public ShootTheBall(string name, Blackboard blackBoard) : base(name, blackBoard)
    {
    }

    public override BTResult Execute()
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var positions = blackBoard.GetValue<List<Transform>>("Shootable Positions");

        var randomPosition =  Random.Range( 0, positions.Count);

        var direction = positions[randomPosition].position - agent.Transform.position;


        Football.Instance.HitBall(direction.normalized, agent.AgentInfo.MaximumShootPower);

        agent.DisableAIForATime(1);

        return BTResult.Success;
    }
}