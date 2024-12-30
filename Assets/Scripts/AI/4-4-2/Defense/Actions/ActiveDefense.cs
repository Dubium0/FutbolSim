using UnityEngine;
using BT_Implementation;
using BT_Implementation.Leaf;
using Player.Controller;
using Unity.VisualScripting;

public class ActiveDefense : ActionNode
{


    public ActiveDefense(string name, Blackboard blackBoard) : base(name, blackBoard)
    {
    }

    public override BTResult Execute()
    {
        
        var ball_position = SoccerBall.Instance.transform.position;
        var ball_velocity = SoccerBall.Instance.RigidBody.linearVelocity;
      
       // var ball_owner = SoccerBall.Instance.CurrentOwnerPlayer;
       //
       // var ball_owner_position = ball_owner.transform.position;
       // var ball_owner_velocity = ball_owner.LinearVelocity;

        var agent = blackBoard.GetValue<FootballAgent>("Owner Agent");
        var info = agent.AgentInfo;

        
        var distanceToBall = Vector3.Distance(agent.transform.position, ball_position);


        



        if (distanceToBall > info.DefenseRadius)
        {
            var direction = (ball_position - agent.transform.position);
            direction.y = 0;
            var prevY = agent.Rigidbody.linearVelocity.y;
            agent.Rigidbody.linearVelocity = (direction.normalized * agent.AgentInfo.MaxSpeed) + Vector3.up * prevY;

            return BTResult.Running;
        }
        else
        {
            var prevY = agent.Rigidbody.linearVelocity.y;
            agent.Rigidbody.linearVelocity = Vector3.up * prevY;

        }

        Debug.Log("Ball is in inside Defense Region");
        return BTResult.Success;
    }
}
