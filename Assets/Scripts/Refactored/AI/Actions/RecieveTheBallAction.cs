using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Unity.Mathematics;



[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RecieveTheBall", story: "[Player] goalkeeper recieves the ball", category: "FootballPlayer/Action", id: "1c300376fafddbabe4f6d5725bfe2d0a")]
public partial class RecieveTheBallAction : Action
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var nextBallPosition = FootballSim.Football.Football.Instance.GetDropPointAfterTSeconds(0.5f);//time 
        var currentBallPosition = FootballSim.Football.Football.Instance.transform.position;
        var ourGoalBounds = Player.Value.TeamFlag == FootballSim.FootballTeam.TeamFlag.Away ? FootballSim.MatchManager.Instance.PitchData.AwayGoalBounds 
        : FootballSim.MatchManager.Instance.PitchData.HomeGoalBounds;

        var distance = nextBallPosition - currentBallPosition;
        Ray ray = new(currentBallPosition, distance.normalized);
        //Debug.DrawRay(ray.origin, ray.direction*10,Color.red);
        //Debug.Log("Recieve ball update!");
        if (ourGoalBounds.IntersectRay(ray, out float distanceFromCurrentBallPosition))
        {   
            var layerMaskToCheck = 1 <<(Player.Value.TeamFlag == FootballSim.FootballTeam.TeamFlag.Home ?
            FootballPlayer.HomeLayerMask : FootballPlayer.AwayLayerMask);
            if (!Physics.Raycast(ray.origin, ray.direction, distanceFromCurrentBallPosition, layerMaskToCheck))
            {

                Debug.Log("Ball Is gonna be goal next tick!");
                var goalHitPoint = currentBallPosition + ray.direction * distanceFromCurrentBallPosition;
                var currentPlayerPos = Player.Value.transform.position;

                var distanceToHitPoint = goalHitPoint - currentPlayerPos;
                //ignore X one  only the z and y one
                distanceToHitPoint.x = 0;
                var angleInRadians = math.acos(Vector3.Dot(distanceToHitPoint.normalized, Player.Value.transform.up.normalized));

                float angleInDegrees = angleInRadians * Mathf.Rad2Deg;
                Player.Value.Transform.Rotate(0, 0, angleInDegrees);
                Player.Value.Rigidbody.AddForce(distanceToHitPoint.normalized * 1000, ForceMode.Impulse);
                
                
            }
            
        }


        return Status.Success;
    }
    
    protected override void OnEnd()
    {
    }
}

