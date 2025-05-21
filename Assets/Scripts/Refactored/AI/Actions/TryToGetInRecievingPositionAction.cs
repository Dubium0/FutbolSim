using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Linq;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TryToGetInRecievingPosition", story: "[Player] tries to be in a nice spot to receieve ball.", category: "FootballPlayer/Action", id: "77cbad19dd3d331ddd75b2b62a499f26")]
public partial class TryToGetInRecievingPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {

        var footballTeam = Player.Value.OwnerTeam;
        var ballPosition = FootballSim.Football.Football.Instance.transform.position;
        var distanceToBallFromHomePosition = ballPosition
                                - Player.Value.CurrentHomePosition.position;
        var directionToBall = distanceToBallFromHomePosition.normalized;
        var leftPlaneDir = new Vector3(-directionToBall.z, directionToBall.y, directionToBall.x);
        var rightPlaneDir = new Vector3(directionToBall.z, directionToBall.y, -directionToBall.x);

        var rayOrigin = Player.Value.CurrentHomePosition.position;

        var layerMaskToCheck = Player.Value.TeamFlag == FootballSim.FootballTeam.TeamFlag.Home ?
        FootballPlayer.AwayLayerMask : FootballPlayer.HomeLayerMask;
        Debug.Log("LayerMask to check" + layerMaskToCheck);

        //current ------------
        var hitResults = Physics.RaycastAll(rayOrigin, directionToBall, distanceToBallFromHomePosition.magnitude, layerMaskToCheck);
       
       
        var currentPosOpen = !Physics.Raycast(rayOrigin, directionToBall, distanceToBallFromHomePosition.magnitude, layerMaskToCheck);
        Debug.DrawRay(rayOrigin, distanceToBallFromHomePosition, Color.white);
        //--------------------
        //left ---------------
        rayOrigin = Player.Value.CurrentHomePosition.position + leftPlaneDir;
        distanceToBallFromHomePosition = ballPosition - rayOrigin;
        directionToBall = distanceToBallFromHomePosition.normalized;

        hitResults = Physics.RaycastAll(rayOrigin, directionToBall, distanceToBallFromHomePosition.magnitude, layerMaskToCheck);
        var leftCloseOpen = hitResults != null ?
        hitResults.Count(hitInfo => { return hitInfo.collider.GetComponent<FootballPlayer>().TeamFlag != Player.Value.TeamFlag; }) == 0 : true;
        //--------------------
        Debug.DrawRay(rayOrigin, distanceToBallFromHomePosition, Color.black);
        rayOrigin = Player.Value.CurrentHomePosition.position + leftPlaneDir * 1.5f;
        distanceToBallFromHomePosition = ballPosition - rayOrigin;
        directionToBall = distanceToBallFromHomePosition.normalized;

        hitResults = Physics.RaycastAll(rayOrigin, directionToBall, distanceToBallFromHomePosition.magnitude, layerMaskToCheck);
        var leftWideOpen = hitResults != null ?
        hitResults.Count(hitInfo => { return hitInfo.collider.GetComponent<FootballPlayer>().TeamFlag != Player.Value.TeamFlag; }) == 0 : true;
            Debug.DrawRay(rayOrigin, distanceToBallFromHomePosition, Color.red);
        rayOrigin = Player.Value.CurrentHomePosition.position + rightPlaneDir;
        distanceToBallFromHomePosition = ballPosition - rayOrigin;
        directionToBall = distanceToBallFromHomePosition.normalized;

        hitResults = Physics.RaycastAll(rayOrigin, directionToBall, distanceToBallFromHomePosition.magnitude, layerMaskToCheck);
        var rightCloseOpen = hitResults != null ?
        hitResults.Count(hitInfo => { return hitInfo.collider.GetComponent<FootballPlayer>().TeamFlag != Player.Value.TeamFlag; }) == 0 : true;
        Debug.DrawRay(rayOrigin, distanceToBallFromHomePosition, Color.blue);
        rayOrigin = Player.Value.CurrentHomePosition.position + rightPlaneDir * 1.5f;
        distanceToBallFromHomePosition = ballPosition - rayOrigin;
        directionToBall = distanceToBallFromHomePosition.normalized;

        hitResults = Physics.RaycastAll(rayOrigin, directionToBall, distanceToBallFromHomePosition.magnitude, layerMaskToCheck);
        var rightWideOpen = hitResults != null ?
        hitResults.Count(hitInfo => { return hitInfo.collider.GetComponent<FootballPlayer>().TeamFlag != Player.Value.TeamFlag; }) == 0 : true;
            Debug.DrawRay(rayOrigin, distanceToBallFromHomePosition, Color.yellow);



        var finalDestination = Player.Value.CurrentHomePosition.position;

        if (currentPosOpen)
        {
            Debug.Log("CurrentPos is open");
        }
        else if (leftCloseOpen)
        {
            Debug.Log("leftCloseOpen is open");
            finalDestination += leftPlaneDir;
        }
        else if (rightCloseOpen)
        {
            Debug.Log("rightCloseOpen is open");
            finalDestination += rightPlaneDir;
        }
        else if (leftWideOpen)
        {
            Debug.Log("leftWideOpen is open");
            finalDestination += leftPlaneDir * 2;
        }
        else if (rightWideOpen)
        {
            Debug.Log("rightWideOpen is open");
            finalDestination += rightPlaneDir * 2;
        }

        finalDestination.y = Player.Value.transform.position.y;
        var distance = finalDestination - Player.Value.transform.position;

        var direction = distance.normalized;


        var distanceSwitch = distance.sqrMagnitude > 0.1f ? 1 : 0;
        Player.Value.Rigidbody.AddForce(direction * Player.Value.Data.RunningAcceleration * distanceSwitch, ForceMode.Acceleration);

        if (direction.magnitude > 0)
        {
            Player.Value.Transform.forward = MathExtra.MoveTowards(Player.Value.Transform.forward, direction, 1 / Player.Value.Data.RotationTime);
        }
        else
        {
            Player.Value.Transform.forward = MathExtra.MoveTowards(Player.Value.Transform.forward, (ballPosition - Player.Value.transform.position).normalized, 1 / Player.Value.Data.RotationTime);
        }
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
    
    
}

