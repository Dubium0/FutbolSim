using UnityEngine;
using BT_Implementation;

using BT_Implementation.Leaf;
using UnityEngine.Rendering.Universal.Internal;


public class TryToGetInReceivingPosition : ActionNode
{


    public TryToGetInReceivingPosition(string name, Blackboard blackBoard) : base(name, blackBoard)
    {
    }

    public override BTResult Execute()
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var footballTeam = blackBoard.GetValue<FootballTeam>("Owner Team");


        var teamIndex = blackBoard.GetValue<int>("Team Index");
        var homePos = footballTeam.GetHomePosition(teamIndex);

        
        var directionToBall = (Football.Instance.transform.position - homePos);

        var leftPlaneDir = new Vector3(-directionToBall.normalized.z, directionToBall.normalized.y, directionToBall.normalized.x);
        var rightPlaneDir = new Vector3(directionToBall.normalized.z, directionToBall.normalized.y, -directionToBall.normalized.x);

        var rayOrigin = homePos;

        var layerMaskToCheck = GameManager.Instance.GetLayerMaskOfEnemy(agent.TeamFlag);

        var currentPosOpen = !Physics.Raycast(rayOrigin,directionToBall.normalized, directionToBall.magnitude, layerMaskToCheck);


        rayOrigin = homePos + leftPlaneDir;
        directionToBall = (Football.Instance.transform.position - rayOrigin);
        var leftCloseOpen = !Physics.Raycast(rayOrigin, directionToBall.normalized, directionToBall.magnitude, layerMaskToCheck);

        rayOrigin = homePos + leftPlaneDir * 1.5f;
        directionToBall = (Football.Instance.transform.position - rayOrigin);
        var leftWideOpen = !Physics.Raycast(rayOrigin, directionToBall.normalized, directionToBall.magnitude, layerMaskToCheck);

        rayOrigin = homePos + rightPlaneDir;
        directionToBall = (Football.Instance.transform.position - rayOrigin);
        var rightCloseOpen = !Physics.Raycast(rayOrigin, directionToBall.normalized, directionToBall.magnitude, layerMaskToCheck);

        rayOrigin = homePos + leftPlaneDir * 1.5f;
        directionToBall = (Football.Instance.transform.position - rayOrigin);
        var rightWideOpen = !Physics.Raycast(rayOrigin, directionToBall.normalized, directionToBall.magnitude, layerMaskToCheck);


         
         
         var finalDestination = homePos;
         if (currentPosOpen)
         {
             //do nothing
         }
         else if (leftCloseOpen)
         {
             finalDestination += leftPlaneDir;
         }
         else if (rightCloseOpen)
         {
             finalDestination += rightPlaneDir;
         }
         else if (leftWideOpen)
         {
             finalDestination += leftPlaneDir * 2;
         }
         else if (rightWideOpen)
         {
             finalDestination += rightPlaneDir * 2;
         }
        
        var direction = (finalDestination - agent.Transform.position);
        direction.y = 0;
        var prevY = agent.Rigidbody.linearVelocity.y;
        agent.Rigidbody.linearVelocity = (Vector3.ClampMagnitude(direction, 1) * agent.AgentInfo.MaxRunSpeed) + Vector3.up * prevY;
        
        if (agent.IsDebugMode)
        {
            Debug.Log("I'm Trying to be in receiving Position!");
            Debug.Log($"Current Open Positions home {currentPosOpen} , leftClose{leftCloseOpen} , leftWide {leftWideOpen}, rightClose {rightCloseOpen}, rightWide {rightWideOpen}");
        }



        return BTResult.Success;

    }
}
