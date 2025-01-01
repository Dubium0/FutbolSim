using UnityEngine;
using BT_Implementation;

using BT_Implementation.Leaf;


public class GoToTheHomePosition : ActionNode
{


    public GoToTheHomePosition(string name, Blackboard blackBoard) : base(name, blackBoard)
    {
    }

    public override BTResult Execute()
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var footballTeam = blackBoard.GetValue<FootballTeam>("Owner Team");
       

        var teamIndex = blackBoard.GetValue<int>("Team Index");
        var homePos = footballTeam.GetHomePosition(teamIndex);

        var direction = (homePos - agent.Transform.position);
        direction.y = 0;
        var prevY = agent.Rigidbody.linearVelocity.y;
        agent.Rigidbody.linearVelocity = (Vector3.ClampMagnitude(direction, 1) * agent.AgentInfo.MaxRunSpeed) + Vector3.up * prevY;

        if (agent.IsDebugMode)
        {
            Debug.Log("I'm going to Home!");
        }



        return BTResult.Success;

    }
}
