using UnityEngine;
using BT_Implementation;
using BT_Implementation.Leaf;
using System.Runtime.CompilerServices;

public class GoToTheBall : ActionNode
{


    public GoToTheBall(string name, Blackboard blackBoard) : base(name, blackBoard)
    {
    }
    
    public override BTResult Execute()
    {

        var futureBallPosition = Football.Instance.GetDropPointAfterTSeconds(2);

        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");


        var distanceToBall = Vector3.Distance(agent.Transform.position, futureBallPosition);
        if(distanceToBall <= agent.AgentInfo.MaxRunSpeed * 2)
        {
            if (agent.IsDebugMode)
            {
                Debug.Log("Can Catch the ball!");
                Debug.Log($"Distance to Ball {distanceToBall}");
            }
           
        }

       

        var finalPosition = Vector3.Lerp(Football.Instance.transform.position, futureBallPosition, distanceToBall > 5 ? 1 : 0 );
        
        
        var direction = (finalPosition - agent.Transform.position);
        direction.y = 0;
        var prevY = agent.Rigidbody.linearVelocity.y;
        agent.Rigidbody.linearVelocity = (Vector3.ClampMagnitude( direction,1 )* agent.AgentInfo.MaxRunSpeed) + Vector3.up * prevY;
          
        if(agent.IsDebugMode)
        {
            Debug.Log("I'm going to the ball!");
        }
        

        return BTResult.Success;
    }
}
