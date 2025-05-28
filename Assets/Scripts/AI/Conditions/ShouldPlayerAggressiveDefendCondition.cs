using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "ShouldPlayerAggressiveDefend", story: "Should [Player] aggressively defend with [MinimumDistanceToGoal]", category: "FootballPlayer/Conditions", id: "7244f9240adeb58243e6467878682855")]
public partial class ShouldPlayerAggressiveDefendCondition : Condition
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;
    [SerializeReference] public BlackboardVariable<float> MinimumDistanceToGoal;
    public override bool IsTrue()
    {
        var distance = FootballSim.Football.Football.Instance.CurrentOwnerPlayer.transform.position - Player.Value.transform.position;
        var playerSTeam = Player.Value.TeamFlag;
        Transform allyGoalBounds;
        switch (playerSTeam)
        {
            case FootballSim.FootballTeam.TeamFlag.Home:
                allyGoalBounds = FootballSim.MatchManager.Instance.PitchData.HomeGoalTransform;
                break;
            case FootballSim.FootballTeam.TeamFlag.Away:
                allyGoalBounds = FootballSim.MatchManager.Instance.PitchData.AwayGoalTransform;
                break;
            default:
                Debug.LogError("This should not happen");
                return false;

        }
        var distanceToAllyGoalPosition = allyGoalBounds.position - FootballSim.Football.Football.Instance.CurrentOwnerPlayer.transform.position;
     
        return  ((distance.sqrMagnitude <= Player.Value.Data.AggressiveDefenseDistance) ||
        distanceToAllyGoalPosition.sqrMagnitude < Player.Value.Data.MinimumShootDistance + MinimumDistanceToGoal.Value) && Player.Value.CanPossesTheBall;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
