using BT_Implementation;
using BT_Implementation.Leaf;
using UnityEngine;

using UnityEngine;

public class Idle : ActionNode
{
    public Idle(string name, Blackboard blackBoard) : base(name, blackBoard) { }

    public override BTResult Execute()
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        agent.Rigidbody.linearVelocity = Vector3.zero;

        if (agent.IsDebugMode) Debug.Log("Goalkeeper is idling.");
        return BTResult.Success;
    }
}

