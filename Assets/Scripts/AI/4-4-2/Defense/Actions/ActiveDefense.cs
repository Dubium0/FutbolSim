using UnityEngine;
using BT_Implementation;
using BT_Implementation.Leaf;
using Player.Controller;

public class ActiveDefense : ActionNode
{


    public ActiveDefense(string name, Blackboard blackBoard) : base(name, blackBoard)
    {
    }

    public override BTResult Execute()
    {
        
        var ball_position = SoccerBall.Instance.transform.position;
        var ball_velocity = SoccerBall.Instance.RigidBody.linearVelocity;
        var ball_owner = SoccerBall.Instance.CurrentOwnerPlayer;
        var ball_owner_position = ball_owner.transform.position;
        var ball_owner_velocity = ball_owner.LinearVelocity;

        var agent = blackBoard.GetValue<FootballAgent>("Owner Agent");
        var info = agent.AgentInfo;

        

        if (Vector3.Distance(agent.transform.position, ball_position) < info.DefenseRadius)
        {
            agent.Rigidbody.linearVelocity = (ball_position - agent.transform.position).normalized * agent.AgentInfo.MaxSpeed;
        }
        else
        {

        }
        


        return BTResult.Success;
    }
}
