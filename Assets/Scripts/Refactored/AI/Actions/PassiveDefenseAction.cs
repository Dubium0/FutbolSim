using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PassiveDefense", story: "[Player] passively defenses", category: "FootballPlayer/Action", id: "c8df348da43391d6ad8a67535a446207")]
public partial class PassiveDefenseAction : Action
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var currentEnemyOwner = FootballSim.Football.Football.Instance.CurrentOwnerPlayer;

        var enemyCurrentLocation = currentEnemyOwner.Transform.position;
   
        var playerSTeam = Player.Value.TeamFlag;
        Bounds allyGoalBounds;
        switch (playerSTeam)
        {
            case FootballSim.FootballTeam.TeamFlag.Home:
                allyGoalBounds = FootballSim.MatchManager.Instance.PitchData.HomeGoalBounds;
                break;
            case FootballSim.FootballTeam.TeamFlag.Away:
                allyGoalBounds = FootballSim.MatchManager.Instance.PitchData.AwayGoalBounds;
                break;
            default:
                Debug.LogError("This should not happen");
                return Status.Failure;

        }
        var enemyLocationAfterTSeconds = enemyCurrentLocation + currentEnemyOwner.Rigidbody.linearVelocity * 1;
        
        var defenseRadius = Player.Value.Data.PassiveDefenseDistance;
       
        var offsetFromPlayer = (allyGoalBounds.center - enemyLocationAfterTSeconds).normalized * defenseRadius;
     
        var direction = enemyLocationAfterTSeconds + offsetFromPlayer - Player.Value.transform.position;
        direction.y = 0;
       
        var enemySpeed =  currentEnemyOwner.Rigidbody.linearVelocity.magnitude;

        var followSpeed = Mathf.Clamp(enemySpeed,  Player.Value.Data.MaxWalkSpeed, Player.Value.Data.MaxRunSpeed);


        Player.Value.Rigidbody.linearVelocity = Vector3.ClampMagnitude(direction, 1) * followSpeed;
    


        if (allyGoalBounds.Contains(offsetFromPlayer))
        {
            Debug.Log("Damn enemy already in the goal");
            return Status.Failure;

        }
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

