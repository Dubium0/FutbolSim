using UnityEngine;
using BT_Implementation;
using BT_Implementation.Leaf;

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

     
        
        var direction = (futureBallPosition - agent.Transform.position);
        direction.y = 0;
        var prevY = agent.Rigidbody.linearVelocity.y;
        agent.Rigidbody.linearVelocity = (Vector3.ClampMagnitude( direction,1 )* agent.AgentInfo.MaxSpeed) + Vector3.up * prevY;
          
        if(agent.IsDebugMode)
        {
            Debug.Log("I'm going to the ball!");
        }
        

        return BTResult.Success;
    }
}
