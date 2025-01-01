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

        
        var directionToBall = -(homePos - Football.Instance.transform.position);

        var leftPlaneDir = new Vector3(-directionToBall.normalized.z, directionToBall.normalized.y, directionToBall.normalized.x);
        var rightPlaneDir = new Vector3(directionToBall.normalized.z, directionToBall.normalized.y, -directionToBall.normalized.x);

        var currentPosOpen = !Physics.Raycast(homePos, directionToBall.normalized, directionToBall.magnitude, 3);
        directionToBall = -(homePos + leftPlaneDir - Football.Instance.transform.position);
        var leftCloseOpen = !Physics.Raycast(homePos + leftPlaneDir, directionToBall.normalized, directionToBall.magnitude, 3);
        directionToBall = -(homePos + leftPlaneDir * 2 - Football.Instance.transform.position);
        var leftWideOpen = !Physics.Raycast(homePos + leftPlaneDir *2, directionToBall.normalized, directionToBall.magnitude, 3 );
        directionToBall = -(homePos + rightPlaneDir - Football.Instance.transform.position);
        var rightCloseOpen = !Physics.Raycast(homePos + rightPlaneDir, directionToBall.normalized, directionToBall.magnitude, 3 );
        directionToBall = -(homePos + rightPlaneDir*2 - Football.Instance.transform.position);
        var rightWideOpen = !Physics.Raycast(homePos + rightPlaneDir * 2, directionToBall.normalized, directionToBall.magnitude, 3 );

        if (agent.IsDebugMode)
        {
            directionToBall = -(homePos - Football.Instance.transform.position);
            Debug.DrawRay(homePos, directionToBall );
            directionToBall = -(homePos + leftPlaneDir - Football.Instance.transform.position);
            Debug.DrawRay(homePos + leftPlaneDir, directionToBall );
            directionToBall = -(homePos + leftPlaneDir * 2 - Football.Instance.transform.position);
            Debug.DrawRay(homePos + leftPlaneDir * 2, directionToBall);
            directionToBall = -(homePos + rightPlaneDir - Football.Instance.transform.position);
            Debug.DrawRay(homePos + rightPlaneDir, directionToBall);
            directionToBall = -(homePos + rightPlaneDir * 2 - Football.Instance.transform.position);
            Debug.DrawRay(homePos + rightPlaneDir * 2, directionToBall);

        }


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
