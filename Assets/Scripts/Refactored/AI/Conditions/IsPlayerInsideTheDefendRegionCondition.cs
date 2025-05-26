using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsPlayerInsideTheDefendRegion", story: "Is [Player] inside the defend region with [MinimumDistanceToGoal] ?", category: "FootballPlayer/Conditions", id: "db87eeca45204f7673dc49bc751d0fa3")]
public partial class IsPlayerInsideTheDefendRegionCondition : Condition
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;
    [SerializeReference] public BlackboardVariable<float> MinimumDistanceToGoal;

    public override bool IsTrue()
    {
        var enemyBallOwner = FootballSim.Football.Football.Instance.CurrentOwnerPlayer;

        var ourGoalPosition = Player.Value.TeamFlag == FootballSim.FootballTeam.TeamFlag.Away ? FootballSim.MatchManager.Instance.PitchData.AwayGoalTransform :
                FootballSim.MatchManager.Instance.PitchData.HomeGoalTransform;


        var criticalDistance = Player.Value.Data.MinimumShootDistance + MinimumDistanceToGoal.Value;
        return (ourGoalPosition.transform.position - enemyBallOwner.transform.position).sqrMagnitude <= criticalDistance * 1.5f
        && (enemyBallOwner.transform.position - Player.Value.transform.position).sqrMagnitude < criticalDistance * 2 && Player.Value.CanPossesTheBall;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
