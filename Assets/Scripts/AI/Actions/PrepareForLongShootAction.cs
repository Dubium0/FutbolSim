using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Unity.Mathematics;


[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PrepareForLongShoot", story: "[Player] defends agains long shoots", category: "FootballPlayer/Action", id: "93e52fe602e57720c753c6fa48e20beb")]
public partial class PrepareForLongShootAction : Action
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;
    
    private float m_DefenseOffset = 5;


    protected override Status OnStart()
    {
    
        return Status.Running;
    }


    protected override Status OnUpdate()
    {
       
        var ownerEnemyPlayer = FootballSim.Football.Football.Instance.CurrentOwnerPlayer;

        //var ballSpeedVector = FootballSim.Football.Football.Instance.transform.position;

        var ourGoalBounds = Player.Value.TeamFlag == FootballSim.FootballTeam.TeamFlag.Away ? FootballSim.MatchManager.Instance.PitchData.AwayGoalBounds 
        : FootballSim.MatchManager.Instance.PitchData.HomeGoalBounds;
        var ourGoalTransform = Player.Value.TeamFlag == FootballSim.FootballTeam.TeamFlag.Away ? FootballSim.MatchManager.Instance.PitchData.AwayGoalTransform 
        : FootballSim.MatchManager.Instance.PitchData.HomeGoalTransform;
        //defend in an arc
        var targetDefensePos = ourGoalBounds.ClosestPoint(ownerEnemyPlayer.transform.position) ;
        var distanceToBall =  FootballSim.Football.Football.Instance.transform.position - Player.Value.transform.position;
        distanceToBall.y = 0;
        var distanceFromGoalCenterToEnemy = ownerEnemyPlayer.transform.position - ourGoalTransform.transform.position;
        distanceFromGoalCenterToEnemy.y = 0;
        

        var cosValueBetweenTwo = Vector3.Dot(distanceFromGoalCenterToEnemy.normalized, -ourGoalTransform.right.normalized);
       
        targetDefensePos += distanceToBall.normalized * math.lerp(1  ,m_DefenseOffset, MathF.Abs(cosValueBetweenTwo));
        targetDefensePos.y = Player.Value.transform.position.y;
        var distance = targetDefensePos - Player.Value.transform.position;

        var direction = distance.normalized;

        var distanceSwitch = distance.sqrMagnitude > 0.1f ? 1 : 0;
        Player.Value.Rigidbody.AddForce(direction * Player.Value.Data.RunningAcceleration * distanceSwitch, ForceMode.Acceleration);


        if (distanceToBall.sqrMagnitude > 0.0f)
        {
            Debug.DrawRay(Player.Value.transform.position, distanceToBall);
            Player.Value.Transform.forward = distanceToBall.normalized;
        }

        return Status.Success;
    }

    

    protected override void OnEnd()
    {

      
    }
}

